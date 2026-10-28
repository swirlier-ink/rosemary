using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Rosemary.Common;
using Rosemary.Content.Elk;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.UI;
using Terraria.Social.Steam;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Rosemary.Content;

public sealed class TempConfig : ModConfig { public override ConfigScope Mode => ConfigScope.ClientSide; }

internal sealed class ModPanel
{
    [OnLoad(Side = ModSide.Client)]
    private static void Load()
    {
        MonoModHooks.Modify(
            typeof(UIMods).GetMethod(
                nameof(UIMods.Update),
                BindingFlags.Instance | BindingFlags.Public
            ),
            Update_DisplaceList
        );

        // Redo following DB's re-impl of ModPanels (keep this impl in the file for the aura factor tho)
        MonoModHooks.Modify(
            typeof(UIModItem).GetMethod(
                "DrawSelf",
                BindingFlags.Instance | BindingFlags.NonPublic
            ),
            DrawSelf_HideErroneousVisuals
        );

        MonoModHooks.Modify(
            typeof(UIModItem).GetMethod(
                nameof(UIModItem.OnInitialize),
                BindingFlags.Instance | BindingFlags.Public
            ),
            OnInitialize_TEMP_DisplayRatings
        );
    }

    private static void OnInitialize_TEMP_DisplayRatings(ILContext il)
    {
        var c = new ILCursor(il);

        var skipChecksLabel = c.DefineLabel();

        c.GotoNext(
            MoveType.Before,
            i => i.MatchCall(typeof(SteamedWraps), $"get_{nameof(SteamedWraps.SteamClient)}")
        );

        c.MoveAfterLabels();

        c.EmitBr(skipChecksLabel);

        c.GotoNext(
            MoveType.After,
            i => i.MatchLdcI4(out _),
            i => i.MatchBneUn(out _)
        );

        c.MarkLabel(skipChecksLabel);
    }

