using GotaInteligente.Painel.Ui;

namespace GotaInteligente.Painel.Telas;

/// <summary>Base dos formulários de cadastro: layout em duas colunas (rótulo + campo) e botões no rodapé.</summary>
public abstract class FormBase : Form
{
    protected readonly TableLayoutPanel Campos = new()
    {
        Dock = DockStyle.Fill,
        ColumnCount = 2,
        Padding = new Padding(20, 16, 28, 8),
        AutoScroll = true
    };

    protected readonly ErrorProvider Erros = new() { BlinkStyle = ErrorBlinkStyle.NeverBlink };
    protected readonly Button BtnSalvar = Tema.BotaoPrimario("Salvar");
    protected readonly Button BtnCancelar = Tema.BotaoSecundario("Cancelar");

    protected FormBase(string titulo, string descricao, Size tamanho)
    {
        Text = titulo;
        Font = Tema.Padrao;
        BackColor = Color.White;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = tamanho;
        Erros.ContainerControl = this;

        Campos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        Campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var cabecalho = new Panel { Dock = DockStyle.Top, Height = 84, BackColor = Tema.PrimariaClara, Padding = new Padding(20, 8, 20, 6) };
        cabecalho.Controls.Add(new Label
        {
            Text = descricao,
            Dock = DockStyle.Fill,
            ForeColor = Tema.PrimariaEscura,
            Font = Tema.Padrao,
            TextAlign = ContentAlignment.MiddleLeft
        });
        cabecalho.Controls.Add(new Label { Text = titulo, Dock = DockStyle.Top, Height = 24, Font = Tema.Negrito, ForeColor = Tema.PrimariaEscura });

        var rodape = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 56,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(12, 12, 12, 0),
            BackColor = Tema.Fundo
        };
        BtnCancelar.DialogResult = DialogResult.Cancel;
        rodape.Controls.Add(BtnSalvar);
        rodape.Controls.Add(BtnCancelar);
        AcceptButton = BtnSalvar;
        CancelButton = BtnCancelar;

        Controls.Add(Campos);
        Controls.Add(rodape);
        Controls.Add(cabecalho);
    }

    /// <summary>Adiciona uma linha "rótulo: campo" ao formulário.</summary>
    protected T AdicionarCampo<T>(string rotulo, T campo, int altura = 34) where T : Control
    {
        var linha = Campos.RowCount++;
        Campos.RowStyles.Add(new RowStyle(SizeType.Absolute, altura));

        var label = new Label
        {
            Text = rotulo,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = Tema.Negrito,
            ForeColor = Tema.Texto
        };
        // Campos de uma linha ficam centralizados na altura; os maiores ocupam a célula toda.
        if (altura > 34 || campo is TextBox { Multiline: true })
            campo.Dock = DockStyle.Fill;
        else
            campo.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        campo.Margin = new Padding(3, 4, 3, 4);

        Campos.Controls.Add(label, 0, linha);
        Campos.Controls.Add(campo, 1, linha);
        return campo;
    }

    /// <summary>Mostra cada erro de validação ao lado do campo correspondente.</summary>
    protected void MostrarErros(IReadOnlyDictionary<string, string> erros, IReadOnlyDictionary<string, Control> mapa)
    {
        Erros.Clear();
        var naoMapeados = new List<string>();

        foreach (var (campo, mensagem) in erros)
        {
            var controle = mapa.FirstOrDefault(m => string.Equals(m.Key, campo, StringComparison.OrdinalIgnoreCase)).Value;
            if (controle is not null) Erros.SetError(controle, mensagem);
            else naoMapeados.Add(mensagem);
        }

        var mensagemGeral = "Corrija os campos destacados:\n\n• " + string.Join("\n• ", erros.Values);
        MessageBox.Show(this, mensagemGeral, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}
