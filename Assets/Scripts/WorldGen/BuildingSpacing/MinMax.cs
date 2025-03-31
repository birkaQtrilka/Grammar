using System;
using UnityEngine;

[Serializable]
public struct MinMax
{
    public int MinX;
    public int MinY;
    public int MaxX;
    public int MaxY;

    public MinMax(bool cheat,
        int minX = int.MaxValue,
        int minY = int.MaxValue,
        int maxX = int.MinValue,
        int maxY = int.MinValue)
    {
        MinX = minX;
        MinY = minY;
        MaxX = maxX;
        MaxY = maxY;
    }

    public readonly int Width => MaxX - MinX;
    public readonly int Height => MaxY - MinY;

    public override readonly string ToString()
    {
        return $"min: {MinX}, {MinY}\nmax:{MaxX}, {MaxY}\nW: {MaxX - MinX}, H: {MaxY - MinY}";

    }

    public void UpdateMinMax(Vector2Int gridPos)
    {
        if (gridPos.x < MinX) MinX = gridPos.x;
        if (gridPos.x > MaxX) MaxX = gridPos.x;

        if (gridPos.y < MinY) MinY = gridPos.y;
        if (gridPos.y > MaxY) MaxY = gridPos.y;
    }

    public void UpdateMinMax(MinMax minMax)
    {
        if (minMax.MinX < MinX) MinX = minMax.MinX;
        if (minMax.MaxX > MaxX) MaxX = minMax.MaxX;

        if (minMax.MinY < MinY) MinY = minMax.MinY;
        if (minMax.MaxY > MaxY) MaxY = minMax.MaxY;
    }
}