    private static void DrawSelf_HideErroneousVisuals(ILContext il)
    {
        var c = new ILCursor(il);

        var skipDrawDividerLabel = c.DefineLabel();
        ILLabel? skipDrawReloadRequiredTextLabel = null;

        var selfIndex = ParameterIndex.Invalid;

        var isRosemaryDefinition = c.AddVariable<bool>();

        c.GotoNext(
            MoveType.After,
            i => i.MatchCallvirt<SpriteBatch>(nameof(SpriteBatch.Draw))
        );

        c.MarkLabel(skipDrawDividerLabel);

        c.GotoPrev(
            MoveType.After,
            i => i.MatchLdarg(out selfIndex),
            i => i.MatchCall<UIElement>(nameof(UIElement.GetInnerDimensions)),
            i => i.MatchStloc(out int _)
        );

        c.EmitLdarg(selfIndex);
        c.EmitDelegate(
            static (UIModItem modItem) =>
            {
                if (!ModLoader.TryGetMod(modItem._mod.Name, out var mod)
                 || mod is not ModImpl)
                {
                    return false;
                }

                return true;
            }
        );
        c.EmitStloc(isRosemaryDefinition);

        c.EmitLdloc(isRosemaryDefinition);
        c.EmitBrtrue(skipDrawDividerLabel);

        c.GotoNext(
            MoveType.After,
            i => i.MatchLdcI4((int)ModSide.Server),
            i => i.MatchBeq(out skipDrawReloadRequiredTextLabel)
        );

        Debug.Assert(skipDrawReloadRequiredTextLabel is not null);

        c.EmitLdloc(isRosemaryDefinition);
        c.EmitBrtrue(skipDrawReloadRequiredTextLabel);
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
                    element._modName.Remove();

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
                        config.HAlign = 0f;
                        config.VAlign = 1f;
                        config.Top.Set(-2f, 0f);
                        config.Left.Set(2f, 0f);

                        if (element._rateButton is not null)
                        {
                            element._rateButton.Remove();

                            var rateButton = new HorizontalRateButton();
                            {
                                rateButton.HAlign = 0f;
                                rateButton.VAlign = 0f;
                                rateButton.Left.Set(2f, 0f);
                                rateButton.Top.Set(-2f, 0f);
                                rateButton.Top.Sub(config.Height.Pixels + 4, 0f);
                                rateButton.Height.Set(20f, 0f);
                                rateButton.Width.Set(76f, 0f);

                                rateButton.OnLeftClickExt += OnLeftClick_Rate;
                            }
                            element._rateButton = rateButton;
                            element.Append(element._rateButton);

                            bottomOffset += rateButton.Height.Pixels + 4;
                        }
                    }
                    else
                    {
                        element._rateButton?.HAlign = 0f;
                        element._rateButton?.VAlign = 1f;
                        element._rateButton?.Left.Set(2f, 0f);
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

                        // Should be safely only the asterisk as the mod name is already removed
                        element.Elements.RemoveAll(e => e is UIText);
                        var asterisk = new UIText(string.Empty);
                        {
                            asterisk.TextOriginX = 0f;
                            
                            asterisk.HAlign = 1f;
                            asterisk.VAlign = 1f;

                            asterisk.Top = stateText.Top;

                            asterisk.Left.Set(0f, 0f);
                            asterisk.Top.Set(-18f, 0f);
                            asterisk.Top.Sub(bottomOffset, 0f);

                            asterisk.OnUpdateExt += OnUpdate_UpdateAsterisk;
                            asterisk.OnDrawExt += OnDraw_AsteriskTooltip;

                            asterisk.IgnoresMouseInteraction = true;
                        }
                        element.Append(asterisk);

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

                    var name = ModImpl.ELK_NAME;

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

                static void OnUpdate_UpdateAsterisk(UIText asterisk)
                {
                    if (asterisk.Parent is not UIModItem modItem)
                    {
                        return;
                    }

                    var status = modItem._mod.Enabled != modItem._loaded || modItem._configChangesRequireReload;

                    var text = status ? "*" : string.Empty;

                    if (asterisk.Text != text)
                    {
                        asterisk.SetText(text);
                    }
                }

                static void OnDraw_AsteriskTooltip(UIText asterisk, SpriteBatch sb)
                {
                    if (asterisk.Parent is not UIModItem modItem)
                    {
                        return;
                    }

                    var status = modItem._mod.Enabled != modItem._loaded || modItem._configChangesRequireReload;

                    var hovering = asterisk.ContainsPoint(UserInterface.ActiveInstance.MousePosition);

                    if (hovering && status)
                    {
                        modItem._tooltip = Language.GetTextValue(modItem._configChangesRequireReload ? "tModLoader.ModReloadForced" : "tModLoader.ModReloadRequired");
                    }
                }

                static void OnDraw_SetWidth(UIModStateText element, SpriteBatch sb)
                {
                    var textSize = FontAssets.MouseText.Value.MeasureString(element.DisplayText).X;

                    element.Width.Pixels = element.Parent.InnerDimensions.Width - 4f;
                    element.PaddingLeft = (int)(5 + (((element.Width.Pixels - 10) - textSize) * 0.5f));
                }

                static void OnLeftClick_Rate(UIMouseEvent evt, HorizontalRateButton element)
                {
                    var mouseX = evt.MousePosition.X;

                    var buttonWidth = (element.Texture.Value.Width * 0.5f) - 2;

                    var hoveringSides = (mouseX < element.Dimensions.X + buttonWidth
                                      || mouseX > element.Dimensions.Right - buttonWidth);

                    var hoveringUp = mouseX < element.Dimensions.X + buttonWidth;

                    if (!hoveringSides
                     || element.Parent is not UIModItem modItem
                     || !modItem._gotRating)
                    {
                        return;
                    }

                    if (modItem._ratedUp is { } rating
                      && hoveringUp == rating)
                    {
                        return;
                    }

                    SoundEngine.PlaySound(in SoundID.MenuTick);

                    modItem._gotRating = false;
                    modItem._ratingCts?.Cancel(throwOnFirstException: false);
                    modItem._ratingCts?.Dispose();
                    modItem._ratingCts = new CancellationTokenSource();

                    Task.Run(async delegate
                    {
                        await SteamedWraps.SetUserRating(modItem._publishId, hoveringUp);
                        await modItem.GetRating();
                    }, modItem._ratingCts.Token);
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

    private sealed class HorizontalRateButton() : UIImage(Assets.ModPanel.Ratings.Asset)
    {
        private int frameCount;

        protected override void DrawSelf(SpriteBatch sb)
        {
            if (Parent is not UIModItem parent)
            {
                return;
            }

            var yFrame = 0;

            if (parent._gotRating)
            {
                if (parent._ratedUp is not { } rating)
                {
                    yFrame = 1;
                }
                else
                {
                    yFrame = rating ? 4 : 5;
                }

                var buttonWidth = (Texture.Value.Width * 0.5f) - 2;

                var mouseX = UserInterface.ActiveInstance.MousePosition.X;

                var hoveringSides = (mouseX < this.Dimensions.X + buttonWidth
                                  || mouseX > this.Dimensions.Right - buttonWidth);

                var hovering = IsMouseHovering && hoveringSides;

                var hoveringUp = mouseX < this.Dimensions.X + buttonWidth;

                if (IsMouseHovering && !hoveringSides)
                {
                    parent._tooltip = string.Empty;
                }

                if (hovering)
                {
                    if (parent._ratedUp is not { } rating2)
                    {
                        yFrame = hoveringUp ? 3 : 2;

                        parent._tooltip = Language.GetTextValue(hoveringUp ? "tModLoader.ModsRateUp" : "tModLoader.ModsRateDown");
                    }
                    else if (hoveringUp != rating2)
                    {
                        yFrame = 6;

                        parent._tooltip = Language.GetTextValue(hoveringUp ? "tModLoader.ModsRateUp" : "tModLoader.ModsRateDown");
                    }
                }
            }

            var frame = Texture.Value.Frame(1, 7, 0, yFrame);
            frame.Height -= 2;

            Frame = frame;

            RemoveFloatingPointsFromDrawPosition = true;

            base.DrawSelf(sb);

            if (yFrame > 0)
            {
                frameCount = 0;
                return;
            }

            frameCount++;

            var buffer = Assets.ModPanel.Ratings_Loading.Asset.Value;

            frame = buffer.Frame(1, 4, 0, (int)(frameCount * 0.14f) % 4);
            frame.Height -= 2;

            sb.Draw(buffer, this.Dimensions.TopLeft(), frame, Color.White);
        }
    }
}
