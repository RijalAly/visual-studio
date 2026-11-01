using System.Drawing.Drawing2D;

namespace Sentinela.Controlos;

public sealed class CartaoMetrica : UserControl
{
    private readonly Label _icone = new();
    private readonly Label _titulo = new();
    private readonly Label _valor = new();
    private readonly Label _descricao = new();
    private Color _cor = Tema.Azul;

    public CartaoMetrica(string icone, string titulo, string valor, string descricao, Color cor)
    {
        _cor = cor;
        DoubleBuffered = true;
        BackColor = Color.Transparent;
        MinimumSize = new Size(170, 112);
        Size = new Size(240, 124);
        Padding = new Padding(16);

        _icone.Text = icone;
        _icone.Font = new Font("Segoe UI Emoji", 18);
        _icone.ForeColor = cor;
        _icone.AutoSize = true;
        _icone.Location = new Point(16, 13);

        _titulo.Text = titulo;
        _titulo.Font = new Font("Segoe UI", 9.2f);
        _titulo.ForeColor = Tema.TextoSuave;
        _titulo.AutoSize = true;
        _titulo.Location = new Point(54, 19);

        _valor.Text = valor;
        _valor.Font = new Font("Segoe UI Semibold", 23);
        _valor.ForeColor = Tema.Texto;
        _valor.AutoSize = true;
        _valor.Location = new Point(16, 48);

        _descricao.Text = descricao;
        _descricao.Font = new Font("Segoe UI", 8.5f);
        _descricao.ForeColor = Tema.TextoSuave;
        _descricao.AutoSize = true;
        _descricao.Location = new Point(18, 91);

        Controls.AddRange(new Control[] { _icone, _titulo, _valor, _descricao });
        SizeChanged += (_, _) => ActualizarRegiao();
        ActualizarRegiao();
    }

    public void Actualizar(string valor, string? descricao = null)
    {
        _valor.Text = valor;
        if (descricao is not null)
            _descricao.Text = descricao;
        Invalidate();
    }

    private void ActualizarRegiao()
    {
        if (Width <= 0 || Height <= 0) return;
        using var caminho = CriarCaminho(new Rectangle(0, 0, Width - 1, Height - 1), 16);
        Region?.Dispose();
        Region = new Region(caminho);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var caminho = CriarCaminho(new Rectangle(0, 0, Width - 1, Height - 1), 16);
        using var fundo = new SolidBrush(Color.White);
        using var contorno = new Pen(Tema.Borda, 1);
        using var faixa = new SolidBrush(_cor);
        e.Graphics.FillPath(fundo, caminho);
        e.Graphics.DrawPath(contorno, caminho);
        e.Graphics.FillRoundedRectangle(faixa, new Rectangle(0, 15, 4, Height - 30), 2);
    }

    private static GraphicsPath CriarCaminho(Rectangle rectangulo, int raio)
    {
        int diametro = raio * 2;
        var caminho = new GraphicsPath();
        caminho.AddArc(rectangulo.X, rectangulo.Y, diametro, diametro, 180, 90);
        caminho.AddArc(rectangulo.Right - diametro, rectangulo.Y, diametro, diametro, 270, 90);
        caminho.AddArc(rectangulo.Right - diametro, rectangulo.Bottom - diametro, diametro, diametro, 0, 90);
        caminho.AddArc(rectangulo.X, rectangulo.Bottom - diametro, diametro, diametro, 90, 90);
        caminho.CloseFigure();
        return caminho;
    }
}

internal static class DesenhoExtensoes
{
    public static void FillRoundedRectangle(this Graphics graphics, Brush brush, Rectangle bounds, int radius)
    {
        int diameter = radius * 2;
        using var path = new GraphicsPath();
        path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        graphics.FillPath(brush, path);
    }
}
