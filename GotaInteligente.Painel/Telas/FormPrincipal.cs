using GotaInteligente.Painel.Dominio;
using GotaInteligente.Painel.Servicos;
using GotaInteligente.Painel.Ui;

namespace GotaInteligente.Painel.Telas;

/// <summary>Painel do Sindicato Rural / Prefeitura: estresse hídrico das fazendas da região e atendimentos técnicos.</summary>
public sealed class FormPrincipal : Form
{
    private const string TodosMunicipios = "Todos os municípios";

    private readonly ServicoPainel _servico;
    private readonly ApiGotaInteligente _api;
    private readonly RepositorioAtendimentos _atendimentos;

    private ResultadoCarga? _carga;
    private bool _carregando;

    // Cabeçalho e barra de ações
    private readonly Label _lblOrigem = new();
    private readonly Button _btnAtualizar = Tema.BotaoPrimario("Atualizar");
    private readonly Button _btnNovaFazenda = Tema.BotaoSecundario("+  Nova fazenda");
    private readonly Button _btnNovoAtendimento = Tema.BotaoSecundario("+  Agendar atendimento");
    private readonly ComboBox _cmbMunicipio = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 200, Font = Tema.Padrao };
    private readonly CheckBox _chkSomenteEstresse = new() { Text = "Somente fazendas com estresse", AutoSize = true, Font = Tema.Padrao };
    private readonly Panel _avisoOffline = new();
    private readonly Label _lblAviso = new();

    // Indicadores
    private readonly CardIndicador _cardFazendas = new("Fazendas monitoradas", Tema.Primaria);
    private readonly CardIndicador _cardEstresse = new("Talhões em estresse", Tema.CorTexto(NivelEstresse.Moderado));
    private readonly CardIndicador _cardSevero = new("Estresse severo", Tema.CorTexto(NivelEstresse.Severo));
    private readonly CardIndicador _cardEvitadas = new("Irrigações evitadas (30 dias)", Color.FromArgb(37, 99, 235));
    private readonly BarraDistribuicao _barra = new();

    // Tabelas
    private readonly DataGridView _gradeFazendas = new() { Dock = DockStyle.Fill };
    private readonly DataGridView _gradeTalhoes = new() { Dock = DockStyle.Fill };
    private readonly DataGridView _gradeAtendimentos = new() { Dock = DockStyle.Fill };
    private readonly Label _lblRecomendacao = new();
    private readonly Label _lblTituloTalhoes = new();
    private readonly Button _btnConcluir = Tema.BotaoPrimario("Concluir atendimento");
    private readonly Button _btnCancelar = Tema.BotaoSecundario("Cancelar atendimento");
    private readonly ToolStripStatusLabel _status = new() { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
    private readonly ToolStripStatusLabel _versao = new();
    private readonly TabControl _abas = new() { Dock = DockStyle.Fill, Font = Tema.Negrito, Padding = new Point(14, 6) };

    public FormPrincipal(ServicoPainel servico, ApiGotaInteligente api, RepositorioAtendimentos atendimentos)
    {
        _servico = servico;
        _api = api;
        _atendimentos = atendimentos;

        Text = "Gota Inteligente — Painel Regional de Estresse Hídrico";
        Font = Tema.Padrao;
        BackColor = Tema.Fundo;
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(1280, 820);
        MinimumSize = new Size(1080, 700);

        MontarTela();
        LigarEventos();
    }

    // ------------------------------------------------------------------ Layout

    private void MontarTela()
    {
        var raiz = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            Padding = new Padding(0),
            BackColor = Tema.Fundo
        };
        raiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));   // cabeçalho
        raiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));   // ações
        raiz.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // aviso offline
        raiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 186));  // indicadores
        raiz.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // abas

        raiz.Controls.Add(MontarCabecalho(), 0, 0);
        raiz.Controls.Add(MontarBarraAcoes(), 0, 1);
        raiz.Controls.Add(MontarAvisoOffline(), 0, 2);
        raiz.Controls.Add(MontarIndicadores(), 0, 3);
        raiz.Controls.Add(MontarAbas(), 0, 4);

        var rodape = new StatusStrip { BackColor = Color.White, SizingGrip = false };
        _versao.Text = $"v{Application.ProductVersion.Split('+')[0]}";
        _versao.ForeColor = Tema.TextoSuave;
        rodape.Items.Add(_status);
        rodape.Items.Add(_versao);

        Controls.Add(raiz);
        Controls.Add(rodape);
    }

    private Control MontarCabecalho()
    {
        var painel = new Panel { Dock = DockStyle.Fill, BackColor = Tema.Primaria, Padding = new Padding(20, 10, 20, 8), Margin = new Padding(0) };

        var titulo = new Label
        {
            Text = "Gota Inteligente — Painel Regional",
            Font = Tema.Titulo,
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(18, 8)
        };
        var subtitulo = new Label
        {
            Text = "Sindicato Rural / Prefeitura · monitoramento do estresse hídrico da agricultura familiar",
            Font = Tema.Subtitulo,
            ForeColor = Color.FromArgb(205, 232, 220),
            AutoSize = true,
            Location = new Point(21, 40)
        };

        _lblOrigem.AutoSize = true;
        _lblOrigem.Font = Tema.Negrito;
        _lblOrigem.Padding = new Padding(10, 4, 10, 4);
        _lblOrigem.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _lblOrigem.Text = "Carregando…";
        _lblOrigem.BackColor = Tema.PrimariaEscura;
        _lblOrigem.ForeColor = Color.White;

        painel.Controls.Add(titulo);
        painel.Controls.Add(subtitulo);
        painel.Controls.Add(_lblOrigem);
        painel.Resize += (_, _) => _lblOrigem.Location = new Point(painel.Width - _lblOrigem.Width - 20, 20);
        _lblOrigem.SizeChanged += (_, _) => _lblOrigem.Location = new Point(painel.Width - _lblOrigem.Width - 20, 20);
        return painel;
    }

    private Control MontarBarraAcoes()
    {
        var barra = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(18, 12, 18, 0),
            WrapContents = false,
            BackColor = Tema.Fundo
        };

        var lblMunicipio = new Label
        {
            Text = "Município:",
            AutoSize = true,
            Font = Tema.Negrito,
            ForeColor = Tema.Texto,
            Margin = new Padding(16, 7, 6, 0)
        };
        _cmbMunicipio.Margin = new Padding(0, 3, 12, 0);
        _chkSomenteEstresse.Margin = new Padding(0, 6, 0, 0);

        barra.Controls.AddRange(new Control[]
        {
            _btnAtualizar, _btnNovaFazenda, _btnNovoAtendimento, lblMunicipio, _cmbMunicipio, _chkSomenteEstresse
        });
        return barra;
    }

    private Control MontarAvisoOffline()
    {
        _avisoOffline.Dock = DockStyle.Fill;
        _avisoOffline.Height = 34;
        _avisoOffline.Margin = new Padding(18, 0, 18, 8);
        _avisoOffline.BackColor = Tema.AlertaFundo;
        _avisoOffline.Visible = false;

        _lblAviso.Dock = DockStyle.Fill;
        _lblAviso.ForeColor = Tema.Alerta;
        _lblAviso.Font = Tema.Negrito;
        _lblAviso.TextAlign = ContentAlignment.MiddleLeft;
        _lblAviso.Padding = new Padding(10, 0, 0, 0);
        _avisoOffline.Controls.Add(_lblAviso);
        return _avisoOffline;
    }

    private Control MontarIndicadores()
    {
        var area = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 2,
            Padding = new Padding(18, 4, 6, 8),
            BackColor = Tema.Fundo
        };
        for (var i = 0; i < 4; i++) area.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        area.RowStyles.Add(new RowStyle(SizeType.Absolute, 110));
        area.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        area.Controls.Add(_cardFazendas, 0, 0);
        area.Controls.Add(_cardEstresse, 1, 0);
        area.Controls.Add(_cardSevero, 2, 0);
        area.Controls.Add(_cardEvitadas, 3, 0);

        var molduraBarra = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(0, 10, 12, 0) };
        var lblBarra = new Label
        {
            Text = "Distribuição dos talhões por nível de estresse",
            Dock = DockStyle.Left,
            Width = 330,
            Font = Tema.Negrito,
            ForeColor = Tema.TextoSuave,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(14, 0, 0, 0)
        };
        molduraBarra.Controls.Add(_barra);
        molduraBarra.Controls.Add(lblBarra);
        area.Controls.Add(molduraBarra, 0, 1);
        area.SetColumnSpan(molduraBarra, 4);
        return area;
    }

    private Control MontarAbas()
    {
        var moldura = new Panel { Dock = DockStyle.Fill, Padding = new Padding(18, 0, 18, 12), BackColor = Tema.Fundo };

        var abaFazendas = new TabPage("Fazendas e prioridades") { BackColor = Color.White, Padding = new Padding(10) };
        var abaAtendimentos = new TabPage("Atendimentos técnicos") { BackColor = Color.White, Padding = new Padding(10) };

        // --- Aba 1: fazendas (em cima) e talhões da fazenda selecionada (embaixo)
        Tema.ConfigurarGrade(_gradeFazendas);
        _gradeFazendas.Columns.AddRange(
            Tema.Coluna(nameof(LinhaFazenda.Prioridade), "Prioridade", 70),
            Tema.Coluna(nameof(LinhaFazenda.Nome), "Fazenda", 150),
            Tema.Coluna(nameof(LinhaFazenda.Municipio), "Município", 100),
            Tema.Coluna(nameof(LinhaFazenda.Talhoes), "Talhões", 55, alinhamento: DataGridViewContentAlignment.MiddleCenter),
            Tema.Coluna(nameof(LinhaFazenda.EmEstresse), "Em estresse", 65, alinhamento: DataGridViewContentAlignment.MiddleCenter),
            Tema.Coluna(nameof(LinhaFazenda.PiorNivel), "Pior nível", 75),
            Tema.Coluna(nameof(LinhaFazenda.Pontuacao), "Risco", 55, alinhamento: DataGridViewContentAlignment.MiddleCenter),
            Tema.Coluna(nameof(LinhaFazenda.Atendimento), "Atendimento", 70));

        Tema.ConfigurarGrade(_gradeTalhoes);
        _gradeTalhoes.Columns.AddRange(
            Tema.Coluna(nameof(LinhaTalhao.Nome), "Talhão", 120),
            Tema.Coluna(nameof(LinhaTalhao.Cultura), "Cultura", 100),
            Tema.Coluna(nameof(LinhaTalhao.Umidade), "Última umidade do solo", 90, alinhamento: DataGridViewContentAlignment.MiddleCenter),
            Tema.Coluna(nameof(LinhaTalhao.Situacao), "Situação", 90));

        _lblTituloTalhoes.Dock = DockStyle.Top;
        _lblTituloTalhoes.Height = 26;
        _lblTituloTalhoes.Font = Tema.Negrito;
        _lblTituloTalhoes.ForeColor = Tema.Texto;
        _lblTituloTalhoes.Text = "Talhões da fazenda selecionada";

        _lblRecomendacao.Dock = DockStyle.Bottom;
        _lblRecomendacao.Height = 34;
        _lblRecomendacao.Font = Tema.Negrito;
        _lblRecomendacao.TextAlign = ContentAlignment.MiddleLeft;
        _lblRecomendacao.Padding = new Padding(10, 0, 0, 0);
        _lblRecomendacao.BackColor = Tema.PrimariaClara;
        _lblRecomendacao.ForeColor = Tema.PrimariaEscura;

        var divisao = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterWidth = 8,
            BackColor = Color.White
        };
        divisao.Panel1.Controls.Add(_gradeFazendas);
        divisao.Panel2.Controls.Add(_gradeTalhoes);
        divisao.Panel2.Controls.Add(_lblTituloTalhoes);
        divisao.Panel2.Controls.Add(_lblRecomendacao);
        abaFazendas.Controls.Add(divisao);
        abaFazendas.Layout += (_, _) =>
        {
            // Mantém ~60% para as fazendas e ~40% para os talhões.
            var alvo = (int)(divisao.Height * 0.58);
            if (alvo > divisao.Panel1MinSize && alvo < divisao.Height - divisao.Panel2MinSize)
                divisao.SplitterDistance = alvo;
        };

        // --- Aba 2: atendimentos
        Tema.ConfigurarGrade(_gradeAtendimentos);
        _gradeAtendimentos.Columns.AddRange(
            Tema.Coluna(nameof(LinhaAtendimento.Status), "Status", 60),
            Tema.Coluna(nameof(LinhaAtendimento.Prioridade), "Prioridade", 60),
            Tema.Coluna(nameof(LinhaAtendimento.Fazenda), "Fazenda", 120),
            Tema.Coluna(nameof(LinhaAtendimento.Tipo), "Tipo", 110),
            Tema.Coluna(nameof(LinhaAtendimento.Data), "Data prevista", 65, alinhamento: DataGridViewContentAlignment.MiddleCenter),
            Tema.Coluna(nameof(LinhaAtendimento.Responsavel), "Responsável", 90),
            Tema.Coluna(nameof(LinhaAtendimento.Observacao), "Observação", 170));

        var acoesAtendimento = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 46, Padding = new Padding(0, 10, 0, 0) };
        var lblArquivo = new Label
        {
            AutoSize = true,
            ForeColor = Tema.TextoSuave,
            Margin = new Padding(12, 8, 0, 0),
            Text = $"Salvo em: {_atendimentos.CaminhoArquivo}"
        };
        acoesAtendimento.Controls.AddRange(new Control[] { _btnConcluir, _btnCancelar, lblArquivo });
        abaAtendimentos.Controls.Add(_gradeAtendimentos);
        abaAtendimentos.Controls.Add(acoesAtendimento);

        _abas.TabPages.Add(abaFazendas);
        _abas.TabPages.Add(abaAtendimentos);
        moldura.Controls.Add(_abas);
        return moldura;
    }

    // ------------------------------------------------------------------ Eventos

    private void LigarEventos()
    {
        Load += async (_, _) => await AtualizarAsync();
        _btnAtualizar.Click += async (_, _) => await AtualizarAsync();
        _btnNovaFazenda.Click += async (_, _) => await AbrirCadastroFazendaAsync();
        _btnNovoAtendimento.Click += (_, _) => AbrirAgendamento(FazendaSelecionada());
        _cmbMunicipio.SelectedIndexChanged += (_, _) => AplicarFiltros();
        _chkSomenteEstresse.CheckedChanged += (_, _) => AplicarFiltros();
        _gradeFazendas.SelectionChanged += (_, _) => MostrarTalhoes(FazendaSelecionada());
        _gradeFazendas.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) AbrirAgendamento(FazendaSelecionada()); };
        _gradeFazendas.CellFormatting += FormatarLinhaFazenda;
        _gradeTalhoes.CellFormatting += FormatarLinhaTalhao;
        _gradeAtendimentos.CellFormatting += FormatarLinhaAtendimento;
        _gradeAtendimentos.SelectionChanged += (_, _) => AtualizarBotoesAtendimento();
        _btnConcluir.Click += (_, _) => AlterarAtendimento(concluir: true);
        _btnCancelar.Click += (_, _) => AlterarAtendimento(concluir: false);
    }

    private async Task AtualizarAsync()
    {
        if (_carregando) return;
        _carregando = true;
        _btnAtualizar.Enabled = false;
        UseWaitCursor = true;
        _status.Text = $"Consultando a API em {_api.UrlBase}…";

        try
        {
            _carga = await _servico.CarregarAsync();
            PreencherMunicipios();
            AtualizarIndicadores();
            AplicarFiltros();
            AtualizarAtendimentos();
            MostrarOrigem();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Não foi possível carregar o painel.\n\n{ex.Message}", "Gota Inteligente",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            _status.Text = "Falha ao carregar o painel.";
        }
        finally
        {
            _carregando = false;
            _btnAtualizar.Enabled = true;
            UseWaitCursor = false;
        }
    }

    private void MostrarOrigem()
    {
        if (_carga is null) return;

        var (texto, cor) = _carga.Origem switch
        {
            OrigemDados.Api => ("● Online · API conectada", Color.FromArgb(22, 120, 64)),
            OrigemDados.Cache => ("● Offline · último carregamento", Tema.Alerta),
            _ => ("● Offline · dados simulados", Tema.Alerta)
        };
        _lblOrigem.Text = texto;
        _lblOrigem.BackColor = Color.White;
        _lblOrigem.ForeColor = cor;

        _avisoOffline.Visible = _carga.Aviso is not null;
        _lblAviso.Text = _carga.Aviso is null ? "" : "Atenção: " + _carga.Aviso;

        _status.Text = $"Atualizado em {_carga.AtualizadoEm:dd/MM/yyyy HH:mm:ss} · fonte: {_api.UrlBase}/api/painel";
        _btnNovaFazenda.Enabled = _carga.Origem == OrigemDados.Api;
    }

    private void PreencherMunicipios()
    {
        var atual = _cmbMunicipio.SelectedItem as string;
        var municipios = _carga!.Fazendas.Select(f => f.Municipio).Distinct().OrderBy(m => m).ToList();

        _cmbMunicipio.BeginUpdate();
        _cmbMunicipio.Items.Clear();
        _cmbMunicipio.Items.Add(TodosMunicipios);
        foreach (var m in municipios) _cmbMunicipio.Items.Add(m);
        _cmbMunicipio.SelectedItem = atual is not null && _cmbMunicipio.Items.Contains(atual) ? atual : TodosMunicipios;
        _cmbMunicipio.EndUpdate();
    }

    private IReadOnlyList<FazendaMonitorada> FazendasFiltradas()
    {
        if (_carga is null) return Array.Empty<FazendaMonitorada>();

        IEnumerable<FazendaMonitorada> fazendas = _carga.Fazendas;
        if (_cmbMunicipio.SelectedItem is string municipio && municipio != TodosMunicipios)
            fazendas = fazendas.Where(f => f.Municipio == municipio);
        if (_chkSomenteEstresse.Checked)
            fazendas = fazendas.Where(f => f.TalhoesEmEstresse > 0);
        return fazendas.ToList();
    }

    private void AtualizarIndicadores()
    {
        var fazendas = FazendasFiltradas();
        var irrigacoes = _carga?.Resumo.IrrigacoesEvitadas30Dias ?? 0;
        var resumo = ResumoRegional.Calcular(fazendas.ToList(), irrigacoes);

        _cardFazendas.Atualizar(resumo.TotalFazendas.ToString(), $"{resumo.TotalTalhoes} talhões com sensor de umidade");
        _cardEstresse.Atualizar(resumo.TalhoesEmEstresse.ToString(), $"{resumo.PercentualEmEstresse:0}% dos talhões monitorados");
        _cardSevero.Atualizar(resumo.TalhoesSeveros.ToString(),
            resumo.TalhoesSeveros > 0 ? "Visita técnica em até 48h" : "Nenhum talhão crítico");
        _cardEvitadas.Atualizar(resumo.IrrigacoesEvitadas30Dias.ToString(), "Chuva prevista: bomba desligada");
        _barra.Atualizar(fazendas);
    }

    private void AplicarFiltros()
    {
        if (_carga is null) return;

        var selecionada = FazendaSelecionada()?.Id;
        var linhas = FazendasFiltradas()
            .Select(f => new LinhaFazenda(f, _atendimentos.ExisteAgendadoPara(f.Id)))
            .ToList();

        _gradeFazendas.DataSource = linhas;
        AtualizarIndicadores();

        // Mantém a seleção anterior, se ainda estiver visível.
        foreach (DataGridViewRow linha in _gradeFazendas.Rows)
        {
            if (linha.DataBoundItem is LinhaFazenda lf && lf.Fazenda.Id == selecionada)
            {
                linha.Selected = true;
                _gradeFazendas.CurrentCell = linha.Cells[0];
                break;
            }
        }
        MostrarTalhoes(FazendaSelecionada());
    }

    private FazendaMonitorada? FazendaSelecionada() =>
        (_gradeFazendas.CurrentRow?.DataBoundItem as LinhaFazenda)?.Fazenda;

    private void MostrarTalhoes(FazendaMonitorada? fazenda)
    {
        if (fazenda is null)
        {
            _gradeTalhoes.DataSource = null;
            _lblTituloTalhoes.Text = "Talhões da fazenda selecionada";
            _lblRecomendacao.Text = "Selecione uma fazenda para ver a recomendação de atendimento.";
            return;
        }

        _gradeTalhoes.DataSource = fazenda.Talhoes
            .OrderByDescending(t => t.Nivel)
            .Select(t => new LinhaTalhao(t))
            .ToList();
        _lblTituloTalhoes.Text = $"Talhões de {fazenda.Nome} ({fazenda.Municipio})";

        var recomendacao = RecomendacaoAtendimento.Para(fazenda);
        _lblRecomendacao.Text = $"Recomendação: {recomendacao.Justificativa}  (duplo clique na fazenda para agendar)";
        _lblRecomendacao.BackColor = Tema.CorFundo(fazenda.PiorNivel);
        _lblRecomendacao.ForeColor = Tema.CorTexto(fazenda.PiorNivel);
    }

    private void FormatarLinhaFazenda(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || e.CellStyle is null) return;
        if (_gradeFazendas.Rows[e.RowIndex].DataBoundItem is not LinhaFazenda linha) return;

        var coluna = _gradeFazendas.Columns[e.ColumnIndex].Name;
        if (coluna == nameof(LinhaFazenda.Prioridade))
        {
            var nivel = Tema.NivelDaPrioridade(linha.Fazenda.Prioridade);
            e.CellStyle.BackColor = Tema.CorFundo(nivel);
            e.CellStyle.ForeColor = Tema.CorTexto(nivel);
            e.CellStyle.Font = Tema.Negrito;
        }
        else if (coluna == nameof(LinhaFazenda.PiorNivel))
        {
            e.CellStyle.ForeColor = Tema.CorTexto(linha.Fazenda.PiorNivel);
            e.CellStyle.Font = Tema.Negrito;
        }
        else if (coluna == nameof(LinhaFazenda.Atendimento) && linha.Atendimento == "Pendente")
        {
            e.CellStyle.ForeColor = Tema.Alerta;
            e.CellStyle.Font = Tema.Negrito;
        }
    }

    private void FormatarLinhaTalhao(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || e.CellStyle is null) return;
        if (_gradeTalhoes.Rows[e.RowIndex].DataBoundItem is not LinhaTalhao linha) return;

        if (_gradeTalhoes.Columns[e.ColumnIndex].Name == nameof(LinhaTalhao.Situacao))
        {
            e.CellStyle.BackColor = Tema.CorFundo(linha.Nivel);
            e.CellStyle.ForeColor = Tema.CorTexto(linha.Nivel);
            e.CellStyle.Font = Tema.Negrito;
        }
    }

    private void FormatarLinhaAtendimento(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || e.CellStyle is null) return;
        if (_gradeAtendimentos.Rows[e.RowIndex].DataBoundItem is not LinhaAtendimento linha) return;

        var coluna = _gradeAtendimentos.Columns[e.ColumnIndex].Name;
        if (coluna == nameof(LinhaAtendimento.Status))
        {
            var nivel = linha.Status switch
            {
                "Atrasado" => NivelEstresse.Severo,
                "Agendado" => NivelEstresse.Leve,
                "Concluído" => NivelEstresse.Nenhum,
                _ => NivelEstresse.SemDados
            };
            e.CellStyle.BackColor = Tema.CorFundo(nivel);
            e.CellStyle.ForeColor = Tema.CorTexto(nivel);
            e.CellStyle.Font = Tema.Negrito;
        }
        else if (coluna == nameof(LinhaAtendimento.Prioridade))
        {
            e.CellStyle.ForeColor = Tema.CorTexto(Tema.NivelDaPrioridade(linha.Atendimento.PrioridadeNaAbertura));
            e.CellStyle.Font = Tema.Negrito;
        }
    }

    // ------------------------------------------------------------------ Ações

    private async Task AbrirCadastroFazendaAsync()
    {
        using var form = new FormCadastroFazenda(_api);
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            _status.Text = $"Fazenda \"{form.NomeCadastrado}\" cadastrada (id {form.IdCadastrado}).";
            await AtualizarAsync();
        }
    }

    private void AbrirAgendamento(FazendaMonitorada? fazendaSugerida)
    {
        if (_carga is null || _carga.Fazendas.Count == 0)
        {
            MessageBox.Show(this, "Carregue o painel antes de agendar um atendimento.", "Gota Inteligente",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var form = new FormAtendimento(_carga.Fazendas, fazendaSugerida);
        if (form.ShowDialog(this) == DialogResult.OK && form.Atendimento is not null)
        {
            _atendimentos.Adicionar(form.Atendimento);
            AtualizarAtendimentos();
            AplicarFiltros();
            _abas.SelectedIndex = 1;
            _status.Text = $"Atendimento agendado para {form.Atendimento.FazendaNome} em {form.Atendimento.DataPrevista:dd/MM/yyyy}.";
        }
    }

    private void AtualizarAtendimentos()
    {
        var hoje = DateTime.Today;
        _gradeAtendimentos.DataSource = _atendimentos.Listar().Select(a => new LinhaAtendimento(a, hoje)).ToList();
        var pendentes = _atendimentos.Listar().Count(a => a.Status == StatusAtendimento.Agendado);
        _abas.TabPages[1].Text = pendentes > 0 ? $"Atendimentos técnicos ({pendentes})" : "Atendimentos técnicos";
        AtualizarBotoesAtendimento();
    }

    private AtendimentoTecnico? AtendimentoSelecionado() =>
        (_gradeAtendimentos.CurrentRow?.DataBoundItem as LinhaAtendimento)?.Atendimento;

    private void AtualizarBotoesAtendimento()
    {
        var podeAlterar = AtendimentoSelecionado()?.Status == StatusAtendimento.Agendado;
        _btnConcluir.Enabled = podeAlterar;
        _btnCancelar.Enabled = podeAlterar;
    }

    private void AlterarAtendimento(bool concluir)
    {
        var atendimento = AtendimentoSelecionado();
        if (atendimento is null) return;

        var acao = concluir ? "concluir" : "cancelar";
        var confirmar = MessageBox.Show(this,
            $"Deseja {acao} o atendimento de {atendimento.FazendaNome} ({atendimento.Tipo.Descricao()})?",
            "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirmar != DialogResult.Yes) return;

        try
        {
            if (concluir) _atendimentos.Concluir(atendimento.Id);
            else _atendimentos.Cancelar(atendimento.Id);
            AtualizarAtendimentos();
            AplicarFiltros();
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(this, ex.Message, "Gota Inteligente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
