using MySqlConnector;
using Sentinela.Modelos;
using Sentinela.Util;

namespace Sentinela.Dados;

public sealed class ClienteDAO
{
    private readonly ConfiguracaoApp _configuracao;

    public ClienteDAO(ConfiguracaoApp configuracao) => _configuracao = configuracao;

    public List<Cliente> Listar()
    {
        var clientes = new List<Cliente>();
        using var ligacao = ConexaoBD.Abrir(_configuracao);
        using var comando = new MySqlCommand(
            "SELECT Id, Nome, NumeroConta, CidadeHabitual, MediaValor, Simulado, CriadoEm FROM clientes ORDER BY Nome",
            ligacao);
        using var leitor = comando.ExecuteReader();

        while (leitor.Read())
            clientes.Add(LerCliente(leitor));
        return clientes;
    }

    public Cliente? ObterPorId(int id)
    {
        using var ligacao = ConexaoBD.Abrir(_configuracao);
        using var comando = new MySqlCommand(
            "SELECT Id, Nome, NumeroConta, CidadeHabitual, MediaValor, Simulado, CriadoEm FROM clientes WHERE Id=@id",
            ligacao);
        comando.Parameters.AddWithValue("@id", id);
        using var leitor = comando.ExecuteReader();
        return leitor.Read() ? LerCliente(leitor) : null;
    }

    public int Inserir(Cliente cliente)
    {
        using var ligacao = ConexaoBD.Abrir(_configuracao);
        using var comando = new MySqlCommand(
            @"INSERT INTO clientes (Nome, NumeroConta, CidadeHabitual, MediaValor, Simulado)
              VALUES (@nome, @conta, @cidade, @media, @simulado);
              SELECT LAST_INSERT_ID();", ligacao);
        PreencherParametros(comando, cliente);
        return Convert.ToInt32(comando.ExecuteScalar());
    }

    public void Actualizar(Cliente cliente)
    {
        using var ligacao = ConexaoBD.Abrir(_configuracao);
        using var comando = new MySqlCommand(
            @"UPDATE clientes SET Nome=@nome, NumeroConta=@conta,
              CidadeHabitual=@cidade, MediaValor=@media WHERE Id=@id", ligacao);
        PreencherParametros(comando, cliente);
        comando.Parameters.AddWithValue("@id", cliente.Id);
        comando.ExecuteNonQuery();
    }

    public void Eliminar(int id)
    {
        using var ligacao = ConexaoBD.Abrir(_configuracao);
        using var comando = new MySqlCommand("DELETE FROM clientes WHERE Id=@id", ligacao);
        comando.Parameters.AddWithValue("@id", id);
        comando.ExecuteNonQuery();
    }

    private static void PreencherParametros(MySqlCommand comando, Cliente cliente)
    {
        comando.Parameters.AddWithValue("@nome", cliente.Nome.Trim());
        comando.Parameters.AddWithValue("@conta", cliente.NumeroConta.Trim());
        comando.Parameters.AddWithValue("@cidade", cliente.CidadeHabitual);
        comando.Parameters.AddWithValue("@media", cliente.MediaValor);
        comando.Parameters.AddWithValue("@simulado", cliente.Simulado);
    }

    private static Cliente LerCliente(MySqlDataReader leitor) => new()
    {
        Id = leitor.GetInt32("Id"),
        Nome = leitor.GetString("Nome"),
        NumeroConta = leitor.GetString("NumeroConta"),
        CidadeHabitual = leitor.GetString("CidadeHabitual"),
        MediaValor = leitor.GetDecimal("MediaValor"),
        Simulado = leitor.GetBoolean("Simulado"),
        CriadoEm = leitor.GetDateTime("CriadoEm")
    };
}
