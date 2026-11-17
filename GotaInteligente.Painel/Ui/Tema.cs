using GotaInteligente.Painel.Dominio;

namespace GotaInteligente.Painel.Ui;

/// <summary>Cores e fontes do painel, num só lugar.</summary>
public static class Tema
{
    public static readonly Color Primaria = Color.FromArgb(14, 107, 79);
    public static readonly Color PrimariaEscura = Color.FromArgb(9, 72, 54);
    public static readonly Color PrimariaClara = Color.FromArgb(226, 241, 234);
    public static readonly Color Fundo = Color.FromArgb(244, 248, 246);
    public static readonly Color Texto = Color.FromArgb(27, 43, 36);
    public static readonly Color TextoSuave = Color.FromArgb(95, 112, 104);
    public static readonly Color Borda = Color.FromArgb(214, 226, 220);
    public static readonly Color Alerta = Color.FromArgb(180, 83, 9);
    public static readonly Color AlertaFundo = Color.FromArgb(255, 243, 214);

    public static readonly Font Titulo = new("Segoe UI Semibold", 15f);
    public static readonly Font Subtitulo = new("Segoe UI", 9.5f);
    public static readonly Font Padrao = new("Segoe UI", 9.75f);
    public static readonly Font Negrito = new("Segoe UI Semibold", 9.75f);
    public static readonly Font ValorCard = new("Segoe UI Semibold", 22f);

    public static Color CorTexto(NivelEstresse nivel) => nivel switch
    {
        NivelEstresse.Severo => Color.FromArgb(176, 32, 32),
        NivelEstresse.Moderado => Color.FromArgb(166, 98, 0),
        NivelEstresse.Leve => Color.FromArgb(128, 112, 0),
        NivelEstresse.Nenhum => Color.FromArgb(22, 120, 64),
        _ => Color.FromArgb(110, 110, 110)
    };

    public static Color CorFundo(NivelEstresse nivel) => nivel switch
    {
        NivelEstresse.Severo => Color.FromArgb(253, 226, 226),
        NivelEstresse.Moderado => Color.FromArgb(255, 238, 204),
        NivelEstresse.Leve => Color.FromArgb(255, 249, 214),
        NivelEstresse.Nenhum => Color.FromArgb(227, 246, 234),
        _ => Color.FromArgb(238, 238, 238)
    };

    public static NivelEstresse NivelDaPrioridade(Prioridade prioridade) => prioridade switch
    {
        Prioridade.Urgente => NivelEstresse.Severo,
        Prioridade.Alta => NivelEstresse.Moderado,
        Prioridade.Media => NivelEstresse.Leve,
        _ => NivelEstresse.Nenhum
    };

    public static Button BotaoPrimario(string texto)
    {
        var b = BotaoBase(texto);
        b.BackColor = Primaria;
        b.ForeColor = Color.White;
        b.FlatAppearance.BorderColor = Primaria;
        b.FlatAppearance.MouseOverBackColor = PrimariaEscura;
        return b;
    }

    public static Button BotaoSecundario(string texto)
    {
        var b = BotaoBase(texto);
        b.BackColor = Color.White;
        b.ForeColor = Primaria;
        b.FlatAppearance.BorderColor = Primaria;
        b.FlatAppearance.MouseOverBackColor = PrimariaClara;
        return b;
    }

    private static Button BotaoBase(string texto) => new()
    {
        Text = texto,
        AutoSize = true,
        FlatStyle = FlatStyle.Flat,
        Font = Negrito,
        Padding = new Padding(10, 4, 10, 4),
        Margin = new Padding(0, 0, 8, 0),
        Cursor = Cursors.Hand,
        UseVisualStyleBackColor = false
    };

    /// <summary>Padroniza a aparência das tabelas do painel.</summary>
    public static void ConfigurarGrade(DataGridView grade)
    {
        grade.AutoGenerateColumns = false;
        grade.ReadOnly = true;
        grade.AllowUserToAddRows = false;
        grade.AllowUserToDeleteRows = false;
        grade.AllowUserToResizeRows = false;
        grade.MultiSelect = false;
        grade.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grade.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grade.RowHeadersVisible = false;
        grade.BackgroundColor = Color.White;
        grade.BorderStyle = BorderStyle.None;
        grade.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grade.GridColor = Borda;
        grade.EnableHeadersVisualStyles = false;
        grade.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grade.ColumnHeadersHeight = 34;
        grade.ColumnHeadersDefaultCellStyle.BackColor = PrimariaClara;
        grade.ColumnHeadersDefaultCellStyle.ForeColor = Texto;
        grade.ColumnHeadersDefaultCellStyle.Font = Negrito;
        grade.ColumnHeadersDefaultCellStyle.SelectionBackColor = PrimariaClara;
        grade.DefaultCellStyle.Font = Padrao;
        grade.DefaultCellStyle.ForeColor = Texto;
        grade.DefaultCellStyle.SelectionBackColor = Color.FromArgb(200, 228, 215);
        grade.DefaultCellStyle.SelectionForeColor = Texto;
        grade.DefaultCellStyle.Padding = new Padding(4, 0, 4, 0);
        grade.RowTemplate.Height = 30;
    }

    public static DataGridViewTextBoxColumn Coluna(string propriedade, string titulo, float peso, string? formato = null,
        DataGridViewContentAlignment alinhamento = DataGridViewContentAlignment.MiddleLeft)
    {
        var coluna = new DataGridViewTextBoxColumn
        {
            DataPropertyName = propriedade,
            Name = propriedade,
            HeaderText = titulo,
            FillWeight = peso,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        coluna.DefaultCellStyle.Alignment = alinhamento;
        if (formato is not null) coluna.DefaultCellStyle.Format = formato;
        return coluna;
    }
}
