#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(SpaceshipSettings))]
public class SpaceshipCustomEditor : Editor
{
    private void OnEnable()
    {
        SpaceshipSettings spaceshipSettings = (SpaceshipSettings)target;
        spaceshipSettings.m_iSizeList = spaceshipSettings.m_lcSpaceshipRecipeCompoment.Count;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        SpaceshipSettings spaceshipSettings = (SpaceshipSettings)target;

        int sizeX = 200;
        int sizeY = 20;

        GUILayout.BeginVertical();

        GUILayoutOption[] inputFieldOptionsLabel = new GUILayoutOption[2];
        inputFieldOptionsLabel[0] = GUILayout.Width(200);
        inputFieldOptionsLabel[1] = GUILayout.Height(sizeY);

        EditorGUILayout.LabelField("Pieces Component", inputFieldOptionsLabel);
        int sizeList = EditorGUILayout.IntField(spaceshipSettings.m_iSizeList, inputFieldOptionsLabel);

        int diff = sizeList - spaceshipSettings.m_iSizeList;

        if (diff < 0)
            spaceshipSettings.m_lcSpaceshipRecipeCompoment.RemoveRange(sizeList - 1, Mathf.Abs(diff));
        else if (diff > 0)
        {
            for (int idx = 0; idx < Mathf.Abs(diff); idx++)
                spaceshipSettings.m_lcSpaceshipRecipeCompoment.Add(new SpaceshipeRecipeComponent());
        }

        spaceshipSettings.m_iSizeList = sizeList;


        for (int verticalIdx = 0; verticalIdx < spaceshipSettings.m_lcSpaceshipRecipeCompoment.Count; verticalIdx++)
        {
            GUILayout.BeginHorizontal();
            SpaceshipeRecipeComponent spaceshipRecipeComponent = spaceshipSettings.m_lcSpaceshipRecipeCompoment[verticalIdx];

            GUILayoutOption[] inputFieldOptions = new GUILayoutOption[2];
            inputFieldOptions[0] = GUILayout.Width(sizeX);
            inputFieldOptions[1] = GUILayout.Height(sizeY);

            spaceshipRecipeComponent.m_cPieceSettings = EditorGUILayout.ObjectField(spaceshipRecipeComponent.m_cPieceSettings, typeof(PieceSettings), false, inputFieldOptions) as PieceSettings;
            spaceshipRecipeComponent.m_iNumber = EditorGUILayout.IntField(spaceshipRecipeComponent.m_iNumber, inputFieldOptions);

            spaceshipSettings.m_lcSpaceshipRecipeCompoment[verticalIdx] = spaceshipRecipeComponent;


            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
        if (GUI.changed)
        {
            EditorUtility.SetDirty(spaceshipSettings);
            serializedObject.ApplyModifiedProperties();
        }
    }

}

#endif