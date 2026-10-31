using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Rosemary.Vanity.Content;

public sealed class GiantAntlers : ModItem
{
    public override string Texture => Assets.Vanity.Fawn.Antlers.KEY;

    public override string LocalizationCategory => "Content";

    public override void SetDefaults()
    {
        Item.width = 20;
        Item.height = 20;

        Item.accessory = true;
        Item.vanity = true;
    }

    public override void UpdateVisibleAccessory(Player player, bool hideVisual)
    {
        if (hideVisual)
        {
            return;
        }

        player.GetModPlayer<AntlerPlayer>().IsActive = true;
    }

    public override void UpdateItemDye(Player player, int dye, bool hideVisual)
    {
        player.GetModPlayer<AntlerPlayer>().Dye = dye;
    }

    private sealed class AntlerPlayer : ModPlayer
    {
        public bool IsActive { get; set; }

        public int Dye { get; set; }

        public override void ResetEffects()
        {
            IsActive = false;
        }
    }

    internal class AntlerBehindDrawLayer : PlayerDrawLayer
    {
        private static bool IsVisible(PlayerDrawSet drawInfo)
        {
            return drawInfo.drawPlayer.GetModPlayer<AntlerPlayer>().IsActive;
        }

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => IsVisible(drawInfo);

        public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.Head);

        public override bool IsHeadLayer => true;

        protected virtual int FrameX => 1;

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            var texture = Assets.Vanity.Fawn.Antlers_Equip.Asset.Value;

            var player = drawInfo.drawPlayer;

            var dir = player.Directions;

            var headOffset = Main.OffsetsPlayerHeadgear[player.bodyFrame.Y / player.bodyFrame.Height].Y;

            var helmetOffset = Vector2.Zero;
            player.ApplyHeadOffsetFromMount(ref helmetOffset);
            helmetOffset += drawInfo.helmetOffset;

            var headPosition = helmetOffset
                             + new Vector2(
                                   (int)(drawInfo.Position.X - Main.screenPosition.X - (player.bodyFrame.Width * 0.5f) + (player.width * 0.5f)),
                                   (int)(drawInfo.Position.Y - Main.screenPosition.Y + player.height - (player.bodyFrame.Height + 4f)))
                             + drawInfo.drawPlayer.headPosition
                             + drawInfo.headVect.Floor();

            var position = headPosition + new Vector2(0, headOffset * (drawInfo.headOnlyRender ? 1f : dir.Y));

            if ((int)player.gravDir == -1 && !drawInfo.headOnlyRender)
            {
                position.Y += player.height - player.headPosition.Y - 4;
            }

            var frame = texture.Frame(2, 1, FrameX);
            // frame.Width -= 2;

            if (FrameX == 0
             && drawInfo.hatHair)
            {
                frame.Height = 26;
            }

            var origin = new Vector2(24, 26);

            var hatData = new DrawData(
                texture,
                position,
                frame,
                drawInfo.colorArmorHead,
                0f,
                origin,
                1f,
                drawInfo.playerEffect
            )
            {
                shader = player.GetModPlayer<AntlerPlayer>().Dye,
            };
            drawInfo.DrawDataCache.Add(hatData);
        }
    }

    internal sealed class AntlerFrontDrawLayer : AntlerBehindDrawLayer
    {
        public override Position GetDefaultPosition() => new AfterParent(ModContent.GetInstance<SiffrinHat.HatFrontDrawLayer>());

        protected override int FrameX => 0;
    }
}
