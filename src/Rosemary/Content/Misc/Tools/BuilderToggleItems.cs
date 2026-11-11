using Microsoft.Xna.Framework.Graphics;
using Rosemary.Common;
using System;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Rosemary.Content.Misc;

public abstract class CommonBuilderToggle : BuilderToggle
{
    public override string HoverTexture => Texture;

    public override bool Draw(SpriteBatch spriteBatch, ref BuilderToggleDrawParams drawParams)
    {
        drawParams.Color = Color.White;
        drawParams.Frame = drawParams.Texture.Frame(NumberOfStates, 2, CurrentState);

        return true;
    }

    public override bool DrawHover(SpriteBatch spriteBatch, ref BuilderToggleDrawParams drawParams)
    {
        drawParams.Frame = drawParams.Texture.Frame(NumberOfStates, 2, CurrentState, 1);
        drawParams.Scale = 0.9f;

        return true;
    }

    public override bool OnLeftClick(ref SoundStyle? sound)
    {
        sound = SoundID.Unlock;

        return true;
    }
}

public static class BuilderToggleExtensions
{
    extension<T>(T) where T : BuilderToggle
    {
        public static bool IsActive => ModContent.GetInstance<T>().Active();

        public static int State => ModContent.GetInstance<T>().CurrentState;
    }
}

