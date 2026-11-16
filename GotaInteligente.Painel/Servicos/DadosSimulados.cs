namespace GotaInteligente.Painel.Servicos;

/// <summary>Dados 100% simulados (sem dados pessoais reais) para demonstrar o painel sem a API.</summary>
public static class DadosSimulados
{
    public static SnapshotPainel Gerar() => new()
    {
        GeradoEm = DateTime.Now,
        Fazendas =
        {
            Fazenda(2, "Chácara Água Viva", "São Paulo", Talhao(3, "Canteiro Principal", "Couve", 16, "Severo")),
            Fazenda(1, "Sítio Boa Esperança", "São Paulo",
                Talhao(1, "Horta Norte", "Alface", 31.5, "Moderado"),
                Talhao(2, "Horta Sul", "Tomate", 49, "Nenhum")),
            Fazenda(4, "Sítio Recanto Verde", "Embu-Guaçu",
                Talhao(5, "Estufa 1", "Pimentão", 33, "Leve"),
                Talhao(6, "Estufa 2", "Pepino", 41, "Nenhum")),
            Fazenda(5, "Horta Comunitária Colônia", "São Paulo", Talhao(7, "Canteiros", "Rúcula", 47, "Nenhum"))
        },
        Resumo = new PainelResumoDto { IrrigacoesEvitadasUltimos30Dias = 1 }
    };

    private static PainelFazendaDto Fazenda(int id, string nome, string municipio, params PainelTalhaoDto[] talhoes) => new()
    {
        FazendaId = id,
        FazendaNome = nome,
        Municipio = municipio,
        Talhoes = talhoes.ToList()
    };

    private static PainelTalhaoDto Talhao(int id, string nome, string cultura, double umidade, string nivel) => new()
    {
        TalhaoId = id,
        Nome = nome,
        Cultura = cultura,
        UltimaUmidade = umidade,
        Nivel = nivel
    };
}
