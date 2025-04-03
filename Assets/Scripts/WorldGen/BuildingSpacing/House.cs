using System.Drawing;

public class House
{
    public Rectangle Rect;
    public UnityEngine.Color DebugColor;

    public House(Rectangle area)
    {
        Rect = area;
        DebugColor = UnityEngine.Random.ColorHSV(0,1,.9f,1,1,1);
    }
}
