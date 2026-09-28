using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Reflection;
using MonoMod.Cil;
using Rosemary.Common;
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

                for (var j = 0; j < 3; j++)
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

                    container.Height.Set(panelSize * 3, 0f);
                    container.Height.Add(self.modList.ListPadding * 2, 0f);
                }
                self.modList._items.Insert(index, container);
                self.modList._innerList.Append(container);

                {
                    element.Width.Set(panelSize, 0f);
                    element.Height.Set(0f, 1f);
                }
                container.Append(element);

                for (var j = 0; j < displacedElements.Count; j++)
                {
                    {
                        displacedElements[j].Left.Set(element.Width.Pixels, 0f);
                        displacedElements[j].Left.Add(self.modList.ListPadding, 0f);

                        displacedElements[j].Width.Set(-displacedElements[j].Left.Pixels, 1f);

                        displacedElements[j].Top.Set((panelSize + self.modList.ListPadding) * j, 0f);

                    }
                    container.Append(displacedElements[j]);
                }

                container.Activate();
                container.Recalculate();

                self.modList.Recalculate();
            }
        );
    }
}
