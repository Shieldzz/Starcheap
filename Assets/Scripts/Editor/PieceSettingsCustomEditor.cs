#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PieceSettings))]
public class PieceSettingsCustomEditor : Editor
{
    private void OnEnable()
    {
        PieceSettings pieceSettings = (PieceSettings)target;
        pieceSettings.m_iSizeList = pieceSettings.m_lcPieceRecipeCompoment.Count;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        PieceSettings pieceSettings = (PieceSettings)target;

        int sizeX = 200;
        int sizeY = 20;

        GUILayout.BeginVertical();

        GUILayoutOption[] inputFieldOptionsLabel = new GUILayoutOption[2];
        inputFieldOptionsLabel[0] = GUILayout.Width(200);
        inputFieldOptionsLabel[1] = GUILayout.Height(sizeY);

        EditorGUILayout.LabelField("ResourceComponent", inputFieldOptionsLabel);
        int sizeList = EditorGUILayout.IntField(pieceSettings.m_iSizeList, inputFieldOptionsLabel);

        int diff = sizeList - pieceSettings.m_iSizeList;

        if (diff < 0)
            pieceSettings.m_lcPieceRecipeCompoment.RemoveRange(sizeList-1, Mathf.Abs(diff));
        else if (diff > 0)
        {
            for (int idx = 0; idx < Mathf.Abs(diff); idx++)
                pieceSettings.m_lcPieceRecipeCompoment.Add(new PieceRecipeComponent());
        }

        pieceSettings.m_iSizeList = sizeList;


        for (int verticalIdx = 0; verticalIdx < pieceSettings.m_lcPieceRecipeCompoment.Count; verticalIdx++)
        {
            GUILayout.BeginHorizontal();
            PieceRecipeComponent pieceRecipeComponent = pieceSettings.m_lcPieceRecipeCompoment  [verticalIdx];

            GUILayoutOption[] inputFieldOptions = new GUILayoutOption[2];
            inputFieldOptions[0] = GUILayout.Width(sizeX);
            inputFieldOptions[1] = GUILayout.Height(sizeY);

            pieceRecipeComponent.m_cResourceSettings = EditorGUILayout.ObjectField  (pieceRecipeComponent.m_cResourceSettings, typeof(ResourceSettings), false,   inputFieldOptions) as ResourceSettings;
            pieceRecipeComponent.m_iNumber = EditorGUILayout.IntField(pieceRecipeComponent.m_iNumber, inputFieldOptions);

            pieceSettings.m_lcPieceRecipeCompoment[verticalIdx] = pieceRecipeComponent;


            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
        if (GUI.changed)
        {
            EditorUtility.SetDirty(pieceSettings);
            serializedObject.ApplyModifiedProperties();
        }   
    }
}

#endif

