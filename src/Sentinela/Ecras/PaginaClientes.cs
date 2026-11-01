using Sentinela.Controlos;
using Sentinela.Dados;
using Sentinela.Modelos;
using Sentinela.Util;

namespace Sentinela.Ecras;

public sealed class PaginaClientes : UserControl
{
    private readonly ClienteDAO _dao;
    private readonly DataGridView _grelha = new();

    public PaginaClientes(ConfiguracaoApp configuracao)
    {
        _dao = new ClienteDAO(configuracao);
        BackColor = Tema.Fundo;
        CriarInterface();
        ActualizarLista();
    }

    private void CriarInterface()
    {
        var topo = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 58,
            BackColor = Color.Transparent
        };

        var adicionar = Tema.CriarBotao("Novo cliente", Tema.Verde);
        adicionar.Click += (_, _) => Adicionar();
        var editar = Tema.CriarBotao("Editar");
        editar.Click += (_, _) => Editar();
        var eliminar = Tema.CriarBotao("Eliminar", Tema.Vermelho);
        eliminar.Click += (_, _) => Eliminar();
        var actualizar = Tema.CriarBotao("Actualizar", Color.FromArgb(90, 105, 125));
        actualizar.Click += (_, _) => ActualizarLista();

        topo.Controls.AddRange(new Control[] { adicionar, editar, eliminar, actualizar });
        ConfigurarGrelha();
        Controls.Add(_grelha);
        Controls.Add(topo);
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
    }

    private void ActualizarLista()
    {
        try
        {
            _grelha.DataSource = _dao.Listar();
            if (_grelha.Columns["Id"] is not null) _grelha.Columns["Id"].Visible = false;
            if (_grelha.Columns["Simulado"] is not null) _grelha.Columns["Simulado"].HeaderText = "Dados simulados";
            if (_grelha.Columns["MediaValor"] is not null)
                _grelha.Columns["MediaValor"].DefaultCellStyle.Format = "N2";
        }
        catch (Exception ex)
        {
            MessageBox.Show("Não foi possível carregar os clientes.\n" + ex.Message,
                "Clientes", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private Cliente? Seleccionar()
    {
        if (_grelha.CurrentRow?.DataBoundItem is Cliente cliente)
            return cliente;
        MessageBox.Show("Seleccione um cliente na lista.", "Clientes",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
        return null;
    }

    private void Adicionar()
    {
        using var janela = new FormularioCliente();
        if (janela.ShowDialog() != DialogResult.OK) return;

        try
        {
            _dao.Inserir(janela.Cliente);
            ActualizarLista();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Não foi possível guardar o cliente. Confirme se o número de conta já existe.\n" + ex.Message,
                "Clientes", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Editar()
    {
        var cliente = Seleccionar();
        if (cliente is null) return;

        using var janela = new FormularioCliente(cliente);
        if (janela.ShowDialog() != DialogResult.OK) return;

        try
        {
            _dao.Actualizar(janela.Cliente);
            ActualizarLista();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Não foi possível actualizar o cliente.\n" + ex.Message,
                "Clientes", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Eliminar()
    {
        var cliente = Seleccionar();
        if (cliente is null) return;

        var confirmacao = MessageBox.Show(
            $"Pretende eliminar {cliente.Nome}? As transacções deste cliente também serão eliminadas.",
            "Confirmar eliminação", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

        if (confirmacao != DialogResult.Yes) return;

        try
        {
            _dao.Eliminar(cliente.Id);
            ActualizarLista();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Não foi possível eliminar o cliente.\n" + ex.Message,
                "Clientes", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
