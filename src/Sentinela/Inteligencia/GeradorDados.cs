using Sentinela.Dados;
using Sentinela.Modelos;
using Sentinela.Util;

namespace Sentinela.Inteligencia;

public sealed class GeradorDados
{
    private static readonly string[] Cidades =
    {
        "Maputo", "Matola", "Beira", "Nampula", "Quelimane", "Tete",
        "Xai-Xai", "Inhambane", "Pemba", "Chimoio", "Lichinga"
    };

    private static readonly string[] Tipos =
    {
        "Levantamento", "Transferência", "Pagamento", "Depósito", "Carteira móvel"
    };

    private static readonly string[] Canais =
    {
        "ATM", "Telemóvel", "Internet banking", "POS"
    };

    private static readonly string[] Nomes =
    {
        "Amélia", "Bento", "Celso", "Dália", "Ernesto", "Fátima",
        "Guilherme", "Helena", "Isaura", "Joaquim", "Lídia", "Manuel",
        "Nádia", "Orlando", "Paulo", "Quitéria", "Rui", "Salomé",
        "Tomás", "Vitória", "Zacarias"
    };

    private readonly ClienteDAO _clientes;
    private readonly TransaccaoDAO _transaccoes;
    private readonly Random _aleatorio = new();

    public GeradorDados(ConfiguracaoApp configuracao)
    {
        _clientes = new ClienteDAO(configuracao);
        _transaccoes = new TransaccaoDAO(configuracao);
    }

    public void Gerar(int quantidadeClientes, int quantidadeTransaccoes)
    {
        if (quantidadeClientes < 2 || quantidadeTransaccoes < 100)
            throw new ArgumentException("Indica pelo menos 2 clientes e 100 transacções.");

        var clientes = new List<Cliente>();
        for (int i = 0; i < quantidadeClientes; i++)
        {
            var media = (decimal)Math.Clamp(Math.Exp(7.4 + Normal() * 0.85), 150, 80000);
            clientes.Add(new Cliente
            {
                Nome = Nomes[_aleatorio.Next(Nomes.Length)] + " " +
                       Nomes[_aleatorio.Next(Nomes.Length)] + " " + (i + 1),
                NumeroConta = "SIM" + Guid.NewGuid().ToString("N")[..12].ToUpperInvariant(),
                CidadeHabitual = Cidades[_aleatorio.Next(Cidades.Length)],
                MediaValor = Math.Round(media, 2),
                Simulado = true
            });
        }

        foreach (var cliente in clientes)
            cliente.Id = _clientes.Inserir(cliente);

        var transaccoes = new List<Transaccao>(quantidadeTransaccoes);
        for (int i = 0; i < quantidadeTransaccoes; i++)
        {
            var cliente = clientes[_aleatorio.Next(clientes.Count)];
            bool suspeita = _aleatorio.NextDouble() < 0.07;
            DateTime dataHora = DateTime.Now.AddDays(-_aleatorio.Next(1, 61))
                .Date.AddHours(_aleatorio.Next(0, 24))
                .AddMinutes(_aleatorio.Next(0, 60));

            decimal valor = Math.Round(cliente.MediaValor * (decimal)Math.Exp(Normal() * 0.65), 2);
            int operacoes = _aleatorio.Next(0, 4);
            bool cidadeDiferente = false;
            bool beneficiarioNovo = _aleatorio.NextDouble() < 0.08;

            int sinaisPretendidos;
            if (suspeita)
                sinaisPretendidos = _aleatorio.NextDouble() < 0.10 ? 1 : _aleatorio.Next(2, 6);
            else
                sinaisPretendidos = _aleatorio.NextDouble() < 0.04 ? 1 : 0;

            var sinais = Enumerable.Range(0, 5).OrderBy(_ => _aleatorio.Next()).Take(sinaisPretendidos).ToHashSet();
            if (sinais.Contains(0))
                valor = Math.Round(cliente.MediaValor * (decimal)(6 + _aleatorio.NextDouble() * 24), 2);
            if (sinais.Contains(1))
                dataHora = dataHora.Date.AddHours(_aleatorio.Next(0, 5)).AddMinutes(_aleatorio.Next(0, 60));
            if (sinais.Contains(2))
                operacoes = _aleatorio.Next(4, 13);
            if (sinais.Contains(3))
                cidadeDiferente = true;
            if (sinais.Contains(4))
                beneficiarioNovo = true;

            string cidade = cidadeDiferente
                ? Cidades.Where(c => c != cliente.CidadeHabitual).OrderBy(_ => _aleatorio.Next()).First()
                : cliente.CidadeHabitual;

            transaccoes.Add(new Transaccao
            {
                ClienteId = cliente.Id,
                DataHora = dataHora,
                Valor = Math.Max(1, valor),
                Tipo = Tipos[_aleatorio.Next(Tipos.Length)],
                Canal = Canais[_aleatorio.Next(Canais.Length)],
                Cidade = cidade,
                CidadeDiferente = cidadeDiferente,
                BeneficiarioNovo = beneficiarioNovo,
                OperacoesUltimaHora = operacoes,
                RelacaoMedia = (float)(valor / Math.Max(1, cliente.MediaValor)),
                Origem = "Simulada",
                Etiqueta = suspeita
            });
        }

        _transaccoes.InserirLote(transaccoes);
    }

    private double Normal()
    {
        double u1 = 1.0 - _aleatorio.NextDouble();
        double u2 = 1.0 - _aleatorio.NextDouble();
        return Math.Sqrt(-2 * Math.Log(u1)) * Math.Sin(2 * Math.PI * u2);
    }
}
