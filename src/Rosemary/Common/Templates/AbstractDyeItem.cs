using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Rosemary.Common;

public abstract class AbstractDyeItem : ModItem
{
    public abstract ShaderData Data { get; }

    public virtual IDyeDefinition? Definition => null;

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 3;

        ItemID.Sets.DyeDefinitions[Type] = Definition;

        if (Main.dedServ)
        {
            return;
        }

        GameShaders.Armor.BindShader(
            Item.type,
            new ArmorShaderData(Data._shader, Data._passName)
        );
    }

    public override void SetDefaults()
    {
        var dye = Item.dye;
        {
            Item.CloneDefaults(ItemID.RedDye);
        }
        Item.dye = dye;
    }
}
