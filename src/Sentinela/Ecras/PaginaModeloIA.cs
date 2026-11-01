using Sentinela.Controlos;
using Sentinela.Dados;
using Sentinela.Inteligencia;
using Sentinela.Util;

namespace Sentinela.Ecras;

public sealed class PaginaModeloIA : UserControl
{
    private readonly ConfiguracaoApp _configuracao;
    private readonly ServicoIA _servicoIA;
    private readonly TransaccaoDAO _transaccoes;
    private readonly TreinoDAO _treinos;
    private readonly GestaoDadosDAO _gestaoDados;
    private readonly NumericUpDown _clientes = new();
    private readonly NumericUpDown _quantidade = new();
    private readonly Button _gerar = Tema.CriarBotao("Gerar dados simulados", Tema.Verde);
    private readonly Button _treinar = Tema.CriarBotao("Treinar / reaprender");
    private readonly Button _eliminar = Tema.CriarBotao("Eliminar dados simulados", Tema.Vermelho);
    private readonly Label _estado = new();
    private readonly TextBox _relatorio = new();

    public PaginaModeloIA(ConfiguracaoApp configuracao, ServicoIA servicoIA)
    {
        _configuracao = configuracao;
        _servicoIA = servicoIA;
        _transaccoes = new TransaccaoDAO(configuracao);
        _treinos = new TreinoDAO(configuracao);
        _gestaoDados = new GestaoDadosDAO(configuracao);
        BackColor = Tema.Fundo;
        ConstruirInterface();
        ActualizarHistorico();
    }

    private void ConstruirInterface()
    {
        var conteudo = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.Transparent };
        var titulo = Tema.CriarTitulo("Treino do modelo de Inteligência Artificial", 19);
        titulo.Location = new Point(8, 8);
        conteudo.Controls.Add(titulo);

        AdicionarLegenda(conteudo, "Número de clientes", _clientes, 75);
        AdicionarLegenda(conteudo, "Número de transacções", _quantidade, 132);
        _clientes.Minimum = 2;
        _clientes.Maximum = 1000;
        _clientes.Value = 40;
        _clientes.Width = 150;
        _quantidade.Minimum = 100;
        _quantidade.Maximum = 100000;
        _quantidade.Increment = 500;
        _quantidade.Value = 6000;
        _quantidade.Width = 150;

        _gerar.Location = new Point(8, 195);
        _gerar.Width = 205;
        _gerar.Click += async (_, _) => await GerarDadosAsync();
        _treinar.Location = new Point(225, 195);
        _treinar.Width = 205;
        _treinar.Click += async (_, _) => await TreinarAsync();
        _eliminar.Location = new Point(442, 195);
        _eliminar.Width = 220;
        _eliminar.Click += async (_, _) => await EliminarDadosAsync();
        conteudo.Controls.Add(_gerar);
        conteudo.Controls.Add(_treinar);
        conteudo.Controls.Add(_eliminar);

        _estado.Location = new Point(8, 250);
        _estado.AutoSize = true;
        _estado.ForeColor = Tema.Azul;
        conteudo.Controls.Add(_estado);

        _relatorio.Location = new Point(8, 280);
        _relatorio.Width = 760;
        _relatorio.Height = 360;
        _relatorio.Multiline = true;
        _relatorio.ReadOnly = true;
        _relatorio.ScrollBars = ScrollBars.Vertical;
        _relatorio.Font = new Font("Consolas", 10);
        _relatorio.BackColor = Color.White;
        conteudo.Controls.Add(_relatorio);

