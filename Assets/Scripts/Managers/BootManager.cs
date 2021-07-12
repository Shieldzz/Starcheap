using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


public class BootManager : MonoBehaviour
{
	[SerializeField]
	private GameObject m_cSceneLoadingPrefab = null;

	[SerializeField]
	private LocalizationManager m_cLocalizationManagerPrefab = null;

	[SerializeField]
	private Controller m_cControllerPrefab = null;

	[SerializeField]
	private string m_sMainMenuSceneName = "";


	private void Awake()
	{
		DontDestroyOnLoad(gameObject);

		Init();

		StartCoroutine(LoadMainMenu());
	}

	private void Init()
	{
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Confined;

		GameObject sceneLoading = Instantiate(m_cSceneLoadingPrefab);
		DontDestroyOnLoad(sceneLoading);

		LocalizationManager localizationManager = Instantiate(m_cLocalizationManagerPrefab);
		localizationManager.LoadLocalizationFile("en");

		for (int idx = 0; idx < 4; ++idx)
		{
			Controller controller = Instantiate(m_cControllerPrefab);
			controller.LockInputs = true;
			controller.Init(idx);
			DontDestroyOnLoad(controller);
		}
	}

	private IEnumerator LoadMainMenu()
	{
		AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(m_sMainMenuSceneName, LoadSceneMode.Single);

		while (!asyncOperation.isDone)
			yield return null;

		Destroy(gameObject);
	}
}
