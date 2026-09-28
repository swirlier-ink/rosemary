using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Rosemary.Common;
using Rosemary.Content.Elk;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Rosemary.Content;

public sealed class TempConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;
}

internal sealed class ModPanel
{
    private static readonly ElkPhrase rosemary =
        ElkLanguage.NewPhrase
                   .CurlB       .UseHeight(6f).UseOffset(new Vector2(-16f, 0f))
                   .Rosemary                  .UseOffset(new Vector2(-8f, 4f))
                   .BranchRightB.UseHeight(5f).UseOffset(new Vector2(17f, 0f))
                   .DotSmall    .UseHeight(0f).UseOffset(new Vector2(-20f, -8f))
                   .FullStop;

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

                        stateText.Left.Set(2f, 0f);
                        stateText.Top.Set(4f, 0f);
                        stateText.Top.Sub(bottomOffset, 0f);

                        stateText.OnDrawExt += OnDraw_SetWidth;

                        bottomOffset += stateText.Height.Pixels + 4f;
                    }

                    if (element._modReferenceIcon is { } depsIcon)
                    {
                        depsIcon.HAlign = 0f;
                        depsIcon.VAlign = 1f;

                        depsIcon.Left.Set(2f, 0f);
                        depsIcon.Top.Set(4f, 0f);
                        depsIcon.Top.Sub(bottomOffset, 0f);
                    }

                    var name = rosemary;

                    element._modName.Remove();
                    element._modName = new ElkLangModName(name, $"v{element._mod.modFile.Version}");
                    {
                        element._modName.HAlign = 0.5f;
                        element._modName.Width.Set(80f, 0f);
                        element._modName.Height.Set(name.Measure(1f).Y, 0f);
                        element._modName.Top.Set(6f, 0f);
                    }
                    element.Append(element._modName);

                    element.Elements.RemoveAll(e => e is UIHoverImage);
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

                static void OnDraw_SetWidth(UIModStateText element, SpriteBatch sb)
                {
                    var textSize = FontAssets.MouseText.Value.MeasureString(element.DisplayText).X;

                    element.Width.Pixels = element.Parent.InnerDimensions.Width - 4f;
                    element.PaddingLeft = (int)(5 + (((element.Width.Pixels - 10) - textSize) * 0.5f));
                }
            }
        );
    }

    private sealed class ElkLangModName(ElkPhrase name, string version) : UIText(string.Empty)
    {
        protected override void DrawSelf(SpriteBatch sb)
        {
            var position = this.Dimensions.Top();
            const float scale = 1f;
            const float version_scale = 0.9f;
            var size = name.Measure(scale);
            var origin = new Vector2(size.X * 0.5f, 0f);

            sb.DrawPhraseWithOutline(name, position, Color.White, Color.Black, 1f, origin);

            DrawVersionText();

            return;

            void DrawVersionText()
            {
                var font = FontAssets.MouseText.Value;

                var lastCharacterHeight = name[^1].Height - name[^1].Position.Y;

                var versionPosition = new Vector2(position.X + (10f * scale), position.Y + size.Y - (lastCharacterHeight * 0.5f * scale));
                versionPosition -= origin * scale;

                var versionRotation = -MathF.PiOver2;

                var versionSize = font.MeasureString(version);

                var versionOrigin = versionSize * new Vector2(0.5f, 1f);

                ChatManager.DrawColorCodedStringWithShadow(
                    sb,
                    font,
                    version,
                    versionPosition,
                    Color.White,
                    Color.Black,
                    versionRotation,
                    versionOrigin,
                    new Vector2(version_scale * scale)
                );
            }
        }
    }
}
