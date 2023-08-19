using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace SyntheticEvolution.Content.Items.Placeable.Furniture
{
    public class SynthPartsFabricator : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 22;
            Item.maxStack = 99;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.value = 5 * 100 * 100;
            Item.createTile = ModContent.TileType<Tiles.Furniture.SynthPartsFabricator>();
        }

        public override bool CanStack(Item source)
        {
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                //.AddTile(TileID.Anvils)
                .Register();
        }
    }
}