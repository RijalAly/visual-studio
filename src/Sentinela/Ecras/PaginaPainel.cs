using System.Drawing.Drawing2D;
using Sentinela.Controlos;
using Sentinela.Dados;
using Sentinela.Modelos;
using Sentinela.Util;

namespace Sentinela.Ecras;

public sealed class PaginaPainel : UserControl
{
    private readonly DashboardDAO _dao;
    private readonly Button _actualizar = Tema.CriarBotaoSecundario("↻  Actualizar");
    private readonly Label _estado = new();
    private readonly Label _ultimaActualizacao = new();
    private readonly DataGridView _grelha = new();
    private readonly GraficoActividade _grafico = new();
    private readonly CartaoMetrica[] _metricas = new CartaoMetrica[6];
    private readonly ProgressBar _pendente = new();
    private readonly ProgressBar _confirmada = new();
    private readonly ProgressBar _descartada = new();
    private readonly Label _textoPendente = new();
    private readonly Label _textoConfirmada = new();
    private readonly Label _textoDescartada = new();

    public event Action? AbrirTransaccoes;
    public event Action? IniciarAnalise;

    public PaginaPainel(ConfiguracaoApp configuracao)
    {
        _dao = new DashboardDAO(configuracao);
        BackColor = Tema.Fundo;
        ConstruirInterface();
        Load += async (_, _) => await ActualizarAsync();
    }

    private void ConstruirInterface()
    {
        var scroll = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = Tema.Fundo,
            Padding = new Padding(4)
        };

