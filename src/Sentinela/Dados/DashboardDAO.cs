using MySqlConnector;
using Sentinela.Modelos;
using Sentinela.Util;

namespace Sentinela.Dados;

public sealed class DashboardDAO
{
    private readonly ConfiguracaoApp _configuracao;

    public DashboardDAO(ConfiguracaoApp configuracao) => _configuracao = configuracao;

    public DadosPainel ObterResumo()
    {
        var resumo = new DadosPainel();
        using var ligacao = ConexaoBD.Abrir(_configuracao);

        using (var comando = new MySqlCommand(
            @"SELECT
                (SELECT COUNT(*) FROM transaccoes) AS TotalTransaccoes,
                (SELECT COUNT(*) FROM transaccoes WHERE DataHora >= CURDATE()) AS TransaccoesHoje,
                (SELECT COUNT(*) FROM clientes) AS TotalClientes,
                (SELECT COUNT(*) FROM transaccoes
                    WHERE Revisao='Pendente' AND Classificacao='Transacção potencialmente suspeita') AS AlertasPendentes,
                (SELECT COUNT(*) FROM transaccoes WHERE Revisao='Confirmada') AS SuspeitasConfirmadas,
                (SELECT COUNT(*) FROM transaccoes WHERE Revisao='Descartada') AS AlertasDescartados,
                (SELECT COALESCE(SUM(Valor),0) FROM transaccoes WHERE DataHora >= CURDATE()) AS VolumeHoje",
            ligacao))
        using (var leitor = comando.ExecuteReader())
        {
            if (leitor.Read())
            {
                resumo.TotalTransaccoes = Convert.ToInt32(leitor["TotalTransaccoes"]);
                resumo.TransaccoesHoje = Convert.ToInt32(leitor["TransaccoesHoje"]);
                resumo.TotalClientes = Convert.ToInt32(leitor["TotalClientes"]);
                resumo.AlertasPendentes = Convert.ToInt32(leitor["AlertasPendentes"]);
                resumo.SuspeitasConfirmadas = Convert.ToInt32(leitor["SuspeitasConfirmadas"]);
                resumo.AlertasDescartados = Convert.ToInt32(leitor["AlertasDescartados"]);
                resumo.VolumeHoje = Convert.ToDecimal(leitor["VolumeHoje"]);
            }
        }

        using (var comando = new MySqlCommand(
            @"SELECT DATE(DataHora) AS Dia, COUNT(*) AS Quantidade
              FROM transaccoes
              WHERE DataHora >= CURDATE() - INTERVAL 29 DAY
              GROUP BY DATE(DataHora)
              ORDER BY Dia",
            ligacao))
        using (var leitor = comando.ExecuteReader())
        {
            while (leitor.Read())
                resumo.Actividade.Add(new AtividadePainel
                {
                    Dia = Convert.ToDateTime(leitor["Dia"]),
                    Quantidade = Convert.ToInt32(leitor["Quantidade"])
                });
        }

        using (var comando = new MySqlCommand(
            @"SELECT t.Id, t.ClienteId, c.Nome AS NomeCliente, c.NumeroConta,
                     t.DataHora, t.Valor, t.Tipo, t.Canal, t.Cidade,
                     t.CidadeDiferente, t.BeneficiarioNovo, t.OperacoesUltimaHora,
                     t.RelacaoMedia, t.Pontuacao, t.Classificacao, t.Revisao,
                     t.Origem, t.Etiqueta
              FROM transaccoes t
              INNER JOIN clientes c ON c.Id=t.ClienteId
              WHERE t.Classificacao IN ('Atenção','Transacção potencialmente suspeita')
              ORDER BY t.DataHora DESC
              LIMIT 8",
            ligacao))
        using (var leitor = comando.ExecuteReader())
        {
            while (leitor.Read())
                resumo.AlertasRecentes.Add(new Transaccao
                {
                    Id = Convert.ToInt64(leitor["Id"]),
                    ClienteId = Convert.ToInt32(leitor["ClienteId"]),
                    NomeCliente = Convert.ToString(leitor["NomeCliente"]) ?? "",
                    NumeroConta = Convert.ToString(leitor["NumeroConta"]) ?? "",
                    DataHora = Convert.ToDateTime(leitor["DataHora"]),
                    Valor = Convert.ToDecimal(leitor["Valor"]),
                    Tipo = Convert.ToString(leitor["Tipo"]) ?? "",
                    Canal = Convert.ToString(leitor["Canal"]) ?? "",
                    Cidade = Convert.ToString(leitor["Cidade"]) ?? "",
                    CidadeDiferente = Convert.ToBoolean(leitor["CidadeDiferente"]),
                    BeneficiarioNovo = Convert.ToBoolean(leitor["BeneficiarioNovo"]),
                    OperacoesUltimaHora = Convert.ToInt32(leitor["OperacoesUltimaHora"]),
                    RelacaoMedia = Convert.ToSingle(leitor["RelacaoMedia"]),
                    Pontuacao = leitor["Pontuacao"] is DBNull ? null : Convert.ToSingle(leitor["Pontuacao"]),
                    Classificacao = Convert.ToString(leitor["Classificacao"]) ?? "",
                    Revisao = Convert.ToString(leitor["Revisao"]) ?? "",
                    Origem = Convert.ToString(leitor["Origem"]) ?? "",
                    Etiqueta = leitor["Etiqueta"] is DBNull ? null : Convert.ToInt32(leitor["Etiqueta"]) == 1
                });
        }

        return resumo;
    }
}