file sealed class BuilderToggleItems : GlobalItem
{
    private static readonly int[] active_guides = [ItemID.DontHurtNatureBook, ItemID.DontHurtCrittersBook, ItemID.DontHurtComboBook];
    private static readonly int[] inactive_guides = [ItemID.DontHurtNatureBookInactive, ItemID.DontHurtCrittersBookInactive, ItemID.DontHurtComboBookInactive];

    public override void SetStaticDefaults()
    {
        foreach (var type in active_guides)
        {
            ItemID.Sets.HasItemSwap[type] = false;
        }

        ItemID.Sets.HasItemSwap[ItemID.EncumberingStone] = false;
    }

    public override bool? UseItem(Item item, Player player)
    {
        if (!active_guides.Contains(item.type)
         && item.type != ItemID.EncumberingStone)
        {
            return null;
        }

        return true;
    }

    public override void SetDefaults(Item item)
    {
        if (active_guides.Contains(item.type)
         || item.type == ItemID.EncumberingStone)
        {
            item.consumable = true;
            item.useStyle = ItemUseStyleID.HoldUp;
            item.useTime = 30;
            item.UseSound = SoundID.Item92;
            item.useAnimation = 30;

            return;
        }

        if (!inactive_guides.Contains(item.type) && item.type != ItemID.UncumberingStone)
        {
            return;
        }

        var type = item.type switch
        {
            ItemID.DontHurtNatureBookInactive => ItemID.DontHurtNatureBook,
            ItemID.DontHurtCrittersBookInactive => ItemID.DontHurtCrittersBook,
            ItemID.DontHurtComboBookInactive => ItemID.DontHurtComboBook,
            _ => ItemID.EncumberingStone,
        };

        item.SetDefaults(type);
    }

    public override bool ConsumeItem(Item item, Player player)
    {
        if (!active_guides.Contains(item.type)
         && item.type != ItemID.EncumberingStone)
        {
            return true;
        }

        var modPlayer = player.GetModPlayer<TogglesPlayer>();

        if (item.type switch
        {
            ItemID.DontHurtNatureBook => modPlayer.UnlockedEnvironmentalPreservation,
            ItemID.DontHurtCrittersBook => modPlayer.UnlockedCritterCompanionship,
            ItemID.DontHurtComboBook => modPlayer.UnlockedEnvironmentalPreservation && modPlayer.UnlockedCritterCompanionship,
            _ => modPlayer.UnlockedEncumberingStone,
        })
        {
            return false;
        }

        return true;
    }

    public override void OnConsumeItem(Item item, Player player)
    {
        var modPlayer = player.GetModPlayer<TogglesPlayer>();

        var combo = item.type == ItemID.DontHurtComboBook;

        if (item.type == ItemID.DontHurtNatureBook || combo)
        {
            modPlayer.UnlockedEnvironmentalPreservation = true;
        }

        if (item.type == ItemID.DontHurtCrittersBook || combo)
        {
            modPlayer.UnlockedCritterCompanionship = true;
        }

        if (item.type == ItemID.EncumberingStone)
        {
            modPlayer.UnlockedEncumberingStone = true;
        }
    }

    internal sealed class TogglesPlayer : ModPlayer
    {
        private record struct Packet(int WhoAmI) : IPacket<Packet>
        {
            public Packet() : this(-1)
            { }

            public void Write(BinaryWriter writer)
            {
                writer.Write(WhoAmI);

                var modPlayer = Main.player[WhoAmI].GetModPlayer<TogglesPlayer>();

                writer.Write(modPlayer.UnlockedEnvironmentalPreservation);
                writer.Write(modPlayer.UnlockedCritterCompanionship);
                writer.Write(modPlayer.UnlockedEncumberingStone);
            }

            public static void Receive(BinaryReader reader, int sender)
            {
                sender = reader.ReadInt32();

                var player = Main.player[sender];

                var modPlayer = player.GetModPlayer<TogglesPlayer>();

                modPlayer.UnlockedEnvironmentalPreservation = reader.ReadBoolean();
                modPlayer.UnlockedCritterCompanionship = reader.ReadBoolean();
                modPlayer.UnlockedEncumberingStone = reader.ReadBoolean();

                if (Main.netMode == NetmodeID.Server)
                {
                    new Packet(sender).Send(PacketDestination.AllExcept(sender));
                }
            }
        }

        public bool UnlockedEnvironmentalPreservation { get; set; }

        public bool UnlockedCritterCompanionship { get; set; }

        public bool UnlockedEncumberingStone { get; set; }

        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            new Packet(Player.whoAmI).Send(PacketDestination.From(toWho, fromWho));
        }

        public override void CopyClientState(ModPlayer targetCopy)
        {
            var clone = (TogglesPlayer)targetCopy;

            clone.UnlockedEnvironmentalPreservation = UnlockedEnvironmentalPreservation;
            clone.UnlockedCritterCompanionship = UnlockedCritterCompanionship;
            clone.UnlockedEncumberingStone = UnlockedEncumberingStone;
        }

        public override void SendClientChanges(ModPlayer clientPlayer)
        {
            var clone = (TogglesPlayer)clientPlayer;

            if (UnlockedEnvironmentalPreservation != clone.UnlockedEnvironmentalPreservation
             || UnlockedCritterCompanionship != clone.UnlockedCritterCompanionship
             || UnlockedEncumberingStone != clone.UnlockedEncumberingStone)
            {
                SyncPlayer(-1, Main.myPlayer, false);
            }
        }

        public override void SaveData(TagCompound tag)
        {
            tag[nameof(UnlockedEnvironmentalPreservation)] = UnlockedEnvironmentalPreservation;
            tag[nameof(UnlockedCritterCompanionship)] = UnlockedCritterCompanionship;
            tag[nameof(UnlockedEncumberingStone)] = UnlockedEncumberingStone;
        }

        public override void LoadData(TagCompound tag)
        {
            UnlockedEnvironmentalPreservation = tag.GetBool(nameof(UnlockedEnvironmentalPreservation));
            UnlockedCritterCompanionship = tag.GetBool(nameof(UnlockedCritterCompanionship));
            UnlockedEncumberingStone = tag.GetBool(nameof(UnlockedEncumberingStone));
        }
    }
}

public static class TogglesPlayerExtensions
{
    extension(Player player)
    {
        public bool UnlockedEnvironmentalPreservation => player.GetModPlayer<BuilderToggleItems.TogglesPlayer>().UnlockedEnvironmentalPreservation;

        public bool UnlockedCritterCompanionship => player.GetModPlayer<BuilderToggleItems.TogglesPlayer>().UnlockedCritterCompanionship;

        public bool UnlockedEncumberingStone => player.GetModPlayer<BuilderToggleItems.TogglesPlayer>().UnlockedEncumberingStone;
    }
}
