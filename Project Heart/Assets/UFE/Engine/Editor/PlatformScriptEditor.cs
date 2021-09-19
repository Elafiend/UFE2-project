using UnityEngine;
using UnityEditor;
using FPLibrary;

[CanEditMultipleObjects]
[CustomEditor(typeof(PlatformScript))]
public class ObjectBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        PlatformScript platform = (PlatformScript)target;

        Rect iRect = platform.rect.ToRect();
        iRect = EditorGUILayout.RectField("Rectangle:", iRect);
        platform.rect = new FPRect(iRect);

        if (GUILayout.Button("Use Mesh Bounds"))
        {
            foreach(PlatformScript platformScript in targets)
            {
                platformScript.RefreshData();
            }
        }
    }
}
