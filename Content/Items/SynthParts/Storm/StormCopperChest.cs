using SyntheticEvolution.Common;
using System.Collections.Generic;

namespace SyntheticEvolution.Content.Items.SynthParts.Storm;

public class StormCopperChest : StormPart
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 1;

        Armor = MaterialID.Copper.CalculateArmor(Common.SynthModels.Storm.ChestArmorScale);
        SocketType = PartSlot.SocketTypeEnum.Chest;
        FeatureDescription = MaterialID.Copper.FeatureDescription;
    }
}