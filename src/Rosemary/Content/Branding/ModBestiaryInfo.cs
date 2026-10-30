using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Rosemary.Common;
using System.Collections.Generic;
using Rosemary.Content.Elk;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace Rosemary.Content;

internal static class ModBestiaryInfo
{
    [OnLoad]
    private static void Load()
    {
        IL_UIBestiaryEntryInfoPage.AddInfoToList += AddInfoToList_CacheElementProviders;
        On_UIBestiaryEntryInfoPage.AddInfoToList += AddInfoToList_Displace;
    }

    private static readonly Dictionary<int, IBestiaryInfoElement> providers = [];

    private static void AddInfoToList_CacheElementProviders(ILContext il)
    {
        var c = new ILCursor(il);

        var elementIndex = VariableIndex.Invalid;

        var selfIndex = ParameterIndex.Invalid;

        c.GotoNext(
            MoveType.Before,
            i => i.MatchLdloc(out int _),
            i => i.MatchCallvirt<IBestiaryInfoElement>(nameof(IBestiaryInfoElement.ProvideUIElement)),
            i => i.MatchStloc(out elementIndex)
        );

        c.EmitDup();

        c.GotoNext(
            MoveType.After,
            i => i.MatchStloc(elementIndex)
        );

        c.FindNext(
            out _,
            i => i.MatchLdarg(out selfIndex)
        );

        c.EmitLdloc(elementIndex);
        c.EmitLdarg(selfIndex);
        c.EmitDelegate(
            static (IBestiaryInfoElement info, UIElement? element, UIBestiaryEntryInfoPage self) =>
            {
                if (element is not null)
                {
                    providers[self._list.Count] = info;
                }
            }
        );
    }

    private static void AddInfoToList_Displace(On_UIBestiaryEntryInfoPage.orig_AddInfoToList orig, UIBestiaryEntryInfoPage self, BestiaryEntry entry, ExtraBestiaryInfoPageInformation extraInfo)
    {
        orig(self, entry, extraInfo);

        var list = self._list;

        var index = -1;

        var targetHeight = (34f * 4f) + (list.ListPadding * 3f);

        var container = new UIElement();
        {
            container.Width.Set(0f, 5f);
            container.Height.Set(targetHeight, 0f);
        }

        for (var i = 0; i < list.Count; i++)
        {
            if (!providers.TryGetValue(i, out var v)
             || v is not RosemarySourceBestiaryInfoElement)
            {
                continue;
            }

            index = i;

            var rsmry = list._items[index];

            // Rosemary
            list.Remove(rsmry);
            container.Append(rsmry);
        }

        if (index == -1)
        {
            return;
        }

        var height = 0f;

        while (height < targetHeight
            && index < list.Count
            && list._items[index] is not UIHorizontalSeparator)
        {
            var displacedElement = list._items[index];

            displacedElement.Top.Add(height, 0f);
            displacedElement.Left.Add(34f + list.ListPadding, 0f);
            displacedElement.Width.Sub(34f + list.ListPadding, 0f);

            height += list._items[index].Dimensions.Height + list.ListPadding;

            list.Remove(displacedElement);
            container.Append(displacedElement);
        }

        // TODO: Insert util
        list._items.Insert(index, container);
        list._innerList.Append(container);

        providers.Clear();
    }

    public sealed class RosemarySourceBestiaryInfoElement(Mod mod, string displayName)
        : ModSourceBestiaryInfoElement(mod, displayName),
          IBestiaryInfoElement,
          IBestiaryPrioritizedElement
    {
        public float OrderPriority => float.MaxValue;

        UIElement? IBestiaryInfoElement.ProvideUIElement(BestiaryUICollectionInfo info)
        {
            if (info.UnlockState == BestiaryEntryUnlockState.NotKnownAtAll_0)
            {
                return null;
            }

            var element = new UIPanel(Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Stat_Panel"), null, 12, 7);
            {
                element.Width.Set(34f, 0f);
                element.Height.Set(0f, 1f);
                element.Left.Set(5f, 0f);

                element.BackgroundColor = new Color(43, 56, 101);
                element.BorderColor = Color.Transparent;
                element.SetPadding(0f);

                element.OnDraw += OnDraw_ElkLangTooltip;
            }

            var filterImage = GetFilterImage();
            {
                filterImage.VAlign = 0f;
                filterImage.Top.Set(5f, 0f);
                filterImage.IgnoresMouseInteraction = true;
            }
            element.Append(filterImage);

            var name = new ElkPhraseElement(ModImpl.SMALL_SCRIPT_ELK_NAME);
            {
                name.Scale = 0.75f;
                name.Top.Set(38f, 0f);
                name.Width.Set(0f, 1f);
                name.IgnoresMouseInteraction = true;
            }
            element.Append(name);

            return element;

            static void OnDraw_ElkLangTooltip(UIElement element, SpriteBatch sb)
            {
                if (!element.IsMouseHovering)
                {
                    return;
                }

                Main.instance.MouseText(string.Empty);
                ElkLanguage.MousePhrase = ModImpl.ELK_NAME;
            }
        }
    }
}
