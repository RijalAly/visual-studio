namespace Sentinela.Modelos;

public sealed class Transaccao
{
    public long Id { get; set; }
    public int ClienteId { get; set; }
    public string NomeCliente { get; set; } = "";
    public string NumeroConta { get; set; } = "";
    public DateTime DataHora { get; set; }
    public decimal Valor { get; set; }
    public string Tipo { get; set; } = "";
    public string Canal { get; set; } = "";
    public string Cidade { get; set; } = "";
    public bool CidadeDiferente { get; set; }
    public bool BeneficiarioNovo { get; set; }
    public int OperacoesUltimaHora { get; set; }
    public float RelacaoMedia { get; set; }
    public float? Pontuacao { get; set; }
    public string Classificacao { get; set; } = "Por analisar";
    public string Revisao { get; set; } = "Pendente";
    public string Origem { get; set; } = "Manual";
    public bool? Etiqueta { get; set; }
}
