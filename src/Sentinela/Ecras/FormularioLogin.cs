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

    public FormularioLogin(ConfiguracaoApp configuracao, ServicoIA servicoIA)
    {
        _configuracao = configuracao;
        _servicoIA = servicoIA;

        Tema.PrepararFormulario(this);
        Text = "Sentinela — Iniciar sessão";
        ClientSize = new Size(850, 520);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;

        var painelMarca = new Panel { Dock = DockStyle.Left, Width = 340, BackColor = Tema.AzulEscuro };
        painelMarca.Paint += (_, e) =>
        {
            using var pincel = new LinearGradientBrush(painelMarca.ClientRectangle,
                Tema.AzulEscuro, Color.FromArgb(45, 82, 127), 35f);
            e.Graphics.FillRectangle(pincel, painelMarca.ClientRectangle);
        };
        painelMarca.Controls.Add(new Label
        {
            Text = "SENTINELA",
            Font = new Font("Segoe UI Semibold", 25),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(42, 150)
        });
        painelMarca.Controls.Add(new Label
        {
            Text = "Inteligência Artificial\npara análise de transacções",
            Font = new Font("Segoe UI", 12),
            ForeColor = Color.FromArgb(220, 230, 242),
            AutoSize = true,
            Location = new Point(45, 205)
        });
        Controls.Add(painelMarca);

        var titulo = Tema.CriarTitulo("Iniciar sessão", 22);
        titulo.Location = new Point(415, 92);
        Controls.Add(titulo);

        AdicionarCampo("Utilizador", _nome, 170);
        AdicionarCampo("Palavra-passe", _palavraPasse, 240);
        _palavraPasse.UseSystemPasswordChar = true;

        _erro.AutoSize = true;
        _erro.ForeColor = Tema.Vermelho;
        _erro.Location = new Point(420, 300);
        Controls.Add(_erro);

        var entrar = Tema.CriarBotao("Entrar");
        entrar.Location = new Point(420, 340);
        entrar.Width = 330;
        entrar.Click += IniciarSessao;
        Controls.Add(entrar);
        AcceptButton = entrar;

        var rodape = new Label
        {
            Text = "Apoio à análise. O resultado não confirma a existência de fraude.",
            AutoSize = true,
            ForeColor = Tema.TextoSuave,
            Location = new Point(400, 470)
        };
        Controls.Add(rodape);
    }

    private void AdicionarCampo(string legenda, TextBox campo, int topo)
    {
        Controls.Add(new Label
        {
            Text = legenda,
            Location = new Point(420, topo - 25),
            AutoSize = true,
            ForeColor = Tema.Texto
        });
        campo.Location = new Point(420, topo);
        campo.Width = 330;
        campo.Height = 34;
        Controls.Add(campo);
    }

    private void IniciarSessao(object? sender, EventArgs e)
    {
        try
        {
            var utilizador = new UtilizadorDAO(_configuracao).Autenticar(
                _nome.Text, _palavraPasse.Text);

            if (utilizador is null)
            {
                _erro.Text = "O utilizador ou a palavra-passe não estão correctos.";
                return;
            }

            Hide();
            using (var principal = new FormularioPrincipal(_configuracao, utilizador, _servicoIA))
                principal.ShowDialog();
            Close();
        }
        catch (Exception ex)
        {
            _erro.Text = "Não foi possível iniciar sessão: " + ex.Message;
        }
    }
}