        var conteudo = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 3,
            RowCount = 5,
            BackColor = Color.Transparent,
            Padding = new Padding(0)
        };
        for (int i = 0; i < 3; i++)
            conteudo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333f));
        conteudo.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
        conteudo.RowStyles.Add(new RowStyle(SizeType.Absolute, 132));
        conteudo.RowStyles.Add(new RowStyle(SizeType.Absolute, 132));
        conteudo.RowStyles.Add(new RowStyle(SizeType.Absolute, 275));
        conteudo.RowStyles.Add(new RowStyle(SizeType.Absolute, 255));

        var cabecalho = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        var titulo = Tema.CriarTitulo("Visão geral", 20);
        titulo.Location = new Point(8, 0);
        cabecalho.Controls.Add(titulo);
        cabecalho.Controls.Add(new Label
        {
            Text = "Acompanhe a actividade e os alertas que precisam de atenção.",
            AutoSize = true,
            Location = new Point(10, 34),
            ForeColor = Tema.TextoSuave
        });

        _actualizar.Width = 125;
        _actualizar.Height = 36;
        _actualizar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _actualizar.Click += async (_, _) => await ActualizarAsync();
        cabecalho.Resize += (_, _) => _actualizar.Left = cabecalho.ClientSize.Width - _actualizar.Width - 8;
        cabecalho.Controls.Add(_actualizar);
        conteudo.Controls.Add(cabecalho, 0, 0);
        conteudo.SetColumnSpan(cabecalho, 3);

        _metricas[0] = new CartaoMetrica("↗", "Operações de hoje", "—", "Movimento desde a meia-noite", Tema.Azul);
        _metricas[1] = new CartaoMetrica("◎", "Total de operações", "—", "Registos no sistema", Tema.Roxo);
        _metricas[2] = new CartaoMetrica("!", "Alertas por rever", "—", "Exigem análise humana", Tema.Ambar);
        _metricas[3] = new CartaoMetrica("✓", "Suspeitas confirmadas", "—", "Após revisão do analista", Tema.Vermelho);
        _metricas[4] = new CartaoMetrica("⌂", "Clientes registados", "—", "Perfis na base de dados", Tema.Verde);
        _metricas[5] = new CartaoMetrica("MZN", "Volume de hoje", "—", "Valor total movimentado", Tema.Azul);

        for (int i = 0; i < _metricas.Length; i++)
        {
            _metricas[i].Dock = DockStyle.Fill;
            _metricas[i].Margin = new Padding(6, 5, 6, 5);
            conteudo.Controls.Add(_metricas[i], i % 3, 1 + i / 3);
        }

        var cartaoGrafico = CriarCartaoSecundario();
        cartaoGrafico.Controls.Add(new Label
        {
            Text = "Actividade dos últimos 30 dias",
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 11),
            ForeColor = Tema.Texto,
            Location = new Point(18, 14)
        });
        cartaoGrafico.Controls.Add(new Label
        {
            Text = "Número de operações registadas por dia",
            AutoSize = true,
            Font = new Font("Segoe UI", 8.5f),
            ForeColor = Tema.TextoSuave,
            Location = new Point(18, 38)
        });
        _grafico.Dock = DockStyle.Fill;
        _grafico.Margin = new Padding(10, 60, 10, 8);
        cartaoGrafico.Controls.Add(_grafico);
        conteudo.Controls.Add(cartaoGrafico, 0, 3);
        conteudo.SetColumnSpan(cartaoGrafico, 2);

        var cartaoEstados = CriarCartaoSecundario();
        cartaoEstados.Controls.Add(new Label
        {
            Text = "Estado das revisões",
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 11),
            ForeColor = Tema.Texto,
            Location = new Point(18, 14)
        });
        AdicionarEstado(cartaoEstados, "Por rever", _pendente, _textoPendente, Tema.Ambar, 58);
        AdicionarEstado(cartaoEstados, "Confirmadas", _confirmada, _textoConfirmada, Tema.Vermelho, 123);
        AdicionarEstado(cartaoEstados, "Descartadas", _descartada, _textoDescartada, Tema.Verde, 188);
        conteudo.Controls.Add(cartaoEstados, 2, 3);

        var cartaoAlertas = CriarCartaoSecundario();
        cartaoAlertas.Controls.Add(new Label
        {
            Text = "Actividade que merece atenção",
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 11),
            ForeColor = Tema.Texto,
            Location = new Point(18, 14)
        });

        var abrir = Tema.CriarBotaoSecundario("Ver todas as operações  →");
        abrir.Width = 190;
        abrir.Height = 34;
        abrir.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        abrir.Click += (_, _) => AbrirTransaccoes?.Invoke();
        cartaoAlertas.Resize += (_, _) => abrir.Left = cartaoAlertas.ClientSize.Width - abrir.Width - 10;
        cartaoAlertas.Controls.Add(abrir);

        Tema.EstilizarGrelha(_grelha);
        _grelha.Dock = DockStyle.Fill;
        _grelha.Margin = new Padding(8, 52, 8, 8);
        _grelha.CellFormatting += FormatarClassificacao;
        _grelha.CellDoubleClick += (_, _) => AbrirTransaccoes?.Invoke();
        cartaoAlertas.Controls.Add(_grelha);
        conteudo.Controls.Add(cartaoAlertas, 0, 4);
        conteudo.SetColumnSpan(cartaoAlertas, 3);

        _estado.AutoSize = true;
        _estado.ForeColor = Tema.TextoSuave;
        _estado.Dock = DockStyle.Bottom;
        _ultimaActualizacao.AutoSize = true;
        _ultimaActualizacao.ForeColor = Tema.TextoSuave;
        _ultimaActualizacao.Dock = DockStyle.Bottom;

        scroll.Controls.Add(conteudo);
        Controls.Add(scroll);
        Controls.Add(_ultimaActualizacao);
        Controls.Add(_estado);
    }

    private static Panel CriarCartaoSecundario()
        => new()
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Margin = new Padding(6, 5, 6, 5),
            Padding = new Padding(10),
            BorderStyle = BorderStyle.FixedSingle
        };

    private static void AdicionarEstado(Control pai, string nome, ProgressBar barra,
        Label texto, Color cor, int topo)
    {
        pai.Controls.Add(new Label
        {
            Text = nome,
            AutoSize = true,
            ForeColor = Tema.Texto,
            Location = new Point(18, topo)
        });
        texto.Text = "0";
        texto.AutoSize = true;
        texto.Font = new Font("Segoe UI Semibold", 9.5f);
        texto.ForeColor = cor;
        texto.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        texto.Location = new Point(210, topo);
        pai.Controls.Add(texto);

        barra.Location = new Point(18, topo + 24);
        barra.Size = new Size(230, 12);
        barra.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        barra.Minimum = 0;
        barra.Maximum = 100;
        barra.Style = ProgressBarStyle.Continuous;
        pai.Controls.Add(barra);
    }

    private async Task ActualizarAsync()
    {
        _actualizar.Enabled = false;
        _estado.Text = "A actualizar os dados do painel...";
        UseWaitCursor = true;

        try
        {
            DadosPainel dados = await Task.Run(_dao.ObterResumo);

            _metricas[0].Actualizar(dados.TransaccoesHoje.ToString("N0"));
            _metricas[1].Actualizar(dados.TotalTransaccoes.ToString("N0"));
            _metricas[2].Actualizar(dados.AlertasPendentes.ToString("N0"));
            _metricas[3].Actualizar(dados.SuspeitasConfirmadas.ToString("N0"));
            _metricas[4].Actualizar(dados.TotalClientes.ToString("N0"));
            _metricas[5].Actualizar(dados.VolumeHoje.ToString("N2") + " MZN");

            int totalRevisoes = dados.AlertasPendentes + dados.SuspeitasConfirmadas + dados.AlertasDescartados;
            ActualizarBarra(_pendente, _textoPendente, dados.AlertasPendentes, totalRevisoes);
            ActualizarBarra(_confirmada, _textoConfirmada, dados.SuspeitasConfirmadas, totalRevisoes);
            ActualizarBarra(_descartada, _textoDescartada, dados.AlertasDescartados, totalRevisoes);

            _grafico.DefinirDados(dados.Actividade);
            _grelha.DataSource = dados.AlertasRecentes.Select(t => new LinhaAlerta
            {
                Id = t.Id,
                Data = t.DataHora.ToString("dd/MM HH:mm"),
                Cliente = t.NomeCliente,
                Valor = t.Valor,
                Cidade = t.Cidade,
                Classificacao = t.Classificacao,
                Revisao = t.Revisao
            }).ToList();

            if (_grelha.Columns["Id"] is not null) _grelha.Columns["Id"].Visible = false;
            if (_grelha.Columns["Valor"] is not null)
                _grelha.Columns["Valor"].DefaultCellStyle.Format = "N2";

            _estado.Text = "Dados actualizados.";
            _ultimaActualizacao.Text = "Última actualização: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        }
        catch (Exception ex)
        {
            _estado.Text = "Não foi possível actualizar o painel. " + ex.Message;
        }
        finally
        {
            UseWaitCursor = false;
            _actualizar.Enabled = true;
        }
    }

    private static void ActualizarBarra(ProgressBar barra, Label texto, int valor, int total)
    {
        int percentagem = total == 0 ? 0 : (int)Math.Round(valor * 100.0 / total);
        barra.Value = Math.Clamp(percentagem, 0, 100);
        texto.Text = valor.ToString("N0") + "  (" + percentagem + "%)";
    }

    private void FormatarClassificacao(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || _grelha.Columns[e.ColumnIndex].Name != "Classificacao" || e.Value is null)
            return;

        string valor = e.Value.ToString() ?? "";
        if (valor.Contains("suspeita")) e.CellStyle.ForeColor = Tema.Vermelho;
        else if (valor == "Atenção") e.CellStyle.ForeColor = Tema.Ambar;
        e.CellStyle.Font = new Font("Segoe UI Semibold", 9f);
    }

    private sealed class LinhaAlerta
    {
        public long Id { get; set; }
        public string Data { get; set; } = "";
        public string Cliente { get; set; } = "";
        public decimal Valor { get; set; }
        public string Cidade { get; set; } = "";
        public string Classificacao { get; set; } = "";
        public string Revisao { get; set; } = "";
    }

    private sealed class GraficoActividade : Control
    {
        private List<AtividadePainel> _dados = new();

        public GraficoActividade()
        {
            DoubleBuffered = true;
            BackColor = Color.White;
            MinimumSize = new Size(280, 160);
        }

        public void DefinirDados(List<AtividadePainel> dados)
        {
            _dados = dados;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_dados.Count == 0)
            {
                using var fonteVazia = new Font("Segoe UI", 9);
                using var pincelVazio = new SolidBrush(Tema.TextoSuave);
                e.Graphics.DrawString("Ainda não existem operações para apresentar.", fonteVazia,
                    pincelVazio, new PointF(18, Height / 2f));
                return;
            }

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            int maximo = Math.Max(1, _dados.Max(x => x.Quantidade));
            const int esquerda = 38;
            const int topo = 12;
            const int baixo = 28;
            int largura = Math.Max(1, ClientSize.Width - esquerda - 14);
            int altura = Math.Max(1, ClientSize.Height - topo - baixo);
            float passo = largura / (float)_dados.Count;
            float larguraBarra = Math.Max(3, passo * 0.58f);

            using var pincel = new SolidBrush(Tema.Azul);
            using var pincelTexto = new SolidBrush(Tema.TextoSuave);
            using var caneta = new Pen(Tema.Borda, 1);
            using var fonte = new Font("Segoe UI", 7.5f);

            for (int nivel = 0; nivel <= 4; nivel++)
            {
                float y = topo + altura - nivel * altura / 4f;
                e.Graphics.DrawLine(caneta, esquerda, y, ClientSize.Width - 8, y);
            }

            for (int i = 0; i < _dados.Count; i++)
            {
                float alturaBarra = _dados[i].Quantidade / (float)maximo * (altura - 8);
                float x = esquerda + i * passo + (passo - larguraBarra) / 2;
                float y = topo + altura - alturaBarra;
                if (alturaBarra > 0)
                    e.Graphics.FillRoundedRectangle(pincel,
                        new Rectangle((int)x, (int)y, (int)larguraBarra, Math.Max(3, (int)alturaBarra)), 3);

                if (i % 5 == 0 || i == _dados.Count - 1)
                    e.Graphics.DrawString(_dados[i].Dia.ToString("dd/MM"), fonte, pincelTexto,
                        x - 8, ClientSize.Height - 22);
            }
        }
    }
}
