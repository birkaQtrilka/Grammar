using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ClusterDataContainer))]
public class ClusterDataContainerDrawer : PropertyDrawer
{
    private static Dictionary<int, bool> foldouts = new ();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ClusterDataContainer target = GetInstance<ClusterDataContainer>(fieldInfo, property);

        SerializedProperty array = property.FindPropertyRelative("_clusters");
        if (array == null)
        {
            EditorGUILayout.LabelField("No cluster formed yet");
            return;
        }

        if (GUILayout.Button("Deselect"))
        {
            GizmosDrawer.Instance.PersistentCall = null;

        }
        int testCount = Mathf.Min(5, array.arraySize);
        for (int i = 0; i < testCount; ++i)
        {
            SerializedProperty arrayElement = array.GetArrayElementAtIndex(i);
            if(GUILayout.Button("Select"))
            {
                Cluster cluster = BuildSpace.GetCluster(arrayElement.FindPropertyRelative("id").intValue);
                GizmosDrawer.Instance.PersistentCall = () => cluster.DrawCells(0);
                SceneView.lastActiveSceneView.LookAt(cluster.GetDrawnCenter() + Vector3.up * 5);
            }
            var data = arrayElement.FindPropertyRelative("Data");
            EditorGUILayout.PropertyField(data);
            // Store foldout state per-object
            
            var obj = data.objectReferenceValue;
            if (obj == null) continue;
            int foldoutKey = i;

            if (!foldouts.ContainsKey(foldoutKey))
                foldouts.Add(foldoutKey, false);

            bool foldout =  EditorGUILayout.Foldout(foldouts[foldoutKey], "Show Data Contents", true);
            foldouts[foldoutKey] =foldout;
            if (!foldout) continue;
            EditorGUI.indentLevel++;

            // Create a serialized object from the ScriptableObject
            SerializedObject dataSO = new (data.objectReferenceValue);
            SerializedProperty prop = dataSO.GetIterator();

            // Needed to start iterating
            prop.NextVisible(true);

            while (prop.NextVisible(false))
            {
                EditorGUILayout.PropertyField(prop, true);
            }

            dataSO.ApplyModifiedProperties();

            EditorGUI.indentLevel--;
            
        }
    }

    public static T GetInstance<T>(FieldInfo fieldInfo, SerializedProperty property) where T : class
    {
        var obj = fieldInfo.GetValue(property.serializedObject.targetObject);
        if (obj == null) { return null; }

        T actualObject = null;
        if (obj.GetType().IsArray)
        {
            var index = Convert.ToInt32(new string(property.propertyPath.Where(c => char.IsDigit(c)).ToArray()));
            actualObject = ((T[])obj)[index];
        }
        else
        {
            actualObject = obj as T;
        }
        return actualObject;
    }
}
