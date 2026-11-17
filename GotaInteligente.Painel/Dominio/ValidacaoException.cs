namespace GotaInteligente.Painel.Dominio;

/// <summary>Erros de validação agrupados por campo, para a tela destacar cada um.</summary>
public sealed class ValidacaoException : Exception
{
    public IReadOnlyDictionary<string, string> Erros { get; }

    public ValidacaoException(IDictionary<string, string> erros)
        : base(string.Join(Environment.NewLine, erros.Values))
    {
        Erros = new Dictionary<string, string>(erros);
    }
}
