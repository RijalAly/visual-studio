using Sentinela.Controlos;
using Sentinela.Dados;
using Sentinela.Modelos;
using Sentinela.Util;

namespace Sentinela.Ecras;

public sealed class PaginaPainel : UserControl
{
    private readonly TransaccaoDAO _dao;
    private readonly Label[] _indicadores = new Label[4];
    private readonly GraficoActividade _grafico = new();
    private readonly DataGridView _grelha = new();

    public PaginaPainel(ConfiguracaoApp configuracao)
    {
        _dao = new TransaccaoDAO(configuracao);
        BackColor = Tema.Fundo;
        ConstruirInterface();
        ActualizarDados();
    }

    private void ConstruirInterface()
    {
        var conteudo = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 3,
            BackColor = Color.Transparent,
            Padding = new Padding(0)
        };
        for (int coluna = 0; coluna < 4; coluna++)
            conteudo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        conteudo.RowStyles.Add(new RowStyle(SizeType.Absolute, 112));
        conteudo.RowStyles.Add(new RowStyle(SizeType.Percent, 58));
        conteudo.RowStyles.Add(new RowStyle(SizeType.Percent, 42));

        string[] nomes = { "Transacções analisadas", "Potencialmente suspeitas", "Taxa de suspeita", "Alertas por rever" };
        for (int i = 0; i < nomes.Length; i++)
        {
            var cartao = Tema.CriarCartao();
            var legenda = new Label
            {
                Text = nomes[i],
                AutoSize = true,
                ForeColor = Tema.TextoSuave,
                Location = new Point(16, 12)
            };
            _indicadores[i] = new Label
            {
                Text = "—",
                AutoSize = true,
                ForeColor = Tema.Texto,
                Font = new Font("Segoe UI Semibold", 23),
                Location = new Point(16, 38)
            };
            cartao.Controls.Add(legenda);
            cartao.Controls.Add(_indicadores[i]);
            conteudo.Controls.Add(cartao, i, 0);
        }

        var cartaoGrafico = Tema.CriarCartao();
        cartaoGrafico.Controls.Add(new Label
        {
            Text = "Movimento dos últimos 14 dias",
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 11),
            ForeColor = Tema.Texto,
            Dock = DockStyle.Top
        });
        _grafico.Dock = DockStyle.Fill;
        cartaoGrafico.Controls.Add(_grafico);
        conteudo.Controls.Add(cartaoGrafico, 0, 1);
        conteudo.SetColumnSpan(cartaoGrafico, 4);

        var cartaoTabela = Tema.CriarCartao();
        cartaoTabela.Controls.Add(new Label
        {
            Text = "Alertas mais recentes",
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 11),
            ForeColor = Tema.Texto,
            Dock = DockStyle.Top
        });

        _grelha.Dock = DockStyle.Fill;
        _grelha.ReadOnly = true;
        _grelha.AllowUserToAddRows = false;
        _grelha.AllowUserToDeleteRows = false;
        _grelha.RowHeadersVisible = false;
        _grelha.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grelha.BackgroundColor = Color.White;
        _grelha.BorderStyle = BorderStyle.None;
        _grelha.EnableHeadersVisualStyles = false;
        _grelha.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(235, 239, 245);
        _grelha.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9);
        cartaoTabela.Controls.Add(_grelha);
        conteudo.Controls.Add(cartaoTabela, 0, 2);
        conteudo.SetColumnSpan(cartaoTabela, 4);

        var actualizar = Tema.CriarBotao("Actualizar painel", Color.FromArgb(90, 105, 125));
        actualizar.Dock = DockStyle.Bottom;
        actualizar.Click += (_, _) => ActualizarDados();
        Controls.Add(conteudo);
        Controls.Add(actualizar);
    }

    private void ActualizarDados()
    {
        try
        {
            var dados = _dao.Listar(10000);
            int total = dados.Count;
            int suspeitas = dados.Count(t => t.Classificacao == "Transacção potencialmente suspeita");
            int pendentes = dados.Count(t => t.Revisao == "Pendente" &&
                t.Classificacao == "Transacção potencialmente suspeita");

            _indicadores[0].Text = total.ToString("N0");
            _indicadores[1].Text = suspeitas.ToString("N0");
            _indicadores[2].Text = total == 0 ? "0,0%" : (suspeitas * 100.0 / total).ToString("N1") + "%";
            _indicadores[3].Text = pendentes.ToString("N0");

            DateTime hoje = DateTime.Today;
            var dias = Enumerable.Range(0, 14).Select(offset => hoje.AddDays(offset - 13)).ToList();
            var contagens = dias.Select(d => dados.Count(t => t.DataHora.Date == d.Date)).ToList();
            _grafico.DefinirDados(dias, contagens);

            _grelha.DataSource = dados
                .Where(t => t.Classificacao == "Transacção potencialmente suspeita")
                .OrderByDescending(t => t.DataHora)
                .Take(8)
                .Select(t => new
                {
                    t.DataHora,
                    t.NomeCliente,
                    t.Valor,
                    t.Cidade,
                    t.Classificacao,
                    Revisao = t.Revisao
                }).ToList();

            if (_grelha.Columns["Valor"] is not null)
                _grelha.Columns["Valor"].DefaultCellStyle.Format = "N2";
        }
        catch (Exception ex)
        {
            MessageBox.Show("Não foi possível actualizar o painel.\n" + ex.Message,
                "Painel", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private sealed class GraficoActividade : Control
    {
        private List<DateTime> _dias = new();
        private List<int> _valores = new();

        public GraficoActividade()
        {
            DoubleBuffered = true;
            BackColor = Color.White;
            MinimumSize = new Size(300, 150);
        }

        public void DefinirDados(List<DateTime> dias, List<int> valores)
        {
            _dias = dias;
            _valores = valores;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_valores.Count == 0) return;

            int maximo = Math.Max(1, _valores.Max());
            int margemEsquerda = 42;
            int margemInferior = 28;
            int larguraDisponivel = Math.Max(1, ClientSize.Width - margemEsquerda - 12);
            int alturaDisponivel = Math.Max(1, ClientSize.Height - margemInferior - 18);
            float larguraBarra = Math.Max(4, larguraDisponivel / (float)_valores.Count * 0.55f);
            float espaco = larguraDisponivel / (float)_valores.Count;

            using var pincel = new SolidBrush(Tema.Azul);
            using var grelha = new Pen(Color.FromArgb(230, 234, 240));
            using var letra = new SolidBrush(Tema.TextoSuave);
            using var fonte = new Font("Segoe UI", 8);

            for (int nivel = 0; nivel <= 4; nivel++)
            {
                float y = 8 + alturaDisponivel - nivel * alturaDisponivel / 4f;
                e.Graphics.DrawLine(grelha, margemEsquerda, y, ClientSize.Width - 8, y);
            }

            for (int i = 0; i < _valores.Count; i++)
            {
                float altura = _valores[i] / (float)maximo * Math.Max(1, alturaDisponivel - 8);
                float x = margemEsquerda + i * espaco + (espaco - larguraBarra) / 2;
                float y = 8 + alturaDisponivel - altura;
                e.Graphics.FillRectangle(pincel, x, y, larguraBarra, altura);
                if (i % 2 == 0)
                    e.Graphics.DrawString(_dias[i].ToString("dd/MM"), fonte, letra, x - 6, ClientSize.Height - 22);
            }
        }
    }
}
