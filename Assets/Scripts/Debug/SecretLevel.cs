using UnityEngine;


public class SecretLevel : MonoBehaviour
{
	static private SecretLevel m_cSecretLevel = null;

	[SerializeField]
	private string m_sSecretLevelName = "";

	[SerializeField]
	private KeyCode[] m_aeSecretLevelSequence = new KeyCode[13];

	private int m_iSequenceIndex = 0;


	#region MonoBehavior
	private void Awake()
	{
		if (m_cSecretLevel == null)
		{
			DontDestroyOnLoad(this);

			m_cSecretLevel = this;
		}
		else
			Destroy(this);
	}

	private void Update()
	{
		if (Input.GetKeyDown(m_aeSecretLevelSequence[m_iSequenceIndex]))
		{
			if (++m_iSequenceIndex == m_aeSecretLevelSequence.Length)
			{
				m_iSequenceIndex = 0;

				GoToSecretLevel();
			}
		}
		else if (Input.anyKeyDown)
			m_iSequenceIndex = 0;
	}
	#endregion

	private void GoToSecretLevel()
	{
		GameManager.Instance.KillSounds();
		UIEndGame.Instance.RestartUI();

		GameManager.Instance.CancelInvoke();

		SceneLoading.Instance.LoadScene(m_sSecretLevelName);

		UIManager.Instance.Init();
	}
}
