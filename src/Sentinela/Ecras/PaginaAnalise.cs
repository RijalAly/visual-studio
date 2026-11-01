using Sentinela.Controlos;
using Sentinela.Dados;
using Sentinela.Inteligencia;
using Sentinela.Modelos;
using Sentinela.Util;

namespace Sentinela.Ecras;

public sealed class PaginaAnalise : UserControl
{
    private readonly ClienteDAO _clienteDAO;
    private readonly TransaccaoDAO _transaccaoDAO;
    private readonly ServicoIA _servicoIA;

    private readonly ComboBox _cliente = new();
    private readonly NumericUpDown _valor = new();
    private readonly DateTimePicker _dataHora = new();
    private readonly ComboBox _tipo = new();
    private readonly ComboBox _canal = new();
    private readonly ComboBox _cidade = new();
    private readonly CheckBox _beneficiario = new();
    private readonly CheckBox _guardar = new();
    private readonly Label _dadosCliente = new();
    private readonly Label _operacoes = new();
    private readonly Label _resultado = new();
    private readonly Label _probabilidade = new();
    private readonly Label _confianca = new();
    private readonly TextBox _factores = new();

    public PaginaAnalise(ConfiguracaoApp configuracao, ServicoIA servicoIA)
    {
        _clienteDAO = new ClienteDAO(configuracao);
        _transaccaoDAO = new TransaccaoDAO(configuracao);
        _servicoIA = servicoIA;

        BackColor = Tema.Fundo;
        ConstruirInterface();
        CarregarClientes();
    }

    private void ConstruirInterface()
    {
        var area = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.Transparent };
        var cartao = Tema.CriarCartao();
        cartao.Location = new Point(8, 8);
        cartao.Size = new Size(650, 560);
        cartao.Anchor = AnchorStyles.Top | AnchorStyles.Left;

        cartao.Controls.Add(Tema.CriarTitulo("Dados da transacção", 17));
        cartao.Controls[0].Location = new Point(18, 15);

        AdicionarCampo(cartao, "Cliente", _cliente, 70);
        AdicionarCampo(cartao, "Valor (MZN)", _valor, 125);
        AdicionarCampo(cartao, "Data e hora", _dataHora, 180);
        AdicionarCampo(cartao, "Tipo de operação", _tipo, 235);
        AdicionarCampo(cartao, "Canal", _canal, 290);
        AdicionarCampo(cartao, "Cidade", _cidade, 345);

        _cliente.DropDownStyle = ComboBoxStyle.DropDownList;
        _cliente.SelectedIndexChanged += (_, _) => ActualizarDadosCliente();

        _valor.DecimalPlaces = 2;
        _valor.Minimum = 1;
        _valor.Maximum = 999999999;
        _valor.ThousandsSeparator = true;

        _dataHora.Format = DateTimePickerFormat.Custom;
        _dataHora.CustomFormat = "dd/MM/yyyy HH:mm";
        _dataHora.ShowUpDown = true;
        _dataHora.Value = DateTime.Now;

        _tipo.DropDownStyle = ComboBoxStyle.DropDownList;
        _tipo.Items.AddRange(new object[] { "Levantamento", "Transferência", "Pagamento", "Depósito", "Carteira móvel" });
        _tipo.SelectedIndex = 0;

        _canal.DropDownStyle = ComboBoxStyle.DropDownList;
        _canal.Items.AddRange(new object[] { "ATM", "Telemóvel", "Internet banking", "POS" });
        _canal.SelectedIndex = 0;

        _cidade.DropDownStyle = ComboBoxStyle.DropDownList;
        _cidade.Items.AddRange(new object[]
        {
            "Maputo", "Matola", "Beira", "Nampula", "Quelimane", "Tete",
            "Xai-Xai", "Inhambane", "Pemba", "Chimoio", "Lichinga"
        });
        _cidade.SelectedIndex = 0;

        _dadosCliente.AutoSize = true;
        _dadosCliente.ForeColor = Tema.TextoSuave;
        _dadosCliente.Location = new Point(18, 395);
        cartao.Controls.Add(_dadosCliente);

        _operacoes.AutoSize = true;
        _operacoes.ForeColor = Tema.TextoSuave;
        _operacoes.Location = new Point(18, 420);
        cartao.Controls.Add(_operacoes);

