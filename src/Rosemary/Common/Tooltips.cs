using MonoMod.Cil;
using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace Rosemary.Common;

public interface ITooltipProvider
{
    bool DrawPanel => true;

    Vector2 GetSize();

    void Draw(SpriteBatch sb, Vector2 topLeft);
}

public static class TooltipRenderer
{
    private static void SetTooltip(ITooltipProvider p)
    {
        Main.instance.MouseText(string.Empty);
        Main.ClearHoverItem();

        tooltip = p;
    }

    [OnLoad]
    private static void Load()
    {
        IL_Main.MouseTextInner += MouseTextInner_CustomTooltip;

        MonoModHooks.Add(
            typeof(Main).GetMethod(
                nameof(Main.MouseTextInner),
                BindingFlags.Instance | BindingFlags.NonPublic
            ),
            MouseTextInner_CustomTooltip_Reset
        );
    }

    private static ITooltipProvider? tooltip;

    private static void MouseTextInner_CustomTooltip_Reset(Action<Main, Main.MouseTextCache> orig, Main self, Main.MouseTextCache info)
    {
        orig(self, info);

        tooltip = null;
    }

    private static void MouseTextInner_CustomTooltip(ILContext il)
    {
        var c = new ILCursor(il);

        var bigXIndex = VariableIndex.Invalid;
        var bigYIndex = VariableIndex.Invalid;

        var jumpDrawStringTarget = c.DefineLabel();

        c.GotoNext(
            MoveType.After,
            i => i.MatchCall(typeof(ChatManager), nameof(ChatManager.GetStringSize))
        );

        c.EmitDelegate(
            static (Vector2 originalSize) =>
            {
                if (tooltip is null)
                {
                    return originalSize;
                }

                var size = tooltip.GetSize();

                if (tooltip.DrawPanel
                 && Main.SettingsEnabled_OpaqueBoxBehindTooltips)
                {
                    size.X += 8 + 18;
                    size.Y += 2 + 18;
                }

                return tooltip.GetSize();
            }
        );

        c.GotoNext(
            i => i.MatchLdfld<Main.MouseTextCache>(nameof(Main.MouseTextCache.buffTooltip)),
            i => i.MatchLdloca(out bigXIndex),
            i => i.MatchLdloca(out bigYIndex)
        );

        c.GotoNext(
            MoveType.After,
            i => i.MatchCall(typeof(ChatManager), nameof(ChatManager.DrawColorCodedStringWithShadow))
        );

        c.MarkLabel(jumpDrawStringTarget);

        c.GotoPrev(
            MoveType.Before,
            i => i.MatchLdsfld<Main>(nameof(Main.spriteBatch))
        );

        c.MoveAfterLabels();

        c.EmitLdloc(bigXIndex);
        c.EmitLdloc(bigYIndex);

        c.EmitDelegate(
            static (int x, int y) =>
            {
                if (tooltip is null)
                {
                    return false;
                }

                var sb = Main.spriteBatch;

                if (tooltip.DrawPanel
                 && Main.SettingsEnabled_OpaqueBoxBehindTooltips)
                {
                    x += 8;
                    y += 2;

                    var color = new Color(23, 25, 81, 255) * 0.925f;

                    var size = tooltip.GetSize();

                    Utils.DrawInvBG(sb, new Rectangle(x - 14, y - 9, (int)size.X + 14 * 2, (int)size.Y + 9 + (9 / 2)), color);
                }

                tooltip.Draw(sb, new Vector2(x, y));

                return true;
            }
        );

        c.EmitBrtrue(jumpDrawStringTarget);
    }

    extension(Main)
    {
        public static void SetCustomTooltip(ITooltipProvider p) => SetTooltip(p);

        public static ITooltipProvider CustomTooltip
        {
            set => SetTooltip(value);
        }
    }
}
