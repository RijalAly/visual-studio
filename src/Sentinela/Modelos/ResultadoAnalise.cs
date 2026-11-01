namespace Sentinela.Modelos;

public sealed class ResultadoAnalise
{
    public float Probabilidade { get; set; }
    public float Confianca { get; set; }
    public bool SuspeitaPrevista { get; set; }
    public string Classificacao { get; set; } = "Normal";
    public List<string> Factores { get; set; } = new();
}
