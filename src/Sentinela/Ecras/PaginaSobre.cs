using Sentinela.Controlos;

namespace Sentinela.Ecras;

public sealed class PaginaSobre : UserControl
{
    public PaginaSobre()
    {
        BackColor = Tema.Fundo;

        var cartao = Tema.CriarCartao();
        cartao.Dock = DockStyle.Top;
        cartao.AutoSize = true;

        cartao.Controls.Add(Tema.CriarTitulo("Sobre o Sentinela", 21));
        cartao.Controls[0].Location = new Point(18, 18);

        var texto = new Label
        {
            AutoSize = true,
            MaximumSize = new Size(900, 0),
            Location = new Point(18, 70),
            ForeColor = Tema.Texto,
            Font = new Font("Segoe UI", 10.5f),
            Text =
                "O Sentinela é um projecto académico para demonstrar a utilização de Inteligência Artificial " +
                "na identificação de comportamentos fora do padrão em transacções financeiras.\n\n" +
                "O programa utiliza C# com Windows Forms, MySQL e ML.NET. O modelo compara FastTree e " +
                "regressão logística, com base em dados preparados e divididos em conjuntos de treino e teste.\n\n" +
                "Os dados de demonstração são simulados. O modelo aprende os padrões criados para este projecto; " +
                "isso não demonstra que detectaria fraude real num banco.\n\n" +
                "A probabilidade apresentada vem do modelo treinado. Os factores de atenção são uma explicação " +
                "de leitura para o analista e não são regras usadas para decidir a classificação.\n\n" +
                "As revisões do analista (suspeita confirmada ou falso alerta) são aproveitadas no treino seguinte.\n\n" +
                "Aviso: o resultado é apenas um apoio à decisão. Uma transacção classificada como potencialmente " +
                "suspeita deve ser analisada por uma pessoa; o sistema não confirma a existência de fraude.\n\n" +
                "Instituição: Instituto Superior Dom Bosco\n" +
                "Curso: Administração de Sistemas e Redes Informáticas — 3.º ano\n" +
                "Módulo: Inteligência Artificial\n" +
                "Discente: Rijal Aly Abdula\n" +
                "Local: Maputo"
        };
        cartao.Controls.Add(texto);
        Controls.Add(cartao);
    }
}
