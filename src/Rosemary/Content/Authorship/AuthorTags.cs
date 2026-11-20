using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rosemary.Common;
using System;
using System.Linq;
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
    private const float dedications_scale = 0.75f;

    private const float padding = 16f;

    bool ITooltipProvider.DrawPanel => true;

    public Vector2 GetSize()
    {
        var size = AuthorSize();

        size.X += DedicationsWidth(mod.GetContent<AuthorTag>().Count()) + padding;

        return size;
    }

    private Vector2 AuthorSize()
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

    private float DedicationsWidth(int num)
    {
        num = (int)(num / dedications_scale);

        var font = FontAssets.MouseText.Value;

        var text = Mods.Rosemary.Dedications.GetTextValue();
        var lines = text.Split('\n');
;
        var count = 0;

        var curWidth = 0f;

        var total = 0f;

        foreach (var line in lines)
        {
            var size = ChatManager.GetStringSize(font, line, Vector2.One * dedications_scale);

            curWidth = MathF.Max(curWidth, size.X);

            count++;

            if (count >= num)
            {
                count = 0;

                total += curWidth + 3;

                curWidth = 0f;
            }
        }

        total += curWidth;

        return total;
    }

    void ITooltipProvider.Draw(SpriteBatch sb, Vector2 topLeft)
    {
        var color = new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, byte.MaxValue);

        var font = FontAssets.MouseText.Value;

        var authorSize = AuthorSize();

        var top = topLeft.Y;

        DrawLabel(authorSize.X);
        DrawDivider(authorSize.X);

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

        var bot = topLeft.Y;

        DrawDivider(authorSize.X);
        DrawLabel(authorSize.X);

        topLeft.Y = top;

        DrawDedications();

        return;

        void DrawDedications()
        {
            topLeft.X += authorSize.X + padding;

            var num = mod.GetContent<AuthorTag>().Count();

            var width = DedicationsWidth(num);

            num = (int)(num / dedications_scale);

            DrawLabel(width, "✧");
            DrawDivider(width);

            var text = Mods.Rosemary.Dedications.GetTextValue();
            var lines = text.Split('\n');

            var position = topLeft;

            var count = 0;

            var curWidth = 0f;

            foreach (var line in lines)
            {
                ChatManager.DrawColorCodedStringWithShadow(
                    sb,
                    font,
                    line,
                    position,
                    color,
                    Color.Black,
                    0f,
                    Vector2.Zero,
                    Vector2.One * dedications_scale
                );

                var size = ChatManager.GetStringSize(font, line, Vector2.One * dedications_scale);

                curWidth = MathF.Max(curWidth, size.X);

                position.Y += size.Y;

                count++;

                if (count >= num)
                {
                    count = 0;

                    position.Y = topLeft.Y;
                    position.X += curWidth + 3;

                    curWidth = 0f;
                }
            }

            topLeft.Y = bot;

            DrawDivider(width);
            DrawLabel(width, "✧");
        }

        void DrawDivider(float width)
        {
            var texture = Main.Assets.Request<Texture2D>("Images/UI/CharCreation/Separator1").Value;

            var divColor = Color.Lerp(Color.White, new Color(63, 65, 151, 255), 0.85f) * 0.9f;

            var position = topLeft;
            position.Y -= 4;

            Utils.DrawPanel(texture, 2, 0, sb, position, width, divColor);

            topLeft.Y += 6;
        }

        void DrawLabel(float width, string star = "✦")
        {
            var size = ChatManager.GetStringSize(font, star, Vector2.One);

            var position = topLeft;
            position.X += width * 0.5f;

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