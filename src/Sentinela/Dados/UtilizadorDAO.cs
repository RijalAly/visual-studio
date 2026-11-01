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

        string sal = Convert.ToString(leitor["Sal"]) ?? string.Empty;
        string hash = Convert.ToString(leitor["HashSenha"]) ?? string.Empty;
        if (!Seguranca.Verificar(palavraPasse, sal, hash)) return null;

        return new Utilizador
        {
            Id = Convert.ToInt32(leitor["Id"]),
            NomeUtilizador = Convert.ToString(leitor["NomeUtilizador"]) ?? string.Empty,
            NomeCompleto = Convert.ToString(leitor["NomeCompleto"]) ?? string.Empty,
            Perfil = Convert.ToString(leitor["Perfil"]) ?? string.Empty
        };
    }
}
