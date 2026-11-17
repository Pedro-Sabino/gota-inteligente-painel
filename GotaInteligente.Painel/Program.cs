using GotaInteligente.Painel.Servicos;
using GotaInteligente.Painel.Telas;

namespace GotaInteligente.Painel;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        var api = new ApiGotaInteligente(ConfiguracaoApp.UrlApi);
        var servicoPainel = new ServicoPainel(api, new CachePainel(ConfiguracaoApp.PastaDados));
        var atendimentos = new RepositorioAtendimentos(ConfiguracaoApp.PastaDados);

        Application.Run(new FormPrincipal(servicoPainel, api, atendimentos));
    }
}
