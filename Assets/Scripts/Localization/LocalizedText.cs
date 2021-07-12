using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class LocalizedText : MonoBehaviour
{
	[SerializeField]
	private string m_sLocalizedKey;

	private Text m_cUnityText = null;
	private TextMeshProUGUI m_cTMPText = null;

	private string m_cText = "";
	public string Text
	{
		get { return m_cText; }
	}


	#region MonoBehavior
	private void Start()
	{
		m_cUnityText = GetComponent<Text>();

		if (m_cUnityText == null)
			m_cTMPText = GetComponent<TextMeshProUGUI>();

		UpdateText();

		LocalizationManager.Instance.OnLocalizationChanged += UpdateText;
	}

	private void OnDestroy()
	{
		LocalizationManager localizationManager = LocalizationManager.Instance;

		if (localizationManager != null)
			localizationManager.OnLocalizationChanged -= UpdateText;
	}
	#endregion

	public void UpdateTextWithKey(string localizedKey)
	{
		m_sLocalizedKey = localizedKey;

		UpdateText();
	}

	private void UpdateText()
	{
		m_cText = LocalizationManager.Instance.GetLocalizedTextFromKey(m_sLocalizedKey);

		if (m_cUnityText != null)
			m_cUnityText.text = m_cText;
		else if (m_cTMPText != null)
			m_cTMPText.text = m_cText;
	}
}
