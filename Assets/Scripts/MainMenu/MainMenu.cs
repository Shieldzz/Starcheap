using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class MainMenu : MonoBehaviour
{
	private static MainMenu m_cInstance = null;
	public static MainMenu Instance
	{
		get { return m_cInstance; }
	}

    [Header("TitleScreen")]
	[SerializeField]
	private Canvas m_cTitleScreenCanvas = null;

    [SerializeField]
    private CanvasGroup m_cTitleScreenCanvasGroup = null;

	[SerializeField]
	private Button m_cPlayButton = null;
    [SerializeField]
    private Button m_cQuitButton = null;

    [Header("Options")]
	[SerializeField]
	private Canvas m_cOptionsMenu = null;

    [SerializeField]
    private CanvasGroup m_cOptionsCanvasGroup = null;

    [SerializeField]
    private Canvas m_cOptionButtonCanvas = null;

    [Header("Popup")]
    [SerializeField]
    private Popup m_cPopup = null;

	[Header("Credits")]
	[SerializeField]
	private Canvas m_cCreditsCanvas = null;

    [Header("Audio")]
    [SerializeField]
    private Canvas m_cAudioScreenCanvas = null;

    [SerializeField]
    private CanvasGroup m_cAudioScreenCanvasGroup = null;

    [SerializeField]
    private Button m_cAudioButton = null;

    [SerializeField]
    private Slider m_cMasterSlider = null;

    [Header("Language")]
    [SerializeField]
    private OptionsMenu m_cLangugageScreen = null;

    [SerializeField]
	private Button m_cLanguageButton = null;

	[SerializeField]
	private string m_sPlayButtonSceneName = "";


    [Header("Animation")]
    [SerializeField]
    private Animator m_cAnimatorShip = null;
    [SerializeField]
    private Animator m_cAnimatorCraneCarry = null;
    [SerializeField]
    private Animator m_cAnimatorTitle = null;
    [SerializeField]
    private Animator m_cAnimatorButton = null;
    private Animator m_cAnimatorCrane = null;

    [SerializeField]
    private GameObject m_cFXShip_smokeRight = null;
    [SerializeField]
    private GameObject m_cFXShip_smokeLeft = null;

    private bool m_bIsAnimEnd = false;
    [SerializeField]
    private float m_fTimerResetAnim = 8.0f;
    private float m_fCurrTimerResetAnim = 0.0f;


	private Controller[] m_acControllers = new Controller[4];
	public Controller[] Controllers
	{
		get { return m_acControllers; }
	}

	private E_MENU_STATE m_eCurrMenuState = E_MENU_STATE.TitleScreen;

	public enum E_MENU_STATE
	{
		TitleScreen,
		Options,
		Credits,

        Language,
        Audio,

		Count
	}


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

		InitControllers();

        m_cAnimatorCrane = GetComponent<Animator>();

    }

	private void Start()
	{
		m_cLangugageScreen.SetActive(false);
        m_cPopup.SetVisible(false);
	}

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.T))
            ResetAnim();

        if (m_bIsAnimEnd && (!m_cOptionsMenu.isActiveAndEnabled && !m_cCreditsCanvas.isActiveAndEnabled && !m_cPopup.CanvasActive()))
        {
            m_fCurrTimerResetAnim += Time.deltaTime;

            if (m_fCurrTimerResetAnim > m_fTimerResetAnim)
                ResetAnim();
        }
    }

    private void OnDestroy()
	{
		foreach (Controller currentController in m_acControllers)
		{
			currentController.OnCancel -= Cancel;
            currentController.OnCancel -= ResetTimerControllerInput;

            currentController.OnAccept -= Skip;
            currentController.OnAccept -= ResetTimerControllerInput;

            currentController.OnLeftStick -= ResetTimerControllerMove;
        }
	}
    #endregion

    #region Buttons
    public void PlayButtonPressed()
	{
        if (m_bIsAnimEnd)
		    LaunchGame();
	}

	public void OptionsButtonPressed()
	{
        if (m_bIsAnimEnd)
            ChangeMenuState(E_MENU_STATE.Options);
	}

    public void AudioButtonPressed()
    {
        if (m_bIsAnimEnd)
            ChangeMenuState(E_MENU_STATE.Audio);
    }

    public void LanguageButtonPressed()
    {
        if (m_bIsAnimEnd)
            ChangeMenuState(E_MENU_STATE.Language);
    }

	public void CreditsButtonPressed()
	{
        if (m_bIsAnimEnd)
            ChangeMenuState(E_MENU_STATE.Credits);
	}

	public void QuitButtonPressed()
	{
        if (m_bIsAnimEnd)
            m_cPopup.Init("QuitPopup",
            () => { Quit(); },
            () => { EventSystem.current.SetSelectedGameObject(m_cQuitButton.gameObject, new BaseEventData(EventSystem.current));  });
	}

    private void Quit()
    {
        #if UNITY_EDITOR
                EditorApplication.isPlaying = false;
        #else
		        Application.Quit();
        #endif
    }
    #endregion

    private void InitControllers()
	{
		m_acControllers = FindObjectsOfType<Controller>();

		#region DevDebug
#if UNITY_EDITOR
		if (m_acControllers.Length == 0)
		{
			SceneManager.LoadScene("Launch");
			return;
		}
#endif
		#endregion

		Array.Sort(m_acControllers, delegate (Controller a, Controller b)
		{
			return a.PlayerNumber.CompareTo(b.PlayerNumber);
		});

		foreach (Controller currentController in m_acControllers)
		{
			currentController.LockInputs = false;

            currentController.OnCancel += Cancel;
            currentController.OnCancel += ResetTimerControllerInput;

            currentController.OnAccept += Skip;
            currentController.OnAccept += ResetTimerControllerInput;

            currentController.OnLeftStick += ResetTimerControllerMove;
        }
	}

    #region State
    private void LaunchGame()
	{
		for (int controllerIdx = 0; controllerIdx < 4; ++controllerIdx)
			m_acControllers[controllerIdx].LockInputs = true;

		SceneLoading.Instance.LoadScene(m_sPlayButtonSceneName);
	}

	private void Cancel()
	{
		switch (m_eCurrMenuState)
		{
			case E_MENU_STATE.Options:
				ResetMenu();
				ChangeMenuState(E_MENU_STATE.TitleScreen);
				break;
			case E_MENU_STATE.Credits:
				ChangeMenuState(E_MENU_STATE.TitleScreen);
				break;
			case E_MENU_STATE.Language:
				ChangeMenuState(E_MENU_STATE.Options);
				break;
			case E_MENU_STATE.Audio:
				ChangeMenuState(E_MENU_STATE.Options);
				break;
			default: break;
		}
	}

    private void ResetMenu()
    {
        m_cAudioScreenCanvas.enabled = false;
        m_cAudioScreenCanvasGroup.interactable = false;
        m_cOptionButtonCanvas.enabled = true;
        m_cLangugageScreen.SetActive(false);
    }

	private void ChangeMenuState(E_MENU_STATE newState)
	{
		switch (newState)
		{
			case E_MENU_STATE.TitleScreen:
				m_cTitleScreenCanvas.enabled = true;
                m_cTitleScreenCanvasGroup.interactable = true;

				EventSystem.current.SetSelectedGameObject(m_cPlayButton.gameObject);
				m_cPlayButton.OnSelect(null);
                
				m_cOptionsMenu.enabled = false;
				m_cOptionButtonCanvas.enabled = false;
				m_cOptionsCanvasGroup.interactable = false;

				m_cCreditsCanvas.enabled = false;
				break;
			case E_MENU_STATE.Options:
                m_cOptionsMenu.enabled = true;
				m_cOptionButtonCanvas.enabled = true;
				m_cOptionsCanvasGroup.interactable = true;

				m_cTitleScreenCanvas.enabled = false;
				m_cTitleScreenCanvasGroup.interactable = false;

				m_cAudioScreenCanvas.enabled = false;
				m_cAudioScreenCanvasGroup.interactable = false;

				m_cLangugageScreen.SetActive(false);

				EventSystem.current.SetSelectedGameObject(m_cAudioButton.gameObject);
				m_cAudioButton.OnSelect(null);
				break;
			case E_MENU_STATE.Credits:
				m_cCreditsCanvas.enabled = true;

				m_cTitleScreenCanvas.enabled = false;
				m_cTitleScreenCanvasGroup.interactable = false;

				EventSystem.current.SetSelectedGameObject(null);
				break;
            case E_MENU_STATE.Audio:
                m_cAudioScreenCanvas.enabled = true;
                m_cAudioScreenCanvasGroup.interactable = true;

                EventSystem.current.SetSelectedGameObject(m_cMasterSlider.gameObject);
                m_cMasterSlider.OnSelect(null);

                m_cOptionButtonCanvas.enabled = false;
                break;
            case E_MENU_STATE.Language:
                m_cLangugageScreen.SetActive(true);

                EventSystem.current.SetSelectedGameObject(m_cLanguageButton.gameObject);
                m_cLanguageButton.OnSelect(null);

                m_cOptionButtonCanvas.enabled = false;
                break;
			default: break;
		}

		m_eCurrMenuState = newState;
	}
    #endregion

    public void AnimationEnd()
    {
        m_bIsAnimEnd = true;
        m_fCurrTimerResetAnim = 0.0f;
    }

    private void Skip()
    {
        m_cAnimatorShip.Play("Anim_ship",0, 4.0f);
        m_cAnimatorCrane.Play("Anim_Crane_carryplanks", 0, 7.0f);
        m_cAnimatorCraneCarry.SetBool("IsCarry", true);
        m_cAnimatorTitle.Play("Anim_Title", 0, 6.15f);
        m_cAnimatorButton.Play("Take 003");

        m_cFXShip_smokeLeft.SetActive(false);
        m_cFXShip_smokeRight.SetActive(false);

        Invoke("AnimationEnd", 0.25f);
    }

    private void ResetAnim()
    {
        m_cAnimatorCrane.Play("Anim_Crane_carryplanks", 0, 0.0f);
        m_cAnimatorShip.Play("Anim_ship", 0, 0.0f);

        m_cAnimatorCraneCarry.SetTrigger("Reset");
        m_cAnimatorCraneCarry.SetBool("IsCarry", false);
        

        m_cAnimatorTitle.Play("Anim_Title", 0, 0.0f);
        m_cAnimatorButton.Play("Take 001");

        m_cFXShip_smokeLeft.SetActive(true);
        m_cFXShip_smokeRight.SetActive(true);

        m_bIsAnimEnd = false;
    }

    private void ResetTimerControllerMove(Vector2 move)
    {
        m_fCurrTimerResetAnim = 0.0f;
    }

    private void ResetTimerControllerInput()
    {
        m_fCurrTimerResetAnim = 0.0f;
    }
}
