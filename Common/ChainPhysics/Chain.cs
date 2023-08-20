using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;

namespace SyntheticEvolution.Common.ChainPhysics;

public class Chain
{
    private ChainPoint[] _points;
    private float[] _segmentLengths;
    private float _normalSegmentLength;
    public Vector2? HoldPosition;
    public Action<Vector2> HoldBack;

    public Vector2 Force = new Vector2(0, 0.4f);
    public float Drag = 0.999f;
    public int Stiffness = 12;

    public ChainPoint FirstPoint => _points[0];
    public ChainPoint LastPoint => _points[^1];

    public float NormalSegmentLength => _normalSegmentLength;

    public static Chain Create(Vector2 start, Vector2 end, float segmentLength)
    {
        var chain = new Chain();
        chain._points = new ChainPoint[(int)MathF.Ceiling(Vector2.Distance(start, end) / segmentLength) + 1];
        chain._normalSegmentLength = segmentLength;
        chain._segmentLengths = new float[chain._points.Length - 1];

        Vector2 normal = Vector2.Normalize(end - start);
        for (int i = 0; i < chain._points.Length; i++)
        {
            chain._points[i] = new ChainPoint();
            chain._points[i].Position = chain._points[i].LastPosition = start + normal * segmentLength * i;
        }

        chain.LastPoint.Position = chain.LastPoint.LastPosition = end;

        for (int i = 0; i < chain._segmentLengths.Length; i++)
        {
            chain._segmentLengths[i] = (chain._points[i + 1].Position - chain._points[i].Position).Length();
        }

        return chain;
    }

    public void DecreaseFromStart(float amount)
    {
        if (_segmentLengths[0] <= amount)
        {
            amount -= _segmentLengths[0];
            
            _points = _points.Skip(1).ToArray();
            _segmentLengths = _segmentLengths.Skip(1).ToArray();
            _segmentLengths[0] -= amount;
        }
        else
        {
            _segmentLengths[0] -= amount;
        }
    }

    public float CalculateLength() => _segmentLengths.Sum();

    public float CalculateVisualLength()
    {
        float sum = 0;
        for (int i = 0; i < _points.Length - 1; i++)
        {
            sum += _points[i].Position.Distance(_points[i + 1].Position);
        }

        return sum;
    }

    public void IncreaseFromStart(float amount)
    {
        if (_normalSegmentLength - _segmentLengths[0] <= amount)
        {
            amount -= _normalSegmentLength - _segmentLengths[0];
            
            _points = _points.Prepend(new ChainPoint()).ToArray();
            _points[0].Position = _points[0].LastPosition = _points[1].Position;
            _segmentLengths[0] = _normalSegmentLength;
            _segmentLengths = _segmentLengths.Prepend(amount).ToArray();
        }
        else
        {
            _segmentLengths[0] += amount;
        }
    }

    public int NumPoints => _points.Length;

    public ChainPoint GetPoint(Index index) => _points[index];

    public int NumSegments => _segmentLengths.Length;

    public float GetSegmentLength(Index index) => _segmentLengths[index];
    public void SetSegmentLength(Index index, float length) => _segmentLengths[index] = length;

    public void UpdatePhysics()
    {
        if (_points.Length == 0) return;

        for (int i = 0; i < _points.Length; i++)
        {
            if (_points[i].Fixed) continue;

            var v = (_points[i].Position - _points[i].LastPosition) * Drag;
            _points[i].LastPosition = _points[i].Position;
            _points[i].Position += v;
            _points[i].Position += Force;
        }

        ChainPoint holdPoint = null;
        if (HoldPosition.HasValue)
        {
            holdPoint = _points[0];
            holdPoint.LastPosition = holdPoint.Position = HoldPosition.Value;
        }

        for (var i = 0; i < Stiffness; i++)
        {
            for (int j = 1; j < _points.Length; j++)
            {
                var delta = _points[j].Position - _points[j - 1].Position;
                var deltaLength = delta.Length();
                var fraction = ((_segmentLengths[j - 1] - deltaLength) / deltaLength) / 2;
                delta *= fraction;
                if (_points[j].Fixed)
                {
                    if (!_points[j - 1].Fixed)
                    {
                        _points[j - 1].Position -= delta * 2;
                    }
                }
                else if (_points[j - 1].Fixed)
                {
                    if (!_points[j].Fixed)
                    {
                        _points[j].Position += delta * 2;
                    }
                }
                else
                {
                    _points[j - 1].Position -= delta;
                    _points[j].Position += delta;
                }
            }

            if (holdPoint != null)
            {
                Vector2 delta = HoldPosition.Value - holdPoint.Position;
                if (HoldBack != null)
                {
                    HoldBack(-delta / 2);
                    holdPoint.LastPosition = holdPoint.Position = HoldPosition.Value + delta / 2;
                }
                else
                {
                    holdPoint.LastPosition = holdPoint.Position = HoldPosition.Value;
                }
            }
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