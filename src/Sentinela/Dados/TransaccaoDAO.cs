using MySqlConnector;
using Sentinela.Modelos;
using Sentinela.Util;

namespace Sentinela.Dados;

public sealed class TransaccaoDAO
{
    private readonly ConfiguracaoApp _configuracao;

    public TransaccaoDAO(ConfiguracaoApp configuracao) => _configuracao = configuracao;

    public void InserirLote(IEnumerable<Transaccao> transaccoes)
    {
        using var ligacao = ConexaoBD.Abrir(_configuracao);
        using var transaccaoBD = ligacao.BeginTransaction();
        const string sql = @"INSERT INTO transaccoes
            (ClienteId, DataHora, Valor, Tipo, Canal, Cidade, CidadeDiferente,
             BeneficiarioNovo, OperacoesUltimaHora, RelacaoMedia, Origem, Etiqueta)
            VALUES (@cliente, @data, @valor, @tipo, @canal, @cidade, @cidadeDif,
                    @benefNovo, @operacoes, @relacao, @origem, @etiqueta)";

        using var comando = new MySqlCommand(sql, ligacao, transaccaoBD);
        comando.Parameters.Add("@cliente", MySqlDbType.Int32);
        comando.Parameters.Add("@data", MySqlDbType.DateTime);
        comando.Parameters.Add("@valor", MySqlDbType.Decimal);
        comando.Parameters.Add("@tipo", MySqlDbType.VarChar);
        comando.Parameters.Add("@canal", MySqlDbType.VarChar);
        comando.Parameters.Add("@cidade", MySqlDbType.VarChar);
        comando.Parameters.Add("@cidadeDif", MySqlDbType.Bool);
        comando.Parameters.Add("@benefNovo", MySqlDbType.Bool);
        comando.Parameters.Add("@operacoes", MySqlDbType.Int32);
        comando.Parameters.Add("@relacao", MySqlDbType.Float);
        comando.Parameters.Add("@origem", MySqlDbType.VarChar);
        comando.Parameters.Add("@etiqueta", MySqlDbType.Byte);

        foreach (var item in transaccoes)
        {
            comando.Parameters["@cliente"].Value = item.ClienteId;
            comando.Parameters["@data"].Value = item.DataHora;
            comando.Parameters["@valor"].Value = item.Valor;
            comando.Parameters["@tipo"].Value = item.Tipo;
            comando.Parameters["@canal"].Value = item.Canal;
            comando.Parameters["@cidade"].Value = item.Cidade;
            comando.Parameters["@cidadeDif"].Value = item.CidadeDiferente;
            comando.Parameters["@benefNovo"].Value = item.BeneficiarioNovo;
            comando.Parameters["@operacoes"].Value = item.OperacoesUltimaHora;
            comando.Parameters["@relacao"].Value = item.RelacaoMedia;
            comando.Parameters["@origem"].Value = item.Origem;
            comando.Parameters["@etiqueta"].Value = item.Etiqueta.HasValue
                ? (object)(byte)(item.Etiqueta.Value ? 1 : 0) : DBNull.Value;
            comando.ExecuteNonQuery();
        }

        transaccaoBD.Commit();
    }

    public int ContarUltimaHora(int clienteId, DateTime dataHora)
    {
        using var ligacao = ConexaoBD.Abrir(_configuracao);
        using var comando = new MySqlCommand(
            @"SELECT COUNT(*) FROM transaccoes
              WHERE ClienteId=@cliente AND DataHora>@inicio AND DataHora<=@fim",
            ligacao);
        comando.Parameters.AddWithValue("@cliente", clienteId);
        comando.Parameters.AddWithValue("@inicio", dataHora.AddHours(-1));
        comando.Parameters.AddWithValue("@fim", dataHora);
        return Convert.ToInt32(comando.ExecuteScalar());
    }

