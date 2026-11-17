using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.UI.Chat;

namespace Rosemary.Content.Authorship;

internal abstract class CommonAuthorTag : AuthorTag
{
    private const string suffix = "Tag";

    public override string Name => base.Name.EndsWith(suffix) ? base.Name[..^suffix.Length] : base.Name;

    public override string Texture => string.Join('/', Assets.Authorship.Incattus.KEY.Split('/')[..^1]) + '/' + Name;
}

internal sealed class ZoeyTag : Daybreak.Content.Authorship.ZoeyTag;

internal sealed class IncattusTag : CommonAuthorTag;

internal sealed class FlyeTag : CommonAuthorTag
{
    public override void DrawIcon(SpriteBatch sb, Vector2 position)
    {
        base.DrawIcon(sb, position);

        position.X += 26;
        position.X += ChatManager.GetStringSize(FontAssets.MouseText.Value, " ", Vector2.One).X;

        position.Y -= 5;

        foreach (var off in ChatManager.ShadowDirections)
        {
            sb.Draw(Assets.Authorship.flye_name.Asset.Value, position + off * 2f, Color.Black);
        }

        var color = new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, byte.MaxValue);

        sb.Draw(Assets.Authorship.flye_name.Asset.Value, position, color);
    }
}

internal sealed class MaxieTag : CommonAuthorTag;

internal sealed class SprunoliaTag : CommonAuthorTag;

internal sealed class CharlotteTag : CommonAuthorTag;