using SyntheticEvolution.Common;

namespace SyntheticEvolution.Content.Items.SynthParts.Storm;

public class StormCopperLegs : StormPart
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        
        Item.width = 32;
        Item.height = 28;
        Item.maxStack = 1;

        Armor = MaterialID.Copper.CalculateArmor(Common.SynthModels.Storm.LegsArmorScale);
        SlideDamage = MaterialID.Copper.CalculateBladeDamage(1.0f);
        SocketType = PartSlot.SocketTypeEnum.Legs;
        FeatureDescription = Common.SynthModels.Storm.LegsFeatureDescription + "\n" + MaterialID.Copper.FeatureDescription;
    }
}