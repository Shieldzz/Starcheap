using UnityEngine;


public class DestructionManager : MonoBehaviour
{
    [SerializeField]
    private Platform m_cPlatform;

	[SerializeField]
	private GameObject m_cDestructionFX = null;

	[SerializeField]
	private float m_fDestructionStartTimer = 0f;
	private float m_fDestructionStartCurrTimer = 0f;
	private bool m_bDestructionStart = false;

    [SerializeField]
    private float m_fDestructionEndTimer = 0f;
    private float m_fDestructionEndCurrTimer = 0f;
    private bool m_bDestructionEnd = false;

    [SerializeField]
    private Color m_cMainColorFeedback = new Color(1f, 0f, 0f, 1f);

    [SerializeField]
    private float m_fTimerToImpule = 0.0f;
    [SerializeField]
    private float m_fFeedbackDuration = 1.0f;

    private PlatformBorder[] m_acPlatformBorders = new PlatformBorder[1];
    private CrumblePlate[] m_acPlates = new CrumblePlate[1];

	private int m_iPlatformBordersNumber;

    [Header("Animation plate")]
    [SerializeField]
    private AnimationCurve m_cFeedbackAnimationCurve = null;

    [SerializeField]
    private Rigidbody[] m_acRigidbodyPlatformDest;
    private int m_iCurrIndexPlatformDest = 0;
    private int m_iIndexPlatformDest = 0;

	[Header("Camera shake")]
	[SerializeField]
	private CameraEffects.CameraShakeData m_cDestructionCameraShake;
	private CameraEffects m_cCameraEffects = null;

	[Header("Controller vibration")]
	[SerializeField]
	private ControllerVibrationData m_cDestructionControllerVibration;


    #region MonoBehaviour
    private void Awake()
    {
		m_cDestructionFX.SetActive(false);

		m_acPlatformBorders = FindObjectsOfType<PlatformBorder>();
		m_iPlatformBordersNumber = m_acPlatformBorders.Length;

        m_acPlates = FindObjectsOfType<CrumblePlate>();

		m_fDestructionStartCurrTimer = m_fDestructionStartTimer;
        m_fDestructionEndCurrTimer = m_fDestructionEndTimer;
        m_iIndexPlatformDest = m_acRigidbodyPlatformDest.Length -1;

		m_cCameraEffects = GameObject.FindGameObjectWithTag("CinemachineCamera").GetComponent<CameraEffects>();

	}

    private void Start()
    {
		foreach (PlatformBorder border in m_acPlatformBorders)
			border.OnDestroyed += PlatformBorderDestroy;

        foreach (CrumblePlate plate in m_acPlates)
            plate.Init(m_cPlatform.Center, m_fFeedbackDuration, m_fTimerToImpule, m_cMainColorFeedback, m_cFeedbackAnimationCurve);
	}
	
	private void Update()
    {
		if (m_bDestructionStart)
		{
			m_fDestructionStartCurrTimer -= Time.deltaTime;

			if (m_fDestructionStartCurrTimer <= 0f)
			{
				m_bDestructionStart = false;

				StartPlatformBordersDestruction();
			}
		}

        if (m_bDestructionEnd)
        {
            m_fDestructionEndCurrTimer -= Time.deltaTime;

            if (m_fDestructionEndCurrTimer <= 0f)
            {
                m_fDestructionEndCurrTimer = m_fDestructionEndTimer;
                PlatformFinishGameDestroy();

                if (m_iCurrIndexPlatformDest == m_iIndexPlatformDest)
                    m_bDestructionEnd = false;

                m_iCurrIndexPlatformDest++;
            }
        }
	}
    #endregion

	public void StartDeterioration()
	{
		m_bDestructionStart = true;
	}

	public void LaunchDestruction()
	{
		m_cDestructionFX.SetActive(true);
        m_bDestructionEnd = true;

		m_cCameraEffects.PlayCameraShake(m_cDestructionCameraShake);

		GameManager.Instance.SetControllersVibration(m_cDestructionControllerVibration);
    }

	private void StartPlatformBordersDestruction()
	{
		foreach (PlatformBorder border in m_acPlatformBorders)
			border.StartDeterioration();
	}

	private void StartPlatesDestruction()
	{
		float currGameTimer = UIManager.Instance.GameTimer.CurrTime;
		int platesNumber = m_acPlates.Length;

		foreach (CrumblePlate plate in m_acPlates)
			plate.CalculeDurability(currGameTimer, platesNumber);
	}

	private void PlatformBorderDestroy()
	{
		--m_iPlatformBordersNumber;

		if (m_iPlatformBordersNumber == 0)
			StartPlatesDestruction();
	}

    private void PlatformFinishGameDestroy()
    {
        Rigidbody rigidbody = m_acRigidbodyPlatformDest[m_iCurrIndexPlatformDest];
        rigidbody.useGravity = true;
        rigidbody.AddForce(transform.right * 10.0f, ForceMode.Impulse);
    }
}
