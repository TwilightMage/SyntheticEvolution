using Microsoft.Xna.Framework;
using SyntheticEvolution.Common.Systems;
using Terraria;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace SyntheticEvolution.Content.Tiles.Furniture
{
    public class SynthPartsFabricator : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16 };
            TileObjectData.addTile(Type);

            AddMapEntry(Color.PaleGreen, CreateMapEntryName());
        }

        public override bool RightClick(int x, int y)
        {
            if (UISystem.IsFabricatorVisible)
            {
                UISystem.CloseFabricator();
            }
            else
            {
                UISystem.OpenFabricator();
            }

            return true;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            
        }
    }
}