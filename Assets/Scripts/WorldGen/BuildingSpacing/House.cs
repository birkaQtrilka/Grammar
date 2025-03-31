using System.Drawing;

public class House
{
    public Rectangle Rect;
    //public List<BuildCell> ClusterCells { get; private set; } = new();
    public UnityEngine.Color DebugColor;

    public House(Rectangle area)
    {
        Rect = area;
        //MinMax = new(false, int.MaxValue, int.MaxValue, int.MinValue, int.MinValue);
        DebugColor = UnityEngine.Random.ColorHSV(0,1,.9f,1,1,1);
    }
}
