using Sentinela.Controlos;
using Sentinela.Dados;
using Sentinela.Util;

namespace Sentinela.Ecras;

public sealed class FormularioLigacao : Form
{
    private readonly TextBox _servidor = new();
    private readonly NumericUpDown _porta = new();
    private readonly TextBox _utilizador = new();
    private readonly TextBox _palavraPasse = new();
    private readonly TextBox _baseDados = new();
    private readonly CheckBox _mostrar = new();
    private readonly Label _estado = new();
    private readonly Button _testar = Tema.CriarBotao("Testar e guardar", Tema.Verde);

    public ConfiguracaoApp Configuracao { get; private set; }

    public FormularioLigacao(ConfiguracaoApp actual)
    {
        Configuracao = actual;
        Tema.PrepararFormulario(this);
        Text = "Sentinela | Configuração da ligação";
        ClientSize = new Size(820, 600);
        MinimumSize = new Size(760, 560);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        var estrutura = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.White,
            Padding = Padding.Empty,
            Margin = Padding.Empty
        };
        estrutura.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
        estrutura.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 66));
        Controls.Add(estrutura);

        var explicacao = new Panel { Dock = DockStyle.Fill, BackColor = Tema.AzulEscuro, Padding = new Padding(26) };
        explicacao.Paint += (_, e) =>
        {
            using var pincel = new System.Drawing.Drawing2D.LinearGradientBrush(
                explicacao.ClientRectangle, Tema.AzulEscuro, Color.FromArgb(38, 77, 125), 40f);
            e.Graphics.FillRectangle(pincel, explicacao.ClientRectangle);
        };
        explicacao.Controls.Add(new Label
        {
            Text = "🔌",
            Font = new Font("Segoe UI Emoji", 35),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(26, 75)
        });
        explicacao.Controls.Add(new Label
        {
            Text = "Ligar à\nbase de dados",
            AutoSize = true,
            ForeColor = Color.White,
            Font = new Font("Segoe UI Semibold", 22),
            Location = new Point(30, 155)
        });
        explicacao.Controls.Add(new Label
        {
            Text = "O Sentinela utiliza MySQL ou MariaDB para guardar clientes, operações, revisões e resultados de treino.",
            AutoSize = true,
            MaximumSize = new Size(215, 0),
            ForeColor = Color.FromArgb(218, 229, 244),
            Font = new Font("Segoe UI", 10),
            Location = new Point(32, 250)
        });
        estrutura.Controls.Add(explicacao, 0, 0);

        var formulario = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Tema.Fundo,
            Padding = new Padding(25)
        };
        estrutura.Controls.Add(formulario, 1, 0);

        var cartao = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(24),
            BorderStyle = BorderStyle.FixedSingle
        };
        formulario.Controls.Add(cartao);

        var titulo = Tema.CriarTitulo("Configurar ligação", 20);
        titulo.Location = new Point(22, 20);
        cartao.Controls.Add(titulo);
        cartao.Controls.Add(new Label
        {
            Text = "Introduza os dados do servidor MySQL ou do MariaDB do XAMPP.",
            AutoSize = true,
            ForeColor = Tema.TextoSuave,
            Location = new Point(24, 56)
        });

        AdicionarCampo(cartao, "Servidor / endereço", _servidor, 105);
        AdicionarCampo(cartao, "Porta", _porta, 171);
        AdicionarCampo(cartao, "Utilizador", _utilizador, 237);
        AdicionarCampo(cartao, "Palavra-passe", _palavraPasse, 303);
        AdicionarCampo(cartao, "Base de dados", _baseDados, 369);

        _porta.Minimum = 1;
        _porta.Maximum = 65535;
        _servidor.Text = actual.Servidor;
        _porta.Value = actual.Porta;
        _utilizador.Text = actual.Utilizador;
        _palavraPasse.Text = actual.PalavraPasse;
        _palavraPasse.UseSystemPasswordChar = true;
        _baseDados.Text = string.IsNullOrWhiteSpace(actual.BaseDados) ? "sentinela_ia" : actual.BaseDados;

        _mostrar.Text = "Mostrar palavra-passe";
        _mostrar.AutoSize = true;
        _mostrar.Location = new Point(24, 425);
        _mostrar.CheckedChanged += (_, _) => _palavraPasse.UseSystemPasswordChar = !_mostrar.Checked;
        cartao.Controls.Add(_mostrar);

        _estado.AutoSize = true;
        _estado.MaximumSize = new Size(500, 0);
        _estado.ForeColor = Tema.TextoSuave;
        _estado.Location = new Point(24, 462);
        cartao.Controls.Add(_estado);

        var cancelar = Tema.CriarBotaoSecundario("Cancelar");
        cancelar.Location = new Point(275, 520);
        cancelar.Width = 120;
        cancelar.DialogResult = DialogResult.Cancel;
        cartao.Controls.Add(cancelar);

        _testar.Location = new Point(410, 520);
        _testar.Width = 170;
        _testar.Click += async (_, _) => await TestarLigacaoAsync();
        cartao.Controls.Add(_testar);
        CancelButton = cancelar;
        AcceptButton = _testar;
    }

    private static void AdicionarCampo(Control pai, string legenda, Control controlo, int topo)
    {
        pai.Controls.Add(new Label
        {
            Text = legenda,
            Location = new Point(24, topo),
            AutoSize = true,
            ForeColor = Tema.Texto,
            Font = new Font("Segoe UI Semibold", 8.8f)
        });
        controlo.Location = new Point(210, topo - 6);
        controlo.Width = 365;
        controlo.Height = 34;
        Tema.EstilizarCampo(controlo);
        pai.Controls.Add(controlo);
    }

    private async Task TestarLigacaoAsync()
    {
        var tentativa = new ConfiguracaoApp
        {
            Servidor = _servidor.Text.Trim(),
            Porta = (uint)_porta.Value,
            Utilizador = _utilizador.Text.Trim(),
            PalavraPasse = _palavraPasse.Text,
            BaseDados = _baseDados.Text.Trim()
        };

        if (string.IsNullOrWhiteSpace(tentativa.Servidor) ||
            string.IsNullOrWhiteSpace(tentativa.Utilizador) ||
            string.IsNullOrWhiteSpace(tentativa.BaseDados))
        {
            _estado.Text = "Preencha o servidor, utilizador e base de dados.";
            _estado.ForeColor = Tema.Vermelho;
            return;
        }

        _testar.Enabled = false;
        _estado.Text = "A testar a ligação e a preparar a base de dados...";
        _estado.ForeColor = Tema.Azul;
        UseWaitCursor = true;

        try
        {
            bool ligada = await Task.Run(() => ConexaoBD.Testar(tentativa, criarBase: true));
            if (!ligada)
            {
                _estado.Text = "Não foi possível ligar. Confirme o serviço, endereço, porta e credenciais.";
                _estado.ForeColor = Tema.Vermelho;
                return;
            }

            Configuracao = tentativa;
            _estado.Text = "Ligação validada. A configuração será guardada neste computador.";
            _estado.ForeColor = Tema.Verde;
            DialogResult = DialogResult.OK;
            Close();
        }
        finally
        {
            UseWaitCursor = false;
            _testar.Enabled = true;
        }
    }
}