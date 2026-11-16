using System.Text.Json;
using GotaInteligente.Painel.Dominio;

namespace GotaInteligente.Painel.Servicos;

/// <summary>Persistência básica dos atendimentos técnicos em um arquivo JSON local.</summary>
public sealed class RepositorioAtendimentos
{
    private sealed class Registro
    {
        public Guid Id { get; set; }
        public int FazendaId { get; set; }
        public string FazendaNome { get; set; } = "";
        public TipoAtendimento Tipo { get; set; }
        public DateTime DataPrevista { get; set; }
        public string Responsavel { get; set; } = "";
        public string Observacao { get; set; } = "";
        public Prioridade Prioridade { get; set; }
        public StatusAtendimento Status { get; set; }
        public DateTime CriadoEm { get; set; }
        public DateTime? FinalizadoEm { get; set; }
    }

    private readonly string _arquivo;
    private readonly List<AtendimentoTecnico> _itens;

    public RepositorioAtendimentos(string pasta)
    {
        _arquivo = Path.Combine(pasta, "atendimentos.json");
        _itens = Carregar();
    }

    public string CaminhoArquivo => _arquivo;

    public IReadOnlyList<AtendimentoTecnico> Listar() =>
        _itens.OrderBy(a => a.Status)
              .ThenByDescending(a => a.PrioridadeNaAbertura)
              .ThenBy(a => a.DataPrevista)
              .ToList();

    public bool ExisteAgendadoPara(int fazendaId) =>
        _itens.Any(a => a.FazendaId == fazendaId && a.Status == StatusAtendimento.Agendado);

    public void Adicionar(AtendimentoTecnico atendimento)
    {
        _itens.Add(atendimento);
        Salvar();
    }

    public void Concluir(Guid id) { Obter(id).Concluir(); Salvar(); }

    public void Cancelar(Guid id) { Obter(id).Cancelar(); Salvar(); }

    private AtendimentoTecnico Obter(Guid id) =>
        _itens.FirstOrDefault(a => a.Id == id) ?? throw new KeyNotFoundException("Atendimento não encontrado.");

    private List<AtendimentoTecnico> Carregar()
    {
        if (!File.Exists(_arquivo)) return new List<AtendimentoTecnico>();
        try
        {
            var registros = JsonSerializer.Deserialize<List<Registro>>(File.ReadAllText(_arquivo), ApiGotaInteligente.Json)
                            ?? new List<Registro>();
            return registros.Select(r => AtendimentoTecnico.Reidratar(r.Id, r.FazendaId, r.FazendaNome, r.Tipo, r.DataPrevista,
                r.Responsavel, r.Observacao, r.Prioridade, r.Status, r.CriadoEm, r.FinalizadoEm)).ToList();
        }
        catch (JsonException)
        {
            // Preserva o arquivo com problema e começa uma lista nova.
            File.Copy(_arquivo, _arquivo + ".corrompido", overwrite: true);
            return new List<AtendimentoTecnico>();
        }
    }

    private void Salvar()
    {
        var registros = _itens.Select(a => new Registro
        {
            Id = a.Id, FazendaId = a.FazendaId, FazendaNome = a.FazendaNome, Tipo = a.Tipo, DataPrevista = a.DataPrevista,
            Responsavel = a.Responsavel, Observacao = a.Observacao, Prioridade = a.PrioridadeNaAbertura, Status = a.Status,
            CriadoEm = a.CriadoEm, FinalizadoEm = a.FinalizadoEm
        }).ToList();

        File.WriteAllText(_arquivo, JsonSerializer.Serialize(registros, ApiGotaInteligente.Json));
    }
}
