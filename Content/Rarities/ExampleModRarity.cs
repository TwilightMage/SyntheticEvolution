using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace SyntheticEvolution.Content.Rarities
{
    public class ExampleModRarity : ModRarity
    {
        public override Color RarityColor => new Color(200, 215, 230);

        public override int GetPrefixedRarity(int offset, float valueMult)
        {
            return Type; // no 'lower' tier to go to, so return the type of this rarity.
        }
    }
}