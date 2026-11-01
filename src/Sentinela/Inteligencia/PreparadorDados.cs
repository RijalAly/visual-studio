using Sentinela.Modelos;

namespace Sentinela.Inteligencia;

public sealed class PreparadorDados
{
    public ResultadoPreparacao Preparar(IEnumerable<DadosIA> origem)
    {
        var resultado = new ResultadoPreparacao();
        var validos = new List<DadosIA>();

        foreach (var item in origem)
        {
            resultado.Recebidos++;

            bool valido =
                float.IsFinite(item.Valor) && item.Valor > 0 &&
                float.IsFinite(item.Hora) && item.Hora >= 0 && item.Hora <= 23 &&
                float.IsFinite(item.RelacaoMedia) && item.RelacaoMedia >= 0 &&
                float.IsFinite(item.OperacoesUltimaHora) && item.OperacoesUltimaHora >= 0 &&
                !string.IsNullOrWhiteSpace(item.Tipo) &&
                !string.IsNullOrWhiteSpace(item.Canal);

            if (!valido)
            {
                resultado.Invalidos++;
                continue;
            }

            validos.Add(item);
        }

        var unicos = validos
            .GroupBy(x => new
            {
                x.Label, x.Valor, x.Hora, x.RelacaoMedia, x.OperacoesUltimaHora,
                x.CidadeDiferente, x.BeneficiarioNovo, x.Tipo, x.Canal
            })
            .Select(g => g.First())
            .ToList();

        resultado.Repetidos = validos.Count - unicos.Count;
        resultado.Dados = unicos;
        resultado.Normais = unicos.Count(x => !x.Label);
        resultado.Suspeitos = unicos.Count(x => x.Label);
        return resultado;
    }
}
