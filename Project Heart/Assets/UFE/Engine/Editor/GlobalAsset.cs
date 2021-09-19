using UnityEngine;
using UnityEditor;
using System;
using UFE3D;

public class GlobalAsset
{
	[MenuItem("Assets/Create/U.F.E./Config File")]
    public static void CreateAsset ()
    {
        ScriptableObjectUtility.CreateAsset<GlobalInfo> ();
    }
}
