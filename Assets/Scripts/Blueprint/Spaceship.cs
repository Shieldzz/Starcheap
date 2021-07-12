using UnityEngine;
using System.Collections.Generic;
using FMODUnity;
using System;


public class Spaceship : Blueprints
{
    public event Action OnWinEvent = null;
    public event Action OnCompleteSpaceshipEvent = null;
    public event Action OnLaunchSpaceship = null;
    public event Action OnFinishFlying = null;
    public event Action OnFinishTakeoff = null;
    public event Action OnLaunchPlatform = null;

    [SerializeField]
    private List<int> m_liIndexMesh = new List<int>();

    [SerializeField]
	private SpaceshipSettings m_cSpaceShipSettings = null;

	[SerializeField]
	private GameObject m_cFinishIndicator = null;

	[Header("Sounds")]
	[SerializeField]
	private StudioEventEmitter m_cPieceAdded = null;
	[SerializeField]
	private StudioEventEmitter m_cSpaceshipFinished = null;
    [SerializeField]
    private StudioEventEmitter m_cStartFlyingSound = null;
    [SerializeField]
    private StudioEventEmitter m_cFlyingSound = null;

    private bool m_bSpaceshipFinish = false;
    public bool SpaceshipFinish
    {
        get { return m_bSpaceshipFinish; }
    }

    private int m_iPlayerInSpaceship = 0;
	public int PlayerInSpaceship
	{
		get { return m_iPlayerInSpaceship; }
	}

    private FollowSplines m_cFollowSpline = null;
    public FollowSplines FollowSpline
    { get { return m_cFollowSpline; } }

    [Header("Animations")]
    [SerializeField]
    private Animator m_cAnimator = null;
	[SerializeField]
	private Animator m_cAnimatorScoring = null;

	[Header("FX")]
    [SerializeField]
    private ParticleSystem m_cSmokeFX = null;
    [SerializeField]
    private ParticleSystem m_cSmokeTakeoffFX = null;

    [Header("Zoom Scoring")]
    [SerializeField]
    private float m_fSizeZoom = 1.7f;
    [SerializeField]
    private float m_fSpeedZoom = 1.0f;
    private bool m_bIsZooming = false;
    private Vector3 m_vScaleZoom = Vector3.zero;

    #region MonoBehavior
    protected override void Awake()
	{
        base.Awake();
        m_cFollowSpline = GetComponent<FollowSplines>();
        m_iMeshDrawCount = m_cMeshRenderer.Count;

        SetMaterial(m_cHologramMaterial);

		m_cFinishIndicator.SetActive(false);

		List<SpaceshipeRecipeComponent> spaceshipRecipeList  = m_cSpaceShipSettings.m_lcSpaceshipRecipeCompoment;

        for (int i = 0; i < spaceshipRecipeList.Count; i++)
            m_liNumberPiece.Add(spaceshipRecipeList[i].m_iNumber);

		m_vScaleZoom = new Vector3(m_fSizeZoom, m_fSizeZoom, m_fSizeZoom);

		if (m_cAnimatorScoring != null)
			m_cAnimatorScoring.enabled = false;
    }

    private void Update()
    {
        if (m_bIsZooming)
        {
            m_cTransform.localScale += m_vScaleZoom * Time.deltaTime * m_fSpeedZoom;

            if (m_cTransform.localScale.x >= m_fSizeZoom)
                m_bIsZooming = false;
        }
    }
    #endregion

    public override bool AddObject(GameObject pieceObject)
    {
        Piece piece = pieceObject.GetComponent<Piece>();
        if (piece == null || piece.InteractEnum != E_INTERACT.End)
            return false;

        int index = piece.IndexPieceRecipeComponent;

        if (!IsPieceInTheRecipe(index))
            return false;

        base.AddObject(pieceObject);

		if (m_eInteractEnum == E_INTERACT.End)
			BlueprintFinish();
		else
			m_cPieceAdded.Play();

        for (int idx = 0; idx < m_liIndexMesh.Count; idx++)
        {
            int meshIndex = m_liIndexMesh[idx];
            if (meshIndex == index)
            {
                SwitchMaterial(m_cMeshRenderer[idx], m_cShipMaterial);
                m_liIndexMesh.RemoveAt(idx);
                m_cMeshRenderer.RemoveAt(idx);
                break;
            }
        }

        PieceManager.Instance.DespawnObject(pieceObject, false);
        return true;
    }

    public override void BlueprintFinish()
    {
        m_bSpaceshipFinish = true;
	    m_cSpaceshipFinished.Play();
        m_cSmokeFX.Play(true);
        m_cGameObject.layer = m_iLayerSpaceshipFinish;

		m_cFinishIndicator.SetActive(true);

		if (OnCompleteSpaceshipEvent != null)
            OnCompleteSpaceshipEvent();
    }

    private bool IsPieceInTheRecipe(int index)
    {
        for (int i = 0; i < m_cSpaceShipSettings.m_lcSpaceshipRecipeCompoment.Count; i++)
        {
			if (m_liNumberPiece[i] > 0)
			{
				if (m_cSpaceShipSettings.m_lcSpaceshipRecipeCompoment[i].m_cPieceSettings.m_iIndex == index)
				{
					m_liNumberPiece[i]--;
					return true;
				}
			}
        }
        return false;
    }

    public void AddPlayerInSpaceship()
    {
        m_iPlayerInSpaceship++;

		if (m_iPlayerInSpaceship == 4)
		{
			m_cFinishIndicator.SetActive(false);

			if (OnLaunchSpaceship != null)
				OnLaunchSpaceship();
		}

        if (OnWinEvent != null && m_iPlayerInSpaceship == 1)
            OnWinEvent();
    }

    public void LaunchSpaceship()
    {
        m_cTransform.parent = null;

        m_cAnimator.enabled = true;
        m_cAnimator.SetInteger("IsTakeoff", 1);
        m_cSmokeTakeoffFX.Play(true);
        m_cStartFlyingSound.Play();

		m_cFinishIndicator.SetActive(false);
    }

    public void PlaySmokeTakeoffFX()
    {
        //m_cSmokeFX.Play(true);
        if (OnLaunchPlatform != null)
            OnLaunchPlatform();
    }

	public void PlayScoringAnim()
	{
		m_cAnimatorScoring.enabled = true;
		m_cAnimatorScoring.Play("Ship_Fly");
	}

    private void StartFlying()
    {
        m_cAnimator.enabled = false;
        m_cFlyingSound.Play();
        m_cSmokeFX.Stop(true);

        m_cFollowSpline.StartMoving();
    }

    public void StartZooming()
    {
        m_bIsZooming = true;
    }
}
