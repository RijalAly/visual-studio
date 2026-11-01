namespace Sentinela.Modelos;

public sealed class DadosPainel
{
    public int TotalTransaccoes { get; set; }
    public int TransaccoesHoje { get; set; }
    public int TotalClientes { get; set; }
    public int AlertasPendentes { get; set; }
    public int SuspeitasConfirmadas { get; set; }
    public int AlertasDescartados { get; set; }
    public decimal VolumeHoje { get; set; }
    public List<AtividadePainel> Actividade { get; set; } = new();
    public List<Transaccao> AlertasRecentes { get; set; } = new();
}

public sealed class AtividadePainel
{
    public DateTime Dia { get; set; }
    public int Quantidade { get; set; }
}
