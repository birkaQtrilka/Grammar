using System;
using UnityEngine;

[Serializable]
public struct BuildCell
{
    public Vector2Int Position;
    public BuildCell(Vector2Int position)
    {
        Position = position;
    }
}
