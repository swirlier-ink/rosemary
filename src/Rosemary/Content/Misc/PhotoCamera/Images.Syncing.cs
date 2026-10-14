using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Rosemary.Content.Misc;

public static partial class Images
{
    /*
     * Client Joining:
     * > Sends a packet to request the server for the identifiers it has, to prevent sending excess data.
     * > Server receives the packet and sends it back to the caller with the identifiers it has.
     * > Client receives the identifiers, and sends a different packet with the image data of all the images it doesn't have, (how do we best limit this?)
     *
     * New Photo Being Taken:
     * > Sends a packet to the server with the image data.
     * (TODO: Should the host of the server in a non-dedicated server context be told it has the images locally? If so, how?)
    */

    private record struct HostIdentifiersPacket(int WhoAmI, bool Asking, bool SendImages) : IPacket<HostIdentifiersPacket>
    {
        public HostIdentifiersPacket() : this(-1, false, false)
        { }

        public void Write(BinaryWriter writer)
        {
            writer.Write(WhoAmI);

            writer.Write(Asking);
            writer.Write(SendImages);

            if (Asking)
            {
                return;
            }

            writer.Write(local_identifiers.Count);

            foreach (var id in local_identifiers)
            {
                writer.Write(id);
            }
        }

        public static void Receive(BinaryReader reader, int sender)
        {
            sender = reader.ReadInt32();

            // Asking
            if (reader.ReadBoolean())
            {
                if (Main.netMode != NetmodeID.Server)
                {
                    return;
                }

                new HostIdentifiersPacket(-1, false, reader.ReadBoolean()).Send(PacketDestination.Only(sender));

                return;
            }

            if (Main.netMode == NetmodeID.Server)
            {
                return;
            }

            var sendImages = reader.ReadBoolean();

            var count = reader.ReadInt32();

            for (var i = 0; i < count; i++)
            {
                multiplayer_host_identifiers.Add(reader.ReadString());
            }

            if (!sendImages)
            {
                return;
            }

            ScanPlayer(Main.LocalPlayer);

            return;

            static void ScanPlayer(Player player)
            {
                var seen = new HashSet<string>();

                ScanItems_Syncing(player.armor, seen);
                ScanItems_Syncing(player.dye, seen);

                // Technically overkill as polaroids cant really be in these slots, but
                foreach (var loadout in player.Loadouts)
                {
                    ScanItems_Syncing(loadout.Armor, seen);
                    ScanItems_Syncing(loadout.Dye, seen);
                }

                ScanItems_Syncing(player.inventory, seen);
                ScanItems_Syncing(player.miscEquips, seen);
                ScanItems_Syncing(player.miscDyes, seen);
                ScanItems_Syncing(player.bank, seen);
                ScanItems_Syncing(player.bank2, seen);
                ScanItems_Syncing(player.bank3, seen);
                ScanItems_Syncing(player.bank4, seen);
            }
        }
    }

    private static void ScanItems_Syncing(Chest chest, HashSet<string> seenIdentifiers) => ScanItems_Syncing(chest.item, seenIdentifiers);

    private static void ScanItems_Syncing(Item[] items, HashSet<string> seenIdentifiers)
    {
        foreach (var item in items)
        {
            if (item.ModItem is not PolaroidItem polaroid)
            {
                continue;
            }

            if (multiplayer_host_identifiers.Contains(polaroid.ImageIdentifier)
             || !seenIdentifiers.Add(polaroid.ImageIdentifier))
            {
                continue;
            }

            new SendImageToHostPacket(polaroid.ImageIdentifier).Send(PacketDestination.Broadcast);
        }
    }

    private record struct SendImageToHostPacket(string Identifier) : IPacket<SendImageToHostPacket>
    {
        public SendImageToHostPacket() : this(string.Empty)
        { }

        public void Write(BinaryWriter writer)
        {
            if (!Directory.Exists(ImageSavesPath))
            {
                return;
            }

            var path = GetImagePath(Identifier);

            var info = new FileInfo(path);

            // 2mb limit, overkill?
            if (info.Length >= 2 * 1024 * 1024)
            {
                writer.Write(false);

                return;
            }

            writer.Write(true);

            writer.Write(Identifier);

            writer.Write(info.Length);
            writer.Write(File.ReadAllBytes(path));
        }

