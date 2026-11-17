using System.Text.Json;

namespace GotaInteligente.Painel.Servicos;

/// <summary>Persistência básica do último painel carregado, para o modo offline.</summary>
public sealed class CachePainel
{
    private readonly string _arquivo;

    public CachePainel(string pasta)
    {
        _arquivo = Path.Combine(pasta, "cache-painel.json");
    }

    public void Salvar(SnapshotPainel snapshot)
    {
        var json = JsonSerializer.Serialize(snapshot, ApiGotaInteligente.Json);
        File.WriteAllText(_arquivo, json);
    }

    public SnapshotPainel? Carregar()
    {
        if (!File.Exists(_arquivo)) return null;
        try
        {
            return JsonSerializer.Deserialize<SnapshotPainel>(File.ReadAllText(_arquivo), ApiGotaInteligente.Json);
        }
        catch (JsonException)
        {
            return null; // arquivo corrompido: ignora e segue com dados simulados
        }
    }
}
