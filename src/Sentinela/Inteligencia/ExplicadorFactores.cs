using Sentinela.Modelos;

namespace Sentinela.Inteligencia;

public static class ExplicadorFactores
{
    public static List<string> Obter(Transaccao transaccao)
    {
        var factores = new List<string>();

        if (transaccao.RelacaoMedia >= 6)
            factores.Add($"O valor é {transaccao.RelacaoMedia:N1} vezes superior à média habitual.");
        if (transaccao.DataHora.Hour < 5)
            factores.Add("A operação foi realizada durante a madrugada.");
        if (transaccao.OperacoesUltimaHora >= 4)
            factores.Add($"Foram registadas {transaccao.OperacoesUltimaHora} operações na última hora.");
        if (transaccao.CidadeDiferente)
            factores.Add("A cidade da operação é diferente da cidade habitual do cliente.");
        if (transaccao.BeneficiarioNovo)
            factores.Add("A operação indica um beneficiário novo.");

        if (factores.Count == 0)
            factores.Add("Não foram encontrados factores de atenção nos campos apresentados.");

        return factores;
    }
}
