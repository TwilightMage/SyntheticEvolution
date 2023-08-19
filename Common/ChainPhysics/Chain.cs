using Microsoft.Xna.Framework;
using System;

namespace SyntheticEvolution.Common.ChainPhysics;

public class Chain
{
    public ChainPoint[] Points;
    public float SegmentLength;
    public Vector2 HoldPosition;

    public float Gravity = 0.9f;
    public float Drag = 0.999f;
    public int Stiffness = 12;

    public ChainPoint First => Points[0];
    public ChainPoint Last => Points[^1];
    
    public static Chain Create(Vector2 start, Vector2 end, float segmentLength)
    {
        var chain = new Chain();
        chain.Points = new ChainPoint[(int)MathF.Ceiling(Vector2.Distance(start, end) / segmentLength) + 1];
        chain.SegmentLength = segmentLength;
        
        Vector2 normal = Vector2.Normalize(end - start);
        for (int i = 0; i < chain.Points.Length; i++)
        {
            chain.Points[i] = new ChainPoint();
            chain.Points[i].Position = chain.Points[i].LastPosition = start + normal * segmentLength * i;
        }

        return chain;
    }

    public void UpdatePhysics()
    {
        if (Points.Length == 0) return;
        if (SegmentLength <= 0) return;

        //if(mouse.buttonRaw & 1){
        //    if(holding < 0){
        //        holding = closestPoint(mouse.x,mouse.y);
        //    }
        //}else{
        //    holding = -1;
        //}

        for (int i = 0; i < Points.Length; i++)
        {
            if (Points[i].Fixed) continue;

            var v = (Points[i].Position - Points[i].LastPosition) * Drag;
            Points[i].LastPosition = Points[i].Position;
            Points[i].Position += v;
            Points[i].Position.Y += Gravity;
        }

        // attach the last link to the mouse
        ChainPoint holdPoint = Points[0];
        holdPoint.LastPosition = holdPoint.Position = HoldPosition;

        for (var i = 0; i < Stiffness; i++)
        {
            for (int j = 1; j < Points.Length; j++)
            {
                var delta = Points[j].Position - Points[j - 1].Position;
                var deltaLength = delta.Length();
                var fraction = ((SegmentLength - deltaLength) / deltaLength) / 2;
                delta *= fraction;
                if (Points[j].Fixed)
                {
                    if (!Points[j - 1].Fixed)
                    {
                        Points[j - 1].Position -= delta * 2;
                    }
                }
                else if (Points[j - 1].Fixed)
                {
                    if (!Points[j].Fixed)
                    {
                        Points[j].Position += delta * 2;
                    }
                }
                else
                {
                    Points[j - 1].Position -= delta;
                    Points[j].Position += delta;
                }
            }
            
            holdPoint.LastPosition = holdPoint.Position = HoldPosition;
        }

        //for (int i = 0; i < vertices.Length; i++)
        //{
        //    float dx = (vertices[i].X - vertices[i].LX) * DRAG;
        //    float dy = (vertices[i].Y - vertices[i].LY) * DRAG;
        //    vertices[i].LX = vertices[i].X;
        //    vertices[i].LY = vertices[i].Y;
        //    vertices[i].X += dx;
        //    vertices[i].Y += dy;
        //    vertices[i].Y += GRAV;  
        //}
    }
}