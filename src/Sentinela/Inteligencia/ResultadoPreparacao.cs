using Sentinela.Modelos;

namespace Sentinela.Inteligencia;

public sealed class ResultadoPreparacao
{
    public List<DadosIA> Dados { get; set; } = new();
    public int Recebidos { get; set; }
    public int Invalidos { get; set; }
    public int Repetidos { get; set; }
    public int Normais { get; set; }
    public int Suspeitos { get; set; }
}
