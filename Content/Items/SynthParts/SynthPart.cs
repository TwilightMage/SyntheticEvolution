using Microsoft.Xna.Framework;
using SyntheticEvolution.Common;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Terraria.Localization;
using Terraria.ModLoader;

namespace SyntheticEvolution.Content.Items.SynthParts;

public abstract class SynthPart : ModItem
{
    public float Armor = 0;
    public string FeatureDescription = null;
    public Type[] SynthTypes = Array.Empty<Type>();
    public PartSlot.SocketTypeEnum SocketType = PartSlot.SocketTypeEnum.Module;
    public bool HaveSpecificSynthTypes = false;

    public override void SetDefaults()
    {
        base.SetDefaults();
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        if (FeatureDescription != null) tooltips.Insert(1, new TooltipLine(Mod, "Feature", FeatureDescription));
        if (Armor > 0) tooltips.Insert(1, new TooltipLine(Mod, "Armor", Language.GetText("Mods.SyntheticEvolution.UI.ToolTip.Armor").Format(Armor.ToString("#0.0"))));
    }

    public virtual void ModifyDisplayStats(Dictionary<string, object> stats)
    {
        stats["Armor"] ??= 0f;
        stats["Armor"] = (float)stats["Armor"] + Armor;
    }
}