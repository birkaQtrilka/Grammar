using Demo;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class SimpleStockSelectorTool
{
    static SimpleStockSelectorTool()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    static void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;

        if (!(e.type == EventType.MouseDown && e.button == 0 && !e.alt)) return;

        GameObject picked = HandleUtility.PickGameObject(e.mousePosition, false);

        if (picked == null) return;
        
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
