namespace GotaInteligente.Painel.Dominio;

/// <summary>Sugestão automática do que fazer por uma fazenda, a partir do seu pior nível de estresse.</summary>
public sealed record RecomendacaoAtendimento(TipoAtendimento Tipo, int PrazoDias, string Justificativa)
{
    public static RecomendacaoAtendimento Para(FazendaMonitorada fazenda) => fazenda.PiorNivel switch
    {
        NivelEstresse.Severo => new(TipoAtendimento.VisitaTecnica, 2,
            $"Estresse severo em {fazenda.TalhoesEmEstresse} talhão(ões): visita técnica em até 48h e avaliar envio de caminhão-pipa."),
        NivelEstresse.Moderado => new(TipoAtendimento.VisitaTecnica, 7,
            "Estresse moderado: visita técnica na semana para revisar a irrigação."),
        NivelEstresse.Leve => new(TipoAtendimento.OrientacaoRemota, 7,
            "Estresse leve: orientação remota ao agricultor sobre horários de irrigação."),
        NivelEstresse.SemDados => new(TipoAtendimento.VisitaTecnica, 14,
            "Sem leituras de umidade: instalar ou verificar o sensor ESP32 em campo."),
        _ => new(TipoAtendimento.MonitoramentoSemanal, 14,
            "Solo dentro do ideal: manter o monitoramento semanal.")
    };
}
