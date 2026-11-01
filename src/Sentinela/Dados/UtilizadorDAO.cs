using MySqlConnector;
using Sentinela.Modelos;
using Sentinela.Util;

namespace Sentinela.Dados;

public sealed class UtilizadorDAO
{
    private readonly ConfiguracaoApp _configuracao;

    public UtilizadorDAO(ConfiguracaoApp configuracao) => _configuracao = configuracao;

    public void AlterarPalavraPasse(int utilizadorId, string palavraPasseActual, string palavraPasseNova)
    {
        if (string.IsNullOrWhiteSpace(palavraPasseActual))
            throw new ArgumentException("Introduza a palavra-passe actual.");
        if (palavraPasseNova.Length < 10 ||
            !palavraPasseNova.Any(char.IsUpper) ||
            !palavraPasseNova.Any(char.IsLower) ||
            !palavraPasseNova.Any(char.IsDigit))
            throw new ArgumentException("A nova palavra-passe deve ter pelo menos 10 caracteres, uma maiúscula, uma minúscula e um número.");

        using var ligacao = ConexaoBD.Abrir(_configuracao);
        string salActual;
        string hashActual;

        using (var consulta = new MySqlCommand(
            "SELECT Sal, HashSenha FROM utilizadores WHERE Id=@id LIMIT 1", ligacao))
        {
            consulta.Parameters.AddWithValue("@id", utilizadorId);
            using var leitor = consulta.ExecuteReader();
            if (!leitor.Read())
                throw new InvalidOperationException("A conta seleccionada já não existe.");

            salActual = Convert.ToString(leitor["Sal"]) ?? "";
            hashActual = Convert.ToString(leitor["HashSenha"]) ?? "";
        }

        if (!Seguranca.Verificar(palavraPasseActual, salActual, hashActual))
            throw new InvalidOperationException("A palavra-passe actual não está correcta.");

        var (novoSal, novoHash) = Seguranca.CriarHash(palavraPasseNova);
        using var actualizar = new MySqlCommand(
            "UPDATE utilizadores SET Sal=@sal, HashSenha=@hash WHERE Id=@id", ligacao);
        actualizar.Parameters.AddWithValue("@sal", novoSal);
        actualizar.Parameters.AddWithValue("@hash", novoHash);
        actualizar.Parameters.AddWithValue("@id", utilizadorId);
        actualizar.ExecuteNonQuery();
    }

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
