using GotaInteligente.Painel.Dominio;
using GotaInteligente.Painel.Servicos;

namespace GotaInteligente.Painel.Telas;

/// <summary>Cadastro de uma nova propriedade da agricultura familiar (enviada para a API).</summary>
public sealed class FormCadastroFazenda : FormBase
{
    private readonly ApiGotaInteligente _api;

    private readonly TextBox _nome = new() { MaxLength = 120 };
    private readonly TextBox _proprietario = new() { MaxLength = 120 };
    private readonly TextBox _municipio = new() { MaxLength = 80, Text = "São Paulo" };
    private readonly ComboBox _uf = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NumericUpDown _area = new() { DecimalPlaces = 2, Minimum = 0, Maximum = 100000, Increment = 0.5m, ThousandsSeparator = true };
    private readonly TextBox _latitude = new() { PlaceholderText = "opcional, ex.: -23,827" };
    private readonly TextBox _longitude = new() { PlaceholderText = "opcional, ex.: -46,729" };

    public int IdCadastrado { get; private set; }
    public string NomeCadastrado { get; private set; } = "";

    public FormCadastroFazenda(ApiGotaInteligente api)
        : base("Nova fazenda", "Cadastre a propriedade do agricultor familiar. As coordenadas permitem que a IA consulte a previsão do tempo do local.",
            new Size(560, 440))
    {
        _api = api;

        _uf.Items.AddRange(NovaFazenda.Ufs.Cast<object>().ToArray());
        _uf.SelectedItem = "SP";

        AdicionarCampo("Nome da fazenda", _nome);
        AdicionarCampo("Proprietário", _proprietario);
        AdicionarCampo("Município", _municipio);
        AdicionarCampo("UF", _uf);
        AdicionarCampo("Área (hectares)", _area);
        AdicionarCampo("Latitude", _latitude);
        AdicionarCampo("Longitude", _longitude);

        BtnSalvar.Click += async (_, _) => await SalvarAsync();
    }

    private async Task SalvarAsync()
    {
        Erros.Clear();
        var mapa = new Dictionary<string, Control>
        {
            ["Nome"] = _nome, ["Proprietario"] = _proprietario, ["Municipio"] = _municipio, ["Estado"] = _uf,
            ["Area"] = _area, ["AreaHectares"] = _area, ["Latitude"] = _latitude, ["Longitude"] = _longitude
        };

        NovaFazenda fazenda;
        try
        {
            fazenda = NovaFazenda.Criar(_nome.Text, _proprietario.Text, _municipio.Text, _uf.SelectedItem as string,
                _area.Value, _latitude.Text, _longitude.Text);
        }
        catch (ValidacaoException ex)
        {
            MostrarErros(ex.Erros, mapa);
            return;
        }

        BtnSalvar.Enabled = false;
        UseWaitCursor = true;
        try
        {
            IdCadastrado = await _api.CadastrarFazendaAsync(fazenda);
            NomeCadastrado = fazenda.Nome;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (ValidacaoException ex)
        {
            MostrarErros(ex.Erros, mapa); // validação feita pela própria API
        }
        catch (ApiIndisponivelException ex)
        {
            MessageBox.Show(this, $"{ex.Message}\n\nO cadastro de fazendas precisa da API ligada (dotnet run no projeto de SOA).",
                Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            BtnSalvar.Enabled = true;
            UseWaitCursor = false;
        }
    }
}
