using Microsoft.Xna.Framework;
using SyntheticEvolution.Common.SynthModels;
using System;
using Terraria;

namespace SyntheticEvolution.Common;

public static class Extensions
{
    public static SynthModel GetSynth(this Player player) => player.GetModPlayer<SynthPlayer>().SynthModel;

    public static Vector2 Normalized(this Vector2 vec) => Vector2.Normalize(vec);
}