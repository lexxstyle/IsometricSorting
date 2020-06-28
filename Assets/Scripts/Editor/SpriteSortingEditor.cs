#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpriteSorting))]
public class SpriteSortingEditor : Editor
{
    public void OnSceneGUI()
    {
        SpriteSorting myTarget = (SpriteSorting)target;

        myTarget.SorterPositionOffset = Handles.FreeMoveHandle(myTarget.transform.position + myTarget.SorterPositionOffset, Quaternion.identity, 0.1f * HandleUtility.GetHandleSize(myTarget.transform.position), Vector3.zero, Handles.DotCap) - myTarget.transform.position;
        if (myTarget.sortType == SpriteSorting.SortType.Line)
        {
            myTarget.SorterPositionOffset2 = Handles.FreeMoveHandle(myTarget.transform.position + myTarget.SorterPositionOffset2, Quaternion.identity, 0.08f * HandleUtility.GetHandleSize(myTarget.transform.position), Vector3.zero, Handles.DotCap) - myTarget.transform.position;
            Handles.DrawLine(myTarget.transform.position + myTarget.SorterPositionOffset, myTarget.transform.position + myTarget.SorterPositionOffset2);

        }
        if (GUI.changed)
        {
            Undo.RecordObject(target, "Updated Sorting Offset");
            EditorUtility.SetDirty(target);
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        SpriteSorting myScript = (SpriteSorting)target;
        if (GUILayout.Button("Sort Visibles"))
        {
            myScript.SortScene();
        }
    }
}
#endif
