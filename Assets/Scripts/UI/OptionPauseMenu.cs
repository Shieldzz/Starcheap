using UnityEngine;
using UnityEngine.EventSystems;


public class OptionPauseMenu : MonoBehaviour
{
    [SerializeField]
    private UIAudioSettings m_cAudioSettings = null;

	[SerializeField]
	private OptionsMenu m_cOptionsMenu = null;

    [SerializeField]
    private GameObject m_cButtonGameObject = null;

    [SerializeField]
    private GameObject m_cFirstButtonSelected = null;

	public event System.Action<bool> OnMenuStateChange = null;

	private GameObject m_cGameObject = null;
	public GameObject GameObject
	{
		get { return m_cGameObject; }
	}


	#region MonoBehavior
	private void Awake()
	{
		m_cGameObject = gameObject;
	}
	#endregion

	public void SetActive(bool active)
	{
		m_cGameObject.SetActive(active);
		m_cButtonGameObject.SetActive(active);
		m_cAudioSettings.SetActive(false);
		m_cOptionsMenu.SetActive(false);

		if (active)
			EventSystem.current.SetSelectedGameObject(m_cFirstButtonSelected, new BaseEventData(EventSystem.current));
		else
		{
			EventSystem.current.SetSelectedGameObject(null);

			Controller[] controllers = GameManager.Instance.Controllers;
			foreach (Controller controller in controllers)
				controller.OnCancel -= GoBackInMenu;
		}
	}

	public void AudioButton()
    {
		if (OnMenuStateChange != null)
			OnMenuStateChange(false);

		EventSystem.current.SetSelectedGameObject(null);

		m_cButtonGameObject.SetActive(false);
		m_cAudioSettings.SetActive(true);

		Controller[] controllers = GameManager.Instance.Controllers;
		foreach (Controller controller in controllers)
			controller.OnCancel += GoBackInMenu;
	}

	public void LanguageButton()
	{
		if (OnMenuStateChange != null)
			OnMenuStateChange(false);

		EventSystem.current.SetSelectedGameObject(null);

		m_cButtonGameObject.SetActive(false);
		m_cOptionsMenu.SetActive(true);

		Controller[] controllers = GameManager.Instance.Controllers;
		foreach (Controller controller in controllers)
			controller.OnCancel += GoBackInMenu;
	}

	private void GoBackInMenu()
	{
		m_cButtonGameObject.SetActive(true);
		m_cAudioSettings.SetActive(false);
		m_cOptionsMenu.SetActive(false);

		Controller[] controllers = GameManager.Instance.Controllers;
		foreach (Controller controller in controllers)
			controller.OnCancel -= GoBackInMenu;

		EventSystem.current.SetSelectedGameObject(m_cFirstButtonSelected, new BaseEventData(EventSystem.current));

		if (OnMenuStateChange != null)
			OnMenuStateChange(true);
	}
}
