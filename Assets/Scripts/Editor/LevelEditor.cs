#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;


public class LevelEditor : Editor
{
	static private string m_sLevelsSettingPath = "Assets/Levels/";


	[MenuItem("Level Editor/New Level Settings")]
	static private void NewLevelSettings()
	{
		LevelSettings asset = CreateInstance<LevelSettings>();

		string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(m_sLevelsSettingPath + "/New " + typeof(LevelSettings).ToString() + ".asset");

		AssetDatabase.CreateAsset(asset, assetPathAndName);

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		EditorUtility.FocusProjectWindow();
		Selection.activeObject = asset;
	}

	[MenuItem("Level Editor/Create Level")]
	static private void CreateLevel()
	{
		string settingFilePath = EditorUtility.OpenFilePanel("Choose level setting file", "Assets/", "asset");

		if (settingFilePath != "")
		{
			GameObject platform = GameObject.FindGameObjectWithTag("Platform");

			if (platform != null)
			{
				settingFilePath = settingFilePath.Substring(settingFilePath.IndexOf("/Assets") + 1);
				LevelSettings levelSetting = AssetDatabase.LoadAssetAtPath<LevelSettings>(settingFilePath);

				if (levelSetting != null)
				{
					DestroyPreviousCells(platform);

					GameObject cellsParent = new GameObject();
					cellsParent.transform.parent = platform.transform;
					cellsParent.tag = "CellsParent";
					cellsParent.name = "Cells";

					for (int verticalIdx = 0; verticalIdx < levelSetting.m_iVerticalCells; ++verticalIdx)
					{
						for (int horizontalIdx = 0; horizontalIdx < levelSetting.m_iHorizontalCells; ++horizontalIdx)
						{
							int currIndex = horizontalIdx + verticalIdx * levelSetting.m_iVerticalCells;
							int cellImportance = levelSetting.m_liForceGrid[currIndex];

							if (cellImportance != -1)
							{
								GameObject cellObj = Instantiate(levelSetting.m_cCellPrefab, cellsParent.transform);
								cellObj.layer = LayerMask.NameToLayer("Cell");

								Cell newCell = cellObj.GetComponent<Cell>();
								newCell.m_iImportance = cellImportance;
								newCell.name = currIndex.ToString();

								Vector3 cellSize = newCell.transform.localScale;
								cellSize.x *= levelSetting.m_vOvalSize.x;
								cellSize.z *= levelSetting.m_vOvalSize.y;
								cellSize.x /= levelSetting.m_iVerticalCells;
								cellSize.z /= levelSetting.m_iHorizontalCells;

								newCell.transform.localScale = cellSize;

								float x = cellSize.x * (verticalIdx + 1) - (cellSize.x / 2f);
								float z = -(cellSize.z * (horizontalIdx + 1) - (cellSize.z / 2f));

								x -= levelSetting.m_vPivotPoint.x;
								z -= levelSetting.m_vPivotPoint.y;

								newCell.transform.localPosition = new Vector3(x, 0f, z);
							}
						}
					}
				}
				else
					Debug.LogError("Could not load level settings file.");
			}
			else
				Debug.LogError("This scene does not contain an object with tag Platform.");
		}
	}

	[MenuItem("Level Editor/Toggle Debug")]
	static private void ToggleDebug()
	{
		GameObject platform = GameObject.FindGameObjectWithTag("Platform");

		if (platform != null)
		{
			Cell[] cells = platform.GetComponentsInChildren<Cell>();

			foreach (Cell cell in cells)
				cell.m_bShowDebug = !cell.m_bShowDebug;
		}
		else
			Debug.LogError("This scene does not contain an object with tag Platform.");
	}

	static private void DestroyPreviousCells(GameObject platform)
	{
		GameObject cellsParent = GameObject.FindGameObjectWithTag("CellsParent");
		if (cellsParent != null)
			DestroyImmediate(cellsParent);

		Cell[] previousCells = platform.GetComponentsInChildren<Cell>();

		foreach (Cell cell in previousCells)
			DestroyImmediate(cell.gameObject);
	}
}
#endif
