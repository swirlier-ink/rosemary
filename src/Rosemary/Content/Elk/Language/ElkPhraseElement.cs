using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.UI;

namespace Rosemary.Content.Elk;

public class ElkPhraseElement(ElkPhrase name) : UIElement
{
    public float Scale { get; set; } = 1f;

    public Color Color { get; set; } = Color.White;

    public Color ShadowColor { get; set; } = Color.Black;

    public override void Recalculate()
    {
        base.Recalculate();

        var size = name.Measure(Scale);

        MinHeight.Set(size.Y + PaddingTop + PaddingBottom, 0f);
    }

    protected override void DrawSelf(SpriteBatch sb)
    {
        base.DrawSelf(sb);

        var position = this.Dimensions.Top();

        var size = name.Measure(1f);

        var origin = new Vector2(size.X * 0.5f, 0f);

        sb.DrawPhraseWithOutline(name, position, Color, ShadowColor, Scale, origin);
    }
}
