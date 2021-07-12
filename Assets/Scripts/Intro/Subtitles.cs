using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class Subtitles : MonoBehaviour
{
	[System.Serializable]
	public struct IntroText
	{
		public string m_sLanguageID;
		public string[] m_asSentences;
		public float[] m_afSentenceTimes;
	}

	[SerializeField]
	private TextMeshProUGUI m_cSubtitleText = null;

	[SerializeField]
	private IntroText[] m_acIntroText;

	private IntroText m_cCurrentIntroText;

	private int m_iSentenceIndex = 0;

	private float m_fSentenceTimer = 0f;

	private bool m_bStop = false;


	#region MonoBehavior
	private void Awake()
	{
		string languageID = LocalizationManager.Instance.CurrentLocalization;

		foreach (IntroText introText in m_acIntroText)
		{
			if (introText.m_sLanguageID == languageID)
			{
				m_cCurrentIntroText = introText;
				break;
			}
		}
	}

	private void Update()
	{
		if (!m_bStop)
		{
			if (m_cCurrentIntroText.m_afSentenceTimes[m_iSentenceIndex] <= m_fSentenceTimer)
			{
				m_cSubtitleText.text = m_cCurrentIntroText.m_asSentences[m_iSentenceIndex];

				++m_iSentenceIndex;

				if (m_iSentenceIndex == m_cCurrentIntroText.m_asSentences.Length)
					m_bStop = true;
			}

			m_fSentenceTimer += Time.deltaTime;
		}
	}
	#endregion
}
