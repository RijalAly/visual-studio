using System.Globalization;
using Sentinela.Dados;
using Sentinela.Ecras;
using Sentinela.Inteligencia;
using Sentinela.Util;

namespace Sentinela;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        var cultura = CultureInfo.GetCultureInfo("pt-PT");
        CultureInfo.DefaultThreadCurrentCulture = cultura;
        CultureInfo.DefaultThreadCurrentUICulture = cultura;

        ApplicationConfiguration.Initialize();
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) =>
            MessageBox.Show($"Ocorreu um erro inesperado.\n{e.Exception.Message}", "Sentinela",
                MessageBoxButtons.OK, MessageBoxIcon.Error);

        try
        {
            var configuracao = ConfiguracaoApp.Carregar();
            while (!ConexaoBD.Testar(configuracao, criarBase: true))
            {
                using var janela = new FormularioLigacao(configuracao);
                if (janela.ShowDialog() != DialogResult.OK)
                    return;
                configuracao = janela.Configuracao;
                ConfiguracaoApp.Guardar(configuracao);
            }

            BaseDados.Inicializar(configuracao);
            var servicoIA = new ServicoIA(configuracao);
            Application.Run(new FormularioLogin(configuracao, servicoIA));
        }
        catch (Exception ex)
        {
            MessageBox.Show("Não foi possível iniciar o Sentinela.\n\n" + ex.Message,
                "Erro de arranque", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}