        var explicacao = new Label
        {
            Text = "Exactidão: proporção total de previsões correctas.\r\n" +
                   "Precisão: entre os alertas previstos, quantos eram realmente suspeitos.\r\n" +
                   "Recall: entre os suspeitos reais, quantos foram encontrados.\r\n" +
                   "F1: equilíbrio entre precisão e recall. AUC: capacidade de separar as duas classes.\r\n" +
                   "A exactidão isolada pode enganar quando existem muito mais registos normais do que suspeitos.",
            AutoSize = true,
            ForeColor = Tema.TextoSuave,
            Location = new Point(8, 655),
            MaximumSize = new Size(780, 0)
        };
        conteudo.Controls.Add(explicacao);
        Controls.Add(conteudo);
    }

    private static void AdicionarLegenda(Control parent, string texto, Control controlo, int topo)
    {
        parent.Controls.Add(new Label
        {
            Text = texto,
            AutoSize = true,
            Location = new Point(8, topo),
            ForeColor = Tema.Texto
        });
        controlo.Location = new Point(240, topo - 6);
        controlo.Height = 30;
        parent.Controls.Add(controlo);
    }

    private async Task GerarDadosAsync()
    {
        ActivarControlos(false);
        _estado.Text = "A gerar clientes e transacções fictícias...";
        try
        {
            int clientes = (int)_clientes.Value;
            int quantidade = (int)_quantidade.Value;
            await Task.Run(() => new GeradorDados(_configuracao).Gerar(clientes, quantidade));
            _estado.Text = $"Foram criados {clientes:N0} clientes e {quantidade:N0} transacções fictícias.";
            MessageBox.Show("Os dados simulados foram guardados na base de dados.",
                "Geração concluída", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            _estado.Text = "A geração não foi concluída.";
            MessageBox.Show(ex.Message, "Geração de dados",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            ActivarControlos(true);
        }
    }

    private async Task EliminarDadosAsync()
    {
        var resposta = MessageBox.Show(
            "Serão eliminados os clientes simulados e as transacções associadas a esses clientes. Pretende continuar?",
            "Eliminar dados simulados", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (resposta != DialogResult.Yes) return;

        ActivarControlos(false);
        _estado.Text = "A eliminar os dados simulados...";
        try
        {
            await Task.Run(_gestaoDados.EliminarDadosSimulados);
            _estado.Text = "Os dados simulados foram eliminados.";
            _relatorio.Clear();
        }
        catch (Exception ex)
        {
            _estado.Text = "A eliminação não foi concluída.";
            MessageBox.Show(ex.Message, "Dados simulados",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            ActivarControlos(true);
        }
    }

    private async Task TreinarAsync()
    {
        ActivarControlos(false);
        _relatorio.Clear();
        _estado.Text = "A ler e preparar os dados...";
        try
        {
            var progresso = new Progress<string>(mensagem => _estado.Text = mensagem);
            var pacote = await Task.Run(() =>
            {
                progresso.Report("A consultar os registos no MySQL...");
                var brutos = _transaccoes.ObterDadosTreino();
                progresso.Report("A validar registos e a remover duplicados...");
                var preparacao = new PreparadorDados().Preparar(brutos);
                int revisoes = _transaccoes.ListarPorRevisao("Confirmada").Count +
                               _transaccoes.ListarPorRevisao("Descartada").Count;

                if (preparacao.Dados.Count < 100)
                    throw new InvalidOperationException("Existem menos de 100 registos válidos. Gere mais dados antes do treino.");

                var resultado = _servicoIA.Treinar(preparacao.Dados, revisoes, progresso.Report);
                progresso.Report("A aplicar o modelo a todas as transacções...");
                _transaccoes.ClassificarTodos(_servicoIA.Analisar);
                return (preparacao, resultado);
            });

            var p = pacote.preparacao;
            var r = pacote.resultado;
            _estado.Text = "Treino concluído. O modelo seleccionado foi guardado no computador.";
            _relatorio.Text =
                "PREPARAÇÃO DOS DADOS\r\n" +
                $"Registos recebidos: {p.Recebidos:N0}\r\n" +
                $"Registos inválidos: {p.Invalidos:N0}\r\n" +
                $"Registos repetidos: {p.Repetidos:N0}\r\n" +
                $"Registos usados: {p.Dados.Count:N0}\r\n" +
                $"Normais: {p.Normais:N0}\r\n" +
                $"Suspeitos: {p.Suspeitos:N0}\r\n\r\n" +
                "MODELO SELECCIONADO\r\n" +
                $"Algoritmo: {r.Algoritmo}\r\n" +
                $"Exactidão: {r.Exactidao:P2}\r\n" +
                $"Precisão: {r.Precisao:P2}\r\n" +
                $"Recall: {r.Recall:P2}\r\n" +
                $"F1: {r.F1:P2}\r\n" +
                $"AUC: {r.Auc:P2}\r\n" +
                $"Matriz de confusão: VP={r.VP}, FP={r.FP}, VN={r.VN}, FN={r.FN}\r\n" +
                $"Revisões de analistas incluídas: {r.Revisoes:N0}\r\n" +
                $"Tempo de treino e avaliação: {r.DuracaoSegundos:N1} segundos\r\n\r\n" +
                $"Comparação: {r.AlgoritmoAlternativo}\r\n\r\n" +
                "As métricas foram calculadas neste treino e não representam o desempenho em dados bancários reais.";
            ActualizarHistorico();
        }
        catch (Exception ex)
        {
            _estado.Text = "O treino não foi concluído.";
            _relatorio.Text = ex.Message;
            MessageBox.Show(ex.Message, "Treino do modelo",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            ActivarControlos(true);
        }
    }

    private void ActualizarHistorico()
    {
        try
        {
            var historico = _treinos.ListarRecentes(2);
            if (historico.Count == 0)
            {
                if (!_servicoIA.TemModelo)
                    _estado.Text = "Ainda não existe um modelo treinado.";
                return;
            }
            _estado.Text = _servicoIA.TemModelo
                ? "Existe um modelo treinado e carregado."
                : "Existe histórico de treino, mas o ficheiro do modelo não foi encontrado.";
        }
        catch (Exception ex)
        {
            _estado.Text = "Não foi possível ler o histórico: " + ex.Message;
        }
    }

    private void ActivarControlos(bool activo)
    {
        _gerar.Enabled = activo;
        _treinar.Enabled = activo;
        _eliminar.Enabled = activo;
        _clientes.Enabled = activo;
        _quantidade.Enabled = activo;
        UseWaitCursor = !activo;
    }
}
