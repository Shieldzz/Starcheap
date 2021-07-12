using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using FMODUnity;


public class OptionsMenu : MonoBehaviour
{
	[System.Serializable]
	struct Language
	{
		public string m_sLocalizationFileName;
		public string m_sLanguageName;
	}

	[SerializeField]
	private GameObject m_cLanguageButton = null;

	[SerializeField]
	private List<Language> m_lcLanguages = new List<Language>();

	[SerializeField]
	private Text m_cLanguageText = null;

	[Header("Sounds")]
	[SerializeField]
	private StudioEventEmitter m_cSwitchSound = null;

	private int m_iLanguagesSize = 0;
	private int m_iLanguageIndex = 0;

	private bool m_bIsInMainMenu = false;


	#region MonoBehavior
	private void Awake()
	{
		m_iLanguagesSize = m_lcLanguages.Count;

		m_bIsInMainMenu = MainMenu.Instance != null;
	}

	private void OnBecameVisible()
	{
		string localization = LocalizationManager.Instance.CurrentLocalization;

		m_iLanguageIndex = m_lcLanguages.FindIndex((Language language) => { return (language.m_sLocalizationFileName == localization); });

		UpdateLanguageText();
	}

	private void OnDestroy()
	{
		if (m_bIsInMainMenu)
		{
			Controller[] controllers = MainMenu.Instance.Controllers;
			foreach (Controller currentController in controllers)
				currentController.OnLeftStick -= ManageControllerStick;
		}
		else
		{
			Controller[] controllers = GameManager.Instance.Controllers;
			foreach (Controller currentController in controllers)
				currentController.OnLeftStick -= ManageControllerStick;
		}
	}
	#endregion

	public void SetActive(bool active)
	{
		gameObject.SetActive(active);

		if (active)
		{
			Controller[] controllers = (m_bIsInMainMenu) ? MainMenu.Instance.Controllers : GameManager.Instance.Controllers;
			foreach (Controller currentController in controllers)
				currentController.OnLeftStick += ManageControllerStick;

			EventSystem.current.SetSelectedGameObject(m_cLanguageButton);
		}
		else
		{
			Controller[] controllers = (m_bIsInMainMenu) ? MainMenu.Instance.Controllers : GameManager.Instance.Controllers;
			foreach (Controller currentController in controllers)
				currentController.OnLeftStick -= ManageControllerStick;

			EventSystem.current.SetSelectedGameObject(null);
		}
	}

	public void ChangeLanguage()
	{
		LocalizationManager.Instance.LoadLocalizationFile(m_lcLanguages[m_iLanguageIndex].m_sLocalizationFileName);
	}

	private void UpdateLanguageText()
	{
		m_cLanguageText.text = m_lcLanguages[m_iLanguageIndex].m_sLanguageName;

		m_cSwitchSound.Play();
	}

	private void ManageControllerStick(Vector2 direction)
	{
	    if (direction.x > 0f)
	        NextLanguage();
	    else if (direction.x < 0f)
            PreviousLanguage();
	}

	private void NextLanguage()
	{
		m_iLanguageIndex = (m_iLanguageIndex + 1) % m_iLanguagesSize;

		UpdateLanguageText();
	}

	private void PreviousLanguage()
	{
		m_iLanguageIndex = (m_iLanguageIndex + m_iLanguagesSize - 1) % m_iLanguagesSize;

		UpdateLanguageText();
	}
}
