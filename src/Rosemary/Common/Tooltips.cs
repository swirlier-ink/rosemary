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
    Vector2 GetSize();

    void Draw(SpriteBatch sb, Vector2 topLeft);
}

public static class TooltipRenderer
{
    public static void SetTooltip(ITooltipProvider p)
    {
        Main.instance.MouseText(string.Empty);

        tooltip = p;
    }

    [OnLoad]
    private static void Load()
    {
        IL_Main.MouseTextInner += MouseTextInner_MousePhrase;

        MonoModHooks.Add(
            typeof(Main).GetMethod(
                nameof(Main.MouseTextInner),
                BindingFlags.Instance | BindingFlags.NonPublic
            ),
            MouseTextInner_MousePhrase_Reset
        );
    }

    private static ITooltipProvider? tooltip;

    private static void MouseTextInner_MousePhrase_Reset(Action<Main, Main.MouseTextCache> orig, Main self, Main.MouseTextCache info)
    {
        orig(self, info);

        tooltip = null;
    }

    private static void MouseTextInner_MousePhrase(ILContext il)
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

                tooltip.Draw(Main.spriteBatch, new Vector2(x, y));

                return true;
            }
        );

        c.EmitBrtrue(jumpDrawStringTarget);
    }

    extension(Main)
    {
        public static void SetCustomTooltip(ITooltipProvider p) => SetTooltip(p);
    }
}
