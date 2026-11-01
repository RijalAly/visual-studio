using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Sentinela.Util;

public sealed class ConfiguracaoApp
{
    private static readonly string Pasta = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Sentinela");
    private static readonly string Ficheiro = Path.Combine(Pasta, "ligacao.json");
    private static readonly byte[] Entropia = Encoding.UTF8.GetBytes("Sentinela-Configuracao-Local");

    public string Servidor { get; set; } = "localhost";
    public uint Porta { get; set; } = 3306;
    public string Utilizador { get; set; } = "root";
    public string PalavraPasse { get; set; } = "";
    public string BaseDados { get; set; } = "sentinela_ia";

    public static ConfiguracaoApp Carregar()
    {
        if (!File.Exists(Ficheiro))
            return new ConfiguracaoApp();

        try
        {
            var json = File.ReadAllText(Ficheiro);
            var dados = JsonSerializer.Deserialize<ConfiguracaoGuardada>(json);
            if (dados is null) return new ConfiguracaoApp();

            string palavraPasse = "";
            if (!string.IsNullOrWhiteSpace(dados.PalavraPasseCifrada))
            {
                var bytes = ProtectedData.Unprotect(
                    Convert.FromBase64String(dados.PalavraPasseCifrada),
                    Entropia, DataProtectionScope.CurrentUser);
                palavraPasse = Encoding.UTF8.GetString(bytes);
            }

            return new ConfiguracaoApp
            {
                Servidor = dados.Servidor,
                Porta = dados.Porta,
                Utilizador = dados.Utilizador,
                PalavraPasse = palavraPasse,
                BaseDados = dados.BaseDados
            };
        }
        catch
        {
            return new ConfiguracaoApp();
        }
    }

    public static void Guardar(ConfiguracaoApp configuracao)
    {
        Directory.CreateDirectory(Pasta);
        var palavra = ProtectedData.Protect(
            Encoding.UTF8.GetBytes(configuracao.PalavraPasse),
            Entropia, DataProtectionScope.CurrentUser);

        var dados = new ConfiguracaoGuardada
        {
            Servidor = configuracao.Servidor,
            Porta = configuracao.Porta,
            Utilizador = configuracao.Utilizador,
            PalavraPasseCifrada = Convert.ToBase64String(palavra),
            BaseDados = configuracao.BaseDados
        };

        File.WriteAllText(Ficheiro,
            JsonSerializer.Serialize(dados, new JsonSerializerOptions { WriteIndented = true }));
    }

    private sealed class ConfiguracaoGuardada
    {
        public string Servidor { get; set; } = "localhost";
        public uint Porta { get; set; } = 3306;
        public string Utilizador { get; set; } = "root";
        public string PalavraPasseCifrada { get; set; } = "";
        public string BaseDados { get; set; } = "sentinela_ia";
    }
}
