using UnityEngine;
using UnityEditor;
using System;
using UFE3D;

public class MoveAsset
{
    [MenuItem("Assets/Create/U.F.E./Move File")]
    public static void CreateAsset ()
    {
        ScriptableObjectUtility.CreateAsset<MoveInfo> ();
    }
}
