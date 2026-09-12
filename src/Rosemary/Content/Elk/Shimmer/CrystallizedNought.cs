using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rosemary.Common;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Rosemary.Content.Elk;

public sealed class CrystallizedNought : ModItem
{
    public override void Load()
    {
        On_Main.DrawStar += DrawStar_Offset;
        On_Star.UpdateStars += UpdateStars_UpdateFlicker;
    }

    private static readonly Vector2 star_bounds = new Vector2(1920, 1200);

    private static float flickerTimer;

    private void UpdateStars_UpdateFlicker(On_Star.orig_UpdateStars orig)
    {
        orig();

        if (flickerTimer <= 0f)
        {
            return;
        }

        flickerTimer += 0.01f;

        if (flickerTimer >= 2f)
        {
            flickerTimer = 0f;
        }
    }

    private void DrawStar_Offset(On_Main.orig_DrawStar orig, Main self, ref Main.SceneArea sceneArea, float starOpacity, Color bgColorForStars, int i, Star star, bool artificial)
    {
        if (!artificial
         || flickerTimer <= 0f
         || star.falling
         || star.hidden)
        {
            orig(self, ref sceneArea, starOpacity, bgColorForStars, i, star, artificial);
            return;
        }

        var topY = sceneArea.bgTopY;

        if (Main.worldSurface <= 30f)
        {
            topY = 0;
        }

        var bounds = new Vector2(sceneArea.totalWidth, sceneArea.totalHeight);

        var focus = Main.LocalPlayer.Center - Main.screenPosition;

        var starPosition = star.position / star_bounds;
        starPosition *= bounds;
        starPosition.Y += topY;

        var difference = starPosition - focus;
        var distance = 1f - MathF.Saturate(difference.Length() / 2200f);

        var endDist = (1f - distance * 0.4f) + 1f;

        var inRange = flickerTimer > distance && endDist > flickerTimer;

        var offset = -difference.Normalized;

        offset.Magnitude =
            inRange
          ? Utils.Remap(flickerTimer, distance, distance + 0.01f, 0f, 25f)
          : 0f;

        var scale = MathF.Max(1f - MathF.Abs(flickerTimer - distance), 1f - MathF.Abs(flickerTimer - endDist));
        scale = Utils.Remap(scale, 0.96f, 1f, 1f, 0f);

        scale = MathF.Pow(scale, 3f);

        scale *= star.scale;

        if (inRange)
        {
            scale = MathF.Max(1.1f, scale);
        }

        using var _ = star.position.Override(star.position + offset);
        using var __ = star.scale.Override(scale);

        orig(self, ref sceneArea, starOpacity, bgColorForStars, i, star, artificial);
    }

    public override string Texture => Assets.Elk.Shimmer.CrystallizedNought.KEY;

    public override string LocalizationCategory => "Content.Elk";

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 20;

        ItemID.Sets.SolidShimmerReaction[Type] = true;

        Main.itemAnimations[Type] = new DrawAnimationStatic(1, 4);
    }

    public override void SetDefaults()
    {
        Item.width = 14;
        Item.height = 26;
        Item.maxStack = Item.CommonMaxStack;

        Item.rare = ItemRarityID.Purple;

        Item.value = Item.buyPrice(gold: 3);
    }

    public override bool PreDrawInWorld(WorldItem item, SpriteBatch sb, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        Main.instance.DrawItem_GetBasics(item.inner, whoAmI, out var texture, out var frame, out _);

        var origin = frame.Size() * 0.5f;

        var off = new Vector2((item.width * 0.5f) - origin.X, item.height - frame.Height);

        var position = (item.position + origin + off);

        sb.Draw(texture, position - Main.screenPosition, frame, Color.Black, rotation, origin, scale, SpriteEffects.None, 0f);

        var right = Lighting.GetSubLight(position + new Vector2(item.width * 0.5f, 0f));
        var down = Lighting.GetSubLight(position + new Vector2(0f, item.height * 0.5f));
        var diagRight = Lighting.GetSubLight(position + new Vector2(item.width, item.height * 0.5f));
        var diagDown = Lighting.GetSubLight(position + new Vector2(item.width * 0.5f, item.height));

        var lightDirection = new Vector2(Sum(diagRight) - Sum(down), Sum(diagDown) - Sum(right));

        lightDirection = lightDirection.RotatedBy(-rotation);
        lightDirection.Y -= 0.15f;

        lightDirection = lightDirection.Normalized;

        var redColor = Color.White * (Vector2.Dot(Vector2.UnitX, lightDirection) + 0.25f);
        var greenColor = Color.White * (Vector2.Dot(-Vector2.UnitX, lightDirection) + 0.1f);
        var blueColor = Color.White * (Vector2.Dot(-Vector2.UnitY, lightDirection) + 0.4f);
        redColor.A = 0;
        greenColor.A = 0;
        blueColor.A = 0;

        sb.Draw(texture, position - Main.screenPosition, texture.Frame(1, 4, 0, 1), redColor, rotation, origin, scale, SpriteEffects.None, 0f);
        sb.Draw(texture, position - Main.screenPosition, texture.Frame(1, 4, 0, 2), greenColor, rotation, origin, scale, SpriteEffects.None, 0f);
        sb.Draw(texture, position - Main.screenPosition, texture.Frame(1, 4, 0, 3), blueColor, rotation, origin, scale, SpriteEffects.None, 0f);

        return false;

        static float Sum(Vector3 vector)
        {
            return vector.X + vector.Y + vector.Z;
        }
    }

    public override bool OnPickup(WorldItem item, Player player)
    {
        flickerTimer += 0.05f;

        return base.OnPickup(item, player);
    }

    public override bool PreDrawInInventory(SpriteBatch sb, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        var texture = TextureAssets.Item[Type].Value;

        sb.Draw(texture, position, frame, Color.Black, 0f, origin, scale, SpriteEffects.None, 0f);

        var lightDirection = new Vector2(Main.screenWidth * 0.3f, 0f) - position;

        lightDirection = lightDirection.Normalized;

        var redColor = Color.White * (Vector2.Dot(Vector2.UnitX, lightDirection) + 0.25f);
        var greenColor = Color.White * (Vector2.Dot(-Vector2.UnitX, lightDirection) + 0.1f);
        var blueColor = Color.White * (Vector2.Dot(-Vector2.UnitY, lightDirection) + 0.4f);
        redColor.A = 0;
        greenColor.A = 0;
        blueColor.A = 0;

        sb.Draw(texture, position, texture.Frame(1, 4, 0, 1), redColor, 0f, origin, scale, SpriteEffects.None, 0f);
        sb.Draw(texture, position, texture.Frame(1, 4, 0, 2), greenColor, 0f, origin, scale, SpriteEffects.None, 0f);
        sb.Draw(texture, position, texture.Frame(1, 4, 0, 3), blueColor, 0f, origin, scale, SpriteEffects.None, 0f);

        sb.Draw(texture, position, frame, Color.White * 0.4f, 0f, origin, scale, SpriteEffects.None, 0f);

        return false;
    }
}
