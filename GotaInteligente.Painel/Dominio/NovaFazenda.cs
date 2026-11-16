using System.Globalization;

namespace GotaInteligente.Painel.Dominio;

/// <summary>Dados de cadastro de uma nova propriedade, já validados.</summary>
public sealed class NovaFazenda
{
    public static readonly string[] Ufs =
    {
        "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA", "MT", "MS", "MG", "PA",
        "PB", "PR", "PE", "PI", "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO"
    };

    public string Nome { get; }
    public string Proprietario { get; }
    public string Municipio { get; }
    public string Estado { get; }
    public double AreaHectares { get; }
    public double? Latitude { get; }
    public double? Longitude { get; }

    private NovaFazenda(string nome, string proprietario, string municipio, string estado, double area, double? lat, double? lon)
    {
        Nome = nome; Proprietario = proprietario; Municipio = municipio; Estado = estado;
        AreaHectares = area; Latitude = lat; Longitude = lon;
    }

    /// <summary>Valida os campos digitados na tela. Lança <see cref="ValidacaoException"/> com o erro de cada campo.</summary>
    public static NovaFazenda Criar(string? nome, string? proprietario, string? municipio, string? estado,
        decimal areaHectares, string? latitudeTexto, string? longitudeTexto)
    {
        var erros = new Dictionary<string, string>();

        var n = (nome ?? "").Trim();
        if (n.Length < 3 || n.Length > 120) erros["Nome"] = "O nome deve ter entre 3 e 120 caracteres.";

        var p = (proprietario ?? "").Trim();
        if (p.Length < 3 || p.Length > 120) erros["Proprietario"] = "Informe o proprietário (3 a 120 caracteres).";

        var m = (municipio ?? "").Trim();
        if (m.Length < 2 || m.Length > 80) erros["Municipio"] = "Informe o município.";

        var uf = (estado ?? "").Trim().ToUpperInvariant();
        if (!Ufs.Contains(uf)) erros["Estado"] = "Selecione uma UF válida.";

        if (areaHectares <= 0) erros["Area"] = "A área deve ser maior que zero.";

        var lat = LerCoordenada(latitudeTexto, -90, 90, "Latitude", erros);
        var lon = LerCoordenada(longitudeTexto, -180, 180, "Longitude", erros);
        if ((lat is null) != (lon is null) && !erros.ContainsKey("Latitude") && !erros.ContainsKey("Longitude"))
            erros["Longitude"] = "Preencha latitude e longitude juntas, ou deixe as duas vazias.";

        if (erros.Count > 0) throw new ValidacaoException(erros);

        return new NovaFazenda(n, p, m, uf, (double)areaHectares, lat, lon);
    }

    /// <summary>Aceita vírgula ou ponto como separador decimal (ex.: -23,827 ou -23.827).</summary>
    private static double? LerCoordenada(string? texto, double min, double max, string campo, Dictionary<string, string> erros)
    {
        if (string.IsNullOrWhiteSpace(texto)) return null;

        var normalizado = texto.Trim().Replace(',', '.');
        if (!double.TryParse(normalizado, NumberStyles.Float, CultureInfo.InvariantCulture, out var valor))
        {
            erros[campo] = $"{campo} inválida. Use números, ex.: -23,827.";
            return null;
        }
        if (valor < min || valor > max)
        {
            erros[campo] = $"{campo} deve estar entre {min} e {max}.";
            return null;
        }
        return valor;
    }
}
