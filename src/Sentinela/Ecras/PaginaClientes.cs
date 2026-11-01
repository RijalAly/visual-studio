using System.Text;
using Sentinela.Controlos;
using Sentinela.Dados;
using Sentinela.Modelos;
using Sentinela.Util;

namespace Sentinela.Ecras;

public sealed class PaginaClientes : UserControl
{
    private readonly ClienteDAO _dao;
    private readonly DataGridView _grelha = new();
    private readonly TextBox _pesquisa = new();
    private readonly CheckBox _apenasSimulados = new();
    private readonly Label _contador = new();
    private readonly Label _estado = new();
    private readonly Button _adicionar = Tema.CriarBotao("＋  Novo cliente", Tema.Verde);
    private readonly Button _editar = Tema.CriarBotaoSecundario("✎  Editar");
    private readonly Button _eliminar = Tema.CriarBotaoSecundario("⌫  Eliminar");
    private readonly Button _exportar = Tema.CriarBotaoSecundario("⇩  Exportar CSV");
    private readonly Button _actualizar = Tema.CriarBotaoSecundario("↻  Actualizar");
    private List<Cliente> _dados = new();
    private List<Cliente> _visiveis = new();

    public PaginaClientes(ConfiguracaoApp configuracao)
    {
        _dao = new ClienteDAO(configuracao);
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
            BackColor = Tema.Fundo
        };
        estrutura.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        estrutura.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        estrutura.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        estrutura.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));

        var cabecalho = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        var titulo = Tema.CriarTitulo("Clientes", 19);
        titulo.Location = new Point(6, 4);
        cabecalho.Controls.Add(titulo);
        _contador.AutoSize = true;
        _contador.ForeColor = Tema.TextoSuave;
        _contador.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        cabecalho.Resize += (_, _) => _contador.Left = cabecalho.ClientSize.Width - _contador.Width - 8;
        cabecalho.Controls.Add(_contador);
        estrutura.Controls.Add(cabecalho, 0, 0);

        var barra = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            WrapContents = true,
            FlowDirection = FlowDirection.LeftToRight,
            BackColor = Color.White,
            Padding = new Padding(8, 7, 8, 4),
            Margin = new Padding(4, 0, 4, 4)
        };

        _pesquisa.Width = 240;
        _pesquisa.PlaceholderText = "Pesquisar nome, conta ou cidade...";
        Tema.EstilizarCampo(_pesquisa);
        _pesquisa.TextChanged += (_, _) => AplicarFiltros();

        _apenasSimulados.Text = "Mostrar apenas dados de demonstração";
        _apenasSimulados.AutoSize = true;
        _apenasSimulados.Margin = new Padding(8, 10, 8, 0);
        _apenasSimulados.CheckedChanged += (_, _) => AplicarFiltros();

        _adicionar.Width = 150;
        _editar.Width = 110;
        _eliminar.Width = 120;
        _exportar.Width = 145;
        _actualizar.Width = 120;
        foreach (var botao in new[] { _adicionar, _editar, _eliminar, _exportar, _actualizar })
            botao.Height = 36;

        _adicionar.Click += (_, _) => Adicionar();
        _editar.Click += (_, _) => Editar();
        _eliminar.Click += (_, _) => Eliminar();
        _exportar.Click += (_, _) => Exportar();
        _actualizar.Click += async (_, _) => await ActualizarListaAsync();

        barra.Controls.AddRange(new Control[]
        {
            _pesquisa, _apenasSimulados, _adicionar, _editar, _eliminar, _exportar, _actualizar
        });
        estrutura.Controls.Add(barra, 0, 1);

        Tema.EstilizarGrelha(_grelha);
        _grelha.Dock = DockStyle.Fill;
        _grelha.Margin = new Padding(4);
        _grelha.CellDoubleClick += (_, _) => Editar();
        _grelha.SelectionChanged += (_, _) => ActualizarBotoes();
        estrutura.Controls.Add(_grelha, 0, 2);

        _estado.Text = "Os dados de demonstração estão identificados separadamente dos registos manuais.";
        _estado.ForeColor = Tema.TextoSuave;
        _estado.AutoSize = true;
        _estado.Margin = new Padding(8, 4, 4, 4);
        estrutura.Controls.Add(_estado, 0, 3);

        Controls.Add(estrutura);
        ActualizarBotoes();
    }

    private async Task ActualizarListaAsync()
    {
        _actualizar.Enabled = false;
        _estado.Text = "A carregar os clientes...";
        UseWaitCursor = true;
        try
        {
            _dados = await Task.Run(_dao.Listar);
            AplicarFiltros();
            _estado.Text = $"Foram carregados {_dados.Count:N0} clientes.";
        }
        catch (Exception ex)
        {
            _estado.Text = "Não foi possível carregar os clientes.";
            MessageBox.Show(ex.Message, "Clientes", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            UseWaitCursor = false;
            _actualizar.Enabled = true;
        }
    }

    private void AplicarFiltros()
    {
        IEnumerable<Cliente> consulta = _dados;
        if (_apenasSimulados.Checked)
            consulta = consulta.Where(c => c.Simulado);

        string termo = _pesquisa.Text.Trim();
        if (!string.IsNullOrWhiteSpace(termo))
        {
            consulta = consulta.Where(c =>
                c.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase) ||
                c.NumeroConta.Contains(termo, StringComparison.OrdinalIgnoreCase) ||
                c.CidadeHabitual.Contains(termo, StringComparison.OrdinalIgnoreCase));
        }

        _visiveis = consulta.OrderBy(c => c.Nome).ToList();
        _grelha.DataSource = _visiveis.Select(c => new LinhaCliente
        {
            Id = c.Id,
            Nome = c.Nome,
            Conta = c.NumeroConta,
            Cidade = c.CidadeHabitual,
            Media = c.MediaValor,
            Origem = c.Simulado ? "Demonstração" : "Registo manual",
            CriadoEm = c.CriadoEm.ToString("dd/MM/yyyy")
        }).ToList();

        if (_grelha.Columns["Id"] is not null) _grelha.Columns["Id"].Visible = false;
        if (_grelha.Columns["Media"] is not null)
        {
            _grelha.Columns["Media"].HeaderText = "Média por operação (MZN)";
            _grelha.Columns["Media"].DefaultCellStyle.Format = "N2";
        }
        _contador.Text = $"{_visiveis.Count:N0} de {_dados.Count:N0} cliente(s)";
        ActualizarBotoes();
    }

    private Cliente? Seleccionar()
    {
        if (_grelha.CurrentRow?.DataBoundItem is LinhaCliente linha)
            return _visiveis.FirstOrDefault(c => c.Id == linha.Id);

        MessageBox.Show("Seleccione um cliente na tabela.", "Clientes",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
        return null;
    }

    private void ActualizarBotoes()
    {
        bool temSeleccao = _grelha.CurrentRow?.DataBoundItem is LinhaCliente;
        _editar.Enabled = temSeleccao;
        _eliminar.Enabled = temSeleccao;
        _exportar.Enabled = _visiveis.Count > 0;
    }

    private void Adicionar()
    {
        using var janela = new FormularioCliente();
        if (janela.ShowDialog(this) != DialogResult.OK) return;
        GuardarCliente(() => _dao.Inserir(janela.Cliente));
    }

    private void Editar()
    {
        var cliente = Seleccionar();
        if (cliente is null) return;
        using var janela = new FormularioCliente(cliente);
        if (janela.ShowDialog(this) != DialogResult.OK) return;
        GuardarCliente(() => _dao.Actualizar(janela.Cliente));
    }

    private void GuardarCliente(Action operacao)
    {
        try
        {
            operacao();
            _estado.Text = "Os dados do cliente foram guardados.";
            _ = ActualizarListaAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Não foi possível guardar o cliente. Confirme se o número de conta já existe.\n" + ex.Message,
                "Clientes", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Eliminar()
    {
        var cliente = Seleccionar();
        if (cliente is null) return;

        string aviso = cliente.Simulado
            ? $"Pretende eliminar o cliente de demonstração {cliente.Nome} e as operações associadas?"
            : $"Pretende eliminar {cliente.Nome}? As operações ligadas a este cliente também serão eliminadas.";

        if (MessageBox.Show(aviso, "Confirmar eliminação",
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;

        try
        {
            _dao.Eliminar(cliente.Id);
            _estado.Text = "Cliente eliminado.";
            _ = ActualizarListaAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Não foi possível eliminar o cliente.\n" + ex.Message,
                "Clientes", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Exportar()
    {
        using var dialogo = new SaveFileDialog
        {
            Filter = "Ficheiro CSV (*.csv)|*.csv",
            FileName = "sentinela_clientes.csv"
        };
        if (dialogo.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            var linhas = new List<string> { "Id;Nome;Conta;Cidade;Media_MZN;Origem;CriadoEm" };
            foreach (var c in _visiveis)
            {
                linhas.Add(string.Join(";", new[]
                {
                    c.Id.ToString(), Escapar(c.Nome), Escapar(c.NumeroConta),
                    Escapar(c.CidadeHabitual),
                    c.MediaValor.ToString("0.00", System.Globalization.CultureInfo.GetCultureInfo("pt-PT")),
                    c.Simulado ? "Demonstração" : "Registo manual",
                    c.CriadoEm.ToString("dd/MM/yyyy")
                }));
            }
            File.WriteAllLines(dialogo.FileName, linhas, new UTF8Encoding(true));
            _estado.Text = $"{_visiveis.Count:N0} cliente(s) exportados.";
        }
        catch (Exception ex)
        {
            MessageBox.Show("Não foi possível exportar os clientes.\n" + ex.Message,
                "Exportação", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static string Escapar(string texto) => "\"" + texto.Replace("\"", "\"\"") + "\"";

    private sealed class LinhaCliente
    {
        public int Id { get; set; }
        public string Nome { get; set; } = "";
        public string Conta { get; set; } = "";
        public string Cidade { get; set; } = "";
        public decimal Media { get; set; }
        public string Origem { get; set; } = "";
        public string CriadoEm { get; set; } = "";
    }
}