namespace GotaInteligente.Painel.Dominio;

/// <summary>
/// Propriedade do agricultor familiar vista pelo Sindicato Rural / Prefeitura.
/// Concentra as regras de decisão do painel: pior nível, pontuação de risco e prioridade.
/// </summary>
public sealed class FazendaMonitorada
{
    private readonly List<TalhaoMonitorado> _talhoes;

    public int Id { get; }
    public string Nome { get; }
    public string Municipio { get; }
    public IReadOnlyList<TalhaoMonitorado> Talhoes => _talhoes;

    public FazendaMonitorada(int id, string nome, string municipio, IEnumerable<TalhaoMonitorado> talhoes)
    {
        if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id), "O id da fazenda deve ser positivo.");
        if (string.IsNullOrWhiteSpace(nome)) throw new ArgumentException("A fazenda precisa de um nome.", nameof(nome));

        Id = id;
        Nome = nome.Trim();
        Municipio = string.IsNullOrWhiteSpace(municipio) ? "Não informado" : municipio.Trim();
        _talhoes = talhoes?.ToList() ?? new List<TalhaoMonitorado>();
    }

    public int TotalTalhoes => _talhoes.Count;

    public int TalhoesEmEstresse => _talhoes.Count(t => t.EmEstresse);

    public NivelEstresse PiorNivel =>
        _talhoes.Count == 0 ? NivelEstresse.SemDados : _talhoes.Max(t => t.Nivel);

    /// <summary>Soma dos pesos de cada talhão: diferencia fazendas com o mesmo pior nível.</summary>
    public int PontuacaoRisco => _talhoes.Sum(t => t.Nivel.Peso());

    /// <summary>Fluxo de decisão da prioridade de atendimento.</summary>
    public Prioridade Prioridade
    {
        get
        {
            if (PiorNivel == NivelEstresse.Severo) return Prioridade.Urgente;
            if (PiorNivel == NivelEstresse.Moderado) return Prioridade.Alta;
            if (PiorNivel == NivelEstresse.Leve || PiorNivel == NivelEstresse.SemDados) return Prioridade.Media;
            return Prioridade.Baixa;
        }
    }
}
