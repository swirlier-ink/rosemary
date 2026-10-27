using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rosemary.Common;
using Terraria;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using LangPhotographer = Rosemary.Core.LocalizationReferences.Mods.Rosemary.Content.Misc.Photographer;

namespace Rosemary.Content.Misc;

// TODO: dialogue, spawn condition, shop, ...
public class Photographer : ModNPC
{
    public override string LocalizationCategory => "Content.Misc";

    public override string Texture => Assets.Misc.Camera.Photographer.KEY;
    
    private static Profiles.StackedNPCProfile? profile;
    
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 25;

        NPCID.Sets.ExtraFramesCount[Type] = 9;
        NPCID.Sets.AttackFrameCount[Type] = 4;
        NPCID.Sets.ActsLikeTownNPC[Type] = true;
        NPCID.Sets.NoTownNPCHappiness[Type] = true;
        NPCID.Sets.SpawnsWithCustomName[Type] = true;
        
        NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new()
        {
            Velocity = 1f,
            Direction = 1,
        });

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

        var night = !Main.IsItDay();
        var surfaceTundra = player.ZoneSnow && !player.ShoppingZone_BelowSurface;

        if (night)
        {
            keys.Add(LangPhotographer.Chat.Night.KEY);

            if (Main.moonPhase == (int)MoonPhase.Full)
            {
                keys.Add(LangPhotographer.Chat.Night.FullMoon.KEY);
            }
        }

        if (surfaceTundra)
        {
            if (night)
            {
                keys.Add(LangPhotographer.Chat.Night.Tundra.KEY);
            }

            keys.Add(LangPhotographer.Chat.Tundra.KEY);
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

        for (var i = 0; i < (NPC.life > 0 ? 1 : 5); i++)
        {
            Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood);
        }
        
        if (NPC.life > 0)
        {
            return;
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

    public override void FindFrame(int frameHeight)
    {
        base.FindFrame(frameHeight);

        NPC.frame.Width = NPC.width;
        NPC.frame.X = NPC.width * (NPC.spriteDirection == 1).ToInt();
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        var texture = TextureAssets.Npc[Type].Value;
        var position = NPC.Center - screenPos + new Vector2(0, NPC.gfxOffY + 2);
        var origin = new Vector2(NPC.frame.Width, NPC.height) / 2f;
        var effects = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        
        Main.EntitySpriteDraw(texture, position, NPC.frame, drawColor, NPC.rotation, origin, NPC.scale, effects);
        
        return false;
    }

    public override void AddShops()
    {
        new NPCShop(Type)
            .Add<PolaroidCamera>()
            .Register();
    }
}