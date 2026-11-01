using Sentinela.Controlos;
using Sentinela.Modelos;

namespace Sentinela.Ecras;

public sealed class FormularioCliente : Form
{
    private readonly TextBox _nome = new();
    private readonly TextBox _conta = new();
    private readonly ComboBox _cidade = new();
    private readonly NumericUpDown _media = new();

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
        ClientSize = new Size(490, 390);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        var titulo = Tema.CriarTitulo(Text, 20);
        titulo.Location = new Point(25, 22);
        Controls.Add(titulo);

        AdicionarCampo("Nome completo", _nome, 85);
        AdicionarCampo("Número de conta", _conta, 145);
        AdicionarCampo("Cidade habitual", _cidade, 205);
        AdicionarCampo("Média por operação (MZN)", _media, 265);

        _cidade.DropDownStyle = ComboBoxStyle.DropDownList;
        _cidade.Items.AddRange(new object[]
        {
            "Maputo", "Matola", "Beira", "Nampula", "Quelimane", "Tete",
            "Xai-Xai", "Inhambane", "Pemba", "Chimoio", "Lichinga"
        });
        _media.DecimalPlaces = 2;
        _media.Maximum = 999999999;
        _media.Minimum = 1;

        _nome.Text = Cliente.Nome;
        _conta.Text = Cliente.NumeroConta;
        if (_cidade.Items.Contains(Cliente.CidadeHabitual))
            _cidade.SelectedItem = Cliente.CidadeHabitual;
        else
            _cidade.SelectedIndex = 0;
        _media.Value = Math.Clamp(Cliente.MediaValor, _media.Minimum, _media.Maximum);

        var guardar = Tema.CriarBotao("Guardar", Tema.Verde);
        guardar.Location = new Point(300, 325);
        guardar.Click += GuardarCliente;
        Controls.Add(guardar);

        var cancelar = Tema.CriarBotao("Cancelar", Color.Gray);
        cancelar.Location = new Point(130, 325);
        cancelar.DialogResult = DialogResult.Cancel;
        Controls.Add(cancelar);
        CancelButton = cancelar;
    }

    private void AdicionarCampo(string legenda, Control campo, int topo)
    {
        Controls.Add(new Label
        {
            Text = legenda,
            AutoSize = true,
            Location = new Point(28, topo),
            ForeColor = Tema.Texto
        });
        campo.Location = new Point(220, topo - 5);
        campo.Width = 235;
        campo.Height = 30;
        Controls.Add(campo);
    }

    private void GuardarCliente(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_nome.Text) ||
            string.IsNullOrWhiteSpace(_conta.Text) || _cidade.SelectedItem is null)
        {
            MessageBox.Show("Preencha todos os campos.", "Validação",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Cliente.Nome = _nome.Text.Trim();
        Cliente.NumeroConta = _conta.Text.Trim();
        Cliente.CidadeHabitual = _cidade.SelectedItem.ToString()!;
        Cliente.MediaValor = _media.Value;

        DialogResult = DialogResult.OK;
        Close();
    }
}