        _beneficiario.Text = "Beneficiário novo";
        _beneficiario.AutoSize = true;
        _beneficiario.Location = new Point(18, 455);
        cartao.Controls.Add(_beneficiario);

        _guardar.Text = "Guardar no histórico depois da análise";
        _guardar.AutoSize = true;
        _guardar.Checked = true;
        _guardar.Location = new Point(18, 485);
        cartao.Controls.Add(_guardar);

        var normal = Tema.CriarBotao("Exemplo normal", Color.FromArgb(90, 105, 125));
        normal.Location = new Point(18, 520);
        normal.Click += (_, _) => PreencherExemplo(false);
        cartao.Controls.Add(normal);

        var suspeito = Tema.CriarBotao("Exemplo de atenção", Tema.Ambar);
        suspeito.Location = new Point(188, 520);
        suspeito.Click += (_, _) => PreencherExemplo(true);
        cartao.Controls.Add(suspeito);

        var analisar = Tema.CriarBotao("Analisar transacção", Tema.Azul);
        analisar.Location = new Point(420, 520);
        analisar.Width = 205;
        analisar.Click += (_, _) => Analisar();
        cartao.Controls.Add(analisar);

        var cartaoResultado = Tema.CriarCartao();
        cartaoResultado.Location = new Point(680, 8);
        cartaoResultado.Size = new Size(380, 560);
        cartaoResultado.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        cartaoResultado.Controls.Add(Tema.CriarTitulo("Resultado da análise", 17));
        cartaoResultado.Controls[0].Location = new Point(18, 15);

        _resultado.Text = "Aguardando análise";
        _resultado.Location = new Point(18, 75);
        _resultado.AutoSize = true;
        _resultado.Font = new Font("Segoe UI Semibold", 16);
        _resultado.ForeColor = Tema.Texto;
        cartaoResultado.Controls.Add(_resultado);

        _probabilidade.Text = "Probabilidade: —";
        _probabilidade.Location = new Point(18, 125);
        _probabilidade.AutoSize = true;
        cartaoResultado.Controls.Add(_probabilidade);

        _confianca.Text = "Confiança da previsão: —";
        _confianca.Location = new Point(18, 155);
        _confianca.AutoSize = true;
        cartaoResultado.Controls.Add(_confianca);

