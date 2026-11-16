using GotaInteligente.Painel.Dominio;

namespace GotaInteligente.Painel.Ui;

/// <summary>Barra horizontal empilhada com a quantidade de talhões em cada nível de estresse.</summary>
public sealed class BarraDistribuicao : Control
{
    private static readonly NivelEstresse[] Ordem =
        { NivelEstresse.Severo, NivelEstresse.Moderado, NivelEstresse.Leve, NivelEstresse.Nenhum, NivelEstresse.SemDados };

    private Dictionary<NivelEstresse, int> _contagem = new();

    public BarraDistribuicao()
    {
        DoubleBuffered = true;
        Height = 54;
        Dock = DockStyle.Fill;
        BackColor = Color.White;
        Font = Tema.Subtitulo;
    }

    public void Atualizar(IEnumerable<FazendaMonitorada> fazendas)
    {
        _contagem = fazendas.SelectMany(f => f.Talhoes)
            .GroupBy(t => t.Nivel)
            .ToDictionary(g => g.Key, g => g.Count());
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        var total = _contagem.Values.Sum();
        var area = new Rectangle(12, 8, Width - 24, 16);
        using (var vazio = new SolidBrush(Tema.Borda)) g.FillRectangle(vazio, area);
        if (total == 0) return;

        // Segmentos proporcionais, do pior para o melhor nível.
        var x = (float)area.X;
        foreach (var nivel in Ordem)
        {
            if (!_contagem.TryGetValue(nivel, out var qtd) || qtd == 0) continue;
            var largura = area.Width * qtd / (float)total;
            using var pincel = new SolidBrush(Tema.CorTexto(nivel));
            g.FillRectangle(pincel, x, area.Y, largura, area.Height);
            x += largura;
        }

        // Legenda
        var lx = 12;
        foreach (var nivel in Ordem)
        {
            if (!_contagem.TryGetValue(nivel, out var qtd) || qtd == 0) continue;
            using var pincel = new SolidBrush(Tema.CorTexto(nivel));
            g.FillRectangle(pincel, lx, 32, 10, 10);
            var texto = $"{nivel.Descricao()}: {qtd}";
            TextRenderer.DrawText(g, texto, Font, new Point(lx + 14, 29), Tema.Texto);
            lx += 14 + TextRenderer.MeasureText(texto, Font).Width + 14;
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Invalidate();
    }
}
