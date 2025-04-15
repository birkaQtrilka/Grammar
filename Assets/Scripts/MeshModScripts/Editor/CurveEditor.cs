// Version 2023
//  (Update: student labture version, with TODO's)

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Curve))]
public class CurveEditor : Editor
{
	private Curve curve;

    private void OnEnable()
    {
		curve = (Curve)target;
    }

    public override void OnInspectorGUI()
    {
		if(GUILayout.Button("Apply to mesh"))
		{
			curve.Apply();
		}
        base.OnInspectorGUI();
    }

    // This method is called by Unity whenever it renders the scene view.
    // We use it to draw gizmos, and deal with changes (dragging objects)
    void OnSceneGUI() {
		if (curve.points==null)
			return;

		bool dirty = false;

		// Add new points if needed:
		Event e = Event.current;
		if ((e.type==EventType.KeyDown && e.keyCode == KeyCode.Space)) {
			Debug.Log("Space pressed - trying to add point to curve");
			dirty |= AddPoint();
		}

		dirty |= ShowAndMovePoints();

		if (dirty) {
			curve.Apply();
		}
 	}

	// Tries to add a point to the curve, where the mouse is in the scene view.
	// Returns true if a change was made.
	bool AddPoint() {
		bool dirty = false;
		Transform handleTransform = curve.transform;

		Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

		if (Physics.Raycast(ray, out RaycastHit hit))
		{
			Debug.Log("Adding spline point at mouse position: " + hit.point);
			Undo.RecordObject(curve, "Add Spline Point");
			curve.points.Add(handleTransform.InverseTransformPoint(hit.point));
            EditorUtility.SetDirty(curve);
            dirty = true;
		}
		else if (_lastModifiedPointIndex != -1 && _lastModifiedPointIndex < curve.points.Count)
		{
            Debug.Log("Adding spline point at on last modified point: " + _lastModifiedPointIndex);
            Undo.RecordObject(curve, "Add Spline Point");
			curve.points.Insert(_lastModifiedPointIndex, curve.points[_lastModifiedPointIndex]);
            EditorUtility.SetDirty(curve);
            dirty = true;
        }

        return dirty;
	}


	int _lastModifiedPointIndex = -1;
	// Show points in scene view, and check if they're changed:
	bool ShowAndMovePoints() {
		bool dirty = false;
		Transform handleTransform = curve.transform;

		Vector3 previousPoint = Vector3.zero;
		for (int i = 0; i < curve.points.Count; i++) {
			Vector3 currentPoint = handleTransform.TransformPoint(curve.points[i]);

            if (i > 0)
            {
                Handles.color = Color.white;
                Handles.DrawLine(previousPoint, currentPoint);
            }

            previousPoint =currentPoint;

			EditorGUI.BeginChangeCheck();
			currentPoint = Handles.DoPositionHandle(currentPoint, Quaternion.identity);
			if (EditorGUI.EndChangeCheck()) {
				Undo.RecordObject(curve, "moved");
				curve.points[i] = handleTransform.InverseTransformPoint(currentPoint);
                EditorUtility.SetDirty(curve);
                _lastModifiedPointIndex = i;
				dirty = true;
			}

		}
		return dirty;
	}
}
