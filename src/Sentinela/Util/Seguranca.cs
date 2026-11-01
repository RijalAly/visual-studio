using System.Security.Cryptography;

namespace Sentinela.Util;

public static class Seguranca
{
    private const int Iteracoes = 100_000;
    private const int TamanhoSal = 16;
    private const int TamanhoHash = 32;

    public static (string Sal, string Hash) CriarHash(string palavraPasse)
    {
        byte[] sal = RandomNumberGenerator.GetBytes(TamanhoSal);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            palavraPasse, sal, Iteracoes, HashAlgorithmName.SHA256, TamanhoHash);

        return (Convert.ToBase64String(sal), Convert.ToBase64String(hash));
    }

    public static bool Verificar(string palavraPasse, string salBase64, string hashBase64)
    {
        try
        {
            byte[] sal = Convert.FromBase64String(salBase64);
            byte[] hashGuardado = Convert.FromBase64String(hashBase64);
            byte[] hashCalculado = Rfc2898DeriveBytes.Pbkdf2(
                palavraPasse, sal, Iteracoes, HashAlgorithmName.SHA256, TamanhoHash);

            return CryptographicOperations.FixedTimeEquals(hashCalculado, hashGuardado);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
