using FMODUnity;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class Intro : MonoBehaviour
{
	[SerializeField]
	private Canvas m_cSkipCanvas = null;

	[Header("Sounds")]
	[SerializeField]
	private StudioEventEmitter m_cCinematicSound = null;

	[SerializeField]
	private float m_fCinematicDuration = 35f;
	private float m_fCinematicTimer;

	[SerializeField]
	private float m_fTransitionDuration = 1f;
	private float m_fTransitionTimer;

	[Header("Transition")]
	[SerializeField]
	private Image m_cTransitionImage = null;

	[SerializeField]
	private string m_sEndCinematicSceneName = "";

	private Controller[] m_acControllers;

	private bool m_bTransition = false;


	#region MonoBehavior
	private void Awake()
	{
		m_cSkipCanvas.enabled = false;

		m_fCinematicTimer = m_fCinematicDuration;
		m_fTransitionTimer = m_fTransitionDuration;

		m_acControllers = FindObjectsOfType<Controller>();

		foreach (Controller controller in m_acControllers)
		{
			controller.OnAccept += Skip;

			controller.m_bLockMenuInputs = false;
		}
	}

	private void Start()
	{
		m_cCinematicSound.Play();
	}

	private void Update()
	{
		if (!m_bTransition)
		{
			m_fCinematicTimer -= Time.deltaTime;

			if (m_fCinematicTimer <= 0f)
				StartCoroutine(Transition());
		}
	}
	#endregion

	private void Skip()
	{
		if (!m_cSkipCanvas.enabled)
			m_cSkipCanvas.enabled = true;
		else
			StartCoroutine(Transition());
	}

	private IEnumerator Transition()
	{
		m_bTransition = true;

		foreach (Controller controller in m_acControllers)
		{
			controller.OnAccept -= Skip;

			controller.m_bLockMenuInputs = true;
		}

		while (m_fTransitionTimer > 0f)
		{
			m_fTransitionTimer -= Time.deltaTime;

			Color color = m_cTransitionImage.color;
			color.a += Time.deltaTime / m_fTransitionDuration;
			m_cTransitionImage.color = color;

			yield return null;
		}

		SceneLoading.Instance.LoadScene(m_sEndCinematicSceneName);
	}
}
