using MySqlConnector;
using Sentinela.Modelos;
using Sentinela.Util;

namespace Sentinela.Dados;

public sealed class TreinoDAO
{
    private readonly ConfiguracaoApp _configuracao;

    public TreinoDAO(ConfiguracaoApp configuracao) => _configuracao = configuracao;

    public void Guardar(ResultadoTreino r, bool escolhido = true)
    {
        using var ligacao = ConexaoBD.Abrir(_configuracao);
        using var comando = new MySqlCommand(
            @"INSERT INTO treinos
              (Algoritmo, Escolhido, NumExemplos, NumRevisoes, NumTeste,
               Exactidao, Precisao, Recall, F1, Auc, VP, FP, VN, FN, Duracao)
              VALUES (@algoritmo, @escolhido, @exemplos, @revisoes, @teste,
               @exactidao, @precisao, @recall, @f1, @auc, @vp, @fp, @vn, @fn, @duracao)",
            ligacao);
        comando.Parameters.AddWithValue("@algoritmo", r.Algoritmo);
        comando.Parameters.AddWithValue("@escolhido", escolhido);
        comando.Parameters.AddWithValue("@exemplos", r.Exemplos);
        comando.Parameters.AddWithValue("@revisoes", r.Revisoes);
        comando.Parameters.AddWithValue("@teste", r.Teste);
        comando.Parameters.AddWithValue("@exactidao", r.Exactidao);
        comando.Parameters.AddWithValue("@precisao", r.Precisao);
        comando.Parameters.AddWithValue("@recall", r.Recall);
        comando.Parameters.AddWithValue("@f1", r.F1);
        comando.Parameters.AddWithValue("@auc", r.Auc);
        comando.Parameters.AddWithValue("@vp", r.VP);
        comando.Parameters.AddWithValue("@fp", r.FP);
        comando.Parameters.AddWithValue("@vn", r.VN);
        comando.Parameters.AddWithValue("@fn", r.FN);
        comando.Parameters.AddWithValue("@duracao", r.DuracaoSegundos);
        comando.ExecuteNonQuery();
    }

    public List<ResultadoTreino> ListarRecentes(int limite = 20)
    {
        var resultados = new List<ResultadoTreino>();
        using var ligacao = ConexaoBD.Abrir(_configuracao);
        using var comando = new MySqlCommand(
            @"SELECT Algoritmo, NumExemplos, NumRevisoes, NumTeste, Exactidao,
                     Precisao, Recall, F1, Auc, VP, FP, VN, FN, Duracao
              FROM treinos ORDER BY DataHora DESC, Id DESC LIMIT @limite", ligacao);
        comando.Parameters.AddWithValue("@limite", Math.Clamp(limite, 1, 100));
        using var leitor = comando.ExecuteReader();
        while (leitor.Read())
        {
            resultados.Add(new ResultadoTreino
            {
                Algoritmo = leitor.GetString("Algoritmo"),
                Exemplos = leitor.GetInt32("NumExemplos"),
                Revisoes = leitor.GetInt32("NumRevisoes"),
                Teste = leitor.GetInt32("NumTeste"),
                Exactidao = leitor.GetDouble("Exactidao"),
                Precisao = leitor.GetDouble("Precisao"),
                Recall = leitor.GetDouble("Recall"),
                F1 = leitor.GetDouble("F1"),
                Auc = leitor.GetDouble("Auc"),
                VP = leitor.GetInt32("VP"),
                FP = leitor.GetInt32("FP"),
                VN = leitor.GetInt32("VN"),
                FN = leitor.GetInt32("FN"),
                DuracaoSegundos = leitor.GetDouble("Duracao")
            });
        }
        return resultados;
    }
}
