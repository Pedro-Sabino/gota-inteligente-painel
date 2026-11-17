namespace GotaInteligente.Painel.Dominio;

public enum TipoAtendimento
{
    VisitaTecnica = 1,
    CaminhaoPipa = 2,
    OrientacaoRemota = 3,
    MonitoramentoSemanal = 4
}

public enum StatusAtendimento
{
    Agendado = 1,
    Concluido = 2,
    Cancelado = 3
}

public static class AtendimentoExtensoes
{
    public static string Descricao(this TipoAtendimento tipo) => tipo switch
    {
        TipoAtendimento.VisitaTecnica => "Visita técnica",
        TipoAtendimento.CaminhaoPipa => "Envio de caminhão-pipa",
        TipoAtendimento.OrientacaoRemota => "Orientação remota",
        _ => "Monitoramento semanal"
    };

    public static string Descricao(this StatusAtendimento status) => status switch
    {
        StatusAtendimento.Concluido => "Concluído",
        StatusAtendimento.Cancelado => "Cancelado",
        _ => "Agendado"
    };
}

/// <summary>
/// Ação do Sindicato Rural / Prefeitura junto a uma fazenda em estresse hídrico.
/// Encapsula as regras de criação e de mudança de status.
/// </summary>
public sealed class AtendimentoTecnico
{
    public const int TamanhoMaximoObservacao = 300;

    public Guid Id { get; }
    public int FazendaId { get; }
    public string FazendaNome { get; }
    public TipoAtendimento Tipo { get; }
    public DateTime DataPrevista { get; }
    public string Responsavel { get; }
    public string Observacao { get; }
    public Prioridade PrioridadeNaAbertura { get; }
    public StatusAtendimento Status { get; private set; }
    public DateTime CriadoEm { get; }
    public DateTime? FinalizadoEm { get; private set; }

    private AtendimentoTecnico(Guid id, int fazendaId, string fazendaNome, TipoAtendimento tipo, DateTime dataPrevista,
        string responsavel, string observacao, Prioridade prioridade, StatusAtendimento status, DateTime criadoEm, DateTime? finalizadoEm)
    {
        Id = id;
        FazendaId = fazendaId;
        FazendaNome = fazendaNome;
        Tipo = tipo;
        DataPrevista = dataPrevista.Date;
        Responsavel = responsavel;
        Observacao = observacao;
        PrioridadeNaAbertura = prioridade;
        Status = status;
        CriadoEm = criadoEm;
        FinalizadoEm = finalizadoEm;
    }

    /// <summary>Cria um novo atendimento validando os dados. Lança <see cref="ValidacaoException"/> se algo estiver errado.</summary>
    public static AtendimentoTecnico Agendar(FazendaMonitorada? fazenda, TipoAtendimento tipo, DateTime dataPrevista,
        string? responsavel, string? observacao, DateTime hoje)
    {
        var erros = new Dictionary<string, string>();

        if (fazenda is null)
            erros["Fazenda"] = "Selecione a fazenda atendida.";
        if (!Enum.IsDefined(tipo))
            erros["Tipo"] = "Selecione o tipo de atendimento.";
        if (dataPrevista.Date < hoje.Date)
            erros["Data"] = "A data prevista não pode estar no passado.";
        if (dataPrevista.Date > hoje.Date.AddDays(90))
            erros["Data"] = "Agende com no máximo 90 dias de antecedência.";

        var nomeResponsavel = (responsavel ?? string.Empty).Trim();
        if (nomeResponsavel.Length < 3)
            erros["Responsavel"] = "Informe o nome do responsável (mínimo 3 letras).";

        var obs = (observacao ?? string.Empty).Trim();
        if (obs.Length > TamanhoMaximoObservacao)
            erros["Observacao"] = $"A observação pode ter no máximo {TamanhoMaximoObservacao} caracteres.";

        if (erros.Count > 0) throw new ValidacaoException(erros);

        return new AtendimentoTecnico(Guid.NewGuid(), fazenda!.Id, fazenda.Nome, tipo, dataPrevista, nomeResponsavel, obs,
            fazenda.Prioridade, StatusAtendimento.Agendado, DateTime.Now, null);
    }

    /// <summary>Recria um atendimento já salvo em disco (sem revalidar a data, que pode ter ficado no passado).</summary>
    public static AtendimentoTecnico Reidratar(Guid id, int fazendaId, string fazendaNome, TipoAtendimento tipo, DateTime dataPrevista,
        string responsavel, string observacao, Prioridade prioridade, StatusAtendimento status, DateTime criadoEm, DateTime? finalizadoEm)
        => new(id, fazendaId, fazendaNome, tipo, dataPrevista, responsavel, observacao, prioridade, status, criadoEm, finalizadoEm);

    public bool Atrasado(DateTime hoje) => Status == StatusAtendimento.Agendado && DataPrevista < hoje.Date;

    public void Concluir()
    {
        if (Status != StatusAtendimento.Agendado)
            throw new InvalidOperationException("Só é possível concluir um atendimento agendado.");
        Status = StatusAtendimento.Concluido;
        FinalizadoEm = DateTime.Now;
    }

    public void Cancelar()
    {
        if (Status != StatusAtendimento.Agendado)
            throw new InvalidOperationException("Só é possível cancelar um atendimento agendado.");
        Status = StatusAtendimento.Cancelado;
        FinalizadoEm = DateTime.Now;
    }
}
