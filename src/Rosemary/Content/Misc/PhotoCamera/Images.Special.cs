using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Rosemary.Content.Misc;

public static partial class Images
{
    private static bool[] hiddenOnCamera = [];

    private static Mod Mod => ModContent.GetInstance<ModImpl>();

    [ModSystemHooks.ResizeArrays]
    private static void ResizeArrays()
    {
        hiddenOnCamera = CreateSet(nameof(hiddenOnCamera), false);

        NPCID.Sets.HiddenOnCamera[NPCID.Vampire] = true;
        NPCID.Sets.HiddenOnCamera[NPCID.VampireBat] = true;

        return;

        static T[] CreateSet<T>(string name, T defaultState)
        {
            return NPCID.Sets.Factory.CreateNamedSet(Mod, name)
                         .RegisterCustomSet(defaultState);
        }
    }

    extension(NPCID.Sets)
    {
        public static bool[] HiddenOnCamera => hiddenOnCamera;
    }

    [OnLoad]
    private static void Load_Special()
    {
        // This same effect could be achieved via GlobalNPC or s
        On_Main.DrawNPCDirect += DrawNPCDirect_Exclusions;
    }

    private static void DrawNPCDirect_Exclusions(On_Main.orig_DrawNPCDirect orig, Main self, SpriteBatch sb, NPC npc, bool behindTiles, Vector2 screenPos)
    {
        if (!captureRequested
         || !NPCID.Sets.HiddenOnCamera[npc.type])
        {
            orig(self, sb, npc, behindTiles, screenPos);
            return;
        }

        var frame = GetCaptureArea();

        var worldFrame = frame;
        worldFrame.X += (int)Main.screenPosition.X;
        worldFrame.Y += (int)Main.screenPosition.Y;

        var npcBounds = npc.Hitbox;

        // TODO: See if scale is correctly accounted for
        var length = (int)npc.Hitbox.Size().Length();

        npcBounds.Inflate(((length - npcBounds.Width) / 2), ((length - npcBounds.Height) / 2));

        if (!npcBounds.Intersects(worldFrame))
        {
            orig(self, sb, npc, behindTiles, screenPos);

            return;
        }

        // Contained entirely within frame, ignore
        if (worldFrame.Contains(npcBounds))
        {
            return;
        }

        var device = self.GraphicsDevice;
        var viewport = device.Viewport.Bounds;

        var sectors = new[]
        {
            // Left
            new Rectangle(0, frame.Y, frame.X, frame.Height),
            // Right
            new Rectangle(frame.Right, frame.Y, viewport.Width - frame.Right, frame.Height),
            // Up (spans whole width)
            new Rectangle(0, 0, viewport.Width, frame.Y),
            // Down (spans whole width)
            new Rectangle(0, frame.Bottom, viewport.Width, viewport.Height - frame.Bottom),
        };

        var npcScreen = npcBounds;
        npcScreen.X -= (int)Main.screenPosition.X;
        npcScreen.Y -= (int)Main.screenPosition.Y;

        foreach (var rect in sectors)
        {
            if (npcScreen.Intersects(rect))
            {
                DrawToSection(rect);
            }
        }

        return;

        void DrawToSection(Rectangle sector)
        {
            sb.End(out var ss);

            var state = ss.RasterizerState;

            var prior = state.ScissorTestEnable;
            state.ScissorTestEnable = true;

            var priorRect = device.ScissorRectangle;
            device.ScissorRectangle = Rectangle.Intersect(priorRect, sector);

            sb.Begin(ss with { RasterizerState = state });
            {
                orig(self, sb, npc, behindTiles, screenPos);
            }
            sb.End();

            device.ScissorRectangle = priorRect;
            state.ScissorTestEnable = prior;

            sb.Begin(ss with { RasterizerState = state });
        }
    }
}