using Sentinela.Controlos;
using Sentinela.Util;

namespace Sentinela.Ecras;

public sealed class FormularioLigacao : Form
{
    private readonly TextBox _servidor = new();
    private readonly NumericUpDown _porta = new();
    private readonly TextBox _utilizador = new();
    private readonly TextBox _palavraPasse = new();
    private readonly TextBox _baseDados = new();

    public ConfiguracaoApp Configuracao { get; private set; }

    public FormularioLigacao(ConfiguracaoApp actual)
    {
        Configuracao = actual;
        Tema.PrepararFormulario(this);
        Text = "Configuração da ligação ao MySQL";
        ClientSize = new Size(520, 520);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        var titulo = Tema.CriarTitulo("Ligação ao MySQL", 21);
        titulo.Location = new Point(28, 22);
        Controls.Add(titulo);

        AdicionarCampo("Servidor", _servidor, 78);
        AdicionarCampo("Porta", _porta, 145);
        AdicionarCampo("Utilizador", _utilizador, 212);
        AdicionarCampo("Palavra-passe", _palavraPasse, 279);
        AdicionarCampo("Base de dados", _baseDados, 346);

        _porta.Minimum = 1;
        _porta.Maximum = 65535;
        _servidor.Text = actual.Servidor;
        _porta.Value = actual.Porta;
        _utilizador.Text = actual.Utilizador;
        _palavraPasse.Text = actual.PalavraPasse;
        _palavraPasse.UseSystemPasswordChar = true;
        _baseDados.Text = actual.BaseDados;

        var nota = new Label
        {
            Text = "Pode usar o MySQL Server ou o MariaDB incluído no XAMPP.\nConfirme que o serviço MySQL está iniciado.",
            AutoSize = true,
            ForeColor = Tema.TextoSuave,
            Location = new Point(28, 408)
        };
        Controls.Add(nota);

        var testar = Tema.CriarBotao("Testar e guardar", Tema.Verde);
        testar.Location = new Point(310, 455);
        testar.Click += TestarLigacao;
        Controls.Add(testar);

        var cancelar = Tema.CriarBotao("Cancelar", Color.Gray);
        cancelar.Location = new Point(140, 455);
        cancelar.DialogResult = DialogResult.Cancel;
        Controls.Add(cancelar);
        CancelButton = cancelar;
    }

    private void AdicionarCampo(string legenda, Control controlo, int topo)
    {
        Controls.Add(new Label
        {
            Text = legenda,
            Location = new Point(30, topo),
            AutoSize = true,
            ForeColor = Tema.Texto
        });
        controlo.Location = new Point(180, topo - 5);
        controlo.Width = 300;
        controlo.Height = 30;
        Controls.Add(controlo);
    }

    private void TestarLigacao(object? sender, EventArgs e)
    {
        Configuracao = new ConfiguracaoApp
        {
            Servidor = _servidor.Text.Trim(),
            Porta = (uint)_porta.Value,
            Utilizador = _utilizador.Text.Trim(),
            PalavraPasse = _palavraPasse.Text,
            BaseDados = _baseDados.Text.Trim()
        };

        if (string.IsNullOrWhiteSpace(Configuracao.Servidor) ||
            string.IsNullOrWhiteSpace(Configuracao.Utilizador) ||
            string.IsNullOrWhiteSpace(Configuracao.BaseDados))
        {
            MessageBox.Show("Preencha o servidor, o utilizador e o nome da base de dados.",
                "Dados em falta", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!Dados.ConexaoBD.Testar(Configuracao, criarBase: true))
        {
            MessageBox.Show("Não foi possível ligar ao MySQL. Confirme o endereço, a porta, as credenciais e se o serviço está iniciado.",
                "Falha na ligação", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }
}
