namespace GotaInteligente.Painel.Dominio;

/// <summary>Mesma escala calculada pela API: quanto maior, pior a situação do solo.</summary>
public enum NivelEstresse
{
    SemDados = 0,
    Nenhum = 1,
    Leve = 2,
    Moderado = 3,
    Severo = 4
}

public static class NivelEstresseExtensoes
{
    public static string Descricao(this NivelEstresse nivel) => nivel switch
    {
        NivelEstresse.Nenhum => "Sem estresse",
        NivelEstresse.Leve => "Leve",
        NivelEstresse.Moderado => "Moderado",
        NivelEstresse.Severo => "Severo",
        _ => "Sem dados"
    };

    /// <summary>Peso usado no cálculo da pontuação de risco da fazenda.</summary>
    public static int Peso(this NivelEstresse nivel) => nivel switch
    {
        NivelEstresse.Severo => 10,
        NivelEstresse.Moderado => 5,
        NivelEstresse.Leve => 2,
        NivelEstresse.SemDados => 1,
        _ => 0
    };
}
