#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class SpaceshipEditor : Editor
{
    static private string m_sSpaceshipSettingPath = "Assets/Pieces&ResourceObjects/Spaceship/";

    [MenuItem("Generate Piece and ResourceObjects/Create Spaceship Settings")]
    static private void NewPieceSettings()
    {
        SpaceshipSettings asset = CreateInstance<SpaceshipSettings>();

        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(m_sSpaceshipSettingPath + "/New " + typeof(SpaceshipSettings).ToString() + ".asset");

        AssetDatabase.CreateAsset(asset, assetPathAndName);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }
}

#endif
