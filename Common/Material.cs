using Microsoft.Xna.Framework;
using System;
using Terraria.ID;
using Terraria.Localization;

namespace SyntheticEvolution.Common;

public class Material
{
    public float ArmorModifier;
    public float BladeDamageModifier;
    public short MaterialType;
    public Color Color;
    public string FeatureDescription;

    public float CalculateArmor(float scale) => MathF.Round(ArmorModifier * scale * 100) / 100;
    public float CalculateBladeDamage(float scale) => MathF.Round(BladeDamageModifier * scale * 100) / 100;
}

public static class MaterialID
{
    public static Material Copper = new Material
    {
        ArmorModifier = 5,
        BladeDamageModifier = 8,
        MaterialType = ItemID.CopperBar,
        Color = Color.Chocolate,
        FeatureDescription = Language.GetTextValue("Mods.SyntheticEvolution.MaterialDescriptions.Copper")
    };
}