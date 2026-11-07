using GoldMeridian.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Rosemary.Common;
using Rosemary.Content.Elk;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI.Chat;

namespace Rosemary.Content;

[ExtensionDataFor<UIBestiaryBar.BestiaryBarItem>]
internal sealed class BestiaryBarItemData
{
    public required bool UseElkName { get; set; }

    public required string Percent { get; set; }
}

public static class ModBestiaryPercentage
{
    private sealed class Tooltip(ElkPhrase phrase, string percent) : ITooltipProvider
    {
        public Vector2 GetSize()
        {
            var size = phrase.Measure(1f);

            var font = FontAssets.MouseText.Value;

            var percentSize = font.MeasureString(percent);

            var origPercentWidth = percentSize.X;

            percentSize /= percentSize.X;
            percentSize *= MathF.Min(origPercentWidth, size.X + 13);

            size.Y += percentSize.Y;

            return size;
        }

        public void Draw(SpriteBatch sb, Vector2 topLeft)
        {
            var size = phrase.Measure(1f);

            topLeft += new Vector2(6f, 8f);

            sb.DrawPhraseWithOutline(phrase, topLeft, Color.White, Color.Black, 1f, Vector2.Zero);

            DrawStack();
            return;

            void DrawStack()
            {
                var font = FontAssets.MouseText.Value;

                var position = new Vector2(topLeft.X + (size.X * 0.5f), topLeft.Y + size.Y);

                var percentSize = font.MeasureString(percent);

                var scale = (size.X + 13) / percentSize.X;
                scale = MathF.Min(1f, scale);

                var origin = percentSize * new Vector2(0.5f, 0f);

                ChatManager.DrawColorCodedStringWithShadow(
                    sb,
                    font,
                    percent,
                    position,
                    Color.White,
                    Color.Black,
                    0f,
                    origin,
                    new Vector2(scale)
                );
            }
        }
    }

    [OnLoad]
    private static void Load()
    {
        MonoModHooks.Modify(
            typeof(UIBestiaryBar).GetMethod(
                nameof(UIBestiaryBar.RecalculateBars),
                BindingFlags.Instance | BindingFlags.Public
            ),
            RecalculateBars_InjectData
        );

        MonoModHooks.Modify(
            typeof(UIBestiaryBar).GetMethod(
                "DrawSelf",
                BindingFlags.Instance | BindingFlags.NonPublic
            ),
            DrawSelf_Tooltip
        );
    }

    private static void DrawSelf_Tooltip(ILContext il)
    {
        var c = new ILCursor(il);

        ILLabel? skipMouseTextLabel = null;

        var bestiaryBarItemIndex = VariableIndex.Invalid;

        c.GotoNext(
            MoveType.Before,
            i => i.MatchLdsfld<Main>(nameof(Main.instance)),
            i => i.MatchLdloc(out bestiaryBarItemIndex)
        );

        c.FindPrev(
            out _,
            i => i.MatchBrfalse(out skipMouseTextLabel)
        );

        Debug.Assert(skipMouseTextLabel is not null);

        c.EmitLdloc(bestiaryBarItemIndex);
        c.EmitDelegate(
            static (UIBestiaryBar.BestiaryBarItem item) =>
            {
                if (item.Data is not { UseElkName: true })
                {
                    return false; 
                }

                Main.SetCustomTooltip(new Tooltip(ModImpl.SMALL_SCRIPT_ELK_NAME, item.Data.Percent));

                return true;
            }
        );
        c.EmitBrtrue(skipMouseTextLabel);
    }

    private static void RecalculateBars_InjectData(ILContext il)
    {
        var c = new ILCursor(il);

        var modIndexIndex = VariableIndex.Invalid;
        var collectedCountIndex = VariableIndex.Invalid;
        var itemsIndex = VariableIndex.Invalid;

        c.GotoNext(
            MoveType.Before,
            i => i.MatchRet()
        );

        c.GotoPrev(
            MoveType.After,
            i => i.MatchNewobj<UIBestiaryBar.BestiaryBarItem>()
        );

        {
            var c2 = c.Clone();

            c2.GotoPrev(
                MoveType.Before,
                i => i.MatchLdloc(out modIndexIndex),
                i => i.MatchLdelemRef(),
                i => i.MatchCallvirt<Mod>($"get_{nameof(Mod.DisplayName)}")
            );

            c2.GotoPrev(
                MoveType.Before,
                i => i.MatchStloc(out collectedCountIndex),
                i => i.MatchLdarg(out int _)
            );

            // Use FindNext/Prev's gap permission to skip over a nasty compiler generated method
            c2.FindPrev(
                out _,
                i => i.MatchLdloc(out itemsIndex),
                i => i.MatchDup(),
                i => i.MatchBrtrue(out _)
            );
        }

        c.EmitDup();
        c.EmitLdloc(modIndexIndex);
        c.EmitLdloc(itemsIndex);
        c.EmitLdloc(collectedCountIndex);
        c.EmitDelegate(
            static (UIBestiaryBar.BestiaryBarItem item, int modIndex, List<BestiaryEntry> items, int collected) =>
            {
                var mod = ModLoader.Mods[modIndex];

                if (mod is not ModImpl)
                {
                    return;
                }

                item.Data = new BestiaryBarItemData
                {
                    UseElkName = true,
                    Percent = $"{collected / (float)items.Count * 100f:N2}%",
                };
            }
        );
    }
}
