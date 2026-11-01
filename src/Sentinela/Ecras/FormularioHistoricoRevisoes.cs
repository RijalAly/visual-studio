using Sentinela.Controlos;
using Sentinela.Modelos;

namespace Sentinela.Ecras;

public sealed class FormularioHistoricoRevisoes : Form
{
    private readonly DataGridView _grelha = new();

    public FormularioHistoricoRevisoes(string cliente, long transaccaoId, IReadOnlyCollection<RevisaoHistorico> revisoes)
    {
        Tema.PrepararFormulario(this);
        Text = "Histórico de auditoria";
        ClientSize = new Size(940, 540);
        MinimumSize = new Size(760, 430);

        var titulo = Tema.CriarTitulo("Histórico de decisões", 20);
        titulo.Location = new Point(22, 18);
        Controls.Add(titulo);

        var subtitulo = new Label
        {
            Text = $"Operação #{transaccaoId}  •  {cliente}  •  {revisoes.Count} registo(s)",
            AutoSize = true,
            ForeColor = Tema.TextoSuave,
            Location = new Point(24, 52)
        };
        Controls.Add(subtitulo);

        Tema.EstilizarGrelha(_grelha);
        _grelha.Location = new Point(20, 88);
        _grelha.Size = new Size(895, 380);
        _grelha.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        _grelha.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        _grelha.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        _grelha.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        _grelha.DataSource = revisoes.Select(r => new
        {
            Data = r.DataHora.ToString("dd/MM/yyyy HH:mm"),
            Analista = r.NomeAnalista,
            EstadoAnterior = r.EstadoAnterior,
            EstadoNovo = r.EstadoNovo,
            Justificação = r.Observacao
        }).ToList();

        if (_grelha.Columns.Count > 0)
        {
            _grelha.Columns[0].Width = 125;
            _grelha.Columns[1].Width = 155;
            _grelha.Columns[2].Width = 110;
            _grelha.Columns[3].Width = 110;
            _grelha.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }
        Controls.Add(_grelha);

        var fechar = Tema.CriarBotaoSecundario("Fechar");
        fechar.Location = new Point(790, 482);
        fechar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        fechar.Click += (_, _) => Close();
        Controls.Add(fechar);
    }
}