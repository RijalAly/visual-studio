using Sentinela.Controlos;
using Sentinela.Modelos;

namespace Sentinela.Ecras;

public sealed class FormularioRevisao : Form
{
    private readonly ComboBox _estado = new();
    private readonly TextBox _observacao = new();
    private readonly Label _contador = new();

    public string EstadoSeleccionado => _estado.SelectedItem?.ToString() ?? "Pendente";
    public string Observacao => _observacao.Text.Trim();

    public FormularioRevisao(Transaccao transaccao)
    {
        Tema.PrepararFormulario(this);
        Text = "Rever operação";
        ClientSize = new Size(600, 500);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        var titulo = Tema.CriarTitulo("Revisão do alerta", 20);
        titulo.Location = new Point(26, 22);
        Controls.Add(titulo);

        var contexto = new Label
        {
            Text = $"{transaccao.NomeCliente}  •  {transaccao.DataHora:dd/MM/yyyy HH:mm}  •  {transaccao.Valor:N2} MZN",
            AutoSize = true,
            ForeColor = Tema.TextoSuave,
            Location = new Point(28, 58)
        };
        Controls.Add(contexto);

        Controls.Add(new Label
        {
            Text = "Estado da revisão",
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 9.5f),
            ForeColor = Tema.Texto,
            Location = new Point(28, 112)
        });

        _estado.DropDownStyle = ComboBoxStyle.DropDownList;
        _estado.Items.AddRange(new object[] { "Pendente", "Confirmada", "Descartada" });
        _estado.SelectedItem = transaccao.Revisao;
        if (_estado.SelectedIndex < 0) _estado.SelectedIndex = 0;
        _estado.Location = new Point(28, 138);
        _estado.Size = new Size(250, 34);
        Tema.Est ilizarCampo(_estado);
        Controls.Add(_estado);

        Controls.Add(new Label
        {
            Text = "Justificação / observação",
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 9.5f),
            ForeColor = Tema.Texto,
            Location = new Point(28, 194)
        });

        _observacao.Location = new Point(28, 220);
        _observacao.Size = new Size(540, 145);
        _observacao.Multiline = true;
        _observacao.ScrollBars = ScrollBars.Vertical;
        _observacao.MaxLength = 500;
        _observacao.PlaceholderText = "Registe o motivo da decisão, os elementos observados ou a razão para manter pendente...";
        Tema.Est ilizarCampo(_observacao);
        _observacao.TextChanged += (_, _) => _contador.Text = $"{_observacao.TextLength}/500";
        Controls.Add(_observacao);

        _contador.Text = "0/500";
        _contador.AutoSize = true;
        _contador.ForeColor = Tema.TextoSuave;
        _contador.Location = new Point(510, 370);
        Controls.Add(_contador);

        var nota = new Label
        {
            Text = "A confirmação ou o descarte exigem uma justificação. Esta informação fica no histórico de auditoria.",
            AutoSize = true,
            MaximumSize = new Size(540, 0),
            ForeColor = Tema.TextoSuave,
            Location = new Point(28, 398)
        };
        Controls.Add(nota);

        var cancelar = Tema.CriarBotaoSecundario("Cancelar");
        cancelar.Location = new Point(330, 445);
        cancelar.DialogResult = DialogResult.Cancel;
        Controls.Add(cancelar);

        var guardar = Tema.CriarBotao("Guardar revisão");
        guardar.Location = new Point(435, 445);
        guardar.Width = 135;
        guardar.Click += (_, _) =>
        {
            if (EstadoSeleccionado != "Pendente" && Observacao.Length < 8)
            {
                MessageBox.Show("Escreva uma justificação com pelo menos oito caracteres.",
                    "Justificação necessária", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _observacao.Focus();
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        };
        Controls.Add(guardar);
        CancelButton = cancelar;
    }
}