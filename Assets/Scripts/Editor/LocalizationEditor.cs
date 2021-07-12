#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;


public class LocalizationEditor : EditorWindow
{
	[SerializeField]
	private LocalizationData m_cLocalizationData;


	[MenuItem("Window/Localization Editor")]
	static private void Init()
	{
		GetWindow(typeof(LocalizationEditor)).Show();
	}

	#region MonoBehavior
	private void OnGUI()
	{
		if (m_cLocalizationData != null)
		{
			SerializedObject serializedObject = new SerializedObject(this);
			SerializedProperty serializedProperty = serializedObject.FindProperty("m_cLocalizationData");
			EditorGUILayout.PropertyField(serializedProperty, true);
			serializedObject.ApplyModifiedProperties();

			if (GUILayout.Button("Save data"))
				SaveGameData();
		}

		if (GUILayout.Button("Load data"))
			LoadGameData();

		if (GUILayout.Button("Create new data"))
			CreateNewData();
	}
	#endregion

	private void LoadGameData()
	{
		string filePath = EditorUtility.OpenFilePanel("Select localization data file", Application.streamingAssetsPath, "json");

		if (!string.IsNullOrEmpty(filePath))
		{
			string dataAsJson = File.ReadAllText(filePath);

			m_cLocalizationData = JsonUtility.FromJson<LocalizationData>(dataAsJson);
		}
	}

	private void SaveGameData()
	{
		string filePath = EditorUtility.SaveFilePanel("Save localization data file", Application.streamingAssetsPath, "", "json");

		if (!string.IsNullOrEmpty(filePath))
		{
			string dataAsJson = JsonUtility.ToJson(m_cLocalizationData);
			File.WriteAllText(filePath, dataAsJson);
		}
	}

	private void CreateNewData()
	{
		m_cLocalizationData = new LocalizationData();
	}

}
#endif
