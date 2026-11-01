namespace Sentinela.Modelos;

public sealed class Cliente
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string NumeroConta { get; set; } = "";
    public string CidadeHabitual { get; set; } = "";
    public decimal MediaValor { get; set; }
    public bool Simulado { get; set; }
    public DateTime CriadoEm { get; set; }
}
