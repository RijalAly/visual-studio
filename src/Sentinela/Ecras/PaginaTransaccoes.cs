using System.Text;
using Sentinela.Controlos;
using Sentinela.Dados;
using Sentinela.Modelos;
using Sentinela.Util;

namespace Sentinela.Ecras;

public sealed class PaginaTransaccoes : UserControl
{
    private readonly TransaccaoDAO _dao;
    private readonly RevisaoDAO _revisaoDAO;
    private readonly Utilizador _utilizador;
    private readonly DataGridView _grelha = new();
    private readonly TextBox _pesquisa = new();
    private readonly ComboBox _filtroClassificacao = new();
    private readonly ComboBox _filtroRevisao = new();
    private readonly CheckBox _usarPeriodo = new();
    private readonly DateTimePicker _dataInicio = new();
    private readonly DateTimePicker _dataFim = new();
    private readonly CheckBox _usarValor = new();
    private readonly NumericUpDown _valorMinimo = new();
    private readonly NumericUpDown _valorMaximo = new();
    private readonly Label _contador = new();
    private readonly Label _estado = new();
    private readonly Button _rever = Tema.CriarBotao("✓  Rever operação", Tema.Verde);
    private readonly Button _historico = Tema.CriarBotaoSecundario("◷  Histórico");
    private readonly Button _detalhe = Tema.CriarBotaoSecundario("ⓘ  Detalhe");
    private readonly Button _exportar = Tema.CriarBotaoSecundario("⇩  Exportar filtrado");
    private readonly Button _actualizar = Tema.CriarBotaoSecundario("↻  Actualizar");
    private List<Transaccao> _dados = new();
    private List<Transaccao> _visiveis = new();

    public PaginaTransaccoes(ConfiguracaoApp configuracao, Utilizador utilizador)
    {
        _dao = new TransaccaoDAO(configuracao);
        _revisaoDAO = new RevisaoDAO(configuracao);
        _utilizador = utilizador;
        BackColor = Tema.Fundo;
        ConstruirInterface();
        Load += async (_, _) => await ActualizarListaAsync();
    }

    private void ConstruirInterface()
    {
        var estrutura = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = Tema.Fundo,
            Padding = new Padding(0)
        };
        estrutura.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        estrutura.RowStyles.Add(new RowStyle(SizeType.Absolute, 106));
        estrutura.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        estrutura.RowStyles.Add(new RowStyle(SizeType.Absolute, 88));

        var cabecalho = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        cabecalho.Controls.Add(Tema.CriarTitulo("Registo de operações", 19));
        cabecalho.Controls[0].Location = new Point(6, 4);
        _contador.AutoSize = true;
        _contador.ForeColor = Tema.TextoSuave;
        _contador.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        cabecalho.Resize += (_, _) => _contador.Left = cabecalho.ClientSize.Width - _contador.Width - 8;
        cabecalho.Controls.Add(_contador);
        estrutura.Controls.Add(cabecalho, 0, 0);

        estrutura.Controls.Add(CriarFiltros(), 0, 1);

        Tema.EstilizarGrelha(_grelha);
        _grelha.Dock = DockStyle.Fill;
        _grelha.Margin = new Padding(4, 4, 4, 4);
        _grelha.CellFormatting += FormatarCelula;
        _grelha.CellDoubleClick += (_, _) => MostrarDetalhe();
        _grelha.SelectionChanged += (_, _) => ActualizarBotoes();
        estrutura.Controls.Add(_grelha, 0, 2);

