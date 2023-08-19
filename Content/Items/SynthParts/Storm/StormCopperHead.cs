using Microsoft.Xna.Framework;
using SyntheticEvolution.Common;
using System.Collections.Generic;

namespace SyntheticEvolution.Content.Items.SynthParts.Storm;

public class StormCopperHead : StormPart
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        
        Item.width = 32;
        Item.height = 23;
        Item.maxStack = 1;

        Armor = MaterialID.Copper.CalculateArmor(Common.SynthModels.Storm.HeadArmorScale);
        SlideDamage = MaterialID.Copper.CalculateBladeDamage(0.3f);
        SocketType = PartSlot.SocketTypeEnum.Head;
        FeatureDescription = MaterialID.Copper.FeatureDescription;
    }
}