    public List<Transaccao> Listar(int limite = 1000)
    {
        var lista = new List<Transaccao>();
        using var ligacao = ConexaoBD.Abrir(_configuracao);
        using var comando = new MySqlCommand(
            @"SELECT t.*, c.Nome AS NomeCliente, c.NumeroConta
              FROM transaccoes t INNER JOIN clientes c ON c.Id=t.ClienteId
              ORDER BY t.DataHora DESC LIMIT @limite", ligacao);
        comando.Parameters.AddWithValue("@limite", Math.Clamp(limite, 1, 500000));
        using var leitor = comando.ExecuteReader();
        while (leitor.Read()) lista.Add(LerTransaccao(leitor));
        return lista;
    }

    public List<Transaccao> ListarPorRevisao(string revisao)
    {
        var lista = new List<Transaccao>();
        using var ligacao = ConexaoBD.Abrir(_configuracao);
        using var comando = new MySqlCommand(
            @"SELECT t.*, c.Nome AS NomeCliente, c.NumeroConta
              FROM transaccoes t INNER JOIN clientes c ON c.Id=t.ClienteId
              WHERE t.Revisao=@revisao ORDER BY t.DataHora DESC", ligacao);
        comando.Parameters.AddWithValue("@revisao", revisao);
        using var leitor = comando.ExecuteReader();
        while (leitor.Read()) lista.Add(LerTransaccao(leitor));
        return lista;
    }

    public void GuardarRevisao(long id, string revisao)
    {
        if (revisao is not ("Confirmada" or "Descartada" or "Pendente"))
            throw new ArgumentException("Estado de revisão inválido.");

        using var ligacao = ConexaoBD.Abrir(_configuracao);
        using var comando = new MySqlCommand(
            "UPDATE transaccoes SET Revisao=@revisao WHERE Id=@id", ligacao);
        comando.Parameters.AddWithValue("@revisao", revisao);
        comando.Parameters.AddWithValue("@id", id);
        comando.ExecuteNonQuery();
    }

    public List<DadosIA> ObterDadosTreino()
    {
        var dados = new List<DadosIA>();
        using var ligacao = ConexaoBD.Abrir(_configuracao);
        using var comando = new MySqlCommand(
            @"SELECT t.DataHora, t.Valor, t.RelacaoMedia, t.OperacoesUltimaHora,
                     t.CidadeDiferente, t.BeneficiarioNovo, t.Tipo, t.Canal,
                     CASE WHEN t.Revisao='Confirmada' THEN 1
                          WHEN t.Revisao='Descartada' THEN 0
                          ELSE t.Etiqueta END AS EtiquetaFinal
              FROM transaccoes t
              WHERE t.Etiqueta IS NOT NULL OR t.Revisao IN ('Confirmada','Descartada')",
            ligacao);
        using var leitor = comando.ExecuteReader();

        while (leitor.Read())
        {
            int etiqueta = Convert.ToInt32(leitor["EtiquetaFinal"]);
            dados.Add(new DadosIA
            {
                Label = etiqueta == 1,
                Valor = Convert.ToSingle(Convert.ToDecimal(leitor["Valor"])),
                Hora = Convert.ToDateTime(leitor["DataHora"]).Hour,
                RelacaoMedia = Convert.ToSingle(leitor["RelacaoMedia"]),
                OperacoesUltimaHora = Convert.ToInt32(leitor["OperacoesUltimaHora"]),
                CidadeDiferente = Convert.ToBoolean(leitor["CidadeDiferente"]) ? 1 : 0,
                BeneficiarioNovo = Convert.ToBoolean(leitor["BeneficiarioNovo"]) ? 1 : 0,
                Tipo = Convert.ToString(leitor["Tipo"]) ?? string.Empty,
                Canal = Convert.ToString(leitor["Canal"]) ?? string.Empty
            });
        }
        return dados;
    }

    public void GuardarPredicoes(IReadOnlyCollection<(long Id, float Probabilidade, string Classe)> resultados)
    {
        using var ligacao = ConexaoBD.Abrir(_configuracao);
        using var transaccao = ligacao.BeginTransaction();
        using var comando = new MySqlCommand(
            "UPDATE transaccoes SET Pontuacao=@p, Classificacao=@c WHERE Id=@id", ligacao, transaccao);
        comando.Parameters.Add("@p", MySqlDbType.Double);
        comando.Parameters.Add("@c", MySqlDbType.VarChar);
        comando.Parameters.Add("@id", MySqlDbType.Int64);

        foreach (var item in resultados)
        {
            comando.Parameters["@p"].Value = item.Probabilidade;
            comando.Parameters["@c"].Value = item.Classe;
            comando.Parameters["@id"].Value = item.Id;
            comando.ExecuteNonQuery();
        }
        transaccao.Commit();
    }

    public void ClassificarTodos(Func<Transaccao, ResultadoAnalise> analisar)
    {
        var lista = Listar(100000);
        var resultados = lista.Select(t =>
        {
            var resultado = analisar(t);
            return (t.Id, resultado.Probabilidade, resultado.Classificacao);
        }).ToList();
        GuardarPredicoes(resultados);
    }

    private static Transaccao LerTransaccao(MySqlDataReader leitor) => new()
    {
        Id = Convert.ToInt64(leitor["Id"]),
        ClienteId = Convert.ToInt32(leitor["ClienteId"]),
        NomeCliente = Convert.ToString(leitor["NomeCliente"]) ?? string.Empty,
        NumeroConta = Convert.ToString(leitor["NumeroConta"]) ?? string.Empty,
        DataHora = Convert.ToDateTime(leitor["DataHora"]),
        Valor = Convert.ToDecimal(leitor["Valor"]),
        Tipo = Convert.ToString(leitor["Tipo"]) ?? string.Empty,
        Canal = Convert.ToString(leitor["Canal"]) ?? string.Empty,
        Cidade = Convert.ToString(leitor["Cidade"]) ?? string.Empty,
        CidadeDiferente = Convert.ToBoolean(leitor["CidadeDiferente"]),
        BeneficiarioNovo = Convert.ToBoolean(leitor["BeneficiarioNovo"]),
        OperacoesUltimaHora = Convert.ToInt32(leitor["OperacoesUltimaHora"]),
        RelacaoMedia = Convert.ToSingle(leitor["RelacaoMedia"]),
        Pontuacao = leitor["Pontuacao"] is DBNull ? null : Convert.ToSingle(leitor["Pontuacao"]),
        Classificacao = Convert.ToString(leitor["Classificacao"]) ?? string.Empty,
        Revisao = Convert.ToString(leitor["Revisao"]) ?? string.Empty,
        Origem = Convert.ToString(leitor["Origem"]) ?? string.Empty,
        Etiqueta = leitor["Etiqueta"] is DBNull ? null : Convert.ToInt32(leitor["Etiqueta"]) == 1
    };
}
