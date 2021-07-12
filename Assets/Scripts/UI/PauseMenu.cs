using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;


public class PauseMenu : MonoBehaviour
{
	[SerializeField]
	private GameObject m_cFirstButtonSelected = null;

	[SerializeField]
	private GameObject m_cRestartButton = null;

	[SerializeField]
	private GameObject m_cButtonsHolder = null;

    [SerializeField]
    private OptionPauseMenu m_cOptionMenu = null;

	private GameObject m_cGameObject = null;
	public GameObject GameObject
	{
		get { return m_cGameObject; }
	}


	#region MonoBehavior
	private void Awake()
	{
		m_cOptionMenu.OnMenuStateChange += SetConnectStateCancel;

		m_cGameObject = gameObject;
	}
	#endregion

	public void SetActive(bool active)
	{
		m_cGameObject.SetActive(active);
		m_cRestartButton.SetActive(SceneManager.GetActiveScene().name == GameManager.Instance.GameSceneName);

		if (active)
			EventSystem.current.SetSelectedGameObject(m_cFirstButtonSelected, new BaseEventData(EventSystem.current));
		else
		{
			EventSystem.current.SetSelectedGameObject(null);
			SetConnectStateCancel(false);

			m_cButtonsHolder.SetActive(true);
			m_cOptionMenu.SetActive(false);
		}
	}

	#region ButtonInteractions
	public void Restart()
	{
        GameManager gameManager = GameManager.Instance;
		gameManager.Restart();
        gameManager.Pause();
	}

	public void Options()
	{
		EventSystem.current.SetSelectedGameObject(null);

		m_cButtonsHolder.SetActive(false);
		m_cOptionMenu.SetActive(true);

		Controller[] controllers = GameManager.Instance.Controllers;
		foreach (Controller controller in controllers)
			controller.OnCancel += GoBackInMenu;
	}

	public void MainMenu()
	{
        GameManager.Instance.KillSounds();
		SceneLoading.Instance.LoadScene("MainMenu_v2");
	}

	public void Quit()
	{
		UIManager.Instance.ShowQuitPopup();
	}
	#endregion

	private void GoBackInMenu()
	{
		EventSystem.current.SetSelectedGameObject(null);

		m_cButtonsHolder.SetActive(true);
		m_cOptionMenu.SetActive(false);

		SetConnectStateCancel(false);

		EventSystem.current.SetSelectedGameObject(m_cFirstButtonSelected, new BaseEventData(EventSystem.current));
	}

	private void SetConnectStateCancel(bool connect)
	{
		Controller[] controllers = GameManager.Instance.Controllers;

		if (connect)
		{
			foreach (Controller controller in controllers)
				controller.OnCancel += GoBackInMenu;
		}
		else
		{
			foreach (Controller controller in controllers)
				controller.OnCancel -= GoBackInMenu;
		}
	}
}
