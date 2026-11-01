using Sentinela.Controlos;
using Sentinela.Modelos;

namespace Sentinela.Ecras;

public sealed class FormularioCliente : Form
{
    private readonly TextBox _nome = new();
    private readonly TextBox _conta = new();
    private readonly ComboBox _cidade = new();
    private readonly NumericUpDown _media = new();
    private readonly Label _erro = new();

    public Cliente Cliente { get; private set; }

    public FormularioCliente(Cliente? cliente = null)
    {
        Cliente = cliente is null ? new Cliente() : new Cliente
        {
            Id = cliente.Id,
            Nome = cliente.Nome,
            NumeroConta = cliente.NumeroConta,
            CidadeHabitual = cliente.CidadeHabitual,
            MediaValor = cliente.MediaValor,
            Simulado = cliente.Simulado
        };

        Tema.PrepararFormulario(this);
        Text = cliente is null ? "Novo cliente" : "Editar cliente";
        ClientSize = new Size(640, 535);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        var cartao = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(24),
            Margin = new Padding(12),
            BorderStyle = BorderStyle.FixedSingle
        };
        Controls.Add(cartao);

        var titulo = Tema.CriarTitulo(Text, 20);
        titulo.Location = new Point(24, 22);
        cartao.Controls.Add(titulo);
        cartao.Controls.Add(new Label
        {
            Text = "Os dados servem para contextualizar o comportamento habitual do cliente.",
            AutoSize = true,
            ForeColor = Tema.TextoSuave,
            Location = new Point(26, 58)
        });

        AdicionarCampo(cartao, "Nome completo", _nome, 115);
        AdicionarCampo(cartao, "Número de conta", _conta, 190);
        AdicionarCampo(cartao, "Cidade habitual", _cidade, 265);
        AdicionarCampo(cartao, "Média por operação (MZN)", _media, 340);

        _nome.MaxLength = 140;
        _conta.MaxLength = 40;
        _cidade.DropDownStyle = ComboBoxStyle.DropDownList;
        _cidade.Items.AddRange(new object[]
        {
            "Maputo", "Matola", "Beira", "Nampula", "Quelimane", "Tete",
            "Xai-Xai", "Inhambane", "Pemba", "Chimoio", "Lichinga"
        });
        _media.DecimalPlaces = 2;
        _media.Maximum = 999999999;
        _media.Minimum = 1;
        _media.ThousandsSeparator = true;

        _nome.Text = Cliente.Nome;
        _conta.Text = Cliente.NumeroConta;
        if (_cidade.Items.Contains(Cliente.CidadeHabitual))
            _cidade.SelectedItem = Cliente.CidadeHabitual;
        else
            _cidade.SelectedIndex = 0;
        _media.Value = Math.Clamp(Cliente.MediaValor, _media.Minimum, _media.Maximum);

        _erro.AutoSize = true;
        _erro.ForeColor = Tema.Vermelho;
        _erro.Location = new Point(26, 398);
        cartao.Controls.Add(_erro);

        var nota = new Label
        {
            Text = cliente?.Simulado == true
                ? "Este cliente foi criado pelo gerador de dados de demonstração."
                : "O número de conta deve ser único. Os registos manuais não são eliminados ao limpar dados de demonstração.",
            AutoSize = true,
            MaximumSize = new Size(560, 0),
            ForeColor = Tema.TextoSuave,
            Location = new Point(26, 425)
        };
        cartao.Controls.Add(nota);

        var cancelar = Tema.CriarBotaoSecundario("Cancelar");
        cancelar.Location = new Point(310, 478);
        cancelar.Width = 125;
        cancelar.DialogResult = DialogResult.Cancel;
        cartao.Controls.Add(cancelar);

        var guardar = Tema.CriarBotao("Guardar cliente", Tema.Verde);
        guardar.Location = new Point(445, 478);
        guardar.Width = 155;
        guardar.Click += GuardarCliente;
        cartao.Controls.Add(guardar);
        CancelButton = cancelar;
        AcceptButton = guardar;
    }

    private static void AdicionarCampo(Control pai, string legenda, Control campo, int topo)
    {
        pai.Controls.Add(new Label
        {
            Text = legenda,
            AutoSize = true,
            Location = new Point(26, topo),
            ForeColor = Tema.Texto,
            Font = new Font("Segoe UI Semibold", 9)
        });
        campo.Location = new Point(250, topo - 6);
        campo.Width = 345;
        campo.Height = 34;
        Tema.EstilizarCampo(campo);
        pai.Controls.Add(campo);
    }

    private void GuardarCliente(object? sender, EventArgs e)
    {
        string nome = _nome.Text.Trim();
        string conta = _conta.Text.Trim();

        if (nome.Length < 3)
        {
            _erro.Text = "Introduza o nome completo do cliente.";
            _nome.Focus();
            return;
        }
        if (conta.Length < 4)
        {
            _erro.Text = "O número de conta deve ter pelo menos quatro caracteres.";
            _conta.Focus();
            return;
        }
        if (_cidade.SelectedItem is null)
        {
            _erro.Text = "Seleccione a cidade habitual.";
            _cidade.Focus();
            return;
        }

        Cliente.Nome = nome;
        Cliente.NumeroConta = conta;
        Cliente.CidadeHabitual = _cidade.SelectedItem.ToString()!;
        Cliente.MediaValor = _media.Value;

        DialogResult = DialogResult.OK;
        Close();
    }
}