using Microsoft.ML;
using Microsoft.ML.Data;
using Sentinela.Dados;
using Sentinela.Modelos;
using Sentinela.Util;

namespace Sentinela.Inteligencia;

public sealed class ServicoIA
{
    private readonly MLContext _ml = new(seed: 42);
    private readonly object _bloqueio = new();
    private readonly string _caminhoModelo;
    private readonly TreinoDAO _treinos;
    private ITransformer? _modelo;
    private PredictionEngine<DadosIA, PredicaoIA>? _motor;

    public bool TemModelo => _modelo is not null;

    public ServicoIA(ConfiguracaoApp configuracao)
    {
        _treinos = new TreinoDAO(configuracao);
        string pasta = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Sentinela");
        Directory.CreateDirectory(pasta);
        _caminhoModelo = Path.Combine(pasta, "modelo.zip");
        CarregarModelo();
    }

    public ResultadoTreino Treinar(IReadOnlyCollection<DadosIA> dados, int numeroRevisoes)
    {
        if (dados.Count < 100)
            throw new InvalidOperationException("São necessários pelo menos 100 registos válidos para treinar o modelo.");

        if (dados.All(x => !x.Label) || dados.All(x => x.Label))
            throw new InvalidOperationException("O conjunto de dados tem de incluir transacções normais e suspeitas.");

        var relogio = System.Diagnostics.Stopwatch.StartNew();
        IDataView conjunto = _ml.Data.LoadFromEnumerable(dados);
        var divisao = _ml.Data.TrainTestSplit(conjunto, testFraction: 0.2, seed: 42);

        var resultadoArvore = TreinarCandidato(divisao.TrainSet, divisao.TestSet, usarArvore: true, dados.Count, numeroRevisoes);
        var resultadoLogistico = TreinarCandidato(divisao.TrainSet, divisao.TestSet, usarArvore: false, dados.Count, numeroRevisoes);

        var escolhido = resultadoArvore.Resultado.F1 > resultadoLogistico.Resultado.F1
            ? resultadoArvore
            : resultadoLogistico.Resultado.F1 > resultadoArvore.Resultado.F1
                ? resultadoLogistico
                : resultadoArvore.Resultado.Auc >= resultadoLogistico.Resultado.Auc
                    ? resultadoArvore : resultadoLogistico;

        var alternativo = ReferenceEquals(escolhido, resultadoArvore)
            ? resultadoLogistico : resultadoArvore;

        Directory.CreateDirectory(Path.GetDirectoryName(_caminhoModelo)!);
        _ml.Model.Save(escolhido.Modelo, divisao.TrainSet.Schema, _caminhoModelo);

        lock (_bloqueio)
        {
            _modelo = escolhido.Modelo;
            _motor = _ml.Model.CreatePredictionEngine<DadosIA, PredicaoIA>(_modelo);
        }

        relogio.Stop();
        escolhido.Resultado.AlgoritmoAlternativo =
            $"{alternativo.Resultado.Algoritmo}: F1 {alternativo.Resultado.F1:P2}, AUC {alternativo.Resultado.Auc:P2}";
        escolhido.Resultado.DuracaoSegundos = relogio.Elapsed.TotalSeconds;

        resultadoArvore.Resultado.DuracaoSegundos = relogio.Elapsed.TotalSeconds;
        resultadoLogistico.Resultado.DuracaoSegundos = relogio.Elapsed.TotalSeconds;
        _treinos.Guardar(resultadoArvore.Resultado, ReferenceEquals(escolhido, resultadoArvore));
        _treinos.Guardar(resultadoLogistico.Resultado, ReferenceEquals(escolhido, resultadoLogistico));

        return escolhido.Resultado;
    }