        public static void Receive(BinaryReader reader, int sender)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                return;
            }

            if (!reader.ReadBoolean())
            {
                return;
            }

            var id = reader.ReadString();

            var length = reader.ReadInt64();

            var bytes = new byte[length];

            for (var i = 0; i < length; i++)
            {
                bytes[i] = reader.ReadByte();
            }

            var path = GetImagePath(id);

            if (File.Exists(path))
            {
                local_identifiers.Add(id);
                new HostIdentifiersPacket(-1, false, false).Send(PacketDestination.Broadcast);

                return;
            }

            File.WriteAllBytesAsync(path, bytes).ContinueWith(_ => { local_identifiers.Add(id); });

            new HostIdentifiersPacket(-1, false, false).Send(PacketDestination.Broadcast);
        }
    }

    private record struct ImageFromHostPacket(int WhoAmI, bool Asking, string Identifier) : IPacket<ImageFromHostPacket>
    {
        public ImageFromHostPacket() : this(-1, false, string.Empty)
        { }

        public void Write(BinaryWriter writer)
        {
            writer.Write(WhoAmI);

            writer.Write(Asking);

            writer.Write(Identifier);

            if (Asking)
            {
                return;
            }

            var path = GetImagePath(Identifier);

            if (!File.Exists(path))
            {
                writer.Write(false);

                return;
            }

            var info = new FileInfo(path);

            // 1mb limit, overkill?
            if (info.Length >= 1024 * 1024)
            {
                writer.Write(false);

                return;
            }

            writer.Write(true);

            writer.Write(Identifier);

            writer.Write(info.Length);
            writer.Write(File.ReadAllBytes(path));
        }

        public static void Receive(BinaryReader reader, int sender)
        {
            sender = reader.ReadInt32();

            var asking = reader.ReadBoolean();

            var id = reader.ReadString();

            if (asking)
            {
                new ImageFromHostPacket(sender, false, id).Send(PacketDestination.Only(sender));
                return;
            }

            if (!reader.ReadBoolean())
            {
                return;
            }

            var length = reader.ReadInt64();

            var bytes = new byte[length];

            for (var i = 0; i < length; i++)
            {
                bytes[i] = reader.ReadByte();
            }

            var path = GetImagePath(id);

            if (File.Exists(path))
            {
                local_identifiers.Add(id);
                waiting_on_identifiers.Remove(id);
                new HostIdentifiersPacket(-1, false, false).Send(PacketDestination.Broadcast);

                return;
            }

            File.WriteAllBytesAsync(path, bytes).ContinueWith(_ => { local_identifiers.Add(id); waiting_on_identifiers.Remove(id); });
        }
    }

    // TODO: How should we handle images being deleted or otherwise removed past load-time?
    private static readonly HashSet<string> local_identifiers = [];

    private static readonly HashSet<string> multiplayer_host_identifiers = [];

    private static readonly HashSet<string> waiting_on_identifiers = [];

    [OnLoad]
    private static void Load_Syncing()
    {
        PopulateLocalIdentifiers();
    }

    [ModSystemHooks.OnWorldLoad]
    private static void OnWorldLoad_Syncing()
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
        {
            return;
        }

        new HostIdentifiersPacket(Main.myPlayer, true, true).Send(PacketDestination.Broadcast);
    }

    [ModSystemHooks.OnWorldUnload]
    private static void OnWorldUnload_Syncing()
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
        {
            return;
        }

        multiplayer_host_identifiers.Clear();
        waiting_on_identifiers.Clear();
    }

    private static void PopulateLocalIdentifiers()
    {
        try
        {
            Directory.CreateDirectory(ImageSavesPath);
        }
        catch
        {
            ModContent.GetInstance<ModImpl>().Logger.Warn($"Could not create directory at: \"{ImageSavesPath}\"!");
            return;
        }

        var files = Directory.EnumerateFiles(ImageSavesPath);

        foreach (var file in files)
        {
            if (Path.GetExtension(file) == image_extension)
            {
                local_identifiers.Add(Path.GetFileNameWithoutExtension(file));
            }
        }
    }
}
