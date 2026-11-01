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
            @"SELECT DataHora, Escolhido, Algoritmo, NumExemplos, NumRevisoes, NumTeste, Exactidao,
                     Precisao, Recall, F1, Auc, VP, FP, VN, FN, Duracao
              FROM treinos ORDER BY DataHora DESC, Id DESC LIMIT @limite", ligacao);
        comando.Parameters.AddWithValue("@limite", Math.Clamp(limite, 1, 100));
        using var leitor = comando.ExecuteReader();
        while (leitor.Read())
        {
            resultados.Add(new ResultadoTreino
            {
                DataHora = Convert.ToDateTime(leitor["DataHora"]),
                Escolhido = Convert.ToBoolean(leitor["Escolhido"]),
                Algoritmo = Convert.ToString(leitor["Algoritmo"]) ?? string.Empty,
                Exemplos = Convert.ToInt32(leitor["NumExemplos"]),
                Revisoes = Convert.ToInt32(leitor["NumRevisoes"]),
                Teste = Convert.ToInt32(leitor["NumTeste"]),
                Exactidao = Convert.ToDouble(leitor["Exactidao"]),
                Precisao = Convert.ToDouble(leitor["Precisao"]),
                Recall = Convert.ToDouble(leitor["Recall"]),
                F1 = Convert.ToDouble(leitor["F1"]),
                Auc = Convert.ToDouble(leitor["Auc"]),
                VP = Convert.ToInt32(leitor["VP"]),
                FP = Convert.ToInt32(leitor["FP"]),
                VN = Convert.ToInt32(leitor["VN"]),
                FN = Convert.ToInt32(leitor["FN"]),
                DuracaoSegundos = Convert.ToDouble(leitor["Duracao"])
            });
        }
        return resultados;
    }
}
