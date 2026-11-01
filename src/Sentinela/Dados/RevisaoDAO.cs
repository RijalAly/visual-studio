using MySqlConnector;
using Sentinela.Modelos;
using Sentinela.Util;

namespace Sentinela.Dados;

public sealed class RevisaoDAO
{
    private readonly ConfiguracaoApp _configuracao;

    public RevisaoDAO(ConfiguracaoApp configuracao) => _configuracao = configuracao;

    public void Registar(long transaccaoId, int utilizadorId, string estadoNovo, string observacao)
    {
        if (estadoNovo is not ("Confirmada" or "Descartada" or "Pendente"))
            throw new ArgumentException("Estado de revisão inválido.");
        if (estadoNovo != "Pendente" && string.IsNullOrWhiteSpace(observacao))
            throw new ArgumentException("Escreva uma justificação para confirmar ou descartar o alerta.");
        if (observacao.Length > 500)
            throw new ArgumentException("A justificação não pode ultrapassar 500 caracteres.");

        using var ligacao = ConexaoBD.Abrir(_configuracao);
        using var transaccao = ligacao.BeginTransaction();

        string estadoAnterior;
        using (var consulta = new MySqlCommand(
            "SELECT Revisao FROM transaccoes WHERE Id=@id FOR UPDATE", ligacao, transaccao))
        {
            consulta.Parameters.AddWithValue("@id", transaccaoId);
            object? valor = consulta.ExecuteScalar();
            if (valor is null || valor is DBNull)
                throw new InvalidOperationException("A transacção seleccionada já não existe.");
            estadoAnterior = Convert.ToString(valor) ?? "Pendente";
        }

        using (var actualizar = new MySqlCommand(
            "UPDATE transaccoes SET Revisao=@estado WHERE Id=@id", ligacao, transaccao))
        {
            actualizar.Parameters.AddWithValue("@estado", estadoNovo);
            actualizar.Parameters.AddWithValue("@id", transaccaoId);
            actualizar.ExecuteNonQuery();
        }

        using (var inserir = new MySqlCommand(
            @"INSERT INTO revisoes_transaccao
                (TransaccaoId, UtilizadorId, EstadoAnterior, EstadoNovo, Observacao)
              VALUES (@transaccao, @utilizador, @anterior, @novo, @observacao)",
            ligacao, transaccao))
        {
            inserir.Parameters.AddWithValue("@transaccao", transaccaoId);
            inserir.Parameters.AddWithValue("@utilizador", utilizadorId);
            inserir.Parameters.AddWithValue("@anterior", estadoAnterior);
            inserir.Parameters.AddWithValue("@novo", estadoNovo);
            inserir.Parameters.AddWithValue("@observacao", observacao.Trim());
            inserir.ExecuteNonQuery();
        }

        transaccao.Commit();
    }

    public List<RevisaoHistorico> Listar(long transaccaoId)
    {
        var lista = new List<RevisaoHistorico>();
        using var ligacao = ConexaoBD.Abrir(_configuracao);
        using var comando = new MySqlCommand(
            @"SELECT r.Id, r.TransaccaoId, r.UtilizadorId, u.NomeCompleto AS NomeAnalista,
                     r.EstadoAnterior, r.EstadoNovo, r.Observacao, r.DataHora
              FROM revisoes_transaccao r
              INNER JOIN utilizadores u ON u.Id=r.UtilizadorId
              WHERE r.TransaccaoId=@id
              ORDER BY r.DataHora DESC, r.Id DESC",
            ligacao);
        comando.Parameters.AddWithValue("@id", transaccaoId);
        using var leitor = comando.ExecuteReader();

        while (leitor.Read())
        {
            lista.Add(new RevisaoHistorico
            {
                Id = Convert.ToInt64(leitor["Id"]),
                TransaccaoId = Convert.ToInt64(leitor["TransaccaoId"]),
                UtilizadorId = Convert.ToInt32(leitor["UtilizadorId"]),
                NomeAnalista = Convert.ToString(leitor["NomeAnalista"]) ?? "",
                EstadoAnterior = Convert.ToString(leitor["EstadoAnterior"]) ?? "",
                EstadoNovo = Convert.ToString(leitor["EstadoNovo"]) ?? "",
                Observacao = Convert.ToString(leitor["Observacao"]) ?? "",
                DataHora = Convert.ToDateTime(leitor["DataHora"])
            });
        }
        return lista;
    }
}
