using Microsoft.Xna.Framework.Graphics;

namespace Rosemary.Content.Misc;

public interface IPolaroidCameraFilter
{
    /// <returns><see langword="true"/> if ending by drawing <paramref name="target"/> to <paramref name="swap"/>.</returns>
    bool ApplyFilter(SpriteBatch sb, GraphicsDevice device, RenderTarget2D target, RenderTarget2D swap);
}