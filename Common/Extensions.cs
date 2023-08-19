using SyntheticEvolution.Common.SynthModels;
using Terraria;

namespace SyntheticEvolution.Common;

public static class Extensions
{
    public static SynthModel GetSynth(this Player player) => player.GetModPlayer<SynthPlayer>().SynthModel;
}