using System.Drawing.Drawing2D;

namespace Sentinela.Controlos;

public static class Tema
{
    public static readonly Color AzulEscuro = Color.FromArgb(20, 31, 51);
    public static readonly Color AzulPainel = Color.FromArgb(29, 44, 68);
    public static readonly Color Azul = Color.FromArgb(50, 105, 210);
    public static readonly Color AzulClaro = Color.FromArgb(232, 240, 255);
    public static readonly Color Fundo = Color.FromArgb(245, 247, 251);
    public static readonly Color Branco = Color.White;
    public static readonly Color Borda = Color.FromArgb(229, 234, 242);
    public static readonly Color Texto = Color.FromArgb(34, 45, 64);
    public static readonly Color TextoSuave = Color.FromArgb(111, 123, 143);
    public static readonly Color Verde = Color.FromArgb(29, 145, 105);
    public static readonly Color VerdeClaro = Color.FromArgb(231, 247, 240);
    public static readonly Color Ambar = Color.FromArgb(199, 132, 31);
    public static readonly Color AmbarClaro = Color.FromArgb(255, 247, 230);
    public static readonly Color Vermelho = Color.FromArgb(190, 65, 73);
    public static readonly Color VermelhoClaro = Color.FromArgb(255, 237, 239);
    public static readonly Color Roxo = Color.FromArgb(120, 92, 190);

    public static void PrepararFormulario(Form formulario)
    {
        formulario.BackColor = Fundo;
        formulario.Font = new Font("Segoe UI", 9.5F);
        formulario.StartPosition = FormStartPosition.CenterScreen;
        formulario.AutoScaleMode = AutoScaleMode.Dpi;
    }

    public static Button CriarBotao(string texto, Color? cor = null)
    {
        var botao = new Button
        {
            Text = texto,
            BackColor = cor ?? Azul,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI Semibold", 9.2F),
            Height = 40,
            Width = 160,
            Cursor = Cursors.Hand,
            Margin = new Padding(4),
            Padding = new Padding(12, 0, 12, 0),
            UseVisualStyleBackColor = false
        };
        botao.FlatAppearance.BorderSize = 0;
        botao.FlatAppearance.MouseOverBackColor = ControlPaint.Light(botao.BackColor, 0.12f);
        botao.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(botao.BackColor, 0.1f);
        return botao;
    }

    public static Button CriarBotaoSecundario(string texto)
    {
        var botao = CriarBotao(texto, Color.White);
        botao.ForeColor = Texto;
        botao.FlatAppearance.BorderSize = 1;
        botao.FlatAppearance.BorderColor = Borda;
        botao.FlatAppearance.MouseOverBackColor = AzulClaro;
        return botao;
    }

    public static Label CriarTitulo(string texto, float tamanho = 20)
        => new()
        {
            Text = texto,
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", tamanho),
            ForeColor = Texto,
            Margin = new Padding(0, 0, 0, 10)
        };

    public static Panel CriarCartao()
        => new()
        {
            BackColor = Branco,
            Padding = new Padding(18),
            Margin = new Padding(6),
            BorderStyle = BorderStyle.FixedSingle
        };

    public static void EstilizarGrelha(DataGridView grelha)
    {
        grelha.ReadOnly = true;
        grelha.AllowUserToAddRows = false;
        grelha.AllowUserToDeleteRows = false;
        grelha.AllowUserToResizeRows = false;
        grelha.MultiSelect = false;
        grelha.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grelha.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grelha.BackgroundColor = Branco;
        grelha.BorderStyle = BorderStyle.None;
        grelha.RowHeadersVisible = false;
        grelha.EnableHeadersVisualStyles = false;
        grelha.GridColor = Borda;
        grelha.ColumnHeadersHeight = 42;
        grelha.RowTemplate.Height = 38;
        grelha.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(242, 245, 250);
        grelha.ColumnHeadersDefaultCellStyle.ForeColor = Texto;
        grelha.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9.2f);
        grelha.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(242, 245, 250);
        grelha.DefaultCellStyle.BackColor = Branco;
        grelha.DefaultCellStyle.ForeColor = Texto;
        grelha.DefaultCellStyle.Font = new Font("Segoe UI", 9.2f);
        grelha.DefaultCellStyle.SelectionBackColor = AzulClaro;
        grelha.DefaultCellStyle.SelectionForeColor = Texto;
        grelha.DefaultCellStyle.Padding = new Padding(6, 2, 6, 2);
        grelha.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 253);
    }

    public static void EstilizarCampo(Control campo)
    {
        campo.Font = new Font("Segoe UI", 9.5f);
        campo.ForeColor = Texto;
        campo.BackColor = Branco;
        if (campo is TextBox caixa)
        {
            caixa.BorderStyle = BorderStyle.FixedSingle;
            caixa.Height = 32;
        }
        else if (campo is ComboBox lista)
        {
            lista.FlatStyle = FlatStyle.Flat;
            lista.IntegralHeight = false;
        }
    }
}
