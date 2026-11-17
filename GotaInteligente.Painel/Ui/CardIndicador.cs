namespace GotaInteligente.Painel.Ui;

/// <summary>Card de indicador com título, valor em destaque e uma nota explicativa.</summary>
public sealed class CardIndicador : Panel
{
    private readonly Label _titulo = new() { AutoSize = false, Dock = DockStyle.Top, Height = 22, Font = Tema.Negrito, ForeColor = Tema.TextoSuave };
    private readonly Label _valor = new() { AutoSize = false, Dock = DockStyle.Top, Height = 44, Font = Tema.ValorCard, ForeColor = Tema.Texto };
    private readonly Label _nota = new() { AutoSize = false, AutoEllipsis = true, Dock = DockStyle.Fill, Font = Tema.Subtitulo, ForeColor = Tema.TextoSuave };
    private Color _destaque;

    public CardIndicador(string titulo, Color destaque)
    {
        _destaque = destaque;
        BackColor = Color.White;
        Margin = new Padding(0, 0, 12, 0);
        Padding = new Padding(16, 10, 12, 8);
        Dock = DockStyle.Fill;
        DoubleBuffered = true;

        _titulo.Text = titulo;
        Controls.Add(_nota);
        Controls.Add(_valor);
        Controls.Add(_titulo);
    }

    public void Atualizar(string valor, string nota, Color? destaque = null)
    {
        _valor.Text = valor;
        _nota.Text = nota;
        if (destaque is { } cor)
        {
            _destaque = cor;
            _valor.ForeColor = cor;
        }
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var faixa = new SolidBrush(_destaque);
        e.Graphics.FillRectangle(faixa, 0, 0, 5, Height);
        using var borda = new Pen(Tema.Borda);
        e.Graphics.DrawRectangle(borda, 0, 0, Width - 1, Height - 1);
    }
}
