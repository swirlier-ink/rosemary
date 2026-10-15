using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using ReLogic.Content;
using Rosemary.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Chat;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;
using Terraria.UI.Chat;
using static System.Net.Mime.MediaTypeNames;

namespace Rosemary.Content.Misc;

public sealed class PolaroidItem : ModItem
{
    public override string Texture => Assets.Misc.Camera.Polaroid.KEY;

    public override string LocalizationCategory => "Content.Misc";

    public override bool CanStack(Item source)
    {
        if (source.ModItem is not PolaroidItem polaroid)
        {
            return false;
        }

        return ImageIdentifier == polaroid.ImageIdentifier
            && !Original
            && !polaroid.Original;
    }

    public override bool CanResearch() => false;

    public override void Load()
    {
        if (Main.dedServ)
        {
            return;
        }

        MonoModHooks.Add(
            typeof(Main).GetMethod(
                nameof(Main.MouseText_DrawItemTooltip),
                BindingFlags.Instance | BindingFlags.NonPublic
            ),
            MouseText_DrawItemTooltip_HideTooltipBox
        );

        On_ItemTagHandler.ItemSnippet.OnHover += OnHover_DisableTooltip;

        IL_Recipe.UpdateRecipeList += UpdateRecipeList_AddClonePolaroids;

        On_ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color += Draw_CopyAsterisk;
    }

    private static void Draw_CopyAsterisk(On_ItemSlot.orig_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color orig, SpriteBatch sb, Item[] inv, int context, int slot, Vector2 position, Color lightColor)
    {
        orig(sb, inv, context, slot, position, lightColor);

        if (inv[slot].type != ModContent.ItemType<PolaroidItem>()
         || inv[slot].ModItem is not PolaroidItem { Original: false })
        {
            return;
        }

        var color = Color.White;
        if (lightColor != Color.Transparent)
        {
            color = lightColor;
        }

        var scale = Main.inventoryScale;

        var font = FontAssets.MouseText.Value;

        const string text = "*";

        var boxSize = TextureAssets.InventoryBack.Value;

        var textSize = font.MeasureString(text);

        ChatManager.DrawColorCodedStringWithShadow(
            sb,
            font,
            "*",
            position + new Vector2(boxSize.Width - 5f, 5f) * scale,
            color,
            0f,
            new Vector2(textSize.X, 0),
            new Vector2(scale),
            -1f,
            scale
        );
    }

    private static void UpdateRecipeList_AddClonePolaroids(ILContext il)
    {
        var c = new ILCursor(il);

        c.GotoNext(
            MoveType.After,
            i => i.MatchCall<Recipe>(nameof(Recipe.ClearAvailableRecipes))
        );

        c.EmitDelegate(
            static () =>
            {
                ClearPolaroidRecipes();

                var identifiers = new HashSet<string>();

                if (Main.guideItem.ModItem is PolaroidItem polaroid)
                {
                    AddPolaroidRecipe(polaroid);
                    return;
                }

                foreach (var chest in Recipe._recipeChests)
                {
                    AddPolaroidsFromChest(chest.item);
                }

                var inv = Main.LocalPlayer.inventory;

                // Bar mouse item
                AddPolaroidsFromChest(inv.Take(58));

                return;

                void ClearPolaroidRecipes()
                {
                    for (var i = 0; i < Main.recipe.Length; i++)
                    {
                        if (Main.recipe[i].createItem.type != ModContent.ItemType<PolaroidItem>())
                        {
                            continue;
                        }

                        Main.recipe[i] = new Recipe();
                        CraftingUI.availableRecipeY[i] = 65f * i;

                        // :sideeye:
                        Recipe.numRecipes--;
                    }
                }

                void AddPolaroidsFromChest(IEnumerable<Item> items)
                {
                    foreach (var item in items)
                    {
                        if (item.ModItem is not PolaroidItem polaroid)
                        {
                            continue;
                        }

                        AddPolaroidRecipe(polaroid);
                    }
                }

                void AddPolaroidRecipe(PolaroidItem parent)
                {
                    if (!identifiers.Add(parent.ImageIdentifier))
                    {
                        return;
                    }

                    var recipe = new Recipe(ModContent.GetInstance<ModImpl>());

                    recipe.createItem = parent.Item.Clone();
                    recipe.createItem.stack = 1;
                    ((PolaroidItem)recipe.createItem.ModItem).Original = false;

                    recipe.notDecraftable = true;

                    // Original polaroids should not be consumed
                    recipe.AddConsumeIngredientCallback(DisableIngredientConsume);

                    var req = parent.Item.Clone();
                    req.stack = 1;

                    // Not a requirement
                    ((PolaroidItem)req.ModItem).Original = true;

                    recipe.requiredItem.Add(req);

                    // Vanilla logic from `Recipe.CreateRequiredItemQuickLookups`
                    recipe.requiredItemQuickLookup = new Recipe.RequiredItemEntry[recipe.requiredItem.Count];
                    for (var j = 0; j < recipe.requiredItem.Count; j++)
                    {
                        var item = recipe.requiredItem[j];
                        var requiredItemEntry = new Recipe.RequiredItemEntry(item.type, item.stack);
                        foreach (var num in recipe.acceptedGroups)
                        {
                            var recipeGroup = RecipeGroup.recipeGroups[num];
                            if (recipeGroup.ValidItems.Contains(item.type))
                            {
                                requiredItemEntry.itemIdOrRecipeGroup = recipeGroup.GetGroupFakeItemId();
                            }
                        }
                        recipe.requiredItemQuickLookup[j] = requiredItemEntry;
                    }

                    recipe.Register();
                }

                static void DisableIngredientConsume(Recipe recipe, int type, ref int amount, bool isDecrafting)
                {
                    amount = 0;
                }
            }
        );
    }

