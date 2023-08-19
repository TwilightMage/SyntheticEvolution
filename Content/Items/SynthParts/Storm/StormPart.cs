using SyntheticEvolution.Common;
using System.Collections.Generic;
using Terraria.Localization;
using Terraria.ModLoader;

namespace SyntheticEvolution.Content.Items.SynthParts.Storm;

public abstract class StormPart : SynthPart
{
    public float SlideDamage = 0;

    public override void SetDefaults()
    {
        base.SetDefaults();
    }
    
    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        base.ModifyTooltips(tooltips);
        
        if (SlideDamage > 0) tooltips.Insert(1, new TooltipLine(Mod, "SlideDamage", Language.GetText("Mods.SyntheticEvolution.UI.ToolTip.SlideDamage").Format(SlideDamage.ToString("#0.0"))));
    }

    public override void ModifyDisplayStats(Dictionary<string, object> stats)
    {
        base.ModifyDisplayStats(stats);
        
        stats["SlideDamage"] ??= 0f;
        stats["SlideDamage"] = (float)stats["SlideDamage"] + SlideDamage;
    }
}