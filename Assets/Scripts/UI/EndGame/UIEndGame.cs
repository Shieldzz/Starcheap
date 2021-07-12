#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.EventSystems;


public class UIEndGame : MonoBehaviour
{
    private static UIEndGame m_cInstance = null;
    public static UIEndGame Instance
    {
        get { return m_cInstance; }
    }

    [SerializeField]
    private UIScore m_cUIWIn = null;

    [SerializeField]
    private LoseUI m_cUILose = null;

    [SerializeField]
    private Canvas m_cButton = null;

    [SerializeField]
    private Popup m_cPopup = null;

    [SerializeField]
    private GameObject m_cUILoseText = null;
    [SerializeField]
    private GameObject m_cUIGlobalText = null;

    [SerializeField]
    private GameObject m_cFirstButtonSelected = null;

    #region MonoBehaviour
    void Start ()
    {
        if (m_cInstance == null)
        {
            m_cInstance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        m_cUILose.OnShowUI += ShowButton;
    }
	
    #endregion

    private void SetVisibleButtons(bool visible)
    {
        m_cButton.enabled = visible;
        if (visible)
        {
            EventSystem.current.SetSelectedGameObject(m_cFirstButtonSelected, new BaseEventData(EventSystem.current));
            //SetScores();
            //Time.timeScale = 0f;
        }
        else
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void SetVisible(bool visible)
    {
        m_cUIWIn.SetActive(visible);
        m_cUILose.SetActive(visible);
        m_cUILoseText.SetActive(visible);
        m_cUIGlobalText.SetActive(visible);
        m_cButton.enabled = visible;
        m_cPopup.SetVisible(visible);
    }
    
    public void ShowWinUI()
    {
        m_cUIWIn.SetActive(true);
        m_cUIGlobalText.SetActive(true);
        SetVisibleButtons(true);

        m_cUIWIn.SetScores(GameManager.Instance.Spaceship.Transform);
    }

    public void ShowLoseUI()
    {
        m_cUILose.SetActive(true);
        m_cUILoseText.SetActive(true);
        m_cUILose.LaunchCrane();
    }

    private void ShowButton()
    {
        SetVisibleButtons(true);
    }

    public void RestartUI()
    {
        m_cUILose.Restart();
    }

    public void Restart()
    {
        GameManager.Instance.Restart();
        Time.timeScale = 1f;
    }

    public void Menu()
    {
        GameManager.Instance.KillSounds();
        SceneLoading.Instance.LoadScene("MainMenu_v2");
    }

    public void ShowPopupQuit()
    {
        m_cPopup.Init("QuitPopup",
           () => { Quit(); },
           () => { EventSystem.current.SetSelectedGameObject(m_cFirstButtonSelected.gameObject, new BaseEventData(EventSystem.current)); });
    }

    private void Quit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
		    Application.Quit();
#endif
    }
}
