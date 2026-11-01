using System.Text;
using Sentinela.Controlos;
using Sentinela.Dados;
using Sentinela.Modelos;
using Sentinela.Util;

namespace Sentinela.Ecras;

public sealed class PaginaTransaccoes : UserControl
{
    private readonly TransaccaoDAO _dao;
    private readonly DataGridView _grelha = new();
    private readonly ComboBox _filtroClassificacao = new();
    private readonly ComboBox _filtroRevisao = new();
    private readonly TextBox _pesquisa = new();
    private List<Transaccao> _dados = new();

    public PaginaTransaccoes(ConfiguracaoApp configuracao)
    {
        _dao = new TransaccaoDAO(configuracao);
        BackColor = Tema.Fundo;
        CriarInterface();
        ActualizarLista();
    }

    private void CriarInterface()
    {
        var filtros = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 48,
            BackColor = Color.Transparent,
            WrapContents = false
        };

        _filtroClassificacao.DropDownStyle = ComboBoxStyle.DropDownList;
        _filtroClassificacao.Width = 205;
        _filtroClassificacao.Items.AddRange(new object[]
        {
            "Todas as classificações", "Normal", "Atenção", "Transacção potencialmente suspeita"
        });
        _filtroClassificacao.SelectedIndex = 0;
        _filtroClassificacao.SelectedIndexChanged += (_, _) => AplicarFiltros();

        _filtroRevisao.DropDownStyle = ComboBoxStyle.DropDownList;
        _filtroRevisao.Width = 145;
        _filtroRevisao.Items.AddRange(new object[] { "Todas as revisões", "Pendente", "Confirmada", "Descartada" });
        _filtroRevisao.SelectedIndex = 0;
        _filtroRevisao.SelectedIndexChanged += (_, _) => AplicarFiltros();

        _pesquisa.Width = 190;
        _pesquisa.PlaceholderText = "Pesquisar cliente ou conta";
        _pesquisa.TextChanged += (_, _) => AplicarFiltros();

        var actualizar = Tema.CriarBotao("Actualizar", Color.FromArgb(90, 105, 125));
        actualizar.Click += (_, _) => ActualizarLista();

        filtros.Controls.AddRange(new Control[] { _filtroClassificacao, _filtroRevisao, _pesquisa, actualizar });

        ConfigurarGrelha();

