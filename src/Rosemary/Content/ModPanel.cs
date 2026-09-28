using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Rosemary.Common;
using System.Collections.Generic;
using System.Reflection;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace Rosemary.Content;

internal sealed class ModPanel
{
    [OnLoad]
    private static void Load()
    {
        MonoModHooks.Modify(
            typeof(UIMods).GetMethod(
                nameof(UIMods.Update),
                BindingFlags.Instance | BindingFlags.Public
            ),
            Update_DisplaceList
        );
    }

    private static void Update_DisplaceList(ILContext il)
    {
        var c = new ILCursor(il);

        var uiModsIndex = ParameterIndex.Invalid;

        c.GotoNext(
            MoveType.After,
            i => i.MatchLdarg(out uiModsIndex),
            i => i.MatchCallvirt<UIElement>(nameof(UIElement.Recalculate))
        );

        c.GotoNext(
            MoveType.Before,
            i => i.MatchRet()
        );

        c.EmitLdarg(uiModsIndex);
        c.EmitDelegate(
            static (UIMods self) =>
            {
                const int mods_to_displace = 4;

                var index = 0;
                UIModItem? element = null;

                for (var i = 0; i < self.modList.Count; i++)
                {
                    if (self.modList._items[i] is not UIModItem modItem)
                    {
                        continue;
                    }

                    // Sketchy?
                    modItem.Height.Pixels = 92f;
                    modItem.Width.Set(0f, 1f);
                    modItem.Left.Set(0f, 0f);
                    modItem.Top.Set(0f, 0f);

                    if (!ModLoader.TryGetMod(modItem._mod.Name, out var mod)
                     || mod is not ModImpl)
                    {
                        continue;
                    }

                    index = i;
                    element = modItem;
                }

                if (element is null)
                {
                    return;
                }

                self.modList.Remove(element);

                var displacedElements = new List<UIElement>();

                for (var j = 0; j < mods_to_displace; j++)
                {
                    if (index >= self.modList.Count)
                    {
                        break;
                    }

                    var cur = self.modList._items[index];

                    displacedElements.Add(cur);

                    self.modList.Remove(cur);
                }

                var panelSize = element.Height.Pixels;

                var container = new UIElement();
                {
                    container.Width.Set(0f, 1f);

                    container.Height.Set(panelSize * mods_to_displace, 0f);
                    container.Height.Add(self.modList.ListPadding * (mods_to_displace - 1), 0f);
                }
                self.modList._items.Insert(index, container);
                self.modList._innerList.Append(container);

                {
                    element.Width.Set(panelSize, 0f);
                    element.Height.Set(0f, 1f);

                    // Delete button cannot manifest while this edit is active, thus we can ignore handling it.
                    element._moreInfoButton.Top.Set(-2f, 0f);
                    element._moreInfoButton.Left.Set(-2f, 0f);
                    element._moreInfoButton.HAlign = 1f;
                    element._moreInfoButton.VAlign = 1f;

                    var bottomOffset = element._moreInfoButton.Height.Pixels + 2;

                    if (element._configButton is { } config)
                    {
                        config.Top.Set(-2f, 0f);
                        config.Left.Set(2f, 0f);
                        config.HAlign = 0f;
                        config.VAlign = 1f;

                        element._rateButton?.Left.Set(-2f, 0f);
                        element._rateButton?.HAlign = 1f;
                        element._rateButton?.Top.Set(-2f, 0f);
                        element._rateButton?.Top.Sub(config.Height.Pixels + 4, 0f);

                        if (element._rateButton is not null)
                        {
                            bottomOffset += config.Height.Pixels + 4;
                        }
                    }
                    else
                    {
                        element._rateButton?.Left.Set(2f, 0f);
                        element._rateButton?.HAlign = 0f;
                        element._rateButton?.Top.Set(-2f, 0f);
                    }

                    element._rateButton?.VAlign = 1f;

                    if (element._uiModStateText is { } stateText)
                    {
                        stateText.HAlign = 0f;
                        stateText.VAlign = 1f;

                        stateText.Left.Set(0f, 0f);
                        stateText.Top.Set(4f, 0f);
                        stateText.Top.Sub(bottomOffset, 0f);

                        stateText.OnDraw += OnDraw_SetWidth;

                        bottomOffset += stateText.Height.Pixels + 4f;
                    }

                    if (element._modReferenceIcon is { } depsIcon)
                    {
                        depsIcon.HAlign = 0f;
                        depsIcon.VAlign = 1f;

                        depsIcon.Left.Set(0f, 0f);
                        depsIcon.Top.Set(4f, 0f);
                        depsIcon.Top.Sub(bottomOffset, 0f);
                    }

                    element._modName.Remove();
                }
                container.Append(element);

                for (var j = 0; j < displacedElements.Count; j++)
                {
                    var dElement = displacedElements[j];
                    {
                        dElement.Left.Set(element.Width.Pixels, 0f);
                        dElement.Left.Add(self.modList.ListPadding, 0f);

                        dElement.Width.Set(-dElement.Left.Pixels, 1f);

                        dElement.Top.Set((panelSize + self.modList.ListPadding) * j, 0f);

                        if (dElement is UIModItem dModItem)
                        {
                            dModItem.tMLUpdateRequired?.MaxWidth.Set(271f, 0f);
                        }
                    }
                    container.Append(dElement);
                }

                container.Activate();
                container.Recalculate();

                self.modList.Recalculate();

                return;

                static void OnDraw_SetWidth(UIElement affectedElement, SpriteBatch sb)
                {
                    affectedElement.Width.Pixels = affectedElement.Dimensions.Width;
                }
            }
        );
    }
}
