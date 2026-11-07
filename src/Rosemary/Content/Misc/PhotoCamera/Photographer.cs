using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rosemary.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using LangPhotographer = Rosemary.Core.LocalizationReferences.Mods.Rosemary.Content.Misc.Photographer;

namespace Rosemary.Content.Misc;

// TODO: dialogue, spawn condition, shop, ...
public sealed class Photographer : ModNPC
{
    public sealed class EmoteBubble : ModEmoteBubble
    {
        public override string Texture => Assets.Misc.Camera.Photographer_Emote.KEY;

        public override LocalizedText Command => LangPhotographer.EmoteBubble.Command.GetText();

        public override void SetStaticDefaults()
        {
            // Add NPC emotes to "Town" category.
            AddToCategory(EmoteID.Category.Town);
        }
    }

    public override string LocalizationCategory => "Content.Misc";

    public override string Texture => Assets.Misc.Camera.Photographer.KEY;
    
    private static Profiles.StackedNPCProfile? profile;
    
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 29;

        NPCID.Sets.ExtraFramesCount[Type] = 13;
        NPCID.Sets.AttackFrameCount[Type] = 0;
        NPCID.Sets.ActsLikeTownNPC[Type] = true;
        NPCID.Sets.NoTownNPCHappiness[Type] = true;
        NPCID.Sets.SpawnsWithCustomName[Type] = true;
        
        NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new()
        {
            Velocity = 1f,
            Direction = -1,
        });

        NPCID.Sets.FaceEmote[Type] = ModContent.EmoteBubbleType<EmoteBubble>();

        NPCID.Sets.NPCPortraits.Add(Type, NPCID.Sets.PrioritizedPortrait().Default(NPCID.Sets.BasicPortrait($"{Texture}_Portrait")));
        
        profile = new Profiles.StackedNPCProfile(new Profiles.DefaultNPCProfile(Texture, -1));
    }

    public override void SetDefaults()
    {
        NPC.friendly = true;
        NPC.width = 30;
        NPC.height = 46;
        NPC.aiStyle = NPCAIStyleID.Passive;
        NPC.damage = 15;
        NPC.defense = 10;
        NPC.lifeMax = 250;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.knockBackResist = 1.2f;

        AnimationType = NPCID.Guide;
    }

    public override void ModifyHoverBoundingBox(ref Rectangle boundingBox)
    {
        boundingBox = NPC.Hitbox;
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        base.SetBestiary(database, bestiaryEntry);

        IBestiaryInfoElement[] info = 
            [
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface, 
                new FlavorTextBestiaryInfoElement(LangPhotographer.Entry.KEY),
            ];
        
        bestiaryEntry.Info.AddRange(info);
    }

    public override bool CanChat() => true;

    public override ITownNPCProfile TownNPCProfile() => profile!;

    public override List<string> SetNPCNameList() => Language.GetAllTextValues(LangPhotographer.Name.KEY);

    public override string GetChat()
    {
        var player = Main.LocalPlayer;

        var keys = new HashSet<string> { LangPhotographer.Chat.Generic.KEY };

        var metrics = Main.SceneMetrics;

        var surface = !player.ShoppingZone_BelowSurface;

        var night = !Main.IsItDay();
        if (night)
        {
            keys.Add(LangPhotographer.Chat.Night.KEY);

            if (Main.moonPhase == (int)MoonPhase.Full)
            {
                keys.Add(LangPhotographer.Chat.Night.FullMoon.KEY);
            }
        }

        if (player.ShoppingZone_Forest && surface)
        {
            keys.Add(night ? LangPhotographer.Chat.Night.Forest.KEY : LangPhotographer.Chat.Forest.KEY);
        }

        if (player.ZoneSnow && surface)
        {
            if (metrics.ZoneRain)
            {
                keys.Add(LangPhotographer.Chat.Tundra.Snowing.KEY);
            }
            else
            {
                keys.Add(night ? LangPhotographer.Chat.Night.Tundra.KEY : LangPhotographer.Chat.Tundra.KEY);
            }
        }

        return GetRandomValue(Rand.Instance.NextFromList(keys.ToArray()));

        string GetRandomValue(string key)
        {
            return Language.SelectRandom(
                (newKey, text) =>
                {
                    if (!newKey.StartsWith(key))
                    {
                        return false;
                    }

                    if (newKey != key)
                    {
                        var id = newKey[(key.Length + 1)..];

                        if (!int.TryParse(id, out _))
                        {
                            return false;
                        }
                    }

                    return text.ConditionsMet;
                }
            ).Value;
        }
    }

    public override void RegisterChatButtons(NPCInteractionList interactions) => interactions.InsertBefore(NPCInteractions.Shop(), NPCInteractionDatabase.CloseButton);

    public override void HitEffect(NPC.HitInfo hit)
    {
        if (Main.dedServ)
        {
            return;
        }

        if (NPC.life > 0)
        {
            for (var i = 0; i < (float)hit.Damage / NPC.lifeMax * 100f; i++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection, -1f);
            }
            return;
        }

        for (var i = 0; i < 50; i++)
        {
            Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, 2.5f * hit.HitDirection, -2.5f);
        }

        for (var i = 0; i < 3; i++)
        {
            var type = Mod.Find<ModGore>($"{Name}{i}").Type;

            Vector2[] offset =
            [
                new(0, -20),
                new(0, -4),
                new(0, 4),
            ];

            Gore.NewGore(NPC.GetSource_Death(), NPC.Center + offset[i], NPC.velocity + Main.rand.NextVector2Circular(5, 5) + new Vector2(hit.HitDirection * 5, 0), type);
        }
    }

    public float AnimationState
    {
        get => NPC.ai[0];
        set => NPC.ai[0] = value;
    }

    public float AnimationTimer
    {
        get => NPC.ai[1];
        set => NPC.ai[1] = value;
    }

    // TODO: Move to particle for both this, and the item variant
    private float flashTimer;

    private Vector2 flashPosition;

    // ReSharper disable CompareOfFloatsByEqualityOperator
    public override void AI()
    {
        if (flashTimer > 0)
        {
            flashTimer--;
        }

        var beingTalkedTo = false;

        foreach (var player in Main.ActivePlayers)
        {
            if (player.talkNPC == NPC.whoAmI)
            {
                beingTalkedTo = true;
            }
        }

        if (AnimationState == 235f)
        {
            if (NPC.velocity.X != 0f)
            {
                NPC.netUpdate = true;
            }

            if (MathF.Abs(NPC.velocity.X) >= 0.1f)
            {
                NPC.velocity.X -= 0.1f * MathF.Sign(NPC.velocity.X);

                return;
            }

            if (AnimationTimer < 0)
            {
                AnimationTimer++;
                if (AnimationTimer == 0)
                {
                    AnimationTimer = Rand.Next(100, 400);
                }

                return;
            }

            AnimationTimer--;

            if (AnimationTimer <= 0)
            {
                AnimationState = 0f;
            }

            if (AnimationTimer == 70)
            {
                SoundEngine.PlaySound(Assets.Misc.Camera.Shoot.Asset, NPC.Center);

                flashTimer = 45f;

                flashPosition = NPC.position + new Vector2(0, NPC.gfxOffY);
                flashPosition += new Vector2(NPC.spriteDirection == 1 ? (NPC.width - 4) : (8), 14);
            }

            return;
        }

        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            return;
        }

        var (i, j) = NPC.Bottom.ToTileCoordinates();
        NPC.AI_007_FindGoodRestingSpot(i, j, out var floorX, out var floorY);

        if (!beingTalkedTo
         && NPC.velocity.Y == 0f
         && AnimationState == 0f
         && Rand.NextBool(400)
            // NPC should only take "risky" photos ?
         && !NPC.AI_007_TownEntities_IsInAGoodRestingSpot(i, j, floorX, floorY))
        {
            UseCamera();
        }

        return;

        void UseCamera()
        {
            // Arbitrary value, should be unused by vanilla NPCs
            AnimationState = 235f;

            AnimationTimer = -40;

            NPC.velocity.X = 0;
            NPC.localAI[3] = 0f;

            NPC.netUpdate = true;
        }
    }

    public override void FindFrame(int frameHeight)
    {
        NPC.frame.Width = NPC.width + 4;
        NPC.frame.X = NPC.frame.Width * (NPC.spriteDirection == 1).ToInt();

        const int photo_frames = 7;

        var startingFrame = Main.npcFrameCount[Type] - 1 - 7;

        if (AnimationState != 235f)
        {
            return;
        }

        var frame = 0;

        if (AnimationTimer <= 0)
        {
            frame = startingFrame + (int)Utils.Remap(AnimationTimer, -40, 0, 0, 3);
        }
        else
        {
            frame = startingFrame + (int)Utils.Remap(AnimationTimer, 40, 0, 3, 7);
        }

        var height = TextureAssets.Npc[Type].Value.Height / Main.npcFrameCount[Type];

        NPC.frame.Y = height * frame;
    }
    // ReSharper restore CompareOfFloatsByEqualityOperator

    public override bool PreDraw(SpriteBatch sb, Vector2 screenPos, Color drawColor)
    {
        var texture = TextureAssets.Npc[Type].Value;
        var position = NPC.Center - screenPos + new Vector2(0, NPC.gfxOffY);
        var origin = new Vector2(NPC.frame.Width, NPC.height) / 2f;
        var effects = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        
        Main.EntitySpriteDraw(texture, position, NPC.frame, drawColor, NPC.rotation, origin, NPC.scale, effects);

        if (flashTimer <= 0f)
        {
            return false;
        }

        var flashTexture = TextureAssets.Extra[ExtrasID.NinetyEight].Value;

        var scale = Utils.Remap(flashTimer, 0f, 45f, 0f, 1f);
        scale = MathF.Pow(scale, 4f);

        var color = Color.White * scale;
        color.A = 0;

        var size = new Vector2(24f, 75) / flashTexture.Size();
        size *= scale;

        sb.Draw(
            flashTexture,
            flashPosition - screenPos,
            null,
            color,
            0f,
            flashTexture.Size() * 0.5f,
            size,
            SpriteEffects.None,
            0f
        );

        size *= 0.75f;

        sb.Draw(
            flashTexture,
            flashPosition - screenPos,
            null,
            color,
            MathF.PiOver2,
            flashTexture.Size() * 0.5f,
            size,
            SpriteEffects.None,
            0f
        );

        return false;
    }

    public override void AddShops()
    {
        new NPCShop(Type)
            .Add<PolaroidCamera>()
            .Register();
    }
}