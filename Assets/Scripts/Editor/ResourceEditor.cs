# if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class ResourceEditor : Editor {

    static private string m_sResourceSettingPath = "Assets/Pieces&ResourceObjects/Resources/";

    [MenuItem("Generate Piece and ResourceObjects/Create Resource Settings")]
    static private void NewPieceSettings()
    {
        ResourceSettings asset = CreateInstance<ResourceSettings>();

        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(m_sResourceSettingPath + "/New " + typeof(ResourceEditor).ToString() + ".asset");

        AssetDatabase.CreateAsset(asset, assetPathAndName);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }
}

#endif