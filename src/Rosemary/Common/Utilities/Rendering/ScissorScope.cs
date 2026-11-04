using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rosemary.Common;

public readonly ref struct ScissorRectangleScope : IDisposable
{
    private readonly Rectangle priorRectangle;

    private readonly RasterizerState? rasterizer;

    private readonly bool? priorScissorTest;

    private readonly GraphicsDevice device;

    public ScissorRectangleScope(GraphicsDevice device, Rectangle rect, bool intersect, RasterizerState? state = null)
    {
        this.device = device;

        priorRectangle = device.ScissorRectangle;
        device.ScissorRectangle = intersect ? Rectangle.Intersect(priorRectangle, rect) : rect;

        rasterizer = state;
        if (rasterizer is not null)
        {
            priorScissorTest = rasterizer.ScissorTestEnable;
            rasterizer.ScissorTestEnable = true;
        }
    }

    public void Dispose()
    {
        device.ScissorRectangle = priorRectangle;
        rasterizer?.ScissorTestEnable = priorScissorTest is true;
    }
}

public static class ScissorRectangleExtensions
{
    extension(GraphicsDevice device)
    {
        public ScissorRectangleScope ScissorScope(Rectangle rect, bool intersect = true, RasterizerState? state = null)
        {
            return new ScissorRectangleScope(device, rect, intersect, state);
        }
    }
}
