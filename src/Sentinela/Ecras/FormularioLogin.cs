using System.Drawing.Drawing2D;
using Sentinela.Controlos;
using Sentinela.Dados;
using Sentinela.Inteligencia;
using Sentinela.Util;

namespace Sentinela.Ecras;

public sealed class FormularioLogin : Form
{
    private readonly ConfiguracaoApp _configuracao;
    private readonly ServicoIA _servicoIA;
    private readonly TextBox _nome = new();
    private readonly TextBox _palavraPasse = new();
    private readonly Label _erro = new();
    private readonly Button _entrar = Tema.CriarBotao("Entrar na plataforma");
    private readonly CheckBox _mostrar = new();

    public FormularioLogin(ConfiguracaoApp configuracao, ServicoIA servicoIA)
    {
        _configuracao = configuracao;
        _servicoIA = servicoIA;

        Tema.PrepararFormulario(this);
        Text = "Sentinela | Iniciar sessão";
        ClientSize = new Size(980, 610);
        MinimumSize = new Size(850, 560);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;

        var estrutura = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.White,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        estrutura.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 43));
        estrutura.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 57));
        Controls.Add(estrutura);

        var marca = new Panel { Dock = DockStyle.Fill, BackColor = Tema.AzulEscuro };
        marca.Paint += (_, e) =>
        {
            using var pincel = new LinearGradientBrush(marca.ClientRectangle,
                Tema.AzulEscuro, Color.FromArgb(37, 76, 127), 35f);
            e.Graphics.FillRectangle(pincel, marca.ClientRectangle);
        };
        marca.Controls.Add(new Label
        {
            Text = "🛡",
            Font = new Font("Segoe UI Emoji", 44),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(42, 108)
        });
        marca.Controls.Add(new Label
        {
            Text = "SENTINELA",
            Font = new Font("Segoe UI Semibold", 27),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(48, 195)
        });
        marca.Controls.Add(new Label
        {
            Text = "Analisar melhor.\nDecidir com contexto.",
            Font = new Font("Segoe UI Semibold", 17),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(50, 254)
        });
        marca.Controls.Add(new Label
        {
            Text = "Uma ferramenta de apoio à análise\nde operações e sinais fora do padrão.",
            Font = new Font("Segoe UI", 10.5f),
            ForeColor = Color.FromArgb(213, 225, 241),
            AutoSize = true,
            Location = new Point(52, 330)
        });
        marca.Controls.Add(new Label
        {
            Text = "Os alertas precisam sempre de revisão humana.",
            Font = new Font("Segoe UI", 8.5f),
            ForeColor = Color.FromArgb(183, 202, 226),
            AutoSize = true,
            Location = new Point(52, 500)
        });
        estrutura.Controls.Add(marca, 0, 0);

        var zonaLogin = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Tema.Fundo,
            Padding = new Padding(36)
        };
        estrutura.Controls.Add(zonaLogin, 1, 0);

        var cartao = new Panel
        {
            BackColor = Color.White,
            Size = new Size(440, 470),
            Location = new Point(0, 0),
            Padding = new Padding(28),
            BorderStyle = BorderStyle.FixedSingle,
            Anchor = AnchorStyles.None
        };
        zonaLogin.Resize += (_, _) =>
        {
            cartao.Left = Math.Max(10, (zonaLogin.ClientSize.Width - cartao.Width) / 2);
            cartao.Top = Math.Max(10, (zonaLogin.ClientSize.Height - cartao.Height) / 2);
        };
        zonaLogin.Controls.Add(cartao);

        cartao.Controls.Add(Tema.CriarTitulo("Bem-vindo de volta", 21));
        cartao.Controls[0].Location = new Point(28, 30);
        cartao.Controls.Add(new Label
        {
            Text = "Entre com os seus dados para aceder ao Sentinela.",
            AutoSize = true,
            ForeColor = Tema.TextoSuave,
            Location = new Point(30, 68)
        });

        AdicionarCampo(cartao, "Nome de utilizador", _nome, 125);
        AdicionarCampo(cartao, "Palavra-passe", _palavraPasse, 215);
        _palavraPasse.UseSystemPasswordChar = true;

        _mostrar.Text = "Mostrar palavra-passe";
        _mostrar.AutoSize = true;
        _mostrar.Location = new Point(30, 286);
        _mostrar.CheckedChanged += (_, _) => _palavraPasse.UseSystemPasswordChar = !_mostrar.Checked;
        cartao.Controls.Add(_mostrar);

        _erro.Location = new Point(30, 325);
        _erro.Size = new Size(370, 38);
        _erro.ForeColor = Tema.Vermelho;
        _erro.AutoSize = false;
        cartao.Controls.Add(_erro);

        _entrar.Location = new Point(30, 374);
        _entrar.Size = new Size(370, 44);
        _entrar.Click += async (_, _) => await IniciarSessaoAsync();
        cartao.Controls.Add(_entrar);

        var rodape = new Label
        {
            Text = "A segurança da conta começa com uma palavra-passe pessoal.",
            AutoSize = true,
            ForeColor = Tema.TextoSuave,
            Font = new Font("Segoe UI", 8),
            Location = new Point(30, 440)
        };
        cartao.Controls.Add(rodape);

        AcceptButton = _entrar;
    }

    private static void AdicionarCampo(Control pai, string legenda, TextBox campo, int topo)
    {
        pai.Controls.Add(new Label
        {
            Text = legenda,
            Location = new Point(30, topo),
            AutoSize = true,
            ForeColor = Tema.Texto,
            Font = new Font("Segoe UI Semibold", 9)
        });
        campo.Location = new Point(30, topo + 25);
        campo.Size = new Size(370, 36);
        Tema.EstilizarCampo(campo);
        pai.Controls.Add(campo);
    }

    private async Task IniciarSessaoAsync()
    {
        if (string.IsNullOrWhiteSpace(_nome.Text) || string.IsNullOrWhiteSpace(_palavraPasse.Text))
        {
            _erro.Text = "Preencha o utilizador e a palavra-passe.";
            return;
        }

        _entrar.Enabled = false;
        _erro.ForeColor = Tema.TextoSuave;
        _erro.Text = "A validar as credenciais...";
        try
        {
            var utilizador = await Task.Run(() =>
                new UtilizadorDAO(_configuracao).Autenticar(_nome.Text, _palavraPasse.Text));

            if (utilizador is null)
            {
                _erro.ForeColor = Tema.Vermelho;
                _erro.Text = "O utilizador ou a palavra-passe não estão correctos.";
                _palavraPasse.Clear();
                _palavraPasse.Focus();
                return;
            }

            Hide();
            using (var principal = new FormularioPrincipal(_configuracao, utilizador, _servicoIA))
                principal.ShowDialog();
            Close();
        }
        catch (Exception ex)
        {
            _erro.ForeColor = Tema.Vermelho;
            _erro.Text = "Não foi possível iniciar sessão: " + ex.Message;
        }
        finally
        {
            _entrar.Enabled = true;
        }
    }
}