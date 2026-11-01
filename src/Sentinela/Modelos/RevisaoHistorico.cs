namespace Sentinela.Modelos;

public sealed class RevisaoHistorico
{
    public long Id { get; set; }
    public long TransaccaoId { get; set; }
    public int UtilizadorId { get; set; }
    public string NomeAnalista { get; set; } = "";
    public string EstadoAnterior { get; set; } = "";
    public string EstadoNovo { get; set; } = "";
    public string Observacao { get; set; } = "";
    public DateTime DataHora { get; set; }
}
