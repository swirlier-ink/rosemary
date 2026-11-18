using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rosemary.Common;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using static System.Net.Mime.MediaTypeNames;

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
    public static void DrawName(SpriteBatch sb, Vector2 position, Color color)
    {
        foreach (var off in ChatManager.ShadowDirections)
        {
            sb.Draw(Assets.Authorship.flye_name.Asset.Value, position + off * 2f, Color.Black);
        }

        sb.Draw(Assets.Authorship.flye_name.Asset.Value, position, color);
    }
}

internal sealed class MaxieTag : CommonAuthorTag;

internal sealed class SprunoliaTag : CommonAuthorTag;

internal sealed class CharlotteTag : CommonAuthorTag
{
    public override void DrawIcon(SpriteBatch sb, Vector2 position)
    {
        if (!ModContent.RequestIfExists<Texture2D>(Texture, out var icon))
        {
            return;
        }

        sb.Draw(
            icon.Value,
            new Rectangle((int)position.X - 4, (int)position.Y - 5, 30, 28),
            Color.White
        );
    }
}

public sealed class AuthorTooltip(Mod mod) : ITooltipProvider
{
    bool ITooltipProvider.DrawPanel => true;

    public Vector2 GetSize()
    {
        var total = Vector2.Zero;

        var font = FontAssets.MouseText.Value;

        foreach (var authorTag in mod.GetContent<AuthorTag>())
        {
            var text = $"[dba:{authorTag.FullName}] {authorTag.DisplayName.Value}";

            var size = ChatManager.GetStringSize(font, text, Vector2.One);

            total.X = MathF.Max(total.X, size.X);
            total.Y += size.Y;
        }

        total.Y += 6 * 2;

        const string star = "✦";

        total.Y += ChatManager.GetStringSize(font, star, Vector2.One).Y * 2;

        return total;
    }

    void ITooltipProvider.Draw(SpriteBatch sb, Vector2 topLeft)
    {
        var color = new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, byte.MaxValue);

        var font = FontAssets.MouseText.Value;

        var totalSize = GetSize();

        DrawStar();
        DrawDivider();

        foreach (var authorTag in mod.GetContent<AuthorTag>())
        {
            var text = $"[dba:{authorTag.FullName}] {authorTag.DisplayName.Value}";

            // Edge case as AuthorTag doesn't provide GetSize
            if (authorTag is FlyeTag)
            {
                text = $"[dba:{authorTag.FullName}] ";

                var position = topLeft;

                position.X += ChatManager.GetStringSize(font, text, Vector2.One).X;
                position.Y -= 5;

                FlyeTag.DrawName(sb, position, color);
            }

            ChatManager.DrawColorCodedStringWithShadow(
                sb,
                font,
                text,
                topLeft,
                color,
                Color.Black,
                0f,
                Vector2.Zero,
                Vector2.One
            );

            topLeft.Y += ChatManager.GetStringSize(font, text, Vector2.One).Y;
        }

        topLeft.Y += 4;

        DrawDivider();
        DrawStar();

        return;

        void DrawDivider()
        {
            var texture = Main.Assets.Request<Texture2D>("Images/UI/CharCreation/Separator1").Value;

            var divColor = Color.Lerp(Color.White, new Color(63, 65, 151, 255), 0.85f) * 0.9f;

            var position = topLeft;
            position.Y -= 4;

            Utils.DrawPanel(texture, 2, 0, sb, position, totalSize.X, divColor);

            topLeft.Y += 6;
        }

        void DrawStar()
        {
            const string star = "✦";

            var size = ChatManager.GetStringSize(font, star, Vector2.One);

            var position = topLeft;
            position.X += totalSize.X * 0.5f;

            ChatManager.DrawColorCodedStringWithShadow(
                sb,
                font,
                star,
                position,
                color,
                Color.Black,
                0f,
                new Vector2(size.X * 0.5f, 0f),
                Vector2.One
            );

            topLeft.Y += size.Y;
        }
    }
}