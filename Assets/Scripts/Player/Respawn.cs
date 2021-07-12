using UnityEngine;
using FMODUnity;

public class Respawn : MonoBehaviour
{
    private Transform m_cTransform = null;

    private Player m_cPlayer = null;

    public event System.Action OnFinishRespawn = null;

    [SerializeField]
    private Transform m_cGrue = null;
    private GameObject m_cGrueObject = null;
    [SerializeField]
    private Transform m_cGruePosPlayer = null;
    private Animator m_cGrueAnimator = null;

    private Camera m_cGameCamera = null;
    private float m_fDistanceCamera = 0.0f;

    private Plane[] m_cPlaneCamera = new Plane[2];
    private int m_iPlaneCameraExitIdx = 0;
    private Vector3 m_vCurrBorderPosition = Vector3.zero;

    private Vector3 m_vRespawnPosition = Vector3.zero;
    private Vector3 m_vPlayerPos = Vector3.zero;

    private Vector3 m_vDir = Vector3.zero;

    private Vector3 m_vAddRotationLeft = new Vector3(0.0f, -180.0f, 0.0f);

    private Quaternion m_qRotationRespawnStart = Quaternion.identity;
    private Quaternion m_qRotationRespawnEnd = Quaternion.identity;
    private float m_fTimeToRotate = 0.0f;

    private Vector3 m_vPlatformCenter = Vector3.zero;

    private enum E_STATE_RESPAWN
    {
        GoToPlayer = 0,
        Lift,
        Respawn,
        Left
    }

    private E_STATE_RESPAWN m_eStateRespawn = E_STATE_RESPAWN.GoToPlayer;

    [Header("Speed")]
    [SerializeField]
    private float m_fSpeedFlyingToPlayer = 1.0f;
    [SerializeField]
    private float m_fSpeedRespawning = 1.0f;
    [SerializeField]
    private float m_fSpeedLeft = 1.0f;

    [SerializeField]
    private float m_fSpeedRotation = 1.0f;

    private bool m_bIsFlyingToRespawn = false;

    private float m_fReachedDistance = 0.5f;

    private StudioEventEmitter m_cGrueSound = null;

    private Vector3 m_cDir = Vector3.zero;


    #region MonoBehviour
    private void Awake()
    {
        m_cTransform =      GetComponent<Transform>();
        m_cGrueObject =     m_cGrue.gameObject;
        m_cGrueAnimator =   m_cGrue.GetComponent<Animator>();
        m_cPlayer =         GetComponent<Player>();
        m_cGameCamera =     GameObject.FindGameObjectWithTag("GameCamera").GetComponent<Camera>();

        m_vRespawnPosition = m_cTransform.position;
        m_vRespawnPosition.y += 2.0f;

        m_fDistanceCamera = m_cGameCamera.transform.position.magnitude;
        m_cPlaneCamera[0] = GeometryUtility.CalculateFrustumPlanes(m_cGameCamera)[0];
        m_cPlaneCamera[1] = GeometryUtility.CalculateFrustumPlanes(m_cGameCamera)[1];

        m_cGrueSound = m_cGrue.GetComponent<StudioEventEmitter>();

        m_cGrueObject.SetActive(false);
    }

    private void Start()
    {
        m_vPlatformCenter = GameManager.Instance.PlatformTransform.position;
    }

    private void Update ()
    {
        UpdateRespawn();
	}
    #endregion

    private void UpdateRespawn()
    {
        if (m_bIsFlyingToRespawn)
        {
            switch (m_eStateRespawn)
            {
                case E_STATE_RESPAWN.GoToPlayer:
                    {
                        CraneFlyToPlayer();
                        break;
                    }
                case E_STATE_RESPAWN.Lift:
                    {
                        LiftPlayer();
                        break;
                    }
                case E_STATE_RESPAWN.Respawn:
                    {
                        Respawning();
                        break;
                    }
                case E_STATE_RESPAWN.Left:
                    {
                        LeftGame();
                        break;
                    }
                default:
                    break;
            }
        }
    }

