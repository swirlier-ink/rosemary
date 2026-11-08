using Rosemary.Common;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Rosemary.Content.Misc;

file sealed class PeacefulCoexistenceGuides : GlobalItem
{
    private static readonly int[] active_guides = [ItemID.DontHurtNatureBook, ItemID.DontHurtCrittersBook, ItemID.DontHurtComboBook];
    private static readonly int[] inactive_guides = [ItemID.DontHurtNatureBookInactive, ItemID.DontHurtCrittersBookInactive, ItemID.DontHurtComboBookInactive];

    public override void SetStaticDefaults()
    {
        foreach (var type in active_guides)
        {
            ItemID.Sets.HasItemSwap[type] = false;
        }
    }

    public override bool? UseItem(Item item, Player player)
    {
        if (!active_guides.Contains(item.type))
        {
            return null;
        }

        return true;
    }

    public override void SetDefaults(Item item)
    {
        if (active_guides.Contains(item.type))
        {
            item.consumable = true;
            item.useStyle = ItemUseStyleID.HoldUp;
            item.useTime = 30;
            item.UseSound = SoundID.Item4;
            item.useAnimation = 30;

            return;
        }

        if (!inactive_guides.Contains(item.type))
        {
            return;
        }

        var type = item.type switch
        {
            ItemID.DontHurtNatureBookInactive => ItemID.DontHurtNatureBook,
            ItemID.DontHurtCrittersBookInactive => ItemID.DontHurtCrittersBook,
            _ => ItemID.DontHurtComboBook,
        };

        item.SetDefaults(type);
    }

    public override bool ConsumeItem(Item item, Player player)
    {
        if (!active_guides.Contains(item.type))
        {
            return true;
        }

        var modPlayer = player.GetModPlayer<GuidesPlayer>();

        if (item.type switch
        {
            ItemID.DontHurtNatureBook => modPlayer.UnlockedEnvironmentalPreservation,
            ItemID.DontHurtCrittersBook => modPlayer.UnlockedCritterCompanionship,
            _ => modPlayer.UnlockedEnvironmentalPreservation && modPlayer.UnlockedCritterCompanionship,
        })
        {
            return false;
        }

        return true;
    }

    public override void OnConsumeItem(Item item, Player player)
    {
        var modPlayer = player.GetModPlayer<GuidesPlayer>();

        var combo = item.type == ItemID.DontHurtComboBook;

        if (item.type == ItemID.DontHurtNatureBook || combo)
        {
            modPlayer.UnlockedEnvironmentalPreservation = true;
        }

        if (item.type == ItemID.DontHurtCrittersBook || combo)
        {
            modPlayer.UnlockedCritterCompanionship = true;
        }
    }

    internal sealed class GuidesPlayer : ModPlayer
    {
        private record struct Packet(int WhoAmI) : IPacket<Packet>
        {
            public Packet() : this(-1)
            { }

            public void Write(BinaryWriter writer)
            {
                writer.Write(WhoAmI);

                var modPlayer = Main.player[WhoAmI].GetModPlayer<GuidesPlayer>();

                writer.Write(modPlayer.UnlockedEnvironmentalPreservation);
                writer.Write(modPlayer.UnlockedCritterCompanionship);
            }

            public static void Receive(BinaryReader reader, int sender)
            {
                sender = reader.ReadInt32();

                var player = Main.player[sender];

                var modPlayer = player.GetModPlayer<GuidesPlayer>();

                modPlayer.UnlockedEnvironmentalPreservation = reader.ReadBoolean();
                modPlayer.UnlockedCritterCompanionship = reader.ReadBoolean();

                if (Main.netMode == NetmodeID.Server)
                {
                    new Packet(sender).Send(PacketDestination.AllExcept(sender));
                }
            }
        }

        public bool UnlockedEnvironmentalPreservation { get; set; }

        public bool UnlockedCritterCompanionship { get; set; }

        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            new Packet(Player.whoAmI).Send(PacketDestination.From(toWho, fromWho));
        }

        public override void CopyClientState(ModPlayer targetCopy)
        {
            var clone = (GuidesPlayer)targetCopy;

            clone.UnlockedEnvironmentalPreservation = UnlockedEnvironmentalPreservation;
            clone.UnlockedCritterCompanionship = UnlockedCritterCompanionship;
        }

        public override void SendClientChanges(ModPlayer clientPlayer)
        {
            var clone = (GuidesPlayer)clientPlayer;

            if (UnlockedEnvironmentalPreservation != clone.UnlockedEnvironmentalPreservation
             || UnlockedCritterCompanionship != clone.UnlockedCritterCompanionship)
            {
                SyncPlayer(-1, Main.myPlayer, false);
            }
        }

        public override void SaveData(TagCompound tag)
        {
            tag[nameof(UnlockedEnvironmentalPreservation)] = UnlockedEnvironmentalPreservation;
            tag[nameof(UnlockedCritterCompanionship)] = UnlockedCritterCompanionship;
        }

        public override void LoadData(TagCompound tag)
        {
            UnlockedEnvironmentalPreservation = tag.GetBool(nameof(UnlockedEnvironmentalPreservation));
            UnlockedCritterCompanionship = tag.GetBool(nameof(UnlockedCritterCompanionship));
        }
    }
}

public static class GuidesPlayerExtensions
{
    extension(Player player)
    {
        public bool UnlockedEnvironmentalPreservation => player.GetModPlayer<PeacefulCoexistenceGuides.GuidesPlayer>().UnlockedEnvironmentalPreservation;

        public bool UnlockedCritterCompanionship => player.GetModPlayer<PeacefulCoexistenceGuides.GuidesPlayer>().UnlockedCritterCompanionship;
    }
}
