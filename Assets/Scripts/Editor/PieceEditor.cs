#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class PieceEditor : Editor {

    static private string m_sPieceSettingPath = "Assets/Pieces&ResourceObjects/Pieces/";

    [MenuItem("Generate Piece and ResourceObjects/Create Piece Settings")]
    static private void NewPieceSettings()
    {
        PieceSettings asset = CreateInstance<PieceSettings>();

        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(m_sPieceSettingPath + "/New " + typeof(PieceEditor).ToString() + ".asset");

        AssetDatabase.CreateAsset(asset, assetPathAndName);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }
}
#endif
