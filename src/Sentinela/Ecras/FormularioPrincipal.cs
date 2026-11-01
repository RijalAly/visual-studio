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
    private readonly Label _subtitulo = new();
    private readonly Label _estadoModelo = new();
    private readonly Label _dataActual = new();
    private readonly Dictionary<string, Button> _botoesMenu = new();
    private readonly Button _acaoRapida = Tema.CriarBotao("＋  Nova análise");
    private string _paginaActual = "Painel";

    private static readonly (string Nome, string Icone, string Descricao)[] Paginas =
    {
        ("Painel", "▦", "Indicadores gerais, actividade recente e alertas."),
        ("Analisar transacção", "⌕", "Avalie uma operação com o apoio do modelo de IA."),
        ("Transacções", "⇄", "Pesquise operações, reveja alertas e consulte o histórico."),
        ("Clientes", "♙", "Consulte e mantenha os perfis dos clientes."),
        ("Modelo de IA", "✧", "Gere dados, treine, compare e acompanhe o modelo."),
        ("Sobre e ajuda", "ⓘ", "Informação do projecto, utilização e limitações.")
    };

    public FormularioPrincipal(ConfiguracaoApp configuracao, Utilizador utilizador, ServicoIA servicoIA)
    {
        _configuracao = configuracao;
        _utilizador = utilizador;
        _servicoIA = servicoIA;

        Tema.PrepararFormulario(this);
        Text = "Sentinela | Inteligência de transacções";
        MinimumSize = new Size(1120, 720);
        Size = new Size(1440, 900);
        WindowState = FormWindowState.Maximized;
        ConstruirInterface();
        AbrirPagina("Painel");
    }

    private void ConstruirInterface()
    {
        var estrutura = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Tema.Fundo,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        estrutura.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 252));
        estrutura.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        Controls.Add(estrutura);

        var menu = ConstruirMenu();
        estrutura.Controls.Add(menu, 0, 0);

        var areaPrincipal = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Tema.Fundo,
            Padding = new Padding(18, 12, 18, 8)
        };
        areaPrincipal.RowStyles.Add(new RowStyle(SizeType.Absolute, 88));
        areaPrincipal.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        areaPrincipal.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        estrutura.Controls.Add(areaPrincipal, 1, 0);

        var cabecalho = ConstruirCabecalho();
        _conteudo.Dock = DockStyle.Fill;
        _conteudo.BackColor = Tema.Fundo;
        _conteudo.Padding = new Padding(0);
        _conteudo.Margin = Padding.Empty;

        var rodape = ConstruirRodape();
        areaPrincipal.Controls.Add(cabecalho, 0, 0);
        areaPrincipal.Controls.Add(_conteudo, 0, 1);
        areaPrincipal.Controls.Add(rodape, 0, 2);
    }

    private Control ConstruirMenu()
    {
        var menu = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Tema.AzulEscuro,
            Padding = new Padding(14, 12, 14, 14)
        };

        var estrutura = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Color.Transparent
        };
        estrutura.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));
        estrutura.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        estrutura.RowStyles.Add(new RowStyle(SizeType.Absolute, 108));
        menu.Controls.Add(estrutura);

        var marca = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        marca.Controls.Add(new Label
        {
            Text = "🛡",
            Font = new Font("Segoe UI Emoji", 29),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(3, 10)
        });
        marca.Controls.Add(new Label
        {
            Text = "SENTINELA",
            Font = new Font("Segoe UI Semibold", 18),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(55, 17)
        });
        marca.Controls.Add(new Label
        {
            Text = "INTELIGÊNCIA DE TRANSACÇÕES",
            Font = new Font("Segoe UI", 7.1f),
            ForeColor = Color.FromArgb(173, 190, 214),
            AutoSize = true,
            Location = new Point(57, 44)
        });
        estrutura.Controls.Add(marca, 0, 0);

        var navegacao = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 8, 0, 6)
        };

        foreach (var pagina in Paginas)
        {
            var botao = new Button
            {
                Text = pagina.Icone + "     " + pagina.Nome,
                Tag = pagina.Nome,
                Width = 218,
                Height = 48,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 8, 0),
                ForeColor = Color.FromArgb(218, 227, 240),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 9.4f),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 3, 0, 5),
                UseVisualStyleBackColor = false
            };
            botao.FlatAppearance.BorderSize = 0;
            botao.FlatAppearance.MouseOverBackColor = Tema.AzulPainel;
            botao.Click += (_, _) => AbrirPagina(pagina.Nome);
            _botoesMenu[pagina.Nome] = botao;
            navegacao.Controls.Add(botao);
        }

        estrutura.Controls.Add(navegacao, 0, 1);

        var perfil = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(31, 47, 71),
            Padding = new Padding(10),
            Margin = new Padding(0, 4, 0, 0)
        };
        var avatar = new Label
        {
            Text = ObterIniciais(_utilizador.NomeCompleto),
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Tema.Azul,
            ForeColor = Color.White,
            Font = new Font("Segoe UI Semibold", 11),
            Size = new Size(38, 38),
            Location = new Point(10, 10)
        };
        perfil.Controls.Add(avatar);
        perfil.Controls.Add(new Label
        {
            Text = _utilizador.NomeCompleto,
            ForeColor = Color.White,
            Font = new Font("Segoe UI Semibold", 9),
            AutoEllipsis = true,
            AutoSize = false,
            Size = new Size(150, 20),
            Location = new Point(58, 9)
        });
        perfil.Controls.Add(new Label
        {
            Text = _utilizador.Perfil,
            ForeColor = Color.FromArgb(177, 193, 215),
            Font = new Font("Segoe UI", 8.2f),
            AutoSize = true,
            Location = new Point(58, 29)
        });

        var terminar = new Button
        {
            Text = "↪  Terminar sessão",
            Dock = DockStyle.Bottom,
            Height = 32,
            ForeColor = Color.FromArgb(220, 230, 242),
            BackColor = Color.Transparent,
            FlatStyle = FlatStyle.Flat,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 8.7f),
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
        terminar.FlatAppearance.BorderSize = 0;
        terminar.FlatAppearance.MouseOverBackColor = Color.FromArgb(49, 66, 91);
        terminar.Click += (_, _) =>
        {
            if (MessageBox.Show("Pretende terminar a sessão?", "Sentinela",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                Close();
        };
        perfil.Controls.Add(terminar);
        estrutura.Controls.Add(perfil, 0, 2);
        return menu;
    }

    private Control ConstruirCabecalho()
    {
        var cabecalho = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(20, 12, 18, 12),
            Margin = new Padding(0, 0, 0, 10)
        };

        _titulo.Text = "Visão geral";
        _titulo.Font = new Font("Segoe UI Semibold", 18);
        _titulo.ForeColor = Tema.Texto;
        _titulo.AutoSize = true;
        _titulo.Location = new Point(18, 10);
        cabecalho.Controls.Add(_titulo);

        _subtitulo.Text = "Acompanhe a actividade e os alertas que precisam de atenção.";
        _subtitulo.Font = new Font("Segoe UI", 8.8f);
        _subtitulo.ForeColor = Tema.TextoSuave;
        _subtitulo.AutoSize = true;
        _subtitulo.Location = new Point(20, 43);
        cabecalho.Controls.Add(_subtitulo);

        _dataActual.Text = DateTime.Now.ToString("dddd, dd 'de' MMMM 'de' yyyy");
        _dataActual.ForeColor = Tema.TextoSuave;
        _dataActual.AutoSize = true;
        _dataActual.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        cabecalho.Controls.Add(_dataActual);

        _acaoRapida.Width = 160;
        _acaoRapida.Height = 38;
        _acaoRapida.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _acaoRapida.Click += (_, _) => AbrirPagina("Analisar transacção");
        cabecalho.Resize += (_, _) =>
        {
            _acaoRapida.Left = cabecalho.ClientSize.Width - _acaoRapida.Width - 8;
            _dataActual.Left = _acaoRapida.Left - _dataActual.Width - 20;
            _dataActual.Top = 20;
        };
        cabecalho.Controls.Add(_acaoRapida);
        return cabecalho;
    }

    private Control ConstruirRodape()
    {
        var rodape = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        var estadoBD = new Label
        {
            Text = "●  Base de dados ligada",
            AutoSize = true,
            ForeColor = Tema.Verde,
            Font = new Font("Segoe UI Semibold", 8),
            Location = new Point(5, 5)
        };
        _estadoModelo.Text = _servicoIA.TemModelo
            ? "●  Modelo de IA carregado"
            : "○  Modelo ainda não treinado";
        _estadoModelo.AutoSize = true;
        _estadoModelo.ForeColor = _servicoIA.TemModelo ? Tema.Verde : Tema.Ambar;
        _estadoModelo.Font = new Font("Segoe UI Semibold", 8);
        _estadoModelo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        rodape.Resize += (_, _) => _estadoModelo.Left = rodape.ClientSize.Width - _estadoModelo.Width - 8;
        rodape.Controls.Add(estadoBD);
        rodape.Controls.Add(_estadoModelo);
        return rodape;
    }

    private void AbrirPagina(string nome)
    {
        _paginaActual = nome;
        var info = Paginas.First(p => p.Nome == nome);
        _titulo.Text = nome == "Painel" ? "Visão geral" : nome;
        _subtitulo.Text = info.Descricao;

        foreach (var item in _botoesMenu)
        {
            bool activo = item.Key == nome;
            item.Value.BackColor = activo ? Tema.AzulPainel : Color.Transparent;
            item.Value.ForeColor = activo ? Color.White : Color.FromArgb(218, 227, 240);
            item.Value.FlatAppearance.BorderColor = activo ? Tema.Azul : Color.Transparent;
            item.Value.FlatAppearance.BorderSize = activo ? 2 : 0;
        }

        foreach (Control controlo in _conteudo.Controls.Cast<Control>().ToArray())
        {
            _conteudo.Controls.Remove(controlo);
            controlo.Dispose();
        }

        UserControl pagina = nome switch
        {
            "Painel" => CriarPaginaPainel(),
            "Analisar transacção" => new PaginaAnalise(_configuracao, _servicoIA),
            "Transacções" => new PaginaTransaccoes(_configuracao, _utilizador),
            "Clientes" => new PaginaClientes(_configuracao),
            "Modelo de IA" => new PaginaModeloIA(_configuracao, _servicoIA),
            _ => new PaginaSobre()
        };

        pagina.Dock = DockStyle.Fill;
        _conteudo.Controls.Add(pagina);
        _estadoModelo.Text = _servicoIA.TemModelo
            ? "●  Modelo de IA carregado"
            : "○  Modelo ainda não treinado";
        _estadoModelo.ForeColor = _servicoIA.TemModelo ? Tema.Verde : Tema.Ambar;
    }

    private PaginaPainel CriarPaginaPainel()
    {
        var painel = new PaginaPainel(_configuracao);
        painel.AbrirTransaccoes += () => AbrirPagina("Transacções");
        painel.IniciarAnalise += () => AbrirPagina("Analisar transacção");
        return painel;
    }

    private static string ObterIniciais(string nome)
    {
        var partes = nome.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (partes.Length == 0) return "S";
        return partes.Length == 1
            ? partes[0][..1].ToUpperInvariant()
            : (partes[0][..1] + partes[^1][..1]).ToUpperInvariant();
    }
}