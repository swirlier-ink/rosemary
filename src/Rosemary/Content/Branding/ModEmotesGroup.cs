using Microsoft.Xna.Framework;
using MonoMod.Cil;
using Rosemary.Common;
using System.Collections.Generic;
using Rosemary.Content.Elk;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace Rosemary.Content;

internal static class ModEmotesGroup
{
    [OnLoad]
    private static void Load()
    {
        // The bubbles look particularly unpleasant in vanilla, should we even be doing this?
        IL_EmoteButton.DrawSelf += DrawSelf_RoundPosition;

        On_UIEmotesMenu.TryAddingList += TryAddingList_RosemaryGroup;
        IL_UIEmotesMenu.InitializePage += _ => { };
    }

    private static void TryAddingList_RosemaryGroup(
        On_UIEmotesMenu.orig_TryAddingList orig,
        UIEmotesMenu self,
        LocalizedText title,
        ref int currentGroupIndex,
        int maxEmotesPerRow,
        List<int>? emoteIds
    )
    {
        if (title.Key != ModContent.GetInstance<ModImpl>().Name)
        {
            orig(self, title, ref currentGroupIndex, maxEmotesPerRow, emoteIds);

            return;
        }

        if (emoteIds is null || emoteIds.Count == 0)
        {
            return;
        }

        self._list.Add(new RosemaryEmotesGroupElement(currentGroupIndex++, emoteIds.ToArray()));
    }

    private static void DrawSelf_RoundPosition(ILContext il)
    {
        var c = new ILCursor(il);

        var positionIndex = VariableIndex.Invalid;

        c.GotoNext(
            MoveType.After,
            i => i.MatchCall<Vector2>("op_Addition"),
            i => i.MatchStloc(out positionIndex)
        );

        c.EmitLdloca(positionIndex);

        c.EmitDelegate(
            static (ref Vector2 position) =>
            {
                position = Utils.Round(position);
            }
        );
    }

    private sealed class RosemaryEmotesGroupElement : EmotesGroupListItem
    {
        public RosemaryEmotesGroupElement(int groupIndex, params int[] emotes)
            : base(LocalizedText.Empty, groupIndex, 14, emotes)
        {
            RemoveAllChildren();

            PaddingLeft = 6f;
            PaddingRight = 6f;

            var nameContainer = new UIElement();
            {
                nameContainer.Width.Set(35f, 0f);
                nameContainer.Height.Set(0f, 1f);
            }
            Append(nameContainer);

            var separator = new UIVerticalSeparator();
            {
                separator.Height.Set(0f, 1f);
                separator.HAlign = 1f;
                separator.Color = new Color(81, 83, 149, 229);
            }
            nameContainer.Append(separator);

            var name = new ElkPhraseElement(ModImpl.SMALL_SCRIPT_ELK_NAME);
            {
                name.Width.Set(0f, 1f);
                name.VAlign = 0.5f;
                name.Scale = 0.8f;
            }
            nameContainer.Append(name);

            MinHeight = name.MinHeight;
            MinHeight.Add(12f, 0f);

            var emoteContainer = new UIElement();
            {
                emoteContainer.Width.Set(-35f, 1f);
                emoteContainer.Height.Set(0f, 1f);
                emoteContainer.HAlign = 1f;
            }
            Append(emoteContainer);

            var iterator = 0;
            var height = 0f;

            foreach (var emote in emotes)
            {
                if (iterator % _maxEmotesPerRow == 0)
                {
                    height += 36f;
                }

                var row = iterator / _maxEmotesPerRow;
                var col = iterator % _maxEmotesPerRow;

                var bubble = new EmoteButton(emote);
                {
                    bubble.Top.Set(row * 36f, 0f);
                    bubble.Left.Set(col * 36f + 6f, 0f);
                }
                emoteContainer.Append(bubble);

                bubble.SetSnapPoint("Group " + groupIndex, iterator);

                iterator++;
            }

            Height.Set(height, 0f);
        }
    }
}
