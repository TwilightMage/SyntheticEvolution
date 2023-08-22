using SyntheticEvolution.Content.Tiles.Furniture;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace SyntheticEvolution.Content.TileEntities;

public class StasisPodTileEntity : ModTileEntity
{
    public string HostName { get; private set; }

    public void SetHost(Player player)
    {
        HostName = player.name;
    }

    public void ClearHost()
    {
        HostName = null;
    }

    public override bool IsTileValidForEntity(int x, int y)
    {
        Tile tile = Main.tile[x, y];
        return tile.HasTile && tile.TileType == ModContent.TileType<StasisPod>();
    }

    public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            int width = 2;
            int height = 4;
            NetMessage.SendTileSquare(Main.myPlayer, i, j, width, height);

            NetMessage.SendData(MessageID.TileEntityPlacement, number: i, number2: j, number3: Type);
        }

        int placedEntity = Place(i - 1, j - 2);
        return placedEntity;
    }

    public override void OnNetPlace()
    {
        if (Main.netMode == NetmodeID.Server)
        {
            NetMessage.SendData(MessageID.TileEntitySharing, number: ID, number2: Position.X, number3: Position.Y);
        }
    }

    public override void SaveData(TagCompound tag)
    {
        base.SaveData(tag);

        tag.Set("HostName", HostName);
    }

    public override void LoadData(TagCompound tag)
    {
        base.LoadData(tag);

        HostName = tag.GetString("HostName");
    }
}