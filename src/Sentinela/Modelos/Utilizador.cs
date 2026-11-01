namespace Sentinela.Modelos;

public sealed class Utilizador
{
    public int Id { get; set; }
    public string NomeUtilizador { get; set; } = "";
    public string NomeCompleto { get; set; } = "";
    public string Perfil { get; set; } = "Analista";
}
