using System.Drawing.Drawing2D;
using Sentinela.Controlos;
using Sentinela.Inteligencia;
using Sentinela.Modelos;
using Sentinela.Util;

namespace Sentinela.Ecras;

public sealed class FormularioPrincipal : Form
{
    private readonly ConfiguracaoApp _configuracao;
    private readonly Utilizador _utilizador;
    private readonly ServicoIA _servicoIA;
    private readonly Panel _conteudo = new();
    private readonly Label _titulo = new();

    public FormularioPrincipal(ConfiguracaoApp configuracao, Utilizador utilizador, ServicoIA servicoIA)
    {
        _configuracao = configuracao;
        _utilizador = utilizador;
        _servicoIA = servicoIA;

        Tema.PrepararFormulario(this);
        Text = "Sentinela — Análise de transacções";
        MinimumSize = new Size(1120, 700);
        Size = new Size(1360, 850);

        var estrutura = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            BackColor = Tema.Fundo
        };
        estrutura.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 235));
        estrutura.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        estrutura.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        estrutura.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(estrutura);

        var menu = new Panel { Dock = DockStyle.Fill, BackColor = Tema.AzulEscuro, Padding = new Padding(14) };
        menu.Paint += (_, e) =>
        {
            using var pincel = new LinearGradientBrush(menu.ClientRectangle,
                Tema.AzulEscuro, Color.FromArgb(32, 52, 80), 90f);
            e.Graphics.FillRectangle(pincel, menu.ClientRectangle);
        };
        menu.Controls.Add(new Label
        {
            Text = "SENTINELA",
            ForeColor = Color.White,
            Font = new Font("Segoe UI Semibold", 18),
            AutoSize = true,
            Location = new Point(18, 25)
        });

        var opcoes = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(4, 90, 4, 4),
            BackColor = Color.Transparent
        };
        string[] paginas = { "Painel", "Analisar transacção", "Transacções", "Clientes", "Modelo de IA", "Sobre e ajuda" };
        foreach (string pagina in paginas)
        {
            var botao = new Button
            {
                Text = "   " + pagina,
                Width = 195,
                Height = 46,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 4, 0, 4),
                Tag = pagina
            };
            botao.FlatAppearance.BorderSize = 0;
            botao.Click += (_, _) => AbrirPagina(pagina);
            opcoes.Controls.Add(botao);
        }

        var utilizadorLabel = new Label
        {
            Text = _utilizador.NomeCompleto + "\n" + _utilizador.Perfil,
            ForeColor = Color.FromArgb(220, 230, 242),
            AutoSize = true,
            Location = new Point(18, 730),
            Anchor = AnchorStyles.Left | AnchorStyles.Bottom
        };
        var terminar = new Button
        {
            Text = "Terminar sessão",
            ForeColor = Color.White,
            BackColor = Color.FromArgb(45, 61, 82),
            FlatStyle = FlatStyle.Flat,
            Width = 190,
            Height = 38,
            Location = new Point(18, 785),
            Anchor = AnchorStyles.Left | AnchorStyles.Bottom
        };
        terminar.FlatAppearance.BorderSize = 0;
        terminar.Click += (_, _) => Close();

        menu.Controls.Add(opcoes);
        menu.Controls.Add(utilizadorLabel);
        menu.Controls.Add(terminar);

        var cabecalho = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(26, 18, 20, 10) };
        _titulo.Text = "Painel";
        _titulo.Font = new Font("Segoe UI Semibold", 19);
        _titulo.ForeColor = Tema.Texto;
        _titulo.AutoSize = true;
        cabecalho.Controls.Add(_titulo);

        _conteudo.Dock = DockStyle.Fill;
        _conteudo.BackColor = Tema.Fundo;
        _conteudo.Padding = new Padding(18);

        estrutura.Controls.Add(menu, 0, 0);
        estrutura.SetRowSpan(menu, 2);
        estrutura.Controls.Add(cabecalho, 1, 0);
        estrutura.Controls.Add(_conteudo, 1, 1);

        AbrirPagina("Painel");
    }

    private void AbrirPagina(string nome)
    {
        _titulo.Text = nome;
        _conteudo.Controls.Clear();

        UserControl pagina = nome switch
        {
            "Painel" => new PaginaPainel(_configuracao),
            "Analisar transacção" => new PaginaAnalise(_configuracao, _servicoIA),
            "Transacções" => new PaginaTransaccoes(_configuracao),
            "Clientes" => new PaginaClientes(_configuracao),
            "Modelo de IA" => new PaginaModeloIA(_configuracao, _servicoIA),
            _ => new PaginaSobre()
        };

        pagina.Dock = DockStyle.Fill;
        _conteudo.Controls.Add(pagina);
    }
}