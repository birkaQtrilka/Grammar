using System;
using UnityEngine;

[Serializable]
public struct BuildCell
{
    public Vector2Int Position;
    public bool Taken;
    public BuildCell(Vector2Int position)
    {
        Position = position;
        Taken = false;
    }

    public void MarkTaken() { Taken = true; }
}
