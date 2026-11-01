using MySqlConnector;
using Sentinela.Modelos;
using Sentinela.Util;

namespace Sentinela.Dados;

public sealed class UtilizadorDAO
{
    private readonly ConfiguracaoApp _configuracao;

    public UtilizadorDAO(ConfiguracaoApp configuracao) => _configuracao = configuracao;

    public Utilizador? Autenticar(string nomeUtilizador, string palavraPasse)
    {
        using var ligacao = ConexaoBD.Abrir(_configuracao);
        using var comando = new MySqlCommand(
            @"SELECT Id, NomeUtilizador, NomeCompleto, Perfil, Sal, HashSenha
              FROM utilizadores WHERE NomeUtilizador = @nome LIMIT 1", ligacao);
        comando.Parameters.AddWithValue("@nome", nomeUtilizador.Trim());

        using var leitor = comando.ExecuteReader();
        if (!leitor.Read()) return null;

        string sal = leitor.GetString("Sal");
        string hash = leitor.GetString("HashSenha");
        if (!Seguranca.Verificar(palavraPasse, sal, hash)) return null;

        return new Utilizador
        {
            Id = leitor.GetInt32("Id"),
            NomeUtilizador = leitor.GetString("NomeUtilizador"),
            NomeCompleto = leitor.GetString("NomeCompleto"),
            Perfil = leitor.GetString("Perfil")
        };
    }
}
