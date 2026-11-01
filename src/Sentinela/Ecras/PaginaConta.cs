using Sentinela.Controlos;
using Sentinela.Dados;
using Sentinela.Modelos;
using Sentinela.Util;

namespace Sentinela.Ecras;

public sealed class PaginaConta : UserControl
{
    private readonly Utilizador _utilizador;
    private readonly UtilizadorDAO _dao;
    private readonly TextBox _actual = new();
    private readonly TextBox _nova = new();
    private readonly TextBox _confirmar = new();
    private readonly CheckBox _mostrar = new();
    private readonly Button _guardar = Tema.CriarBotao("Guardar nova palavra-passe");
    private readonly Label _estado = new();

    public PaginaConta(ConfiguracaoApp configuracao, Utilizador utilizador)
    {
        _utilizador = utilizador;
        _dao = new UtilizadorDAO(configuracao);
        BackColor = Tema.Fundo;
        ConstruirInterface();
    }

    private void ConstruirInterface()
    {
        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Tema.Fundo };
        var pagina = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.Transparent,
            Padding = new Padding(8)
        };
        pagina.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48));
        pagina.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52));

        var perfil = Tema.CriarCartao();
        perfil.Height = 260;
        perfil.Dock = DockStyle.Top;
        perfil.Margin = new Padding(6);
        perfil.Controls.Add(Tema.CriarTitulo("A sua conta", 18));
        perfil.Controls[0].Location = new Point(18, 18);

        var avatar = new Label
        {
            Text = ObterIniciais(_utilizador.NomeCompleto),
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI Semibold", 22),
            ForeColor = Color.White,
            BackColor = Tema.Azul,
            Size = new Size(78, 78),
            Location = new Point(20, 68)
        };
        perfil.Controls.Add(avatar);
        perfil.Controls.Add(new Label
        {
            Text = _utilizador.NomeCompleto,
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 13),
            ForeColor = Tema.Texto,
            Location = new Point(116, 79)
        });
        perfil.Controls.Add(new Label
        {
            Text = "@" + _utilizador.NomeUtilizador + "  •  " + _utilizador.Perfil,
            AutoSize = true,
            Font = new Font("Segoe UI", 9),
            ForeColor = Tema.TextoSuave,
            Location = new Point(118, 108)
        });

        var nota = new Label
        {
            Text = "Mantenha os dados de acesso pessoais e não partilhe a sua palavra-passe. A palavra-passe é guardada como hash protegido, não em texto simples.",
            AutoSize = true,
            MaximumSize = new Size(360, 0),
            ForeColor = Tema.TextoSuave,
            Location = new Point(20, 176)
        };
        perfil.Controls.Add(nota);

        var seguranca = Tema.CriarCartao();
        seguranca.Height = 530;
        seguranca.Dock = DockStyle.Top;
        seguranca.Margin = new Padding(6);
        seguranca.Controls.Add(Tema.CriarTitulo("Alterar palavra-passe", 18));
        seguranca.Controls[0].Location = new Point(18, 18);

        AdicionarCampo(seguranca, "Palavra-passe actual", _actual, 76);
        AdicionarCampo(seguranca, "Nova palavra-passe", _nova, 142);
        AdicionarCampo(seguranca, "Confirmar nova palavra-passe", _confirmar, 208);
        foreach (var campo in new[] { _actual, _nova, _confirmar })
            campo.UseSystemPasswordChar = true;

        _mostrar.Text = "Mostrar palavras-passe";
        _mostrar.AutoSize = true;
        _mostrar.Location = new Point(20, 270);
        _mostrar.CheckedChanged += (_, _) =>
        {
            bool ocultar = !_mostrar.Checked;
            _actual.UseSystemPasswordChar = ocultar;
            _nova.UseSystemPasswordChar = ocultar;
            _confirmar.UseSystemPasswordChar = ocultar;
        };
        seguranca.Controls.Add(_mostrar);

        seguranca.Controls.Add(new Label
        {
            Text = "Requisitos: pelo menos 10 caracteres, uma letra maiúscula, uma minúscula e um número.",
            AutoSize = true,
            MaximumSize = new Size(440, 0),
            ForeColor = Tema.TextoSuave,
            Location = new Point(20, 305)
        });

        _estado.AutoSize = true;
        _estado.MaximumSize = new Size(440, 0);
        _estado.Location = new Point(20, 364);
        _estado.ForeColor = Tema.TextoSuave;
        seguranca.Controls.Add(_estado);

        _guardar.Width = 230;
        _guardar.Location = new Point(20, 410);
        _guardar.Click += async (_, _) => await GuardarAsync();
        seguranca.Controls.Add(_guardar);

        pagina.Controls.Add(perfil, 0, 0);
        pagina.Controls.Add(seguranca, 1, 0);
        scroll.Controls.Add(pagina);
        Controls.Add(scroll);
    }

    private static void AdicionarCampo(Control pai, string legenda, TextBox campo, int topo)
    {
        pai.Controls.Add(new Label
        {
            Text = legenda,
            AutoSize = true,
            ForeColor = Tema.TextoSuave,
            Font = new Font("Segoe UI Semibold", 8.5f),
            Location = new Point(20, topo)
        });
        campo.Location = new Point(20, topo + 24);
        campo.Size = new Size(440, 34);
        Tema.EstilizarCampo(campo);
        pai.Controls.Add(campo);
    }

    private async Task GuardarAsync()
    {
        if (_nova.Text != _confirmar.Text)
        {
            _estado.Text = "A confirmação não coincide com a nova palavra-passe.";
            _estado.ForeColor = Tema.Vermelho;
            return;
        }

        _guardar.Enabled = false;
        _estado.Text = "A actualizar a palavra-passe...";
        _estado.ForeColor = Tema.TextoSuave;
        try
        {
            await Task.Run(() => _dao.AlterarPalavraPasse(
                _utilizador.Id, _actual.Text, _nova.Text));

            _actual.Clear();
            _nova.Clear();
            _confirmar.Clear();
            _estado.Text = "Palavra-passe alterada com sucesso.";
            _estado.ForeColor = Tema.Verde;
        }
        catch (Exception ex)
        {
            _estado.Text = ex.Message;
            _estado.ForeColor = Tema.Vermelho;
        }
        finally
        {
            _guardar.Enabled = true;
        }
    }

    private static string ObterIniciais(string nome)
    {
        var partes = nome.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (partes.Length == 0) return "S";
        return partes.Length == 1 ? partes[0][..1].ToUpperInvariant()
            : (partes[0][..1] + partes[^1][..1]).ToUpperInvariant();
    }
}