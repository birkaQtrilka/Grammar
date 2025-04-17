
using UnityEngine;
[ExecuteInEditMode]
public class MultiplyTest : MonoBehaviour
{
    [SerializeField] Vector3 pos;
    [SerializeField] float scale = 3;
    [SerializeField] float cellSize = .75f;
    [SerializeField] float opposite = .75f;
    
    void Update()
    {
        pos = transform.position * (scale / cellSize);
        opposite = 1 / cellSize;
    }

    private void OnDrawGizmos()
    {
        Draw(GetGridPosition(transform));
    }
    Vector2Int GetGridPosition(Transform target)
    {
        Vector3 worldPos = target.position;

        return new Vector2Int(
            Mathf.FloorToInt(worldPos.x * (3 / cellSize)),
            Mathf.FloorToInt(worldPos.z * (3 / cellSize))
        );
    }
    readonly Vector3[] drawArr = new Vector3[8];

    void Draw(Vector2Int cell)
    {
        float x = cell.x;
        float y = cell.y;
        float x1 = cell.x+1;
        float y1 = cell.y+1;
        var up = transform.position.y;
        var corner_TL = (.3333f * cellSize) * new Vector3(x,  up , y);
        var corner_TR = (.3333f * cellSize) * new Vector3(x1, up , y);
        var corner_BR = (.3333f * cellSize) * new Vector3(x1, up , y1);
        var corner_BL = (.3333f * cellSize) * new Vector3(x,  up , y1);
        drawArr[0] = corner_TL;
        drawArr[1] = corner_TR;

        drawArr[2] = corner_TR;
        drawArr[3] = corner_BR;

        drawArr[4] = corner_BR;
        drawArr[5] = corner_BL;

        drawArr[6] = corner_BL;
        drawArr[7] = corner_TL;

        Gizmos.color = Color.red;
        Gizmos.DrawLineList(drawArr);
    }
}