        cartaoResultado.Controls.Add(new Label
        {
            Text = "Factores de atenção",
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 10),
            Location = new Point(18, 205),
            ForeColor = Tema.Texto
        });

        _factores.Location = new Point(18, 235);
        _factores.Size = new Size(335, 180);
        _factores.Multiline = true;
        _factores.ReadOnly = true;
        _factores.BackColor = Color.White;
        _factores.BorderStyle = BorderStyle.FixedSingle;
        _factores.ScrollBars = ScrollBars.Vertical;
        cartaoResultado.Controls.Add(_factores);

        cartaoResultado.Controls.Add(new Label
        {
            Text = "A previsão é um apoio à decisão.\nNão confirma a existência de fraude.",
            AutoSize = true,
            ForeColor = Tema.Vermelho,
            Font = new Font("Segoe UI Semibold", 9.5f),
            Location = new Point(18, 445)
        });

        area.Controls.Add(cartao);
        area.Controls.Add(cartaoResultado);
        Controls.Add(area);
    }

    private static void AdicionarCampo(Control pai, string legenda, Control campo, int topo)
    {
        pai.Controls.Add(new Label
        {
            Text = legenda,
            AutoSize = true,
            Location = new Point(18, topo),
            ForeColor = Tema.Texto
        });
        campo.Location = new Point(210, topo - 6);
        campo.Width = 410;
        campo.Height = 30;
        pai.Controls.Add(campo);
    }

    private void CarregarClientes()
    {
        try
        {
            _cliente.DataSource = _clienteDAO.Listar();
            _cliente.DisplayMember = "Nome";
            _cliente.ValueMember = "Id";
            ActualizarDadosCliente();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Não foi possível carregar a lista de clientes.\n" + ex.Message,
                "Analisar transacção", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private Cliente? ClienteSeleccionado => _cliente.SelectedItem as Cliente;

    private void ActualizarDadosCliente()
    {
        var cliente = ClienteSeleccionado;
        if (cliente is null) return;
        _dadosCliente.Text = $"Cidade habitual: {cliente.CidadeHabitual}    Média por operação: {cliente.MediaValor:N2} MZN";
        int quantidade = _transaccaoDAO.ContarUltimaHora(cliente.Id, _dataHora.Value);
        _operacoes.Text = $"Operações deste cliente na última hora: {quantidade} (calculadas pelo histórico)";
        if (_cidade.Items.Contains(cliente.CidadeHabitual))
            _cidade.SelectedItem = cliente.CidadeHabitual;
    }

    private void PreencherExemplo(bool suspeito)
    {
        var cliente = ClienteSeleccionado;
        if (cliente is null)
        {
            MessageBox.Show("Crie primeiro um cliente ou gere dados simulados.", "Exemplos",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        _valor.Value = Math.Clamp(
            suspeito ? cliente.MediaValor * 10 : cliente.MediaValor * 0.8m,
            _valor.Minimum, _valor.Maximum);

        _dataHora.Value = suspeito
            ? DateTime.Today.AddHours(2).AddMinutes(15)
            : DateTime.Today.AddHours(14).AddMinutes(30);

        _tipo.SelectedItem = suspeito ? "Transferência" : "Pagamento";
        _canal.SelectedItem = suspeito ? "Internet banking" : "POS";
        _cidade.SelectedItem = suspeito
            ? (_cidade.Items.Cast<string>().FirstOrDefault(c => c != cliente.CidadeHabitual) ?? cliente.CidadeHabitual)
            : cliente.CidadeHabitual;
        _beneficiario.Checked = suspeito;
        ActualizarDadosCliente();
    }

    private void Analisar()
    {
        var cliente = ClienteSeleccionado;
        if (cliente is null)
        {
            MessageBox.Show("Seleccione um cliente.", "Análise",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            if (!_servicoIA.TemModelo)
                throw new InvalidOperationException("Ainda não existe um modelo treinado. Aceda ao ecrã Modelo de IA e treine primeiro.");

            int operacoes = _transaccaoDAO.ContarUltimaHora(cliente.Id, _dataHora.Value);
            string cidade = _cidade.SelectedItem?.ToString() ?? cliente.CidadeHabitual;
            decimal valor = _valor.Value;

            var transaccao = new Transaccao
            {
                ClienteId = cliente.Id,
                NomeCliente = cliente.Nome,
                NumeroConta = cliente.NumeroConta,
                DataHora = _dataHora.Value,
                Valor = valor,
                Tipo = _tipo.SelectedItem?.ToString() ?? "Pagamento",
                Canal = _canal.SelectedItem?.ToString() ?? "POS",
                Cidade = cidade,
                CidadeDiferente = !string.Equals(cidade, cliente.CidadeHabitual, StringComparison.OrdinalIgnoreCase),
                BeneficiarioNovo = _beneficiario.Checked,
                OperacoesUltimaHora = operacoes,
                RelacaoMedia = (float)(valor / Math.Max(1, cliente.MediaValor)),
                Origem = "Manual",
                Etiqueta = null
            };

            var resultado = _servicoIA.Analisar(transaccao);
            _resultado.Text = resultado.Classificacao;
            _resultado.ForeColor = resultado.Probabilidade >= 0.5f ? Tema.Vermelho
                : resultado.Probabilidade >= 0.25f ? Tema.Ambar : Tema.Verde;
            _probabilidade.Text = $"Probabilidade de comportamento suspeito: {resultado.Probabilidade:P1}";
            _confianca.Text = $"Confiança da previsão: {resultado.Confianca:P1}";
            _factores.Text = string.Join(Environment.NewLine, resultado.Factores.Select(f => "• " + f));

            if (_guardar.Checked)
            {
                _transaccaoDAO.InserirLote(new[] { transaccao });
                _transaccaoDAO.ClassificarTodos(_servicoIA.Analisar);
                MessageBox.Show("A transacção foi guardada e classificada no histórico.",
                    "Análise concluída", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Não foi possível analisar a transacção.\n" + ex.Message,
                "Análise", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
