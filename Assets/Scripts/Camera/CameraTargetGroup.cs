using Cinemachine;
using System.Collections.Generic;
using UnityEngine;


public class CameraTargetGroup : MonoBehaviour
{
	[SerializeField]
	private Player[] m_acPlayers = new Player[4];

	[SerializeField]
	private CinemachineTargetGroup m_cCinemachineTargetGroup = null;

	[SerializeField]
	private Transform m_cCameraCenter = null;
	[SerializeField]
	private float m_fCenterWeight = 1f;

	private CinemachineTargetGroup.Target m_cCenterTarget;

	private List<CinemachineTargetGroup.Target> m_lcTargets = new List<CinemachineTargetGroup.Target>();

    private bool m_bIsTargetPlayer = true;

    [SerializeField]
    private CinemachineVirtualCamera m_cCinemachineVirtualCamera = null;

    [Header("Scoring")]
    [SerializeField]
    private Vector3 m_vAngleCameraScoringScreen = new Vector3(90.0f, 0.0f, 0.0f);
    private Quaternion m_qEndAngleCameraScoringScreen = Quaternion.identity;
    private Quaternion m_qStartAngleCameraScoringScreen = Quaternion.identity;

    private bool m_bIsRotateToFocusSpaceship = false;

    [SerializeField]
    private float m_fSpeedMoveCamera = 1.0f;
    private float m_fTimeCountRotation = 0.0f;
    [SerializeField]
    private Vector3 m_vDampingCameraScoreScene = Vector3.zero;

    [SerializeField]
    private float m_fTimerToUpdateFOV = 0.0f;
    private float m_fCurrTimerFOV = 0.0f;
    private bool m_bIsUpdateFOV = false;

    private CinemachineFramingTransposer m_cFramingTransposer = null;

    private Vector3 m_vSpaceshipTarget = Vector3.zero;

    #region MonoBehavior
    private void Awake()
	{
		m_cCenterTarget = new CinemachineTargetGroup.Target
		{
			target = m_cCameraCenter,
			weight = m_fCenterWeight,
			radius = 0
		};

        CinemachineComponentBase[] cinemachineComponent = m_cCinemachineVirtualCamera.GetComponentPipeline();
        foreach (CinemachineComponentBase component in cinemachineComponent)
        {
            if (component is CinemachineFramingTransposer)
            {
                m_cFramingTransposer = ((CinemachineFramingTransposer)component);
            }
        }

        m_qEndAngleCameraScoringScreen = Quaternion.Euler(m_vAngleCameraScoringScreen);
    }

    private void FixedUpdate()
	{
        if (m_bIsRotateToFocusSpaceship)
        {
            m_fTimeCountRotation += Time.deltaTime * m_fSpeedMoveCamera;
            m_cCinemachineVirtualCamera.transform.rotation = Quaternion.Slerp(m_qStartAngleCameraScoringScreen, m_qEndAngleCameraScoringScreen, m_fTimeCountRotation);
        }

        if (m_bIsTargetPlayer)
        {
            m_lcTargets.Clear();
            m_lcTargets.Add(m_cCenterTarget);

            foreach (Player player in m_acPlayers)
            {
                if (!player.IsFalling)
                    m_lcTargets.Add(new CinemachineTargetGroup.Target
                    {
                        target = player.transform,
                        weight = 1,
                        radius = 0
                    });
            }

            m_cCinemachineTargetGroup.m_Targets = m_lcTargets.ToArray();
        }
	}
    #endregion

    public void SetSpaceshipTarget(Transform spaceshipTransform)
    {
        m_bIsTargetPlayer = false;
        CinemachineTargetGroup.Target target = new CinemachineTargetGroup.Target
        {
            target = spaceshipTransform,
            weight = m_fCenterWeight,
            radius = 0
        };
        m_lcTargets.Clear();
        m_lcTargets.Add(target);
        m_cCinemachineTargetGroup.m_Targets = m_lcTargets.ToArray();
    }

    public void SetCameraScoringScreen(Transform spaceshipTransform)
    {
        m_bIsRotateToFocusSpaceship = true;
        m_qStartAngleCameraScoringScreen = m_cCinemachineVirtualCamera.transform.localRotation;
        m_vSpaceshipTarget = spaceshipTransform.position;

        m_cFramingTransposer.m_XDamping = m_vDampingCameraScoreScene.x;
        m_cFramingTransposer.m_YDamping = m_vDampingCameraScoreScene.y;
        m_cFramingTransposer.m_ZDamping = m_vDampingCameraScoreScene.z;
    }

    public void Clear()
    {
        m_bIsTargetPlayer = false;
        m_lcTargets.Clear();
        m_lcTargets.Add(m_cCenterTarget);
        m_cCinemachineTargetGroup.m_Targets = m_lcTargets.ToArray();
    }



    
}
