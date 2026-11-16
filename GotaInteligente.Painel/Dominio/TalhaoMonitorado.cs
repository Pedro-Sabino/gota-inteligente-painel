namespace GotaInteligente.Painel.Dominio;

/// <summary>Área de plantio monitorada por um sensor de umidade (ESP32).</summary>
public sealed class TalhaoMonitorado
{
    public int Id { get; }
    public string Nome { get; }
    public string Cultura { get; }
    public double? UltimaUmidade { get; }
    public NivelEstresse Nivel { get; }

    public TalhaoMonitorado(int id, string nome, string cultura, double? ultimaUmidade, NivelEstresse nivel)
    {
        if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id), "O id do talhão deve ser positivo.");
        if (string.IsNullOrWhiteSpace(nome)) throw new ArgumentException("O talhão precisa de um nome.", nameof(nome));
        if (ultimaUmidade is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(ultimaUmidade), "A umidade deve estar entre 0 e 100%.");

        Id = id;
        Nome = nome.Trim();
        Cultura = string.IsNullOrWhiteSpace(cultura) ? "Não informada" : cultura.Trim();
        UltimaUmidade = ultimaUmidade;
        Nivel = nivel;
    }

    public bool EmEstresse => Nivel >= NivelEstresse.Moderado;
}
