using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapConstructor), true)]
public class MapConstructorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapConstructor _target = (MapConstructor)target;
        
        serializedObject.Update();
        
        DrawDefaultInspector();
        
        GUI.backgroundColor = new Color(0.85f, 1f, 1f, 1);
        if (GUILayout.Button("Generate map"))
        {
            _target.GenerateNewMap();
        }
    }
}