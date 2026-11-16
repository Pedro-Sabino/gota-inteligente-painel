using GotaInteligente.Painel.Dominio;

namespace GotaInteligente.Painel.Servicos;

public enum OrigemDados
{
    Api,
    Cache,
    Simulado
}

public sealed record ResultadoCarga(
    IReadOnlyList<FazendaMonitorada> Fazendas,
    ResumoRegional Resumo,
    OrigemDados Origem,
    DateTime AtualizadoEm,
    string? Aviso);

/// <summary>
/// Orquestra a carga do painel: tenta a API; se ela estiver fora, usa o último cache salvo;
/// se nunca houve carga, usa dados simulados para a demonstração.
/// </summary>
public sealed class ServicoPainel
{
    private readonly ApiGotaInteligente _api;
    private readonly CachePainel _cache;

    public ServicoPainel(ApiGotaInteligente api, CachePainel cache)
    {
        _api = api;
        _cache = cache;
    }

    public async Task<ResultadoCarga> CarregarAsync()
    {
        try
        {
            var fazendas = await _api.ObterEstresseHidricoAsync();
            var resumo = await _api.ObterResumoAsync();

            // O painel da API só traz fazendas com talhões; as recém-cadastradas entram como "sem dados".
            var comTalhoes = fazendas.Select(f => f.FazendaId).ToHashSet();
            foreach (var f in await _api.ObterFazendasAsync())
            {
                if (!comTalhoes.Contains(f.Id))
                    fazendas.Add(new PainelFazendaDto { FazendaId = f.Id, FazendaNome = f.Nome, Municipio = f.Municipio });
            }
            var snapshot = new SnapshotPainel { GeradoEm = DateTime.Now, Fazendas = fazendas, Resumo = resumo };

            TentarSalvarCache(snapshot);
            return Montar(snapshot, OrigemDados.Api, null);
        }
        catch (ApiIndisponivelException ex)
        {
            var cache = _cache.Carregar();
            if (cache is not null)
                return Montar(cache, OrigemDados.Cache,
                    $"{ex.Message} Exibindo o último carregamento salvo ({cache.GeradoEm:dd/MM/yyyy HH:mm}).");

            return Montar(DadosSimulados.Gerar(), OrigemDados.Simulado,
                $"{ex.Message} Exibindo dados simulados para demonstração.");
        }
    }

    private void TentarSalvarCache(SnapshotPainel snapshot)
    {
        try { _cache.Salvar(snapshot); }
        catch (IOException) { /* sem cache não impede o uso do painel */ }
        catch (UnauthorizedAccessException) { }
    }

    private static ResultadoCarga Montar(SnapshotPainel snapshot, OrigemDados origem, string? aviso)
    {
        var fazendas = snapshot.Fazendas
            .Select(Converter)
            .OrderByDescending(f => f.Prioridade)
            .ThenByDescending(f => f.PontuacaoRisco)
            .ThenBy(f => f.Nome)
            .ToList();

        var resumo = ResumoRegional.Calcular(fazendas, snapshot.Resumo.IrrigacoesEvitadasUltimos30Dias);
        return new ResultadoCarga(fazendas, resumo, origem, snapshot.GeradoEm, aviso);
    }

    private static FazendaMonitorada Converter(PainelFazendaDto dto) =>
        new(dto.FazendaId, dto.FazendaNome, dto.Municipio,
            dto.Talhoes.Select(t => new TalhaoMonitorado(t.TalhaoId, t.Nome, t.Cultura, t.UltimaUmidade, LerNivel(t.Nivel))));

    private static NivelEstresse LerNivel(string? texto) =>
        Enum.TryParse<NivelEstresse>(texto, ignoreCase: true, out var nivel) ? nivel : NivelEstresse.SemDados;
}
