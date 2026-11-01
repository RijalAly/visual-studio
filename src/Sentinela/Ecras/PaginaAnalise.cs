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
    private readonly Label _relacaoMedia = new();
    private readonly Label _resultado = new();
    private readonly Label _probabilidade = new();
    private readonly Label _confianca = new();
    private readonly Label _estado = new();
    private readonly TextBox _factores = new();
    private readonly ProgressBar _indicador = new();
    private readonly Button _analisar = Tema.CriarBotao("✧  Analisar operação");
    private readonly Button _normal = Tema.CriarBotaoSecundario("Exemplo normal");
    private readonly Button _atencao = Tema.CriarBotaoSecundario("Exemplo de atenção");

    public PaginaAnalise(ConfiguracaoApp configuracao, ServicoIA servicoIA)
    {
        _clienteDAO = new ClienteDAO(configuracao);
        _transaccaoDAO = new TransaccaoDAO(configuracao);
        _servicoIA = servicoIA;
        BackColor = Tema.Fundo;
        ConstruirInterface();
        Load += async (_, _) => await CarregarClientesAsync();
    }

    private void ConstruirInterface()
    {
        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Tema.Fundo };
        var estrutura = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            RowCount = 2,
            BackColor = Color.Transparent
        };
        estrutura.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 57));
        estrutura.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 43));
        estrutura.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
        estrutura.RowStyles.Add(new RowStyle(SizeType.Absolute, 610));

        var cabecalho = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        var titulo = Tema.CriarTitulo("Analisar uma operação", 20);
        titulo.Location = new Point(8, 0);
        cabecalho.Controls.Add(titulo);
        cabecalho.Controls.Add(new Label
        {
            Text = "Preencha os dados conhecidos. O modelo apresenta uma estimativa para apoiar a revisão.",
            AutoSize = true,
            ForeColor = Tema.TextoSuave,
            Location = new Point(10, 35)
        });
        estrutura.Controls.Add(cabecalho, 0, 0);
        estrutura.SetColumnSpan(cabecalho, 2);

        var cartaoDados = CriarCartao();
        var formulario = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 7,
            BackColor = Color.Transparent,
            Padding = new Padding(8)
        };
        formulario.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        formulario.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        for (int i = 0; i < 7; i++)
            formulario.RowStyles.Add(new RowStyle(SizeType.Percent, i == 6 ? 18 : 13.666f));

        PrepararCombo(_cliente);
        PrepararNumero(_valor, 1, 999999999, 2);
        _valor.ThousandsSeparator = true;
        _dataHora.Format = DateTimePickerFormat.Custom;
        _dataHora.CustomFormat = "dd/MM/yyyy HH:mm";
        _dataHora.ShowUpDown = true;
        _dataHora.Value = DateTime.Now;
        PrepararCombo(_tipo);
        _tipo.Items.AddRange(new object[] { "Levantamento", "Transferência", "Pagamento", "Depósito", "Carteira móvel" });
        _tipo.SelectedIndex = 0;
        PrepararCombo(_canal);
        _canal.Items.AddRange(new object[] { "ATM", "Telemóvel", "Internet banking", "POS" });
        _canal.SelectedIndex = 0;
        PrepararCombo(_cidade);
        _cidade.Items.AddRange(new object[]
        {
            "Maputo", "Matola", "Beira", "Nampula", "Quelimane", "Tete",
            "Xai-Xai", "Inhambane", "Pemba", "Chimoio", "Lichinga"
        });
        _cidade.SelectedIndex = 0;

        formulario.Controls.Add(CriarCampo("Cliente", _cliente), 0, 0);
        formulario.Controls.Add(CriarCampo("Data e hora da operação", _dataHora), 1, 0);
        formulario.Controls.Add(CriarCampo("Valor (MZN)", _valor), 0, 1);
        formulario.Controls.Add(CriarCampo("Tipo de operação", _tipo), 1, 1);
        formulario.Controls.Add(CriarCampo("Canal", _canal), 0, 2);
        formulario.Controls.Add(CriarCampo("Cidade da operação", _cidade), 1, 2);

        var contexto = new Panel { Dock = DockStyle.Fill, BackColor = Tema.AzulClaro, Padding = new Padding(10) };
        _dadosCliente.Text = "Seleccione um cliente para consultar a cidade habitual e a média.";
        _dadosCliente.ForeColor = Tema.Texto;
        _dadosCliente.Dock = DockStyle.Top;
        _dadosCliente.AutoSize = true;
        contexto.Controls.Add(_dadosCliente);
        _relacaoMedia.Text = "Relação do valor com a média habitual: —";
        _relacaoMedia.ForeColor = Tema.Azul;
        _relacaoMedia.Font = new Font("Segoe UI Semibold", 9);
        _relacaoMedia.Dock = DockStyle.Bottom;
        _relacaoMedia.AutoSize = true;
        contexto.Controls.Add(_relacaoMedia);
        formulario.Controls.Add(contexto, 0, 3);
        formulario.SetColumnSpan(contexto, 2);

        var opcoes = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            WrapContents = true,
            FlowDirection = FlowDirection.LeftToRight,
            BackColor = Color.Transparent,
            Padding = new Padding(6, 6, 0, 0)
        };
        _beneficiario.Text = "Beneficiário novo";
        _beneficiario.AutoSize = true;
        _beneficiario.Margin = new Padding(4, 8, 18, 4);
        _guardar.Text = "Guardar esta operação no histórico";
        _guardar.AutoSize = true;
        _guardar.Checked = true;
        _guardar.Margin = new Padding(4, 8, 4, 4);
        opcoes.Controls.AddRange(new Control[] { _beneficiario, _guardar });
        formulario.Controls.Add(opcoes, 0, 4);
        formulario.SetColumnSpan(opcoes, 2);

        var exemplos = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            WrapContents = false,
            FlowDirection = FlowDirection.LeftToRight,
            BackColor = Color.Transparent
        };
        _normal.Width = 145;
        _atencao.Width = 160;
        _normal.Click += (_, _) => PreencherExemplo(false);
        _atencao.Click += (_, _) => PreencherExemplo(true);
        exemplos.Controls.AddRange(new Control[] { _normal, _atencao });
        formulario.Controls.Add(exemplos, 0, 5);
        formulario.SetColumnSpan(exemplos, 2);

        _analisar.Width = 220;
        _analisar.Height = 42;
        _analisar.Click += async (_, _) => await AnalisarAsync();
        var accao = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            BackColor = Color.Transparent
        };
        accao.Controls.Add(_analisar);
        formulario.Controls.Add(accao, 0, 6);
        formulario.SetColumnSpan(accao, 2);

        cartaoDados.Controls.Add(formulario);
        estrutura.Controls.Add(cartaoDados, 0, 1);

        var cartaoResultado = CriarCartao();
        var resultadoLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 8,
            BackColor = Color.Transparent,
            Padding = new Padding(12)
        };
        resultadoLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        resultadoLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        resultadoLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        resultadoLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        resultadoLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        resultadoLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        resultadoLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        resultadoLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));

        resultadoLayout.Controls.Add(Tema.CriarTitulo("Resultado da análise", 16), 0, 0);

        _resultado.Text = "Aguardando dados";
        _resultado.Font = new Font("Segoe UI Semibold", 17);
        _resultado.ForeColor = Tema.Texto;
        _resultado.TextAlign = ContentAlignment.MiddleLeft;
        _resultado.Dock = DockStyle.Fill;
        resultadoLayout.Controls.Add(_resultado, 0, 1);

        _probabilidade.Text = "Probabilidade de comportamento suspeito: —";
        _probabilidade.ForeColor = Tema.Texto;
        _probabilidade.Dock = DockStyle.Fill;
        resultadoLayout.Controls.Add(_probabilidade, 0, 2);

        _confianca.Text = "Confiança da previsão: —";
        _confianca.ForeColor = Tema.TextoSuave;
        _confianca.Dock = DockStyle.Fill;
        resultadoLayout.Controls.Add(_confianca, 0, 3);

        _indicador.Minimum = 0;
        _indicador.Maximum = 100;
        _indicador.Dock = DockStyle.Fill;
        _indicador.Style = ProgressBarStyle.Continuous;
        resultadoLayout.Controls.Add(_indicador, 0, 4);

        resultadoLayout.Controls.Add(new Label
        {
            Text = "Factores de atenção para consulta",
            AutoSize = true,
            ForeColor = Tema.Texto,
            Font = new Font("Segoe UI Semibold", 9.5f),
            Dock = DockStyle.Fill
        }, 0, 5);

        _factores.Dock = DockStyle.Fill;
        _factores.Multiline = true;
        _factores.ReadOnly = true;
        _factores.ScrollBars = ScrollBars.Vertical;
        _factores.BackColor = Color.FromArgb(249, 250, 253);
        _factores.BorderStyle = BorderStyle.FixedSingle;
        resultadoLayout.Controls.Add(_factores, 0, 6);

        _estado.Text = "O resultado é um apoio à decisão e precisa de revisão humana.";
        _estado.Dock = DockStyle.Fill;
        _estado.ForeColor = Tema.TextoSuave;
        _estado.Font = new Font("Segoe UI", 8.2f);
        _estado.AutoSize = true;
        resultadoLayout.Controls.Add(_estado, 0, 7);

        cartaoResultado.Controls.Add(resultadoLayout);
        estrutura.Controls.Add(cartaoResultado, 1, 1);

        scroll.Controls.Add(estrutura);
        Controls.Add(scroll);

        _cliente.SelectedIndexChanged += (_, _) => ActualizarContextoCliente();
        _valor.ValueChanged += (_, _) => ActualizarRelacao();
        _cidade.SelectedIndexChanged += (_, _) => ActualizarRelacao();
        _dataHora.ValueChanged += (_, _) => ActualizarRelacao();
    }

    private static Panel CriarCartao()
        => new()
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(12),
            Margin = new Padding(5),
            BorderStyle = BorderStyle.FixedSingle
        };

    private static Control CriarCampo(string legenda, Control controlo)
    {
        var painel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.Transparent,
            Padding = new Padding(4, 2, 8, 4)
        };
        painel.RowStyles.Add(new RowStyle(SizeType.Absolute, 23));
        painel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        painel.Controls.Add(new Label
        {
            Text = legenda,
            AutoSize = true,
            ForeColor = Tema.TextoSuave,
            Font = new Font("Segoe UI Semibold", 8.5f),
            Dock = DockStyle.Fill
        }, 0, 0);
        controlo.Dock = DockStyle.Fill;
        Tema.EstilizarCampo(controlo);
        painel.Controls.Add(controlo, 0, 1);
        return painel;
    }

    private static void PrepararCombo(ComboBox combo)
    {
        combo.DropDownStyle = ComboBoxStyle.DropDownList;
        combo.Font = new Font("Segoe UI", 9.5f);
        combo.Dock = DockStyle.Fill;
    }

    private static void PrepararNumero(NumericUpDown numero, decimal minimo, decimal maximo, int casas)
    {
        numero.Minimum = minimo;
        numero.Maximum = maximo;
        numero.DecimalPlaces = casas;
        numero.ThousandsSeparator = true;
        numero.Dock = DockStyle.Fill;
        numero.Font = new Font("Segoe UI", 9.5f);
    }

    private async Task CarregarClientesAsync()
    {
        try
        {
            _cliente.Enabled = false;
            var clientes = await Task.Run(_clienteDAO.Listar);
            _cliente.DataSource = clientes;
            _cliente.DisplayMember = "Nome";
            _cliente.ValueMember = "Id";
            _cliente.Enabled = true;
            ActualizarContextoCliente();
        }
        catch (Exception ex)
        {
            _estado.Text = "Não foi possível carregar clientes. " + ex.Message;
        }
    }

    private Cliente? ClienteSeleccionado => _cliente.SelectedItem as Cliente;

    private void ActualizarContextoCliente()
    {
        var cliente = ClienteSeleccionado;
        if (cliente is null)
        {
            _dadosCliente.Text = "Ainda não existem clientes. Crie um no menu Clientes.";
            return;
        }

        _dadosCliente.Text =
            $"Cidade habitual: {cliente.CidadeHabitual}   |   Média por operação: {cliente.MediaValor:N2} MZN";
        if (_cidade.Items.Contains(cliente.CidadeHabitual))
            _cidade.SelectedItem = cliente.CidadeHabitual;
        ActualizarRelacao();
    }

    private void ActualizarRelacao()
    {
        var cliente = ClienteSeleccionado;
        if (cliente is null || cliente.MediaValor <= 0)
        {
            _relacaoMedia.Text = "Relação do valor com a média habitual: —";
            return;
        }

        decimal relacao = _valor.Value / cliente.MediaValor;
        string cidade = _cidade.SelectedItem?.ToString() ?? cliente.CidadeHabitual;
        bool cidadeDiferente = !string.Equals(cidade, cliente.CidadeHabitual, StringComparison.OrdinalIgnoreCase);
        _relacaoMedia.Text = $"Valor equivalente a {relacao:N1} × a média habitual" +
            (cidadeDiferente ? "   •   Cidade diferente" : "");
        _relacaoMedia.ForeColor = relacao >= 6 || cidadeDiferente ? Tema.Ambar : Tema.Azul;
    }

    private void PreencherExemplo(bool comAtencao)
    {
        var cliente = ClienteSeleccionado;
        if (cliente is null)
        {
            MessageBox.Show("Crie ou carregue primeiro um cliente.", "Exemplos",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        _valor.Value = Math.Clamp(
            comAtencao ? cliente.MediaValor * 10 : cliente.MediaValor * 0.8m,
            _valor.Minimum, _valor.Maximum);
        _dataHora.Value = comAtencao
            ? DateTime.Today.AddHours(2).AddMinutes(15)
            : DateTime.Today.AddHours(14).AddMinutes(30);
        _tipo.SelectedItem = comAtencao ? "Transferência" : "Pagamento";
        _canal.SelectedItem = comAtencao ? "Internet banking" : "POS";
        _cidade.SelectedItem = comAtencao
            ? (_cidade.Items.Cast<string>().FirstOrDefault(c => c != cliente.CidadeHabitual) ?? cliente.CidadeHabitual)
            : cliente.CidadeHabitual;
        _beneficiario.Checked = comAtencao;
        ActualizarRelacao();
    }

    private async Task AnalisarAsync()
    {
        var cliente = ClienteSeleccionado;
        if (cliente is null)
        {
            MessageBox.Show("Seleccione um cliente antes de analisar.", "Análise",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!_servicoIA.TemModelo)
        {
            MessageBox.Show("Ainda não existe um modelo treinado. Abra Modelo de IA e execute o treino.",
                "Modelo não disponível", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (_valor.Value <= 0)
        {
            MessageBox.Show("Introduza um valor superior a zero.", "Validação",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _analisar.Enabled = false;
        _normal.Enabled = false;
        _atencao.Enabled = false;
        _estado.Text = "A consultar o histórico e a executar o modelo...";
        UseWaitCursor = true;

        try
        {
            int clienteId = cliente.Id;
            DateTime dataHora = _dataHora.Value;
            decimal valor = _valor.Value;
            string cidade = _cidade.SelectedItem?.ToString() ?? cliente.CidadeHabitual;
            string tipo = _tipo.SelectedItem?.ToString() ?? "Pagamento";
            string canal = _canal.SelectedItem?.ToString() ?? "POS";
            bool beneficiarioNovo = _beneficiario.Checked;
            bool guardar = _guardar.Checked;

            var pacote = await Task.Run(() =>
            {
                int operacoes = _transaccaoDAO.ContarUltimaHora(clienteId, dataHora);
                var transaccao = new Transaccao
                {
                    ClienteId = cliente.Id,
                    NomeCliente = cliente.Nome,
                    NumeroConta = cliente.NumeroConta,
                    DataHora = dataHora,
                    Valor = valor,
                    Tipo = tipo,
                    Canal = canal,
                    Cidade = cidade,
                    CidadeDiferente = !string.Equals(cidade, cliente.CidadeHabitual, StringComparison.OrdinalIgnoreCase),
                    BeneficiarioNovo = beneficiarioNovo,
                    OperacoesUltimaHora = operacoes,
                    RelacaoMedia = (float)(valor / Math.Max(1, cliente.MediaValor)),
                    Origem = "Manual",
                    Etiqueta = null
                };

                var resultado = _servicoIA.Analisar(transaccao);
                long? id = guardar ? _transaccaoDAO.InserirComPredicao(transaccao, resultado) : null;
                return (transaccao, resultado, id);
            });

            _resultado.Text = pacote.resultado.Classificacao;
            _resultado.ForeColor = pacote.resultado.Probabilidade >= 0.5f ? Tema.Vermelho
                : pacote.resultado.Probabilidade >= 0.25f ? Tema.Ambar : Tema.Verde;
            _probabilidade.Text = $"Probabilidade de comportamento suspeito: {pacote.resultado.Probabilidade:P1}";
            _confianca.Text = $"Confiança da previsão: {pacote.resultado.Confianca:P1}";
            _indicador.Value = Math.Clamp((int)Math.Round(pacote.resultado.Probabilidade * 100), 0, 100);

            var factores = pacote.resultado.Factores.ToList();
            factores.Add($"Operações do cliente na hora anterior: {pacote.transaccao.OperacoesUltimaHora}.");
            factores.Add($"Valor: {pacote.transaccao.RelacaoMedia:N1} vezes a média habitual.");
            _factores.Text = string.Join(Environment.NewLine, factores.Select(x => "• " + x));

            _estado.Text = pacote.id.HasValue
                ? $"Operação #{pacote.id.Value} guardada no histórico. A revisão continua pendente."
                : "Análise concluída. Não foi guardada no histórico.";
        }
        catch (Exception ex)
        {
            _estado.Text = "Não foi possível concluir a análise.";
            MessageBox.Show("Não foi possível analisar a operação.\n" + ex.Message,
                "Análise", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            UseWaitCursor = false;
            _analisar.Enabled = true;
            _normal.Enabled = true;
            _atencao.Enabled = true;
        }
    }
}