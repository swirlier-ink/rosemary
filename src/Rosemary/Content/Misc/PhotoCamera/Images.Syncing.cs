using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Rosemary.Content.Misc;

public static partial class Images
{
    /// <summary>
    ///     Updates the host's image identifiers and the identifiers that clients think the host has.<br/><br/>
    ///
    ///     <b>Client joining world:</b>
    ///     <list type="bullet">
    ///         <item><description>
    ///             Sends this packet with <paramref name="Asking"/> and
    ///             <paramref name="SendImages"/> set to <see langword="true"/>.
    ///         </description></item>
    ///         <item><description>
    ///             Host (server) receives the packet, and returns it to the
    ///             sender with <paramref name="Asking"/> set to <see langword="false"/>,
    ///             which writes all the server's local identifiers.
    ///         </description></item>
    ///         <item><description>
    ///             Client receives the host's identifiers and writes them to <see cref="multiplayer_host_identifiers"/>;<br/>
    ///             if <paramref name="SendImages"/> is <see langword="true"/> then the client will
    ///             scan its personal inventories for polaroids with identifiers that the host doesn't have,
    ///             then sends them with <see cref="Images.SendImage"/>.
    ///         </description></item>
    ///     </list>
    /// </summary>
    private record struct HostIdentifiersPacket(int WhoAmI, bool Asking, bool SendImages) : IPacket<HostIdentifiersPacket>
    {
        public HostIdentifiersPacket() : this(-1, false, false) { }

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

                // Technically overkill as polaroids cant really be in these slots, but
                ScanItems_Syncing(player.armor, seen);
                ScanItems_Syncing(player.dye, seen);

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

            SendImage(polaroid.ImageIdentifier, -1);
        }
    }

    private record struct RequestImageFromHostPacket(int FromWho, string Identifier) : IPacket<RequestImageFromHostPacket>
    {
        public RequestImageFromHostPacket() : this(-1, string.Empty) { }

        public void Write(BinaryWriter writer)
        {
            writer.Write(FromWho);
            writer.Write(Identifier);
        }

        public static void Receive(BinaryReader reader, int sender)
        {
            sender = reader.ReadInt32();
            var id = reader.ReadString();

            SendImage(id, sender);
        }
    }

    private record struct SendImageSegmentPacket(string Identifier, int TotalSize, byte[] Segment) : IPacket<SendImageSegmentPacket>
    {
        public SendImageSegmentPacket() : this(string.Empty, -1, []) { }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Identifier);
            writer.Write(TotalSize);
            writer.Write(Segment.Length);
            writer.Write(Segment);
        }

        public static void Receive(BinaryReader reader, int sender)
        {
            var id = reader.ReadString();

            var totalCount = reader.ReadInt32();

            var count = reader.ReadInt32();
            var bytes = reader.ReadBytes(count);

            if (!incoming_images.TryGetValue(id, out var info))
            {
                info = incoming_images[id] = new IncomingImageData(totalCount);
            }

            Array.Copy(bytes, 0, info.Data, info.Index, bytes.Length);

            info.Index += count;

            if (info.Index < totalCount)
            {
                return;
            }

            var path = GetImagePath(id);

            File.WriteAllBytesAsync(path, info.Data).ContinueWith(
                _ =>
                {
                    local_identifiers.Add(id);
                    waiting_on_identifiers.Remove(id);

                    if (Main.netMode != NetmodeID.Server)
                    {
                        return;
                    }

                    new HostIdentifiersPacket(-1, false, false).Send(PacketDestination.Broadcast);
                }
            );
        }
    }

    private sealed class IncomingImageData(int length)
    {
        public readonly byte[] Data = new byte[length];

        public int Index = 0;
    }

    private static readonly Dictionary<string, IncomingImageData> incoming_images = [];

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

        incoming_images.Clear();

        new HostIdentifiersPacket(Main.myPlayer, true, true).Send(PacketDestination.Broadcast);
    }

    [ModSystemHooks.OnWorldUnload]
    private static void OnWorldUnload_Syncing()
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
        {
            return;
        }

        incoming_images.Clear();

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

    /// <summary>
    ///     Sends the image with identifier <paramref name="id"/> to the target client <paramref name="toWho"/>.
    /// </summary>
    /// <param name="id">
    ///     Identifier of an image that exists locally.
    /// </param>
    /// <param name="toWho">
    ///     Target client, should be <c>-1</c> being run on a client, (as clients should only send images to the host.)
    /// </param>
    public static void SendImage(string id, int toWho)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
        {
            return;
        }

        var path = GetImagePath(id);

        if (!File.Exists(path))
        {
            return;
        }

        var info = new FileInfo(path);

        // 1mb limit, overkill?
        if (info.Length >= 1024 * 1024)
        {
            return;
        }

        const ushort max_bytes = ushort.MaxValue;

        var stringSize = 5
                       + Encoding.UTF8.GetByteCount(id);

        var segmentByteCount = max_bytes
                             - stringSize
                             - sizeof(int) // Total length
                             - sizeof(int) // Segment length
                             - 128;        // Padding because I don't trust something to not go wrong

        var bytes = File.ReadAllBytes(path);

        for (var i = 0; i < bytes.Length; i += segmentByteCount)
        {
            var segment = bytes.Skip(i).Take(segmentByteCount).ToArray();

            new SendImageSegmentPacket(id, bytes.Length, segment).Send(PacketDestination.Only(toWho));
        }
    }
}