    private static void OnHover_DisableTooltip(On_ItemTagHandler.ItemSnippet.orig_OnHover orig, TextSnippet self)
    {
        orig(self);

        if (Main.HoverItem.ModItem is PolaroidItem polaroid)
        {
            polaroid.ShowTooltip = false;
        }
    }

    private static void MouseText_DrawItemTooltip_HideTooltipBox(Action<Main, Main.MouseTextCache, int, byte, int, int> orig, Main self, Main.MouseTextCache info, int rare, byte diff, int x, int y)
    {
        using var _ = Main.SettingsEnabled_OpaqueBoxBehindTooltips.Cache();

        if (Main.HoverItem.ModItem is PolaroidItem { ShowTooltip: true })
        {
            Main.SettingsEnabled_OpaqueBoxBehindTooltips = false;
        }

        orig(self, info, rare, diff, x, y);
    }

    public override void SetStaticDefaults()
    {
        ItemID.Sets.DisableAutomaticPlaceableDrop[Type] = true;
    }

    public override void SetDefaults()
    {
        Item.width = 40;
        Item.height = 40;

        Item.value = 0;

        // Purely to prevent burning
        Item.rare = ItemRarityID.Blue;

        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTurn = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.autoReuse = true;
        Item.consumable = true;

        Item.createTile = ModContent.TileType<PolaroidTile>();

        Age = 0;
    }

    public bool ShowTooltip = true;

    public string ImageIdentifier = string.Empty;

    public uint Age;

    public bool Original;

    public bool OfCryptid
    {
        get;

        set
        {
            Item.rare = value ? ItemRarityID.Orange : ItemRarityID.Blue;
            field = value;
        }
    }

    public override ModItem Clone(Item newEntity)
    {
        var clone = (PolaroidItem)base.Clone(newEntity);
        {
            clone.ImageIdentifier = ImageIdentifier;
            clone.Age = Age;
            clone.OfCryptid = OfCryptid;
            clone.Original = Original;
        }
        return clone;
    }

    public override void SaveData(TagCompound tag)
    {
        tag[nameof(ImageIdentifier)] = ImageIdentifier;
        tag[nameof(Age)] = Age;
        tag[nameof(OfCryptid)] = OfCryptid;
        tag[nameof(Original)] = Original;
    }

    public override void LoadData(TagCompound tag)
    {
        ImageIdentifier = tag.Get<string>(nameof(ImageIdentifier));
        Age = tag.Get<uint>(nameof(Age));
        OfCryptid = tag.Get<bool>(nameof(OfCryptid));
        Original = tag.Get<bool>(nameof(Original));
    }

    public override void NetSend(BinaryWriter writer)
    {
        writer.Write(ImageIdentifier);
        writer.Write(Age);
        writer.Write(OfCryptid);
        writer.Write(Original);
    }

    public override void NetReceive(BinaryReader reader)
    {
        ImageIdentifier = reader.ReadString();
        Age = reader.ReadUInt32();
        OfCryptid = reader.ReadBoolean();
        Original = reader.ReadBoolean();

        Images.TrackImageDevelopment(this);
    }

    // TODO:
    // - Also make the image ONLY be requested if hovered in the inventory, not in world/chat
    public override bool PreDrawTooltip(ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y)
    {
        if (!ShowTooltip)
        {
            return true;
        }

        var position = new Vector2(x, y);

        if (!Images.TryRequestImage(ImageIdentifier, out var texture))
        {
            DrawPolaroid(Asset<Texture2D>.DefaultValue);

            return false;
        }

        DrawPolaroid(texture);

        return false;

        void DrawPolaroid(Texture2D texture)
        {
            var sb = Main.spriteBatch;

            position -= new Vector2(4f);

            var screenSize = new Vector2(Main.screenWidth, Main.screenHeight);

            var polaroidBase = Assets.Misc.Camera.Polaroid_Tooltip.Asset.Value;

            var bottomRight = position + polaroidBase.Size();

            position -= Vector2.Clamp(bottomRight - screenSize, Vector2.Zero, screenSize);

            var bounds = new Rectangle((int)position.X + 18, (int)position.Y + 32, Images.BASE_RESOLUTION, Images.BASE_RESOLUTION);

            sb.Draw(polaroidBase, position, Color.White);
            
            sb.End(out var ss);

            var shader = Assets.Misc.Camera.Vignette.CreateVignetteShader();
            shader.Parameters.Intensity = 3;
            shader.Parameters.Power = 6;
            shader.Apply();
            sb.Begin(ss with { CustomEffect = shader.Shader });
            {
                sb.Draw(texture, bounds, Color.White);

                var blackFade = 1f - MathHelper.Clamp((Age - 120f) / 400f, 0, 1);

                sb.Draw(texture, bounds, Color.Black * blackFade);
            }
            
            sb.Restart(ss);
        }
    }
}
