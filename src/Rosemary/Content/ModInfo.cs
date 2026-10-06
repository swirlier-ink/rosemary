using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.OS.Windows;
using Rosemary.Content.Elk;
using System;
using System.Reflection;
using System.Xml.Linq;
using Rosemary.Common;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace Rosemary.Content;

internal static class ModInfo
{
    [OnLoad(Side = ModSide.Client)]
    private static void Load()
    {
        MonoModHooks.Add(
            typeof(UIModInfo).GetMethod(
                nameof(UIModInfo.Update),
                BindingFlags.Public | BindingFlags.Instance
            ),
            Update_Replace
        );

        MonoModHooks.Add(
            typeof(UIModInfo).GetMethod(
                nameof(UIModInfo.OnActivate),
                BindingFlags.Public | BindingFlags.Instance
            ),
            OnActivate_Replace
        );

        On_UIScrollbar.DrawBar += DrawBar_ModifyColor;
        IL_UIScrollbar.DrawSelf += _ => { };
    }

    private static float opacity = 1f;
    private static float textOpacity = 1f;

    private static UIScrollbar? modInfoScrollbar;

    private static void DrawBar_ModifyColor(On_UIScrollbar.orig_DrawBar orig, UIScrollbar self, SpriteBatch spriteBatch, Texture2D texture, Rectangle dimensions, Color color)
    {
        if (self == modInfoScrollbar)
        {
            color *= opacity;
        }

        orig(self, spriteBatch, texture, dimensions, color);
    }

    private static void OnActivate_Replace(Action<UIModInfo> orig, UIModInfo self)
    {
        orig(self);

        UpdateInfo(self);
    }

    private static void Update_Replace(Action<UIModInfo, GameTime> orig, UIModInfo self, GameTime gameTime)
    {
        var shouldModifyText = !self._loading && self._ready;

        orig(self, gameTime);

        FadePanel();

        if (!shouldModifyText)
        {
            return;
        }

        UpdateInfo(self);

        return;

        void FadePanel()
        {
            if (!ModLoader.TryGetMod(self._localMod.Name, out var mod)
             || mod is not ModImpl)
            {
                return;
            }

            var message = self._modInfo;

            opacity -= 0.005f;
            opacity = MathF.Saturate(opacity);

            textOpacity *= 0.9982f;

            modInfoScrollbar = message._scrollbar;

            if (opacity <= 0.7f)
            {
                message._scrollbar.IgnoresMouseInteraction = true;
            }

            message.BorderColor = Color.Black * opacity;
            message.BackgroundColor = UICommon.DefaultUIBlueMouseOver * opacity;

            if (message.Parent is UIPanel panel)
            {
                panel.BorderColor = Color.Black * opacity;
                panel.BackgroundColor = UICommon.MainPanelBackground * opacity;
            }

            message._textElement?.TextColor = Color.White * textOpacity;
        }
    }

    private static void UpdateInfo(UIModInfo modInfo)
    {
        var message = modInfo._modInfo;

        opacity = 1f;
        textOpacity = 1f;

        ResetInfo();

        if (!ModLoader.TryGetMod(modInfo._localMod.Name, out var mod)
         || mod is not ModImpl)
        {
            return;
        }

        SetRosemaryInfo();

        return;

        void ResetInfo()
        {
            message._textElement?.TextOriginX = 0f;
            message._textElement?.TextOriginY = 0f;
            message._textElement?.Height.Set(0f, 0f);

            message.BorderColor = Color.Black;
            message.BackgroundColor = UICommon.DefaultUIBlueMouseOver;

            message._scrollbar.IgnoresMouseInteraction = false;

            if (message.Parent is UIPanel panel)
            {
                panel.BorderColor = Color.Black;
                panel.BackgroundColor = UICommon.MainPanelBackground;
            }

            if (!modInfo._uIElement.HasChild(modInfo._uITextPanel))
            {
                modInfo._uIElement.Append(modInfo._uITextPanel);
            }

            var index = modInfo.Elements.FindIndex(e => e is ElkLangPanel);

            if (index != -1)
            {
                modInfo.Elements[index].Remove();
            }
        }

        void SetRosemaryInfo()
        {
            message._textElement?.TextOriginX = 0.5f;
            message._textElement?.TextOriginY = 0.5f;

            message.SetText(Mods.Rosemary.Description.GetTextValue());

            message._textElement?.Height.Set(message.InnerDimensions.Height, 0f);

            modInfo._uITextPanel.Remove();

            var panel = new ElkLangPanel(ModImpl.ELK_NAME);
            {
                panel.OnUpdate += OnUpdate_UpdatePosition;

                panel.BackgroundColor = UICommon.DefaultUIBlue;
            }
            modInfo.Append(panel);

            modInfo.Recalculate();
            panel.Update(Main.gameTimeCache);

            return;

            void OnUpdate_UpdatePosition(UIElement element)
            {
                var dims = modInfo._modInfo.Parent.Dimensions;

                element.Left.Set(dims.X - element.Dimensions.Width - 6f, 0f);
                element.Top.Set(dims.Y, 0f);

                element.Recalculate();
            }
        }
    }

    // TODO: Abstract out when more UI is needed?
    private sealed class ElkLangPanel(ElkPhrase name) : UIPanel
    {
        public override void Recalculate()
        {
            base.Recalculate();

            const float scale = 1f;
            var size = name.Measure(scale);

            MinWidth.Set(size.X + PaddingLeft + PaddingRight, 0f);
            MinHeight.Set(size.Y + PaddingTop + PaddingBottom, 0f);
        }

        protected override void DrawSelf(SpriteBatch sb)
        {
            using var _ = BackgroundColor.Override(BackgroundColor * opacity);
            using var __ = BorderColor.Override(BorderColor * opacity);

            base.DrawSelf(sb);

            var position = this.Dimensions.Top();
            const float scale = 1f;
            var size = name.Measure(scale);
            var origin = new Vector2(size.X * 0.5f, 0f);

            var alpha = MathF.Pow(textOpacity, 3f);

            sb.DrawPhraseWithOutline(name, position, Color.White * alpha, Color.Black * alpha, 1f, origin);
        }
    }
}
