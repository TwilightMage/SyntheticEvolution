using Microsoft.Xna.Framework;
using System;
using Terraria.ID;

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
        FeatureDescription = "[c/CD8647:Copper] is a [c/7F7F7F:light-weight] material, but [c/7F7F7F:not] very [c/7F7F7F:tough]."
    };
}