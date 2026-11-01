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

        using (var eliminarTransaccoes = new MySqlCommand(
            "DELETE FROM transaccoes WHERE Origem='Simulada'", ligacao, transaccao))
        {
            eliminarTransaccoes.ExecuteNonQuery();
        }

        // Os clientes simulados e as suas operações associadas são removidos em conjunto.
        using (var eliminarClientes = new MySqlCommand(
            "DELETE FROM clientes WHERE Simulado=TRUE", ligacao, transaccao))
        {
            eliminarClientes.ExecuteNonQuery();
        }

        transaccao.Commit();
    }
}
