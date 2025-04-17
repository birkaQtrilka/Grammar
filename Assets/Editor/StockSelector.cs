using Demo;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class StockSelector
{

    static StockSelector()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }
    
    static void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.P)
        {
            Debug.Log("esc");
            GizmosDrawer.Instance.PersistentCall = null;
            return;
        }
        if (!(e.type == EventType.MouseDown && e.button == 0 && !e.alt)) return;

        GameObject picked = HandleUtility.PickGameObject(e.mousePosition, false);
        if (picked == null) return;
        //selecting tile
        if (picked.layer == LayerMask.NameToLayer("Tile"))
        {
            BuildSpace clusterPiece = picked.GetComponentInChildren<BuildSpace>(true);
            if (clusterPiece == null || !BuildSpace.TryGetCluster(clusterPiece.ClusterID, out Cluster cluster)) return;
            GizmosDrawer.Instance.PersistentCall = () => cluster.DrawCells(0);
            SceneView.lastActiveSceneView.LookAt(cluster.GetDrawnCenter() + Vector3.up * 5);
            Debug.Log("clusterID " + clusterPiece.ClusterID);
            return;
        }

        GameObject topMost = FindTopmostSimpleStock(picked);

        if (topMost != null)
        {
            Selection.activeGameObject = topMost;
            Debug.Log("Selected topmost SimpleStock: " + topMost.name);
            e.Use(); // consume the event
        }
        
    }

    static GameObject FindTopmostSimpleStock(GameObject start)
    {
        Transform current = start.transform;
        GameObject topMostWithStock = null;
        
        while (current != null)
        {
            if (current.TryGetComponent<SimpleStock>(out _))
                topMostWithStock = current.gameObject;

            current = current.parent;
        }

        return topMostWithStock;
    }
}
