using FMODUnity;
using System.Collections;
using UnityEngine;


public class Platform : MonoBehaviour
{
    public System.Action OnFinishResetPlatform = null;

    [Header("Physics")]
    [SerializeField]
    private Transform m_cPlatformTransform = null;
    public Transform Transform { get { return m_cPlatformTransform; } }
    [SerializeField]
    private Vector3 m_vCenter = Vector3.zero;
    public Vector3 Center { get { return m_vCenter; } }
	[SerializeField]
	private float m_fStability = 1f;
    public float Stability
    {
        get { return m_fStability; }
		private set { m_fStability = value; }
    }
	public Vector3 m_vWindDirection = Vector3.zero;

	[SerializeField]
	private float m_fAcceleration = 1f;
	public float Acceleration
	{
		get { return m_fAcceleration; }
		private set { m_fAcceleration = value; }
	}

	[SerializeField]
	private Vector2 m_vLockAngle = Vector2.zero;

	[SerializeField]
	private float m_fStartFreezeDuration = 5f;
	private float m_fStartFreezeTimer;
	private bool m_bActivatePhysics = false;
    private bool m_bActivateAnim = false;
    private bool m_bIsStartingGame = true;

	[Header("Sounds")]
	[SerializeField]
	private StudioEventEmitter m_cUnstableSound;
	[SerializeField]
	private Vector2 m_vUnstableAngle = Vector2.zero;
	[SerializeField]
	private float m_fUnstableResetSound = 1f;
	private float m_fUnstableResetTimer = 0f;

	private Cell[] m_cCells;

	private Vector3 m_vBalanceDir = Vector3.zero;
	private Vector3 m_vPrevBalanceDir = Vector3.zero;

    [Header("Mesh")]
    [SerializeField]
    private GameObject m_cPlatformModel = null;
    [SerializeField]
    private GameObject m_cPlatformBorder = null;
    [SerializeField]
    private GameObject m_cPieceMachine = null;

    private FollowSplines m_cFollowSpline = null;

    private bool m_bIsFindAgainBasicPosition = false;
    private float m_fTimeCountRotation = 0.0f;
    private Quaternion m_qStartRotationSpaceship = Quaternion.identity;
    [SerializeField]
    private float m_fSpeedRotate = 1.0f;

    #region MonoBehavior
    private void Awake()
	{
		m_cCells = GetComponentsInChildren<Cell>();
        m_cFollowSpline = GetComponent<FollowSplines>();

		m_fStartFreezeTimer = m_fStartFreezeDuration;
		m_fUnstableResetTimer = m_fUnstableResetSound;
	}

	private void Update()
	{
		if (!m_bActivatePhysics && m_bIsStartingGame)
		{
			m_fStartFreezeTimer -= Time.deltaTime;

            if (m_fStartFreezeTimer <= 0f)
            {
                m_bActivatePhysics = true;
                m_bIsStartingGame = false;
            }
		}

        if (m_bIsFindAgainBasicPosition)
            ResetLerp();

		m_fUnstableResetTimer += Time.deltaTime;
	}

	private void FixedUpdate()
	{
		if (m_bActivatePhysics)
			ComputeBalance();
	}
	#endregion

	private void ComputeBalance()
	{
		m_vPrevBalanceDir = m_vBalanceDir;
		m_vBalanceDir = Vector3.zero;

		Vector3 platformRot = m_cPlatformTransform.rotation.eulerAngles;

		float xAngle = (platformRot.x > 180f) ? platformRot.x - 360f : platformRot.x;
		float zAngle = (platformRot.z > 180f) ? Mathf.Abs(platformRot.z - 360f) : -platformRot.z;

		Vector3 slope = new Vector3(zAngle, 0f, xAngle) + m_vWindDirection;

		for (int idx = 0; idx < m_cCells.Length; ++idx)
			m_vBalanceDir += m_cCells[idx].ComputeForce(m_vCenter, slope);

		m_vBalanceDir.y = 0f;
		m_vBalanceDir /= m_fStability;


		float sameDirDot = Vector3.Dot(m_vBalanceDir.normalized, m_vPrevBalanceDir.normalized);
		if (sameDirDot > 0f)
			m_vBalanceDir += m_vPrevBalanceDir * sameDirDot * m_fAcceleration;


		float absoluteXAngle = Mathf.Abs(xAngle);
		float absoluteZAngle = Mathf.Abs(zAngle);

		if (/*absoluteZAngle >= m_vUnstableAngle.x &&*/ Mathf.Sign(zAngle) == Mathf.Sign(m_vBalanceDir.x))
		{
			//PlatformUnstable();

			if (absoluteZAngle >= m_vLockAngle.x)
				m_vBalanceDir.x = 0f;
		}

		if (/*absoluteXAngle >= m_vUnstableAngle.y && */Mathf.Sign(xAngle) == Mathf.Sign(m_vBalanceDir.z))
		{
			//PlatformUnstable();

			if (absoluteXAngle >= m_vLockAngle.y)
				m_vBalanceDir.z = 0f;
		}

        if (Mathf.Abs(m_vBalanceDir.z) >= m_vUnstableAngle.y || Mathf.Abs(m_vBalanceDir.x) >= m_vUnstableAngle.x)
            PlatformUnstable();

        platformRot.x += m_vBalanceDir.z * Time.fixedDeltaTime;
		platformRot.z += -m_vBalanceDir.x * Time.fixedDeltaTime;
		m_cPlatformTransform.rotation = Quaternion.Euler(platformRot);
	}
    public void DisablePlatformLose()
    {
        m_bActivatePhysics = false;
        m_bIsStartingGame = false;
        m_cFollowSpline.StartMoving();
    }

    public void DisablePlatformWin()
    {
        m_bActivatePhysics = false;
        m_bIsFindAgainBasicPosition = true;
        m_qStartRotationSpaceship = m_cPlatformTransform.rotation;
    }

    private void PlatformUnstable()
    {
    	if (m_fUnstableResetTimer >= m_fUnstableResetSound)
    	{
    		m_fUnstableResetTimer = 0f;

    		m_cUnstableSound.Play();
    	}
    }

    private void ResetLerp()
    {
        m_fTimeCountRotation += Time.deltaTime * m_fSpeedRotate;
        if (m_fTimeCountRotation <= 1.0f)
            m_cPlatformTransform.rotation = Quaternion.Lerp(m_qStartRotationSpaceship, Quaternion.identity, m_fTimeCountRotation);
        else
        {
            m_bIsFindAgainBasicPosition = false;
            if (OnFinishResetPlatform != null)
                OnFinishResetPlatform();
            m_cFollowSpline.StartMoving();
        }
    }


#if UNITY_EDITOR
    #region Gizmos
    private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawLine(Vector3.zero, m_vBalanceDir * 3f);
	}
	#endregion
#endif
}
