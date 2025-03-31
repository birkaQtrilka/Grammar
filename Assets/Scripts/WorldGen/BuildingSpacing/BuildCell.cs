using UnityEngine;

public struct BuildCell
{
    public Vector2Int Position;
    public bool Taken;
    public BuildCell(Vector2Int position)
    {
        Position = position;
        Taken = false;
    }
    public BuildCell(Vector2Int position, bool taken)
    {
        Position = position;
        Taken = taken;
    }
    public void MarkTaken() 
    { 
        Taken = true; 
    }
}