        var accoes = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 56,
            BackColor = Color.Transparent,
            WrapContents = false
        };

        var confirmar = Tema.CriarBotao("Confirmar suspeita", Tema.Verde);
        confirmar.Click += (_, _) => Rever("Confirmada");
        var descartar = Tema.CriarBotao("Falso alerta", Tema.Vermelho);
        descartar.Click += (_, _) => Rever("Descartada");
        var pendente = Tema.CriarBotao("Repor pendente", Color.FromArgb(120, 130, 145));
        pendente.Click += (_, _) => Rever("Pendente");
        var exportar = Tema.CriarBotao("Exportar CSV");
        exportar.Click += (_, _) => Exportar();

        accoes.Controls.AddRange(new Control[] { confirmar, descartar, pendente, exportar });
        Controls.Add(_grelha);
        Controls.Add(accoes);
        Controls.Add(filtros);
    }

    private void ConfigurarGrelha()
    {
        _grelha.Dock = DockStyle.Fill;
        _grelha.ReadOnly = true;
        _grelha.AllowUserToAddRows = false;
        _grelha.AllowUserToDeleteRows = false;
        _grelha.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grelha.MultiSelect = false;
        _grelha.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grelha.BackgroundColor = Color.White;
        _grelha.BorderStyle = BorderStyle.None;
        _grelha.RowHeadersVisible = false;
        _grelha.EnableHeadersVisualStyles = false;
        _grelha.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(235, 239, 245);
        _grelha.ColumnHeadersDefaultCellStyle.ForeColor = Tema.Texto;
        _grelha.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9.5f);
        _grelha.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 234, 250);
        _grelha.DefaultCellStyle.SelectionForeColor = Tema.Texto;
        _grelha.CellFormatting += FormatarCelula;
        _grelha.CellDoubleClick += (_, _) => MostrarDetalhe();
    }

    private void ActualizarLista()
    {
        try
        {
            _dados = _dao.Listar(10000);
            AplicarFiltros();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Não foi possível carregar as transacções.\n" + ex.Message,
                "Transacções", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void AplicarFiltros()
    {
        IEnumerable<Transaccao> consulta = _dados;
        if (_filtroClassificacao.SelectedIndex > 0)
            consulta = consulta.Where(t => t.Classificacao == _filtroClassificacao.SelectedItem!.ToString());
        if (_filtroRevisao.SelectedIndex > 0)
            consulta = consulta.Where(t => t.Revisao == _filtroRevisao.SelectedItem!.ToString());

        string termo = _pesquisa.Text.Trim();
        if (termo.Length > 0)
            consulta = consulta.Where(t =>
                t.NomeCliente.Contains(termo, StringComparison.OrdinalIgnoreCase) ||
                t.NumeroConta.Contains(termo, StringComparison.OrdinalIgnoreCase));

        _grelha.DataSource = consulta.Select(t => new LinhaTransaccao
        {
            Id = t.Id,
            DataHora = t.DataHora,
            Cliente = t.NomeCliente,
            Conta = t.NumeroConta,
            Valor = t.Valor,
            Tipo = t.Tipo,
            Cidade = t.Cidade,
            Classificacao = t.Classificacao,
            Pontuacao = t.Pontuacao.HasValue ? t.Pontuacao.Value.ToString("P1") : "Por calcular",
            Revisao = t.Revisao
        }).ToList();

        if (_grelha.Columns["Id"] is not null) _grelha.Columns["Id"].Visible = false;
        if (_grelha.Columns["Valor"] is not null) _grelha.Columns["Valor"].DefaultCellStyle.Format = "N2";
    }

    private void FormatarCelula(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (_grelha.Columns[e.ColumnIndex].Name != "Classificacao" || e.Value is null) return;

        string valor = e.Value.ToString() ?? "";
        e.CellStyle.ForeColor = valor.Contains("suspeita") ? Tema.Vermelho
            : valor == "Atenção" ? Tema.Ambar
            : valor == "Normal" ? Tema.Verde : Tema.Texto;
        e.CellStyle.Font = new Font("Segoe UI Semibold", 9f);
    }

    private LinhaTransaccao? Seleccionar()
    {
        if (_grelha.CurrentRow?.DataBoundItem is LinhaTransaccao linha)
            return linha;
        MessageBox.Show("Seleccione uma transacção.", "Transacções",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
        return null;
    }

    private void Rever(string estado)
    {
        var linha = Seleccionar();
        if (linha is null) return;

        try
        {
            _dao.GuardarRevisao(linha.Id, estado);
            ActualizarLista();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Não foi possível guardar a revisão.\n" + ex.Message,
                "Revisão", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void MostrarDetalhe()
    {
        var linha = Seleccionar();
        if (linha is null) return;

        var transaccao = _dados.FirstOrDefault(t => t.Id == linha.Id);
        if (transaccao is null) return;

        var factores = Inteligencia.ExplicadorFactores.Obter(transaccao);
        MessageBox.Show(
            $"Cliente: {transaccao.NomeCliente}\nConta: {transaccao.NumeroConta}\n" +
            $"Data: {transaccao.DataHora:dd/MM/yyyy HH:mm}\nValor: {transaccao.Valor:N2} MZN\n" +
            $"Classificação: {transaccao.Classificacao}\nProbabilidade: {(transaccao.Pontuacao ?? 0):P1}\n\n" +
            "Factores de atenção (explicação de leitura):\n- " + string.Join("\n- ", factores) +
            "\n\nO resultado é um apoio à decisão e não confirma fraude.",
            "Detalhe da transacção", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void Exportar()
    {
        using var dialogo = new SaveFileDialog
        {
            Filter = "Ficheiro CSV (*.csv)|*.csv",
            FileName = "transaccoes_sent inela.csv".Replace(" ", "")
        };
        if (dialogo.ShowDialog() != DialogResult.OK) return;

        try
        {
            var linhas = new List<string>
            {
                "Id;Data;Cliente;Conta;Valor_MZN;Tipo;Cidade;Classificacao;Probabilidade;Revisao"
            };

            foreach (var item in _dados)
            {
                linhas.Add(string.Join(";", new[]
                {
                    item.Id.ToString(),
                    item.DataHora.ToString("dd/MM/yyyy HH:mm"),
                    Escapar(item.NomeCliente),
                    Escapar(item.NumeroConta),
                    item.Valor.ToString("0.00", System.Globalization.CultureInfo.GetCultureInfo("pt-PT")),
                    Escapar(item.Tipo),
                    Escapar(item.Cidade),
                    Escapar(item.Classificacao),
                    item.Pontuacao?.ToString("P2", System.Globalization.CultureInfo.GetCultureInfo("pt-PT")) ?? "",
                    Escapar(item.Revisao)
                }));
            }

            File.WriteAllLines(dialogo.FileName, linhas, new UTF8Encoding(true));
            MessageBox.Show("O ficheiro CSV foi exportado.", "Exportação",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Não foi possível exportar o ficheiro.\n" + ex.Message,
                "Exportação", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static string Escapar(string valor)
        => "\"" + valor.Replace("\"", "\"\"") + "\"";

    private sealed class LinhaTransaccao
    {
        public int Id { get; set; }
        public DateTime DataHora { get; set; }
        public string Cliente { get; set; } = "";
        public string Conta { get; set; } = "";
        public decimal Valor { get; set; }
        public string Tipo { get; set; } = "";
        public string Cidade { get; set; } = "";
        public string Classificacao { get; set; } = "";
        public string Pontuacao { get; set; } = "";
        public string Revisao { get; set; } = "";
    }
}
