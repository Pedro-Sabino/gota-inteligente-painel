namespace GotaInteligente.Painel.Servicos;

public static class ConfiguracaoApp
{
    /// <summary>Endereço da API Gota Inteligente (SOA). Pode ser trocado pela variável de ambiente GOTA_API_URL.</summary>
    public static string UrlApi =>
        Environment.GetEnvironmentVariable("GOTA_API_URL") is { Length: > 0 } url ? url : "http://localhost:5080";

    /// <summary>Pasta onde o painel guarda o cache do último carregamento e os atendimentos.</summary>
    public static string PastaDados
    {
        get
        {
            var pasta = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "GotaInteligente", "Painel");
            Directory.CreateDirectory(pasta);
            return pasta;
        }
    }
}
