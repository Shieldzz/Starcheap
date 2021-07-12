using System.Linq;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(LevelSettings))]
public class LevelSettingCustomEditor : Editor
{
	SerializedProperty horizontalCells;
	SerializedProperty verticalCells;

	private void OnEnable()
	{
		horizontalCells = serializedObject.FindProperty("m_iHorizontalCells");
		verticalCells = serializedObject.FindProperty("m_iVerticalCells");
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		serializedObject.Update();

		LevelSettings levelSettings = (LevelSettings)target;

		int horizontalCellsSize = horizontalCells.intValue;
		int verticalCellsSize = verticalCells.intValue;

		int gridSize = horizontalCellsSize * verticalCellsSize;
		
		if (gridSize != levelSettings.m_liForceGrid.Count)
		{
			if (gridSize < levelSettings.m_liForceGrid.Count)
				levelSettings.m_liForceGrid.RemoveRange(gridSize, levelSettings.m_liForceGrid.Count - gridSize);
			else
			{
				levelSettings.m_liForceGrid.Capacity = gridSize;
				levelSettings.m_liForceGrid.AddRange(Enumerable.Repeat(0, gridSize - levelSettings.m_liForceGrid.Count));
			}
		}

		int sizeX = 20;
		int sizeY = 20;

		GUILayout.BeginHorizontal();
		for (int horizontalIdx = 0; horizontalIdx < horizontalCells.intValue; ++horizontalIdx)
		{
			GUILayout.BeginVertical();
			for (int verticalIdx = 0; verticalIdx < verticalCells.intValue; ++verticalIdx)
			{
				int currIdx = verticalIdx + horizontalIdx * horizontalCells.intValue;

				GUILayoutOption[] inputFieldOptions = new GUILayoutOption[2];
				inputFieldOptions[0] = GUILayout.Width(sizeX);
				inputFieldOptions[1] = GUILayout.Height(sizeY);
				levelSettings.m_liForceGrid[currIdx] = EditorGUILayout.IntField(levelSettings.m_liForceGrid[currIdx], inputFieldOptions);
			}
			GUILayout.EndVertical();
		}
		GUILayout.EndHorizontal();
		
		serializedObject.ApplyModifiedProperties();
	}
}
