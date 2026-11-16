using GotaInteligente.Painel.Dominio;
using GotaInteligente.Painel.Ui;

namespace GotaInteligente.Painel.Telas;

/// <summary>Agendamento de atendimento técnico, com sugestão automática conforme o estresse da fazenda.</summary>
public sealed class FormAtendimento : FormBase
{
    private sealed record OpcaoFazenda(FazendaMonitorada Fazenda)
    {
        public override string ToString() => $"[{Fazenda.Prioridade.Descricao().ToUpperInvariant()}] {Fazenda.Nome} — {Fazenda.Municipio}";
    }

    private sealed record OpcaoTipo(TipoAtendimento Tipo)
    {
        public override string ToString() => Tipo.Descricao();
    }

    private readonly ComboBox _fazenda = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _tipo = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DateTimePicker _data = new() { Format = DateTimePickerFormat.Short };
    private readonly TextBox _responsavel = new() { MaxLength = 80, PlaceholderText = "Técnico ou agente responsável" };
    private readonly TextBox _observacao = new() { Multiline = true, ScrollBars = ScrollBars.Vertical, MaxLength = AtendimentoTecnico.TamanhoMaximoObservacao };
    private readonly Label _sugestao = new() { AutoSize = false, TextAlign = ContentAlignment.MiddleLeft, Font = Tema.Negrito, Padding = new Padding(8, 0, 8, 0) };

    public AtendimentoTecnico? Atendimento { get; private set; }

    public FormAtendimento(IEnumerable<FazendaMonitorada> fazendas, FazendaMonitorada? sugerida)
        : base("Agendar atendimento técnico", "Registre a ação do Sindicato Rural / Prefeitura. A sugestão considera o pior nível de estresse da fazenda.",
            new Size(640, 480))
    {
        var hoje = DateTime.Today;
        _data.MinDate = hoje;
        _data.MaxDate = hoje.AddDays(90);
        _data.Value = hoje;

        foreach (var f in fazendas.OrderByDescending(f => f.Prioridade).ThenByDescending(f => f.PontuacaoRisco))
            _fazenda.Items.Add(new OpcaoFazenda(f));
        foreach (var t in Enum.GetValues<TipoAtendimento>())
            _tipo.Items.Add(new OpcaoTipo(t));

        AdicionarCampo("Fazenda", _fazenda);
        AdicionarCampo("Sugestão", _sugestao, 52);
        AdicionarCampo("Tipo", _tipo);
        AdicionarCampo("Data prevista", _data);
        AdicionarCampo("Responsável", _responsavel);
        AdicionarCampo("Observação", _observacao, 90);

        _fazenda.SelectedIndexChanged += (_, _) => AplicarSugestao();
        BtnSalvar.Text = "Agendar";
        BtnSalvar.Click += (_, _) => Salvar();

        var indice = sugerida is null ? 0 : _fazenda.Items.Cast<OpcaoFazenda>().ToList().FindIndex(o => o.Fazenda.Id == sugerida.Id);
        _fazenda.SelectedIndex = _fazenda.Items.Count == 0 ? -1 : Math.Max(0, indice);
    }

    /// <summary>Preenche tipo e data conforme a recomendação para a fazenda escolhida.</summary>
    private void AplicarSugestao()
    {
        if (_fazenda.SelectedItem is not OpcaoFazenda opcao) return;

        var recomendacao = RecomendacaoAtendimento.Para(opcao.Fazenda);
        _sugestao.Text = recomendacao.Justificativa;
        _sugestao.BackColor = Tema.CorFundo(opcao.Fazenda.PiorNivel);
        _sugestao.ForeColor = Tema.CorTexto(opcao.Fazenda.PiorNivel);

        _tipo.SelectedItem = _tipo.Items.Cast<OpcaoTipo>().First(o => o.Tipo == recomendacao.Tipo);
        var data = DateTime.Today.AddDays(recomendacao.PrazoDias);
        _data.Value = data > _data.MaxDate ? _data.MaxDate : data;
    }

    private void Salvar()
    {
        Erros.Clear();
        try
        {
            Atendimento = AtendimentoTecnico.Agendar(
                (_fazenda.SelectedItem as OpcaoFazenda)?.Fazenda,
                (_tipo.SelectedItem as OpcaoTipo)?.Tipo ?? 0,
                _data.Value,
                _responsavel.Text,
                _observacao.Text,
                DateTime.Today);

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (ValidacaoException ex)
        {
            MostrarErros(ex.Erros, new Dictionary<string, Control>
            {
                ["Fazenda"] = _fazenda, ["Tipo"] = _tipo, ["Data"] = _data, ["Responsavel"] = _responsavel, ["Observacao"] = _observacao
            });
        }
    }
}
