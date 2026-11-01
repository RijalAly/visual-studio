using Microsoft.ML.Data;

namespace Sentinela.Modelos;

public sealed class DadosIA
{
    public bool Label { get; set; }
    public float Valor { get; set; }
    public float Hora { get; set; }
    public float RelacaoMedia { get; set; }
    public float OperacoesUltimaHora { get; set; }
    public float CidadeDiferente { get; set; }
    public float BeneficiarioNovo { get; set; }
    public string Tipo { get; set; } = "";
    public string Canal { get; set; } = "";
}

public sealed class PredicaoIA
{
    [ColumnName("PredictedLabel")]
    public bool Prevista { get; set; }

    public float Probability { get; set; }
    public float Score { get; set; }
}