    public void StartCraneWillTakePlayer()
    {
        m_cGrueObject.SetActive(true);
        m_eStateRespawn = E_STATE_RESPAWN.GoToPlayer;
        m_bIsFlyingToRespawn = true;
        m_cGrueAnimator.enabled = true;
        m_fTimeToRotate = 0.0f;

        Vector3 dir = m_cTransform.position.normalized;

        if (dir.x < 0)
        {
            m_vCurrBorderPosition = -m_cPlaneCamera[0].normal * m_cPlaneCamera[0].distance;
            m_iPlaneCameraExitIdx = 1;
        }
        else
        {
            m_vCurrBorderPosition = -m_cPlaneCamera[1].normal * m_cPlaneCamera[1].distance;
            m_iPlaneCameraExitIdx = 0;
        }

        m_vCurrBorderPosition.y = 3.0f;
        Vector3 position = new Vector3(m_vCurrBorderPosition.x, m_cTransform.position.y + (m_cPlayer.HeightCapsuleHalf * 2.0f), m_cTransform.position.z);
        m_cGrue.position = position;

        m_vCurrBorderPosition = -m_cPlaneCamera[m_iPlaneCameraExitIdx].normal * m_cPlaneCamera[m_iPlaneCameraExitIdx].distance;
        m_vCurrBorderPosition.y = m_vRespawnPosition.y;
        m_vCurrBorderPosition.z = m_vRespawnPosition.z;

        m_vPlayerPos = m_cTransform.position;
        m_vPlayerPos.y += m_cPlayer.HeightCapsuleHalf * 2.0f;
        Vector3 dirPlayer = (m_vPlayerPos - m_cGrue.position).normalized;
        m_cGrue.forward = -dirPlayer;
    }

    private void CraneFlyToPlayer()
    {
        Vector3 dir = (m_vPlayerPos - m_cGrue.position);
        m_cGrue.position += dir.normalized * m_fSpeedFlyingToPlayer * Time.deltaTime;

        if (HasReachedCrtTarget(dir))
        {
            m_eStateRespawn = E_STATE_RESPAWN.Lift;
            m_cGrueAnimator.SetBool("IsCarry", true);
            m_cPlayer.m_cPlayerModel.Animator.SetBool("IsFalling", false);
        }
    }

    private void LiftPlayer()
    {
        Vector3 pos = (m_cTransform.position).normalized;
        float x = 7.0f;
        if (pos.x < 0)
            x = -x;

        Vector3 repawnPos = new Vector3(x, m_vRespawnPosition.y, m_cTransform.position.z);
        Vector3 dir = (repawnPos - m_cGrue.position);
        m_cGrue.position += dir.normalized * m_fSpeedRespawning * Time.deltaTime;

        m_cTransform.position = m_cGruePosPlayer.position - (Vector3.up * m_cPlayer.HeightCapsuleHalf * 2.0f);

        if (HasReachedCrtTarget(dir))
        {
            m_eStateRespawn = E_STATE_RESPAWN.Respawn;
            m_cGrueSound.Play();
            m_qRotationRespawnStart = m_cGrue.rotation;
            m_fTimeToRotate = 0.0f;
        }
    }

    private void Respawning()
    {
        Vector3 dir = (m_vRespawnPosition - m_cGrue.position);
        m_cGrue.position += dir.normalized * m_fSpeedRespawning * Time.deltaTime;
        m_cTransform.position = m_cGruePosPlayer.position - (Vector3.up * m_cPlayer.HeightCapsuleHalf * 2.0f);
        SetRotationGrue(m_vRespawnPosition, m_vAddRotationLeft);

        if (HasReachedCrtTarget(dir))
        {
            if (OnFinishRespawn != null)
                OnFinishRespawn();

            m_cGrueAnimator.SetBool("IsCarry", false);
            m_eStateRespawn = E_STATE_RESPAWN.Left;

            m_qRotationRespawnStart = m_cGrue.rotation;
            m_fTimeToRotate = 0.0f;
        }
    }

    private void LeftGame()
    {
        m_cDir = (m_vCurrBorderPosition - m_cGrue.position);
        m_cGrue.position += m_cDir.normalized * m_fSpeedLeft * Time.deltaTime;
        SetRotationGrue(m_vCurrBorderPosition, m_vAddRotationLeft);

        if (HasReachedCrtTarget(m_cDir))
        {
            m_bIsFlyingToRespawn = false;
            m_cGrueObject.SetActive(false);
        }
    }

    private void SetRotationGrue(Vector3 focusPos, Vector3 angle)
    {
        if (m_fTimeToRotate <= 1.0f)
        {
            m_cGrue.LookAt(focusPos);

            Quaternion targetRot = m_cGrue.rotation;
            targetRot *= Quaternion.Euler(angle);
            m_cGrue.rotation = targetRot;

            m_qRotationRespawnEnd = m_cGrue.rotation;
            m_cTransform.rotation = m_qRotationRespawnStart;

            m_fTimeToRotate += Time.deltaTime * m_fSpeedRotation;
            m_cGrue.rotation = Quaternion.Lerp(m_qRotationRespawnStart, m_qRotationRespawnEnd, m_fTimeToRotate);
        }
    }

    private bool HasReachedCrtTarget(Vector3 direction)
    {
        if (direction.magnitude <= m_fReachedDistance)
            return true;
        return false;
    }

    public void Disable()
    {
        m_cGrue.gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1.0f, 0.0f, 0.0f);
        Gizmos.DrawLine(m_cGrue.position, m_cGrue.position + (m_cDir.normalized * 10.0f));
    }
}
