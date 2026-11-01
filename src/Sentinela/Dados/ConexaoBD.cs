using MySqlConnector;
using Sentinela.Util;

namespace Sentinela.Dados;

public static class ConexaoBD
{
    public static string CriarCadeia(ConfiguracaoApp configuracao, bool incluirBase = true)
    {
        var construtor = new MySqlConnectionStringBuilder
        {
            Server = configuracao.Servidor,
            Port = configuracao.Porta,
            UserID = configuracao.Utilizador,
            Password = configuracao.PalavraPasse,
            Database = incluirBase ? configuracao.BaseDados : "",
            CharacterSet = "utf8mb4",
            Pooling = true,
            ConnectionTimeout = 8,
            DefaultCommandTimeout = 30
        };
        return construtor.ConnectionString;
    }

    public static MySqlConnection Abrir(ConfiguracaoApp configuracao)
    {
        var ligacao = new MySqlConnection(CriarCadeia(configuracao));
        ligacao.Open();
        return ligacao;
    }

    public static bool Testar(ConfiguracaoApp configuracao, bool criarBase)
    {
        try
        {
            using var ligacao = new MySqlConnection(CriarCadeia(configuracao, incluirBase: !criarBase));
            ligacao.Open();

            if (criarBase)
            {
                using var comando = ligacao.CreateCommand();
                comando.CommandText = $"CREATE DATABASE IF NOT EXISTS `{configuracao.BaseDados}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci";
                comando.ExecuteNonQuery();
            }
            return true;
        }
        catch
        {
            return false;
        }
    }
}
