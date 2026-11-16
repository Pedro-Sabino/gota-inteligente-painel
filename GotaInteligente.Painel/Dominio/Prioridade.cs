namespace GotaInteligente.Painel.Dominio;

public enum Prioridade
{
    Baixa = 1,
    Media = 2,
    Alta = 3,
    Urgente = 4
}

public static class PrioridadeExtensoes
{
    public static string Descricao(this Prioridade prioridade) => prioridade switch
    {
        Prioridade.Urgente => "Urgente",
        Prioridade.Alta => "Alta",
        Prioridade.Media => "Média",
        _ => "Baixa"
    };
}
