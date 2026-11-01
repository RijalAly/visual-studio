using Sentinela.Controlos;
using Sentinela.Dados;
using Sentinela.Inteligencia;
using Sentinela.Modelos;
using Sentinela.Util;

namespace Sentinela.Ecras;

public sealed class PaginaModeloIA : UserControl
{
    private readonly ConfiguracaoApp _configuracao;
    private readonly ServicoIA _servicoIA;
    private readonly TransaccaoDAO _transaccoes;
    private readonly TreinoDAO _treinos;
    private readonly GestaoDadosDAO _gestaoDados;
    private readonly NumericUpDown _clientes = new();
    private readonly NumericUpDown _quantidade = new();
    private readonly Button _gerar = Tema.CriarBotao("＋  Gerar dados", Tema.Verde);
    private readonly Button _treinar = Tema.CriarBotao("✧  Treinar / reaprender");
    private readonly Button _eliminar = Tema.CriarBotaoSecundario("Eliminar demonstração");
    private readonly Label _estado = new();
    private readonly Label _estadoModelo = new();
    private readonly TextBox _relatorio = new();
    private readonly DataGridView _historico = new();

    public PaginaModeloIA(ConfiguracaoApp configuracao, ServicoIA servicoIA)
    {
        _configuracao = configuracao;
        _servicoIA = servicoIA;
        _transaccoes = new TransaccaoDAO(configuracao);
        _treinos = new TreinoDAO(configuracao);
        _gestaoDados = new GestaoDadosDAO(configuracao);
        BackColor = Tema.Fundo;
        ConstruirInterface();
        Load += async (_, _) => await ActualizarHistoricoAsync();
    }

