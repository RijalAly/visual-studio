using MySqlConnector;
using Sentinela.Util;

namespace Sentinela.Dados;

public sealed class GestaoDadosDAO
{
    private readonly ConfiguracaoApp _configuracao;

    public GestaoDadosDAO(ConfiguracaoApp configuracao) => _configuracao = configuracao;

    public void EliminarDadosSimulados()
    {
        using var ligacao = ConexaoBD.Abrir(_configuracao);
        using var transaccao = ligacao.BeginTransaction();

        using (var eliminarAuditoria = new MySqlCommand(
            @"DELETE FROM revisoes_transaccao
              WHERE TransaccaoId IN (SELECT Id FROM transaccoes WHERE Origem='Simulada')",
            ligacao, transaccao))
        {
            eliminarAuditoria.ExecuteNonQuery();
        }

        using (var eliminarTransaccoes = new MySqlCommand(
            "DELETE FROM transaccoes WHERE Origem='Simulada'", ligacao, transaccao))
        {
            eliminarTransaccoes.ExecuteNonQuery();
        }

        // Mantém os clientes de demonstração que tenham operações manuais ligadas.
        using (var eliminarClientesSemOperacoes = new MySqlCommand(
            @"DELETE FROM clientes
              WHERE Simulado=TRUE
                AND Id NOT IN (SELECT ClienteId FROM transaccoes)",
            ligacao, transaccao))
        {
            eliminarClientesSemOperacoes.ExecuteNonQuery();
        }

        transaccao.Commit();
    }
}
