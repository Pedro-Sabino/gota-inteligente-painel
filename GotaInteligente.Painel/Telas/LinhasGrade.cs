using GotaInteligente.Painel.Dominio;

namespace GotaInteligente.Painel.Telas;

// Modelos de exibição das tabelas (o DataGridView lê as propriedades públicas).

internal sealed class LinhaFazenda
{
    public LinhaFazenda(FazendaMonitorada fazenda, bool temAtendimentoAgendado)
    {
        Fazenda = fazenda;
        Prioridade = fazenda.Prioridade.Descricao();
        Nome = fazenda.Nome;
        Municipio = fazenda.Municipio;
        Talhoes = fazenda.TotalTalhoes;
        EmEstresse = fazenda.TalhoesEmEstresse;
        PiorNivel = fazenda.PiorNivel.Descricao();
        Pontuacao = fazenda.PontuacaoRisco;
        Atendimento = temAtendimentoAgendado ? "Agendado" : (fazenda.Prioridade >= Dominio.Prioridade.Alta ? "Pendente" : "-");
    }

    public FazendaMonitorada Fazenda { get; }
    public string Prioridade { get; }
    public string Nome { get; }
    public string Municipio { get; }
    public int Talhoes { get; }
    public int EmEstresse { get; }
    public string PiorNivel { get; }
    public int Pontuacao { get; }
    public string Atendimento { get; }
}

internal sealed class LinhaTalhao
{
    public LinhaTalhao(TalhaoMonitorado talhao)
    {
        Nivel = talhao.Nivel;
        Nome = talhao.Nome;
        Cultura = talhao.Cultura;
        Umidade = talhao.UltimaUmidade is { } u ? $"{u:0.0}%" : "sem leitura";
        Situacao = talhao.Nivel.Descricao();
    }

    public NivelEstresse Nivel { get; }
    public string Nome { get; }
    public string Cultura { get; }
    public string Umidade { get; }
    public string Situacao { get; }
}

internal sealed class LinhaAtendimento
{
    public LinhaAtendimento(AtendimentoTecnico a, DateTime hoje)
    {
        Atendimento = a;
        Fazenda = a.FazendaNome;
        Tipo = a.Tipo.Descricao();
        Data = a.DataPrevista.ToString("dd/MM/yyyy");
        Prioridade = a.PrioridadeNaAbertura.Descricao();
        Responsavel = a.Responsavel;
        Status = a.Atrasado(hoje) ? "Atrasado" : a.Status.Descricao();
        Observacao = a.Observacao;
    }

    public AtendimentoTecnico Atendimento { get; }
    public string Fazenda { get; }
    public string Tipo { get; }
    public string Data { get; }
    public string Prioridade { get; }
    public string Responsavel { get; }
    public string Status { get; }
    public string Observacao { get; }
}