    private void ConstruirInterface()
    {
        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Tema.Fundo };
        var conteudo = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            RowCount = 4,
            BackColor = Color.Transparent,
            Padding = new Padding(4)
        };
        conteudo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48));
        conteudo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52));
        conteudo.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        conteudo.RowStyles.Add(new RowStyle(SizeType.Absolute, 210));
        conteudo.RowStyles.Add(new RowStyle(SizeType.Absolute, 300));
        conteudo.RowStyles.Add(new RowStyle(SizeType.Absolute, 260));

        var cabecalho = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        cabecalho.Controls.Add(Tema.CriarTitulo("Modelo de Inteligência Artificial", 20));
        cabecalho.Controls[0].Location = new Point(7, 0);
        cabecalho.Controls.Add(new Label
        {
            Text = "Prepare os dados, treine os classificadores e acompanhe o desempenho.",
            AutoSize = true,
            ForeColor = Tema.TextoSuave,
            Location = new Point(9, 34)
        });
        conteudo.Controls.Add(cabecalho, 0, 0);
        conteudo.SetColumnSpan(cabecalho, 2);

        var cartaoGeracao = CriarCartao();
        cartaoGeracao.Controls.Add(new Label
        {
            Text = "Dados de demonstração",
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 11),
            ForeColor = Tema.Texto,
            Location = new Point(18, 15)
        });
        AdicionarCampo(cartaoGeracao, "Número de clientes", _clientes, 55);
        AdicionarCampo(cartaoGeracao, "Número de operações", _quantidade, 103);
        _clientes.Minimum = 2;
        _clientes.Maximum = 1000;
        _clientes.Value = 40;
        _clientes.Width = 150;
        _quantidade.Minimum = 100;
        _quantidade.Maximum = 100000;
        _quantidade.Increment = 500;
        _quantidade.Value = 6000;
        _quantidade.Width = 150;

        _gerar.Location = new Point(18, 157);
        _gerar.Size = new Size(155, 38);
        _gerar.Click += async (_, _) => await GerarDadosAsync();
        _eliminar.Location = new Point(185, 157);
        _eliminar.Size = new Size(180, 38);
        _eliminar.Click += async (_, _) => await EliminarDadosAsync();
        cartaoGeracao.Controls.Add(_gerar);
        cartaoGeracao.Controls.Add(_eliminar);
        conteudo.Controls.Add(cartaoGeracao, 0, 1);

        var cartaoModelo = CriarCartao();
        cartaoModelo.Controls.Add(new Label
        {
            Text = "Estado do modelo",
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 11),
            ForeColor = Tema.Texto,
            Location = new Point(18, 15)
        });
        _estadoModelo.Location = new Point(18, 52);
        _estadoModelo.Size = new Size(440, 48);
        _estadoModelo.Font = new Font("Segoe UI Semibold", 12);
        _estadoModelo.ForeColor = _servicoIA.TemModelo ? Tema.Verde : Tema.Ambar;
        _estadoModelo.Text = _servicoIA.TemModelo
            ? "●  Modelo carregado e pronto para análise"
            : "○  Ainda não existe modelo treinado";
        cartaoModelo.Controls.Add(_estadoModelo);

        cartaoModelo.Controls.Add(new Label
        {
            Text = "O sistema treina e compara dois algoritmos: FastTree e regressão logística (SDCA). O modelo seleccionado é guardado no computador e carregado quando a aplicação inicia.",
            AutoSize = true,
            MaximumSize = new Size(445, 0),
            ForeColor = Tema.TextoSuave,
            Location = new Point(18, 112)
        });
        _treinar.Location = new Point(18, 157);
        _treinar.Size = new Size(220, 38);
        _treinar.Click += async (_, _) => await TreinarAsync();
        cartaoModelo.Controls.Add(_treinar);
        conteudo.Controls.Add(cartaoModelo, 1, 1);

        var cartaoRelatorio = CriarCartao();
        cartaoRelatorio.Controls.Add(new Label
        {
            Text = "Relatório do último treino",
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 11),
            ForeColor = Tema.Texto,
            Location = new Point(18, 14)
        });
        _relatorio.Location = new Point(14, 47);
        _relatorio.Size = new Size(440, 225);
        _relatorio.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        _relatorio.Multiline = true;
        _relatorio.ReadOnly = true;
        _relatorio.ScrollBars = ScrollBars.Vertical;
        _relatorio.Font = new Font("Consolas", 9.2f);
        _relatorio.BackColor = Color.FromArgb(249, 250, 253);
        _relatorio.BorderStyle = BorderStyle.FixedSingle;
        cartaoRelatorio.Controls.Add(_relatorio);
        cartaoRelatorio.Resize += (_, _) => _relatorio.Width = cartaoRelatorio.ClientSize.Width - 28;
        conteudo.Controls.Add(cartaoRelatorio, 0, 2);

        var cartaoMetricas = CriarCartao();
        cartaoMetricas.Controls.Add(new Label
        {
            Text = "Como interpretar os resultados",
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 11),
            ForeColor = Tema.Texto,
            Location = new Point(18, 14)
        });
        var explicacao = new Label
        {
            AutoSize = true,
            MaximumSize = new Size(455, 0),
            ForeColor = Tema.TextoSuave,
            Font = new Font("Segoe UI", 9),
            Location = new Point(18, 48),
            Text =
                "Exactidão — percentagem de previsões correctas no total.\r\n\r\n" +
                "Precisão — entre os alertas previstos, quantos correspondiam a suspeitas etiquetadas.\r\n\r\n" +
                "Recall — entre os casos suspeitos do teste, quantos foram identificados.\r\n\r\n" +
                "F1 — equilíbrio entre precisão e recall. AUC — capacidade de separar as duas classes.\r\n\r\n" +
                "VP / FP / VN / FN — verdadeiros positivos, falsos positivos, verdadeiros negativos e falsos negativos."
        };
        cartaoMetricas.Controls.Add(explicacao);
        conteudo.Controls.Add(cartaoMetricas, 1, 2);

        var cartaoHistorico = CriarCartao();
        cartaoHistorico.Controls.Add(new Label
        {
            Text = "Comparação e histórico dos treinos",
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 11),
            ForeColor = Tema.Texto,
            Location = new Point(18, 14)
        });
        Tema.EstilizarGrelha(_historico);
        _historico.Location = new Point(12, 46);
        _historico.Size = new Size(900, 195);
        _historico.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        _historico.CellFormatting += FormatarHistorico;
        cartaoHistorico.Controls.Add(_historico);
        cartaoHistorico.Resize += (_, _) => _historico.Width = cartaoHistorico.ClientSize.Width - 24;
        conteudo.Controls.Add(cartaoHistorico, 0, 3);
        conteudo.SetColumnSpan(cartaoHistorico, 2);

        _estado.Text = "Os resultados dependem das etiquetas e dos padrões dos dados de demonstração.";
        _estado.Dock = DockStyle.Bottom;
        _estado.AutoSize = true;
        _estado.ForeColor = Tema.TextoSuave;

        scroll.Controls.Add(conteudo);
        Controls.Add(scroll);
        Controls.Add(_estado);
    }

    private static Panel CriarCartao()
        => new()
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Margin = new Padding(6),
            Padding = new Padding(12),
            BorderStyle = BorderStyle.FixedSingle
        };

    private static void AdicionarCampo(Control pai, string texto, Control campo, int topo)
    {
        pai.Controls.Add(new Label
        {
            Text = texto,
            AutoSize = true,
            ForeColor = Tema.TextoSuave,
            Location = new Point(18, topo)
        });
        campo.Location = new Point(210, topo - 6);
        campo.Height = 30;
        Tema.EstilizarCampo(campo);
        pai.Controls.Add(campo);
    }

    private async Task GerarDadosAsync()
    {
        ActivarControlos(false);
        _estado.Text = "A gerar clientes e operações fictícias...";
        try
        {
            int clientes = (int)_clientes.Value;
            int quantidade = (int)_quantidade.Value;
            await Task.Run(() => new GeradorDados(_configuracao).Gerar(clientes, quantidade));
            _estado.Text = $"Foram criados {clientes:N0} clientes e {quantidade:N0} operações fictícias.";
            MessageBox.Show("Os dados de demonstração foram guardados na base de dados.",
                "Geração concluída", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            _estado.Text = "A geração não foi concluída.";
            MessageBox.Show(ex.Message, "Geração de dados",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            ActivarControlos(true);
        }
    }

    private async Task EliminarDadosAsync()
    {
        var resposta = MessageBox.Show(
            "Serão eliminados os clientes de demonstração e as operações ligadas a esses clientes. Os registos manuais serão preservados. Pretende continuar?",
            "Eliminar dados de demonstração", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (resposta != DialogResult.Yes) return;

        ActivarControlos(false);
        _estado.Text = "A eliminar os dados de demonstração...";
        try
        {
            await Task.Run(_gestaoDados.EliminarDadosSimulados);
            _estado.Text = "Os dados de demonstração foram eliminados.";
            _relatorio.Clear();
            await ActualizarHistoricoAsync();
        }
        catch (Exception ex)
        {
            _estado.Text = "A eliminação não foi concluída.";
            MessageBox.Show(ex.Message, "Dados de demonstração",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            ActivarControlos(true);
        }
    }

    private async Task TreinarAsync()
    {
        ActivarControlos(false);
        _relatorio.Clear();
        _estado.Text = "A preparar os dados para treino...";
        try
        {
            IProgress<string> progresso = new Progress<string>(mensagem => _estado.Text = mensagem);
            var pacote = await Task.Run(() =>
            {
                progresso.Report("A consultar operações etiquetadas...");
                var brutos = _transaccoes.ObterDadosTreino();
                progresso.Report("A validar registos e a remover duplicados...");
                var preparacao = new PreparadorDados().Preparar(brutos);
                int revisoes = _transaccoes.ListarPorRevisao("Confirmada").Count +
                               _transaccoes.ListarPorRevisao("Descartada").Count;

                if (preparacao.Dados.Count < 100)
                    throw new InvalidOperationException("Existem menos de 100 registos válidos. Gere mais dados de demonstração ou reveja alertas antes de treinar.");

                var resultado = _servicoIA.Treinar(preparacao.Dados, revisoes, progresso.Report);
                progresso.Report("A actualizar a classificação das operações...");
                _transaccoes.ClassificarTodos(_servicoIA.Analisar);
                return (preparacao, resultado);
            });

            var p = pacote.preparacao;
            var r = pacote.resultado;
            _estado.Text = "Treino concluído. O modelo seleccionado está disponível para análise.";
            _estadoModelo.Text = "●  Modelo treinado: " + r.Algoritmo;
            _estadoModelo.ForeColor = Tema.Verde;
            _relatorio.Text =
                "PREPARAÇÃO DOS DADOS\r\n" +
                $"Recebidos: {p.Recebidos:N0}   •   Inválidos: {p.Invalidos:N0}   •   Repetidos: {p.Repetidos:N0}\r\n" +
                $"Utilizados: {p.Dados.Count:N0}   •   Normais: {p.Normais:N0}   •   Suspeitos: {p.Suspeitos:N0}\r\n\r\n" +
                "MODELO SELECCIONADO\r\n" +
                $"Algoritmo: {r.Algoritmo}\r\n" +
                $"Exactidão: {r.Exactidao:P2}    Precisão: {r.Precisao:P2}\r\n" +
                $"Recall: {r.Recall:P2}        F1: {r.F1:P2}        AUC: {r.Auc:P2}\r\n" +
                $"Matriz: VP={r.VP}  FP={r.FP}  VN={r.VN}  FN={r.FN}\r\n" +
                $"Revisões incluídas: {r.Revisoes:N0}    Tempo: {r.DuracaoSegundos:N1} s\r\n\r\n" +
                $"Alternativa: {r.AlgoritmoAlternativo}\r\n\r\n" +
                "As métricas são calculadas no conjunto de teste e não demonstram desempenho com operações bancárias reais.";
            await ActualizarHistoricoAsync();
        }
        catch (Exception ex)
        {
            _estado.Text = "O treino não foi concluído.";
            _relatorio.Text = ex.Message;
            MessageBox.Show(ex.Message, "Treino do modelo",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            ActivarControlos(true);
        }
    }

    private async Task ActualizarHistoricoAsync()
    {
        try
        {
            var historico = await Task.Run(() => _treinos.ListarRecentes(12));
            _historico.DataSource = historico.Select(r => new LinhaTreino
            {
                Data = r.DataHora.ToString("dd/MM HH:mm"),
                Algoritmo = r.Algoritmo,
                Escolhido = r.Escolhido ? "Modelo utilizado" : "Alternativa",
                Exactidao = r.Exactidao,
                Precisao = r.Precisao,
                Recall = r.Recall,
                F1 = r.F1,
                AUC = r.Auc
            }).ToList();

            foreach (string coluna in new[] { "Exactidao", "Precisao", "Recall", "F1", "AUC" })
                if (_historico.Columns[coluna] is not null)
                    _historico.Columns[coluna].DefaultCellStyle.Format = "P1";

            _estadoModelo.Text = _servicoIA.TemModelo
                ? "●  Existe um modelo treinado e carregado"
                : "○  Ainda não existe um modelo treinado";
            _estadoModelo.ForeColor = _servicoIA.TemModelo ? Tema.Verde : Tema.Ambar;
        }
        catch (Exception ex)
        {
            _estado.Text = "Não foi possível carregar o histórico: " + ex.Message;
        }
    }

    private void FormatarHistorico(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || e.CellStyle is null || e.Value is null) return;
        if (_historico.Columns[e.ColumnIndex].Name == "Escolhido" &&
            e.Value.ToString() == "Modelo utilizado")
        {
            e.CellStyle.ForeColor = Tema.Verde;
            e.CellStyle.Font = new Font("Segoe UI Semibold", 8.5f);
        }
    }

    private void ActivarControlos(bool activo)
    {
        _gerar.Enabled = activo;
        _treinar.Enabled = activo;
        _eliminar.Enabled = activo;
        _clientes.Enabled = activo;
        _quantidade.Enabled = activo;
        UseWaitCursor = !activo;
    }

    private sealed class LinhaTreino
    {
        public string Data { get; set; } = "";
        public string Algoritmo { get; set; } = "";
        public string Escolhido { get; set; } = "";
        public double Exactidao { get; set; }
        public double Precisao { get; set; }
        public double Recall { get; set; }
        public double F1 { get; set; }
        public double AUC { get; set; }
    }
}