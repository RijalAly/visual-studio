using Sentinela.Controlos;
using Sentinela.Inteligencia;
using Sentinela.Modelos;

namespace Sentinela.Ecras;

public sealed class FormularioDetalheTransaccao : Form
{
    public FormularioDetalheTransaccao(Transaccao transaccao)
    {
        Tema.PrepararFormulario(this);
        Text = "Detalhe da operação";
        ClientSize = new Size(700, 620);
        MinimumSize = new Size(620, 560);

        var titulo = Tema.CriarTitulo("Detalhe da operação", 20);
        titulo.Location = new Point(24, 18);
        Controls.Add(titulo);

        var estado = new Label
        {
            Text = transaccao.Classificacao.ToUpperInvariant(),
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 9.5f),
            ForeColor = transaccao.Classificacao.Contains("suspeita") ? Tema.Vermelho :
                transaccao.Classificacao == "Atenção" ? Tema.Ambar : Tema.Verde,
            Location = new Point(28, 58)
        };
        Controls.Add(estado);

        var resumo = new Panel
        {
            Location = new Point(24, 92),
            Size = new Size(650, 110),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        resumo.Controls.Add(new Label
        {
            Text = $"{transaccao.Valor:N2} MZN",
            Font = new Font("Segoe UI Semibold", 26),
            ForeColor = Tema.Texto,
            AutoSize = true,
            Location = new Point(18, 12)
        });
        resumo.Controls.Add(new Label
        {
            Text = $"Probabilidade apresentada pelo modelo: {(transaccao.Pontuacao ?? 0):P1}",
            ForeColor = Tema.TextoSuave,
            AutoSize = true,
            Location = new Point(20, 65)
        });
        Controls.Add(resumo);

        var dados = new TableLayoutPanel
        {
            Location = new Point(24, 220),
            Size = new Size(650, 200),
            ColumnCount = 2,
            RowCount = 5,
            BackColor = Color.White,
            Padding = new Padding(14)
        };
        dados.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        dados.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        for (int i = 0; i < 5; i++)
            dados.RowStyles.Add(new RowStyle(SizeType.Percent, 20));

        AdicionarDado(dados, "Cliente", transaccao.NomeCliente, 0, 0);
        AdicionarDado(dados, "Conta", transaccao.NumeroConta, 1, 0);
        AdicionarDado(dados, "Data e hora", transaccao.DataHora.ToString("dd/MM/yyyy HH:mm"), 0, 1);
        AdicionarDado(dados, "Origem", transaccao.Origem, 1, 1);
        AdicionarDado(dados, "Tipo / canal", $"{transaccao.Tipo} • {transaccao.Canal}", 0, 2);
        AdicionarDado(dados, "Cidade", transaccao.Cidade, 1, 2);
        AdicionarDado(dados, "Revisão", transaccao.Revisao, 0, 3);
        AdicionarDado(dados, "Operações na última hora", transaccao.OperacoesUltimaHora.ToString(), 1, 3);
        AdicionarDado(dados, "Valor / média habitual", $"{transaccao.RelacaoMedia:N1} ×", 0, 4);
        AdicionarDado(dados, "Beneficiário novo", transaccao.BeneficiarioNovo ? "Sim" : "Não", 1, 4);
        Controls.Add(dados);

        Controls.Add(new Label
        {
            Text = "Factores de atenção",
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 10),
            ForeColor = Tema.Texto,
            Location = new Point(26, 438)
        });

        var factores = new TextBox
        {
            Location = new Point(24, 464),
            Size = new Size(650, 92),
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            BackColor = Color.White,
            Text = string.Join(Environment.NewLine, ExplicadorFactores.Obter(transaccao).Select(x => "• " + x))
        };
        Controls.Add(factores);

        Controls.Add(new Label
        {
            Text = "A classificação é um sinal de apoio à análise e não confirma fraude.",
            AutoSize = true,
            ForeColor = Tema.TextoSuave,
            Location = new Point(26, 572)
        });

        var fechar = Tema.CriarBotao("Fechar");
        fechar.Location = new Point(555, 570);
        fechar.Click += (_, _) => Close();
        Controls.Add(fechar);
    }

    private static void AdicionarDado(TableLayoutPanel tabela, string titulo,
        string valor, int coluna, int linha)
    {
        var painel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        painel.Controls.Add(new Label
        {
            Text = titulo,
            AutoSize = true,
            ForeColor = Tema.TextoSuave,
            Font = new Font("Segoe UI", 8),
            Location = new Point(4, 1)
        });
        painel.Controls.Add(new Label
        {
            Text = valor,
            AutoSize = true,
            ForeColor = Tema.Texto,
            Font = new Font("Segoe UI Semibold", 9.3f),
            Location = new Point(4, 20)
        });
        tabela.Controls.Add(painel, coluna, linha);
    }
}