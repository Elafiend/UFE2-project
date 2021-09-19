using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UFE3D;

[CustomEditor(typeof(AIInfo))]
public class AIEditor : Editor {
	public override void OnInspectorGUI(){
		if (GUILayout.Button("Open A.I. Editor")) 
			AIEditorWindow.Init();
			
	}
}
