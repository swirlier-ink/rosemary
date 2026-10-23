using Rosemary.Common;
using Terraria.Graphics.Shaders;

namespace Rosemary.Vanity.Content;

public sealed class EmptinessDye : AbstractDyeItem
{
    public override string Texture => Assets.Vanity.Siffrin.EmptinessDye.KEY;

    public override string LocalizationCategory => "Content";

    public override ShaderData Data => Assets.Vanity.Siffrin.InvertPlayerDye.CreateInvertPlayerDyeShader();

    public override IDyeDefinition Definition => new SpecialDye(Type);
}