    private Candidato TreinarCandidato(IDataView treino, IDataView teste,
        bool usarArvore, int total, int revisoes)
    {
        var transformacoes = _ml.Transforms.Categorical.OneHotEncoding(
                "TipoCodificado", nameof(DadosIA.Tipo))
            .Append(_ml.Transforms.Categorical.OneHotEncoding(
                "CanalCodificado", nameof(DadosIA.Canal)))
            .Append(_ml.Transforms.Concatenate("Caracteristicas",
                nameof(DadosIA.Valor),
                nameof(DadosIA.Hora),
                nameof(DadosIA.RelacaoMedia),
                nameof(DadosIA.OperacoesUltimaHora),
                nameof(DadosIA.CidadeDiferente),
                nameof(DadosIA.BeneficiarioNovo),
                "TipoCodificado",
                "CanalCodificado"));

        IEstimator<ITransformer> pipeline;
        string algoritmo;

        if (usarArvore)
        {
            algoritmo = "FastTree";
            pipeline = transformacoes.Append(
                _ml.BinaryClassification.Trainers.FastTree(
                    labelColumnName: nameof(DadosIA.Label),
                    featureColumnName: "Caracteristicas"));
        }
        else
        {
            algoritmo = "Regressão logística (SDCA)";
            pipeline = transformacoes
                .Append(_ml.Transforms.NormalizeMeanVariance(
                    "CaracteristicasNormalizadas", "Caracteristicas"))
                .Append(_ml.BinaryClassification.Trainers.SdcaLogisticRegression(
                    labelColumnName: nameof(DadosIA.Label),
                    featureColumnName: "CaracteristicasNormalizadas"));
        }

        ITransformer modelo = pipeline.Fit(treino);
        IDataView previsoes = modelo.Transform(teste);
        var metricas = _ml.BinaryClassification.Evaluate(
            previsoes, labelColumnName: nameof(DadosIA.Label));

        int vp = 0, fp = 0, vn = 0, fn = 0;
        var linhas = _ml.Data.CreateEnumerable<LinhaAvaliacao>(
            previsoes, reuseRowObject: false);

        foreach (var linha in linhas)
        {
            if (linha.Etiqueta && linha.Previsao) vp++;
            else if (!linha.Etiqueta && linha.Previsao) fp++;
            else if (!linha.Etiqueta && !linha.Previsao) vn++;
            else fn++;
        }

        var resultado = new ResultadoTreino
        {
            Algoritmo = algoritmo,
            Exemplos = total,
            Revisoes = revisoes,
            Teste = vp + fp + vn + fn,
            Exactidao = metricas.Accuracy,
            Precisao = metricas.PositivePrecision,
            Recall = metricas.PositiveRecall,
            F1 = metricas.F1Score,
            Auc = metricas.AreaUnderRocCurve,
            VP = vp,
            FP = fp,
            VN = vn,
            FN = fn
        };

        return new Candidato(modelo, resultado);
    }

    public void CarregarModelo()
    {
        if (!File.Exists(_caminhoModelo)) return;

        try
        {
            var modelo = _ml.Model.Load(_caminhoModelo, out _);
            lock (_bloqueio)
            {
                _modelo = modelo;
                _motor = _ml.Model.CreatePredictionEngine<DadosIA, PredicaoIA>(_modelo);
            }
        }
        catch
        {
            _modelo = null;
            _motor = null;
        }
    }

    public ResultadoAnalise Analisar(Transaccao transaccao)
    {
        var dados = new DadosIA
        {
            Valor = (float)transaccao.Valor,
            Hora = transaccao.DataHora.Hour,
            RelacaoMedia = transaccao.RelacaoMedia,
            OperacoesUltimaHora = transaccao.OperacoesUltimaHora,
            CidadeDiferente = transaccao.CidadeDiferente ? 1 : 0,
            BeneficiarioNovo = transaccao.BeneficiarioNovo ? 1 : 0,
            Tipo = transaccao.Tipo,
            Canal = transaccao.Canal
        };

        PredicaoIA previsao;
        lock (_bloqueio)
        {
            if (_motor is null)
                throw new InvalidOperationException("Ainda não existe um modelo treinado. Abra o ecrã Modelo de IA e execute o treino.");
            previsao = _motor.Predict(dados);
        }

        float probabilidade = Math.Clamp(previsao.Probability, 0, 1);
        string classificacao = probabilidade < 0.25f ? "Normal"
            : probabilidade < 0.50f ? "Atenção"
            : "Transacção potencialmente suspeita";

        return new ResultadoAnalise
        {
            Probabilidade = probabilidade,
            SuspeitaPrevista = previsao.Prevista,
            Confianca = previsao.Prevista ? probabilidade : 1 - probabilidade,
            Classificacao = classificacao,
            Factores = ExplicadorFactores.Obter(transaccao)
        };
    }

    private sealed class Candidato
    {
        public ITransformer Modelo { get; }
        public ResultadoTreino Resultado { get; }

        public Candidato(ITransformer modelo, ResultadoTreino resultado)
        {
            Modelo = modelo;
            Resultado = resultado;
        }
    }

    private sealed class LinhaAvaliacao
    {
        public bool Label { get; set; }

        [ColumnName("PredictedLabel")]
        public bool Previsao { get; set; }
    }
}
