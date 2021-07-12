using System.Collections.Generic;
using UnityEngine;
using System.IO;


public class LocalizationManager : MonoBehaviour
{
	private static LocalizationManager m_cInstance = null;
	public static LocalizationManager Instance
	{
		get { return m_cInstance; }
	}

	[SerializeField]
	private string m_sLocalizationFolder = "";

	private Dictionary<string, string> m_dssLocalizedText = new Dictionary<string, string>();

	private string m_sCurrentLocalization = "";
	public string CurrentLocalization
	{
		get { return m_sCurrentLocalization; }
	}

	public event System.Action OnLocalizationChanged;


	#region MonoBehavior
	private void Awake()
	{
		if (m_cInstance == null)
			m_cInstance = this;
		else
		{
			Destroy(gameObject);
			return;
		}

		DontDestroyOnLoad(gameObject);

		m_sLocalizationFolder = Path.Combine(Application.streamingAssetsPath, m_sLocalizationFolder);
	}

	private void OnDestroy()
	{
		OnLocalizationChanged = null;
	}
	#endregion

	public void LoadLocalizationFile(string fileName)
	{
		m_dssLocalizedText = new Dictionary<string, string>();
		string filePath = Path.Combine(m_sLocalizationFolder, fileName + ".json");

		if (File.Exists(filePath))
		{
			string dataAsJson = File.ReadAllText(filePath);
			LocalizationData loadedData = JsonUtility.FromJson<LocalizationData>(dataAsJson);

			int dataSize = loadedData.m_acLocalizationItems.Length;
			for (int idx = 0; idx < dataSize; idx++)
			{
				LocalizationItem currentItem = loadedData.m_acLocalizationItems[idx];
				m_dssLocalizedText.Add(currentItem.m_sKey, currentItem.m_sValue);
			}

			m_sCurrentLocalization = fileName;

			if (OnLocalizationChanged != null)
				OnLocalizationChanged();
		}
	}

	public string GetLocalizedTextFromKey(string key)
	{
		string result = "";

		if (m_dssLocalizedText.ContainsKey(key))
			result = m_dssLocalizedText[key];

		return result;
	}
}