        var accoes = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 4, 0, 0)
        };
        accoes.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        accoes.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var barra = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            BackColor = Color.Transparent
        };
        _rever.Width = 175;
        _historico.Width = 142;
        _detalhe.Width = 125;
        _exportar.Width = 185;
        _actualizar.Width = 125;
        _rever.Click += (_, _) => Rever();
        _historico.Click += (_, _) => MostrarHistorico();
        _detalhe.Click += (_, _) => MostrarDetalhe();
        _exportar.Click += (_, _) => Exportar();
        _actualizar.Click += async (_, _) => await ActualizarListaAsync();
        barra.Controls.AddRange(new Control[] { _rever, _historico, _detalhe, _exportar, _actualizar });
        accoes.Controls.Add(barra, 0, 0);

        _estado.AutoSize = true;
        _estado.ForeColor = Tema.TextoSuave;
        _estado.Location = new Point(8, 4);
        accoes.Controls.Add(_estado, 0, 1);

        estrutura.Controls.Add(accoes, 0, 3);
        Controls.Add(estrutura);
        ActualizarBotoes();
    }

    private Control CriarFiltros()
    {
        var painel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.White,
            Padding = new Padding(10, 8, 10, 6),
            Margin = new Padding(4, 0, 4, 0),
            BorderStyle = BorderStyle.FixedSingle
        };
        painel.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        painel.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));

        var linha1 = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            WrapContents = false,
            FlowDirection = FlowDirection.LeftToRight,
            BackColor = Color.Transparent
        };
        _pesquisa.Width = 235;
        _pesquisa.PlaceholderText = "Pesquisar nome, conta ou ID...";
        Tema.EstilizarCampo(_pesquisa);
        _pesquisa.TextChanged += (_, _) => AplicarFiltros();

        _filtroClassificacao.DropDownStyle = ComboBoxStyle.DropDownList;
        _filtroClassificacao.Width = 220;
        _filtroClassificacao.Items.AddRange(new object[]
        {
            "Todas as classificações", "Normal", "Atenção", "Transacção potencialmente suspeita"
        });
        _filtroClassificacao.SelectedIndex = 0;
        Tema.EstilizarCampo(_filtroClassificacao);
        _filtroClassificacao.SelectedIndexChanged += (_, _) => AplicarFiltros();

        _filtroRevisao.DropDownStyle = ComboBoxStyle.DropDownList;
        _filtroRevisao.Width = 155;
        _filtroRevisao.Items.AddRange(new object[] { "Todas as revisões", "Pendente", "Confirmada", "Descartada" });
        _filtroRevisao.SelectedIndex = 0;
        Tema.EstilizarCampo(_filtroRevisao);
        _filtroRevisao.SelectedIndexChanged += (_, _) => AplicarFiltros();

        linha1.Controls.AddRange(new Control[] { _pesquisa, _filtroClassificacao, _filtroRevisao });

        var linha2 = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            WrapContents = false,
            FlowDirection = FlowDirection.LeftToRight,
            BackColor = Color.Transparent
        };

        _usarPeriodo.Text = "Período";
        _usarPeriodo.AutoSize = true;
        _usarPeriodo.CheckedChanged += (_, _) => { ActivarDatas(); AplicarFiltros(); };
        _dataInicio.Format = DateTimePickerFormat.Short;
        _dataInicio.Value = DateTime.Today.AddDays(-30);
        _dataInicio.Width = 115;
        _dataInicio.ValueChanged += (_, _) => AplicarFiltros();
        _dataFim.Format = DateTimePickerFormat.Short;
        _dataFim.Value = DateTime.Today;
        _dataFim.Width = 115;
        _dataFim.ValueChanged += (_, _) => AplicarFiltros();

        _usarValor.Text = "Valor MZN";
        _usarValor.AutoSize = true;
        _usarValor.CheckedChanged += (_, _) => { ActivarValores(); AplicarFiltros(); };
        _valorMinimo.DecimalPlaces = 2;
        _valorMinimo.Maximum = 999999999;
        _valorMinimo.Width = 125;
        _valorMinimo.ThousandsSeparator = true;
        _valorMinimo.ValueChanged += (_, _) => AplicarFiltros();
        _valorMaximo.DecimalPlaces = 2;
        _valorMaximo.Maximum = 999999999;
        _valorMaximo.Value = 999999999;
        _valorMaximo.Width = 125;
        _valorMaximo.ThousandsSeparator = true;
        _valorMaximo.ValueChanged += (_, _) => AplicarFiltros();

        var limpar = Tema.CriarBotaoSecundario("Limpar filtros");
        limpar.Width = 125;
        limpar.Height = 30;
        limpar.Click += (_, _) => LimparFiltros();

        linha2.Controls.AddRange(new Control[]
        {
            _usarPeriodo, CriarEtiqueta("De"), _dataInicio, CriarEtiqueta("Até"), _dataFim,
            _usarValor, CriarEtiqueta("Mín."), _valorMinimo, CriarEtiqueta("Máx."), _valorMaximo, limpar
        });
        ActivarDatas();
        ActivarValores();

        painel.Controls.Add(linha1, 0, 0);
        painel.Controls.Add(linha2, 0, 1);
        return painel;
    }

    private static Label CriarEtiqueta(string texto)
        => new()
        {
            Text = texto,
            AutoSize = true,
            ForeColor = Tema.TextoSuave,
            Margin = new Padding(4, 8, 0, 0)
        };

    private void ActivarDatas()
    {
        _dataInicio.Enabled = _usarPeriodo.Checked;
        _dataFim.Enabled = _usarPeriodo.Checked;
    }

    private void ActivarValores()
    {
        _valorMinimo.Enabled = _usarValor.Checked;
        _valorMaximo.Enabled = _usarValor.Checked;
    }

    private void LimparFiltros()
    {
        _pesquisa.Clear();
        _filtroClassificacao.SelectedIndex = 0;
        _filtroRevisao.SelectedIndex = 0;
        _usarPeriodo.Checked = false;
        _usarValor.Checked = false;
        _valorMinimo.Value = 0;
        _valorMaximo.Value = _valorMaximo.Maximum;
        AplicarFiltros();
    }

    private async Task ActualizarListaAsync()
    {
        _actualizar.Enabled = false;
        _estado.Text = "A carregar operações...";
        UseWaitCursor = true;
        try
        {
            _dados = await Task.Run(() => _dao.Listar(50000));
            AplicarFiltros();
            _estado.Text = $"Foram carregadas {_dados.Count:N0} operações. Seleccione uma linha para consultar ou rever.";
        }
        catch (Exception ex)
        {
            _estado.Text = "Não foi possível carregar a lista: " + ex.Message;
            MessageBox.Show(_estado.Text, "Transacções",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            UseWaitCursor = false;
            _actualizar.Enabled = true;
        }
    }

    private void AplicarFiltros()
    {
        IEnumerable<Transaccao> consulta = _dados;

        if (_filtroClassificacao.SelectedIndex > 0)
            consulta = consulta.Where(t => t.Classificacao == _filtroClassificacao.SelectedItem?.ToString());

        if (_filtroRevisao.SelectedIndex > 0)
            consulta = consulta.Where(t => t.Revisao == _filtroRevisao.SelectedItem?.ToString());

        string termo = _pesquisa.Text.Trim();
        if (!string.IsNullOrWhiteSpace(termo))
        {
            consulta = consulta.Where(t =>
                t.NomeCliente.Contains(termo, StringComparison.OrdinalIgnoreCase) ||
                t.NumeroConta.Contains(termo, StringComparison.OrdinalIgnoreCase) ||
                t.Id.ToString().Contains(termo, StringComparison.OrdinalIgnoreCase));
        }

        if (_usarPeriodo.Checked)
        {
            DateTime inicio = _dataInicio.Value.Date;
            DateTime fim = _dataFim.Value.Date.AddDays(1).AddTicks(-1);
            consulta = inicio <= fim
                ? consulta.Where(t => t.DataHora >= inicio && t.DataHora <= fim)
                : Enumerable.Empty<Transaccao>();
        }

        if (_usarValor.Checked && _valorMinimo.Value <= _valorMaximo.Value)
            consulta = consulta.Where(t => t.Valor >= _valorMinimo.Value && t.Valor <= _valorMaximo.Value);

        _visiveis = consulta.OrderByDescending(t => t.DataHora).ToList();
        _grelha.DataSource = _visiveis.Select(t => new LinhaTransaccao
        {
            Id = t.Id,
            Data = t.DataHora.ToString("dd/MM/yyyy HH:mm"),
            Cliente = t.NomeCliente,
            Conta = t.NumeroConta,
            Valor = t.Valor,
            Tipo = t.Tipo,
            Cidade = t.Cidade,
            Classificacao = t.Classificacao,
            Probabilidade = t.Pontuacao.HasValue ? t.Pontuacao.Value.ToString("P1") : "—",
            Revisao = t.Revisao,
            Origem = t.Origem
        }).ToList();

        if (_grelha.Columns["Id"] is not null) _grelha.Columns["Id"].Visible = false;
        if (_grelha.Columns["Valor"] is not null)
            _grelha.Columns["Valor"].DefaultCellStyle.Format = "N2";

        _contador.Text = $"{_visiveis.Count:N0} operação(ões)";
        ActualizarBotoes();
    }

    private void FormatarCelula(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || e.CellStyle is null || e.Value is null) return;
        string coluna = _grelha.Columns[e.ColumnIndex].Name;
        string valor = e.Value.ToString() ?? "";

        if (coluna == "Classificacao")
        {
            e.CellStyle.ForeColor = valor.Contains("suspeita") ? Tema.Vermelho
                : valor == "Atenção" ? Tema.Ambar
                : valor == "Normal" ? Tema.Verde : Tema.Texto;
            e.CellStyle.Font = new Font("Segoe UI Semibold", 9f);
        }
        else if (coluna == "Revisao")
        {
            e.CellStyle.ForeColor = valor == "Confirmada" ? Tema.Vermelho
                : valor == "Descartada" ? Tema.Verde : Tema.Ambar;
            e.CellStyle.Font = new Font("Segoe UI Semibold", 9f);
        }
    }

    private LinhaTransaccao? SeleccionarLinha()
        => _grelha.CurrentRow?.DataBoundItem as LinhaTransaccao;

    private Transaccao? SeleccionarTransaccao()
    {
        var linha = SeleccionarLinha();
        if (linha is null)
        {
            MessageBox.Show("Seleccione primeiro uma operação na tabela.", "Transacções",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return null;
        }
        return _visiveis.FirstOrDefault(t => t.Id == linha.Id);
    }

    private void ActualizarBotoes()
    {
        bool seleccionada = SeleccionarLinha() is not null;
        _rever.Enabled = seleccionada;
        _historico.Enabled = seleccionada;
        _detalhe.Enabled = seleccionada;
        _exportar.Enabled = _visiveis.Count > 0;
    }

    private void Rever()
    {
        var transaccao = SeleccionarTransaccao();
        if (transaccao is null) return;

        using var janela = new FormularioRevisao(transaccao);
        if (janela.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            _revisaoDAO.Registar(transaccao.Id, _utilizador.Id,
                janela.EstadoSeleccionado, janela.Observacao);
            _estado.Text = $"Revisão da operação #{transaccao.Id} guardada com sucesso.";
            _ = ActualizarListaAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Não foi possível guardar a decisão.\n" + ex.Message,
                "Revisão", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void MostrarHistorico()
    {
        var transaccao = SeleccionarTransaccao();
        if (transaccao is null) return;

        try
        {
            var historico = _revisaoDAO.Listar(transaccao.Id);
            using var janela = new FormularioHistoricoRevisoes(
                transaccao.NomeCliente, transaccao.Id, historico);
            janela.ShowDialog(this);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Não foi possível consultar o histórico.\n" + ex.Message,
                "Auditoria", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void MostrarDetalhe()
    {
        var transaccao = SeleccionarTransaccao();
        if (transaccao is null) return;
        using var janela = new FormularioDetalheTransaccao(transaccao);
        janela.ShowDialog(this);
    }

    private void Exportar()
    {
        using var dialogo = new SaveFileDialog
        {
            Filter = "Ficheiro CSV (*.csv)|*.csv",
            FileName = "sentinela_operacoes_filtradas.csv"
        };
        if (dialogo.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            var linhas = new List<string>
            {
                "Id;Data;Cliente;Conta;Valor_MZN;Tipo;Cidade;Classificacao;Probabilidade;Revisao;Origem"
            };

            foreach (var item in _visiveis)
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
                    Escapar(item.Revisao),
                    Escapar(item.Origem)
                }));
            }

            File.WriteAllLines(dialogo.FileName, linhas, new UTF8Encoding(true));
            _estado.Text = $"{_visiveis.Count:N0} operação(ões) exportadas.";
            MessageBox.Show("A exportação das operações filtradas foi concluída.",
                "Exportação CSV", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        public long Id { get; set; }
        public string Data { get; set; } = "";
        public string Cliente { get; set; } = "";
        public string Conta { get; set; } = "";
        public decimal Valor { get; set; }
        public string Tipo { get; set; } = "";
        public string Cidade { get; set; } = "";
        public string Classificacao { get; set; } = "";
        public string Probabilidade { get; set; } = "";
        public string Revisao { get; set; } = "";
        public string Origem { get; set; } = "";
    }
}