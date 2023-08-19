using Microsoft.Xna.Framework;

namespace SyntheticEvolution.Common.ChainPhysics;

public class ChainPoint
{
    public Vector2 Position;
    public Vector2 LastPosition;
    public bool Fixed;
    public float Radius;
}