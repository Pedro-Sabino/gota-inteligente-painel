using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using GotaInteligente.Painel.Dominio;

namespace GotaInteligente.Painel.Servicos;

public sealed class ApiIndisponivelException : Exception
{
    public ApiIndisponivelException(string mensagem, Exception? interna = null) : base(mensagem, interna) { }
}

/// <summary>Cliente HTTP da API REST Gota Inteligente (projeto de SOA).</summary>
public sealed class ApiGotaInteligente
{
    internal static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
        WriteIndented = true
    };

    private readonly HttpClient _http;

    public string UrlBase { get; }

    public ApiGotaInteligente(string urlBase)
    {
        UrlBase = urlBase.TrimEnd('/');
        _http = new HttpClient { BaseAddress = new Uri(UrlBase + "/"), Timeout = TimeSpan.FromSeconds(5) };
    }

    public Task<List<PainelFazendaDto>> ObterEstresseHidricoAsync() =>
        ExecutarAsync(async () =>
            await _http.GetFromJsonAsync<List<PainelFazendaDto>>("api/painel/estresse-hidrico", Json) ?? new List<PainelFazendaDto>());

    public Task<List<FazendaResponseDto>> ObterFazendasAsync() =>
        ExecutarAsync(async () =>
            await _http.GetFromJsonAsync<List<FazendaResponseDto>>("api/fazendas", Json) ?? new List<FazendaResponseDto>());

    public Task<PainelResumoDto> ObterResumoAsync() =>
        ExecutarAsync(async () =>
            await _http.GetFromJsonAsync<PainelResumoDto>("api/painel/resumo", Json) ?? new PainelResumoDto());

    /// <summary>Cadastra uma fazenda na API e devolve o id criado.</summary>
    public Task<int> CadastrarFazendaAsync(NovaFazenda fazenda) =>
        ExecutarAsync(async () =>
        {
            var corpo = new FazendaRequestDto
            {
                Nome = fazenda.Nome,
                Proprietario = fazenda.Proprietario,
                Municipio = fazenda.Municipio,
                Estado = fazenda.Estado,
                AreaHectares = fazenda.AreaHectares,
                Latitude = fazenda.Latitude,
                Longitude = fazenda.Longitude
            };

            using var resposta = await _http.PostAsJsonAsync("api/fazendas", corpo, Json);
            if (resposta.StatusCode == HttpStatusCode.BadRequest)
                throw new ValidacaoException(await LerErrosAsync(resposta));
            resposta.EnsureSuccessStatusCode();

            var criada = await resposta.Content.ReadFromJsonAsync<FazendaResponseDto>(Json);
            return criada?.Id ?? 0;
        });

    private static async Task<T> ExecutarAsync<T>(Func<Task<T>> acao)
    {
        try
        {
            return await acao();
        }
        catch (HttpRequestException ex)
        {
            throw new ApiIndisponivelException("Não foi possível conectar à API Gota Inteligente.", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new ApiIndisponivelException("A API Gota Inteligente demorou demais para responder.", ex);
        }
    }

    /// <summary>Converte o ProblemDetails de validação do ASP.NET em erros por campo.</summary>
    private static async Task<Dictionary<string, string>> LerErrosAsync(HttpResponseMessage resposta)
    {
        var erros = new Dictionary<string, string>();
        try
        {
            using var doc = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync());
            if (doc.RootElement.TryGetProperty("errors", out var lista))
            {
                foreach (var campo in lista.EnumerateObject())
                {
                    var mensagens = campo.Value.EnumerateArray().Select(m => m.GetString()).Where(m => m is not null);
                    erros[campo.Name] = string.Join(" ", mensagens);
                }
            }
        }
        catch (JsonException) { }

        if (erros.Count == 0) erros["Api"] = "A API recusou os dados enviados.";
        return erros;
    }
}
