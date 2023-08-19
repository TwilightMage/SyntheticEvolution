using Microsoft.Xna.Framework;
using SyntheticEvolution.Common;
using SyntheticEvolution.Common.SynthModels;
using SyntheticEvolution.Content.TileEntities;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Utils = SyntheticEvolution.Common.Utils;

namespace SyntheticEvolution.Content.Tiles.Furniture
{
    public class StasisPod : ModTile
    {
        private static LocalizedText _mapName;
        private static LocalizedText _mapNameFilled;
        
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
            TileObjectData.newTile.Height = 4;
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<StasisPodTileEntity>().Hook_AfterPlacement, -1, 0, false);
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16, 16 };
            TileObjectData.addTile(Type);

            _mapName = Language.GetText("Mods.SyntheticEvolution.Tiles.StasisPod.MapEntry");
            _mapNameFilled = Language.GetText("Mods.SyntheticEvolution.Tiles.StasisPod.MapEntryFilled");
            
            AddMapEntry(Color.MediumSlateBlue, _mapName, MapName);
        }

        public static string MapName(string name, int i, int j)
        {
            if (Utils.TryGetTileEntityAs(i, j, out StasisPodTileEntity entity) && entity.HostName != null) return _mapNameFilled.Format(entity.HostName);
            return _mapName.Value;
        }

        public override bool RightClick(int x, int y)
        {
            var player = Main.LocalPlayer.GetModPlayer<SynthPlayer>();
            if (player.SynthModel == null)
            {
                if (player.SetSynthModel<Storm>())
                {
                    if (Utils.TryGetTileEntityAs(x, y, out StasisPodTileEntity entity))
                    {
                        entity.SetHost(player.Player);
                    }
                }
            }
            else
            {
                player.ClearSynthModel();
                
                if (Utils.TryGetTileEntityAs(x, y, out StasisPodTileEntity entity))
                {
                    entity.ClearHost();
                }
            }

            return true;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            ModContent.GetInstance<StasisPodTileEntity>().Kill(i, j);
        }
    }
}