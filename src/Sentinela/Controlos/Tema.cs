using System.Drawing.Drawing2D;

namespace Sentinela.Controlos;

public static class Tema
{
    public static readonly Color AzulEscuro = Color.FromArgb(23, 34, 56);
    public static readonly Color Azul = Color.FromArgb(43, 102, 178);
    public static readonly Color Fundo = Color.FromArgb(244, 246, 250);
    public static readonly Color Texto = Color.FromArgb(42, 52, 68);
    public static readonly Color TextoSuave = Color.FromArgb(111, 122, 139);
    public static readonly Color Verde = Color.FromArgb(36, 145, 105);
    public static readonly Color Ambar = Color.FromArgb(203, 142, 37);
    public static readonly Color Vermelho = Color.FromArgb(190, 65, 73);

    public static void PrepararFormulario(Form formulario)
    {
        formulario.BackColor = Fundo;
        formulario.Font = new Font("Segoe UI", 9.5F);
        formulario.StartPosition = FormStartPosition.CenterScreen;
    }

    public static Button CriarBotao(string texto, Color? cor = null)
    {
        var botao = new Button
        {
            Text = texto,
            BackColor = cor ?? Azul,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI Semibold", 9.5F),
            Height = 40,
            Width = 160,
            Cursor = Cursors.Hand,
            Margin = new Padding(4)
        };
        botao.FlatAppearance.BorderSize = 0;
        botao.FlatAppearance.MouseOverBackColor = ControlPaint.Light(botao.BackColor, 0.12f);
        botao.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(botao.BackColor, 0.1f);
        return botao;
    }

    public static Label CriarTitulo(string texto, float tamanho = 20)
        => new()
        {
            Text = texto,
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", tamanho),
            ForeColor = Texto,
            Margin = new Padding(0, 0, 0, 12)
        };

    public static Panel CriarCartao()
        => new()
        {
            BackColor = Color.White,
            Padding = new Padding(18),
            Margin = new Padding(8),
            BorderStyle = BorderStyle.FixedSingle
        };
}
