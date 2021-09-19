using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;
using UFE3D;

public static class ScriptableObjectUtility
{
    public static void CreateAsset<T> (T data = null, T oldFile = null) where T : ScriptableObject
    {
        T asset = ScriptableObject.CreateInstance<T> ();
        Object referencePath = Selection.activeObject;
        if (data != null) {
            asset = data;
            if (oldFile != null) referencePath = oldFile;
        }
        
        string path = AssetDatabase.GetAssetPath (referencePath);
        if (path == "") {
            path = "Assets";
        } else if (Path.GetExtension (path) != "") {
            path = path.Replace (Path.GetFileName (AssetDatabase.GetAssetPath (referencePath)), "");
        }

        string fileName;
        if (oldFile != null) {
            fileName = oldFile.name;
		}else if (asset is MoveInfo) {
			fileName = "New Move";
		}else if (asset is UFE3D.CharacterInfo) {
			fileName = "New Character";
		}else if (asset.GetType().ToString().Equals("UFE3D.AIInfo")) {
			fileName = "New AI Instructions";
		}else if (asset is CustomHitBoxesInfo) {
			fileName = "New Hitbox Map";
		}else if (asset is GlobalInfo) {
			fileName = "New UFE Config";
		}else if (asset is StanceInfo) {
            fileName = "New Combat Stance";
		}else{
			fileName = typeof(T).ToString();
		}
        string assetPathAndName = oldFile != null? path + fileName + ".asset" : AssetDatabase.GenerateUniqueAssetPath (path + "/" + fileName + ".asset");
        
        if (!AssetDatabase.Contains(asset)) AssetDatabase.CreateAsset (asset, assetPathAndName);
        
        AssetDatabase.SaveAssets ();
        EditorUtility.FocusProjectWindow ();
        Selection.activeObject = asset;
		
		if (asset is MoveInfo) {
			MoveEditorWindow.Init();
		}else if (asset is GlobalInfo) {
			GlobalEditorWindow.Init();
		}else if (asset.GetType().ToString().Equals("UFE3D.AIInfo")){
			UFE.SearchClass("AIEditorWindow").GetMethod(
				"Init", 
				BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy,
				null,
				null,
				null
			).Invoke(null, new object[]{});
		}else if (asset is UFE3D.CharacterInfo) {
			CharacterEditorWindow.Init();
		}
		
    }
}