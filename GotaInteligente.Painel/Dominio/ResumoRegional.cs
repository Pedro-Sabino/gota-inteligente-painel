namespace GotaInteligente.Painel.Dominio;

/// <summary>Indicadores agregados da região exibidos nos cards do painel.</summary>
public sealed class ResumoRegional
{
    public int TotalFazendas { get; }
    public int TotalTalhoes { get; }
    public int TalhoesEmEstresse { get; }
    public int TalhoesSeveros { get; }
    public int IrrigacoesEvitadas30Dias { get; }

    public ResumoRegional(int totalFazendas, int totalTalhoes, int talhoesEmEstresse, int talhoesSeveros, int irrigacoesEvitadas)
    {
        TotalFazendas = Math.Max(0, totalFazendas);
        TotalTalhoes = Math.Max(0, totalTalhoes);
        TalhoesEmEstresse = Math.Max(0, talhoesEmEstresse);
        TalhoesSeveros = Math.Max(0, talhoesSeveros);
        IrrigacoesEvitadas30Dias = Math.Max(0, irrigacoesEvitadas);
    }

    /// <summary>Monta o resumo a partir das fazendas carregadas (usado também no modo offline).</summary>
    public static ResumoRegional Calcular(IReadOnlyCollection<FazendaMonitorada> fazendas, int irrigacoesEvitadas)
    {
        var talhoes = fazendas.SelectMany(f => f.Talhoes).ToList();
        return new ResumoRegional(
            fazendas.Count,
            talhoes.Count,
            talhoes.Count(t => t.EmEstresse),
            talhoes.Count(t => t.Nivel == NivelEstresse.Severo),
            irrigacoesEvitadas);
    }

    public double PercentualEmEstresse => TotalTalhoes == 0 ? 0 : 100.0 * TalhoesEmEstresse / TotalTalhoes;
}
