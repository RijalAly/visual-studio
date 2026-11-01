namespace Sentinela.Modelos;

public sealed class ResultadoTreino
{
    public string Algoritmo { get; set; } = "";
    public string AlgoritmoAlternativo { get; set; } = "";
    public int Exemplos { get; set; }
    public int Revisoes { get; set; }
    public int Teste { get; set; }
    public double Exactidao { get; set; }
    public double Precisao { get; set; }
    public double Recall { get; set; }
    public double F1 { get; set; }
    public double Auc { get; set; }
    public int VP { get; set; }
    public int FP { get; set; }
    public int VN { get; set; }
    public int FN { get; set; }
    public double DuracaoSegundos { get; set; }
    public string ResumoPreparacao { get; set; } = "";
}
