#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class GenerateID : Editor
{
    static private string m_sResourcesSettingsPath = "Assets/Pieces&ResourceObjects/Resources";
    static private string m_sPiecesSettingsPath = "Assets/Pieces&ResourceObjects/Pieces";

    [MenuItem("Generate Piece and ResourceObjects/Generarte ID")]
    static private void GenerateNewID()
    {
        string[] GUIDSResources = AssetDatabase.FindAssets("t:ResourceSettings", new[] { m_sResourcesSettingsPath });

        string[] GUIDSPieces = AssetDatabase.FindAssets("t:PieceSettings", new[] { m_sPiecesSettingsPath });

        for (int idxResource = 0; idxResource < GUIDSResources.Length; idxResource++)
        {
            string assetPathResource = AssetDatabase.GUIDToAssetPath(GUIDSResources[idxResource]);
            ResourceSettings resourcesSetting = AssetDatabase.LoadAssetAtPath<ResourceSettings>(assetPathResource);

            resourcesSetting.m_iIndex = idxResource;
        }

        for (int idxPiece = 0; idxPiece < GUIDSPieces.Length; idxPiece++)
        {
            string assetPathPiece = AssetDatabase.GUIDToAssetPath(GUIDSPieces[idxPiece]);
            PieceSettings piecesSetting = AssetDatabase.LoadAssetAtPath<PieceSettings>(assetPathPiece);

            piecesSetting.m_iIndex = idxPiece;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

}

#endif
