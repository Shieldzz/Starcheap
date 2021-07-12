using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class SceneLoading : MonoBehaviour
{
	private static SceneLoading m_cInstance = null;
	public static SceneLoading Instance
	{
		get { return m_cInstance; }
	}

	[SerializeField]
	private string m_sLoadingSceneName = "";

	[SerializeField]
	private float m_fMinimumLoadingTime = 1f;
	private float m_fMinimumLoadingTimer = 0f;

	private string m_sSceneName = "";

	private bool m_bIsLoading = false;
	private bool m_bAsyncLoadFinished = false;


	private void Awake()
	{
		if (m_cInstance == null)
			m_cInstance = this;
		else
		{
			Destroy(gameObject);
			return;
		}
	}

	private void Update()
	{
		if (m_bIsLoading)
			m_fMinimumLoadingTimer += Time.deltaTime;
	}

	public void LoadScene(string sceneToLoad)
	{
		m_sSceneName = sceneToLoad;
		m_fMinimumLoadingTimer = 0f;
		m_bAsyncLoadFinished = false;

		Time.timeScale = 1f;

		StartCoroutine(Loading());
	}

	private IEnumerator Loading()
	{
		AsyncOperation loadingScreenAsyncOperation = SceneManager.LoadSceneAsync(m_sLoadingSceneName, LoadSceneMode.Single);

		while (!loadingScreenAsyncOperation.isDone)
			yield return null;

		AsyncOperation nextSceneAsyncOperation = SceneManager.LoadSceneAsync(m_sSceneName, LoadSceneMode.Additive);
		nextSceneAsyncOperation.allowSceneActivation = false;

		m_bIsLoading = true;

		while (!m_bAsyncLoadFinished)
		{
			if (nextSceneAsyncOperation.progress >= 0.9f && m_fMinimumLoadingTimer >= m_fMinimumLoadingTime)
			{
				m_bAsyncLoadFinished = true;
				m_bIsLoading = false;
			}

			yield return null;
		}

		nextSceneAsyncOperation.allowSceneActivation = true;

		while (!nextSceneAsyncOperation.isDone)
			yield return null;

		SceneManager.SetActiveScene(SceneManager.GetSceneByName(m_sSceneName));

		SceneManager.UnloadSceneAsync(m_sLoadingSceneName);
	}
}
