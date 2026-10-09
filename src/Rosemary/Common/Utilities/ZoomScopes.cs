using System;
using System.Collections.Generic;
using System.Text;
using Terraria;
using Terraria.GameInput;

namespace Rosemary.Common;

public enum ZoomScaleType
{
    Background,
    World,
    MouseInWorld,
    UI,
    Unscaled
}

public readonly ref struct PlayerInputZoomScope : IDisposable
{
    private readonly int originalMouseX;
    private readonly int originalMouseY;

    private readonly int originalLastMouseX;
    private readonly int originalLastMouseY;

    private readonly int originalScreenWidth;
    private readonly int originalScreenHeight;

    public PlayerInputZoomScope(ZoomScaleType type)
    {
        switch (type)
        {
            case ZoomScaleType.Background:
            {
                PlayerInput.SetZoom_Background();
            }
            break;

            case ZoomScaleType.World:
            {
                PlayerInput.SetZoom_World();
            }
            break;

            case ZoomScaleType.MouseInWorld:
            {
                PlayerInput.SetZoom_MouseInWorld();
            }
            break;

            case ZoomScaleType.UI:
            {
                PlayerInput.SetZoom_UI();
            }
            break;

            case ZoomScaleType.Unscaled:
            default:
            {
                PlayerInput.SetZoom_Unscaled();
            }
            break;
        }
    }

    public PlayerInputZoomScope(float scale)
    {
        PlayerInput.SetZoom_Scaled(scale);
    }

    public void Dispose()
    {
        Main.lastMouseX = originalLastMouseX;
        Main.lastMouseY = originalLastMouseY;
        Main.mouseX = originalMouseX;
        Main.mouseY = originalMouseY;
        Main.screenWidth = originalScreenWidth;
        Main.screenHeight = originalScreenHeight;
    }
}

public static class PlayerInputExtensions
{
    extension(PlayerInput)
    {
        public static PlayerInputZoomScope ZoomScope(ZoomScaleType type) => new(type);

        public static PlayerInputZoomScope ScaledZoomScope(float scale) => new(scale);
    }
}
