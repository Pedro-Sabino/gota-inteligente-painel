namespace GotaInteligente.Painel.Servicos;

// Contratos JSON trocados com a API (mesmos nomes do backend SOA, em camelCase).

public sealed class PainelFazendaDto
{
    public int FazendaId { get; set; }
    public string FazendaNome { get; set; } = "";
    public string Municipio { get; set; } = "";
    public int TotalTalhoes { get; set; }
    public int TalhoesEmEstresse { get; set; }
    public string PiorNivel { get; set; } = "SemDados";
    public List<PainelTalhaoDto> Talhoes { get; set; } = new();
}

public sealed class PainelTalhaoDto
{
    public int TalhaoId { get; set; }
    public string Nome { get; set; } = "";
    public string Cultura { get; set; } = "";
    public double? UltimaUmidade { get; set; }
    public string Nivel { get; set; } = "SemDados";
}

public sealed class PainelResumoDto
{
    public int TotalFazendas { get; set; }
    public int TotalTalhoes { get; set; }
    public Dictionary<string, int> TalhoesPorNivel { get; set; } = new();
    public int IrrigacoesEvitadasUltimos30Dias { get; set; }
}

public sealed class FazendaRequestDto
{
    public string Nome { get; set; } = "";
    public string Proprietario { get; set; } = "";
    public string Municipio { get; set; } = "";
    public string Estado { get; set; } = "";
    public double AreaHectares { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

public sealed class FazendaResponseDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string Municipio { get; set; } = "";
}

/// <summary>Foto do painel salva em disco para o modo offline.</summary>
public sealed class SnapshotPainel
{
    public DateTime GeradoEm { get; set; }
    public List<PainelFazendaDto> Fazendas { get; set; } = new();
    public PainelResumoDto Resumo { get; set; } = new();
}
