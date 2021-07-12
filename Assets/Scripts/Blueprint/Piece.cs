using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Piece : Blueprints
{
	[Header("Piece")]
	[SerializeField]
	private GameObject m_cProjector = null;

	private Vector3 m_vBlueprintLocalPosition;

    [Header("Shader transparency")]
    [SerializeField, Range(0f, 1f)]
    private float m_fFillRate = 0f;

    private PieceRecipeComponent[] m_lcPieceRecipeComponent;
	public PieceRecipeComponent[] PieceRecipeComponents
	{
		get { return m_lcPieceRecipeComponent; }
	}

    private int m_iIndexPieceRecipeComponent = 0;
    public int IndexPieceRecipeComponent
    {
        get { return m_iIndexPieceRecipeComponent; }
    }

	// feedback wrong resource
	private Color m_cMainColorFeedback = new Color(1f, 0f, 0f, 1f);
	private Color m_cSecondaryColorFeedback = new Color(0.97f, 0.38f, 0.38f, 1f);
	private float m_fFeedbackDuration = 1f;
	private float m_fFeedbackTimer = 0f;
	private float m_fFeedbackInterval = 0.08f;
	private bool m_bFeedbackActive = false;

    [Header("FX")]
    [SerializeField]
    private ParticleSystem m_cBuildFX = null;
    [SerializeField]
    private ParticleSystem m_cBuildDoneFX = null;
	[SerializeField]
	private float m_fBuildDoneFXOffset = 0.5f;
	[SerializeField]
	private ParticleToUI m_cParticleTrailToUI = null;

    [SerializeField]
    private float m_fFeedBackDurationFX = 1.0f;

	[Header("Sounds")]
	[SerializeField]
	private StudioEventEmitter m_cResourceAdded = null;
    [SerializeField]
    private StudioEventEmitter m_cWrongResource = null;

	private Transform m_cCameraTransform = null;

    private Sprite[] m_acUISprites = new Sprite[4];
	public Sprite[] UISprites
	{
		get { return m_acUISprites; }
	}

	private PieceManager.E_PIECE_COLOR m_ePieceColor;
	public PieceManager.E_PIECE_COLOR PieceColor
	{
		get { return m_ePieceColor; }
	}

	private bool m_bPlaceBlueprint = false;

	public event Action<Piece> OnPiecePlaced;
	public event Action<int> OnResourceAdded;
	public event Action<Piece> OnPieceCompleted;

	private int m_iLayerMaskPlatformBorder;


	#region MonoBehavior
	protected override void Awake() 
    {
        base.Awake();

		m_eObjectType = E_OBJECT_TYPE.Piece;
		m_eInteractEnum = E_INTERACT.Idle;

        m_cRigidbody = GetComponent<Rigidbody>();
		gameObject.layer = m_iLayerNoCollision;

        SetMaterial(m_cHologramMaterial);

		m_vBlueprintLocalPosition = m_cMeshRenderer[0].transform.localPosition;

		m_iLayerMaskPlatformBorder = 1 << m_iLayerPlatformBorder;
	}

	protected void Start()
	{
		m_cCameraTransform = GameObject.FindGameObjectWithTag("GameCamera").transform;
	}

	protected void Update()
	{
		if (m_bFeedbackActive)
		{
			m_fFeedbackTimer += Time.deltaTime;

			if (m_fFeedbackTimer >= m_fFeedbackDuration)
			{
				m_bFeedbackActive = false;
			}
		}

		if (m_cGameObject.layer != m_iLayerNoCollision && m_cRigidbody.IsSleeping())
			m_cRigidbody.WakeUp();
	}

	protected override void OnCollisionEnter(Collision collision)
	{
		base.OnCollisionEnter(collision);

		if (m_bPlaceBlueprint && m_bIsOnGroud)
		{
			ContactPoint[] contactPoints = collision.contacts;

			foreach (ContactPoint contactPoint in contactPoints)
			{
				if (contactPoint.normal == m_cTransform.up)
				{
					m_bPlaceBlueprint = false;

					if (OnPiecePlaced != null)
						OnPiecePlaced(this);

					return;
				}
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		OnPiecePlaced = null;
		OnResourceAdded = null;
		OnPieceCompleted = null;
	}
	#endregion

	public void Init(Sprite[] sprites, int weight, float slopeCoeff, PieceManager.E_PIECE_COLOR pieceColor, Material blueprintMat)
	{
		m_acUISprites = sprites;
		m_iCurrWeight = weight;
		m_fSlopeCoeff = slopeCoeff;
		m_ePieceColor = pieceColor;
		m_cHologramMaterial = blueprintMat;

		SetMaterial(m_cHologramMaterial);

		m_cMainColorHologram = m_cHologramMaterial.GetColor("_Color");
		m_cSecondaryColorHologram = m_cHologramMaterial.GetColor("_Fresnel_Color");

		m_cProjector.SetActive(true);
		m_cMeshRenderer[0].transform.localPosition = m_vBlueprintLocalPosition;
	}

	public override bool AddObject(GameObject resourceObject)
    {
		if (m_eInteractEnum == E_INTERACT.End)
			return false;

        Resource resource = resourceObject.GetComponent<Resource>();

		if (!IsResourceInTheRecipe(resource.m_iIndex))
		{
			StartCoroutine(FeedbackResourceNotInRecipe());
            m_cWrongResource.Play();
			return false;
		}

        base.AddObject(resourceObject);

		if (OnResourceAdded != null)
			OnResourceAdded(resource.m_iIndex);

        if (m_eInteractEnum != E_INTERACT.End)
		{
			m_cResourceAdded.Play();

            StartCoroutine(FeedBackAddResource());
		}
		else
        {
            BlueprintFinish();
            FeedBackPieceIsDone();
        }

		ResourceObjectManager.Instance.Destroy(resourceObject);
        return true;
    }

	public override void Drop(Player player)
	{
        bool prevPlayersModeState = TwoPlayersMode;

        base.Drop(player);

        if (!prevPlayersModeState)
        {
            if (m_eInteractEnum != E_INTERACT.End)
                gameObject.layer = m_iLayerBlueprintObject;
            else
                gameObject.layer = m_iLayerPiece;
        }
	}

	public bool Place(Player player)
    {
		Collider[] hitColliders = Physics.OverlapBox(m_cTransform.position, m_cBoxCollider.bounds.extents, Quaternion.identity, m_iLayerMaskPlatformBorder);

		if (hitColliders.Length == 0)
		{
			Drop(player);

			if (m_eInteractEnum != E_INTERACT.End)
			{
				m_eInteractEnum = E_INTERACT.Blueprint;

				m_bPlaceBlueprint = true;
			}

			return true;
		}

		return false;
    }

	public void SetParticleTrailDestination(Vector3 position, Color color)
	{
		m_cParticleTrailToUI.gameObject.SetActive(true);
		m_cParticleTrailToUI.Init(position, color);
	}

	public override void ReturnToPool()
	{
		base.ReturnToPool();

		m_eInteractEnum = E_INTERACT.Idle;
		m_fFillRate = 0f;

        SetMaterial(m_cHologramMaterial);

		m_cGameObject.layer = m_iLayerNoCollision;

		m_bPlaceBlueprint = false;

		OnPiecePlaced = null;
		OnResourceAdded = null;
		OnPieceCompleted = null;
	}

	public override void BlueprintFinish()
    {
		m_eMovement = E_MOVEMENT.Movable;
		m_bActivateSlope = true;

        gameObject.layer = m_iLayerPiece;

        SetMaterial(m_cShipMaterial);

        EnablePhysics();

		if (OnPieceCompleted != null)
			OnPieceCompleted(this);

		OnPiecePlaced = null;
		OnResourceAdded = null;
		OnPieceCompleted = null;
    }

    public void SetPieceRecoverFromTheMachine()
    {
		m_eMovement = E_MOVEMENT.Fixed;
		m_bActivateSlope = false;

        m_cTransform.localPosition = Vector3.zero;
        m_cTransform.localRotation = Quaternion.identity;
        m_cTransform.localScale = new Vector3(1f, 1f, 1f);

        gameObject.layer = m_iLayerNoCollision;

        DisablePhysics();
    }

    public void SetPhysicsPickPiece()
    {
        m_cTransform.localPosition = Vector3.zero;
        m_cTransform.localRotation = Quaternion.identity;
        m_cTransform.localScale = new Vector3(1f, 1f, 1f);

		gameObject.layer = m_iLayerLiftedObject;

        DisablePhysics();
    }

    #region Recipe
    public void SetRecipe(List<PieceRecipeComponent> listPieceRecipe, int index)
    {
		m_liNumberPiece.Clear();
		m_iMeshDrawCount = 0;
		m_iMeshDrawIndex = 0;

		m_lcPieceRecipeComponent = listPieceRecipe.ToArray();
        m_iIndexPieceRecipeComponent = index;

        for (int i = 0; i < m_lcPieceRecipeComponent.Length; i++)
        {
            m_liNumberPiece.Add(m_lcPieceRecipeComponent[i].m_iNumber);
            m_iMeshDrawCount += m_lcPieceRecipeComponent[i].m_iNumber;
        }
    }

    private bool IsResourceInTheRecipe(int index)
    {
        for (int i = 0; i < m_lcPieceRecipeComponent.Length; i++)
        {
            if (m_liNumberPiece[i] > 0)
                if (m_lcPieceRecipeComponent[i].m_cResourceSettings.m_iIndex == index)
                {
                    m_liNumberPiece[i]--;
                    return true;
                }
        }
        return false;
    }

	private IEnumerator FeedbackResourceNotInRecipe()
	{
		m_fFeedbackTimer = 0f;
		m_bFeedbackActive = true;

		while (m_bFeedbackActive)
		{
			m_cMeshRenderer[0].material.SetColor("_Color", m_cMainColorFeedback);
			m_cMeshRenderer[0].material.SetColor("_Fresnel_Color", m_cSecondaryColorFeedback);

			yield return new WaitForSeconds(m_fFeedbackInterval);

			m_cMeshRenderer[0].material.SetColor("_Color", m_cMainColorHologram);
			m_cMeshRenderer[0].material.SetColor("_Fresnel_Color", m_cSecondaryColorHologram);

			yield return new WaitForSeconds(m_fFeedbackInterval);
		}

		m_cMeshRenderer[0].material.SetColor("_Color", m_cMainColorHologram);
		m_cMeshRenderer[0].material.SetColor("_Fresnel_Color", m_cSecondaryColorHologram);
	}

    private IEnumerator FeedBackAddResource()
    {
        m_cBuildFX.Play(true);

        float timer = m_fFeedBackDurationFX;
        while (timer > 0.0f)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        m_cBuildFX.Stop(true);
    }

    private void FeedBackPieceIsDone()
    {
		Vector3 direction = (m_cCameraTransform.position - transform.position).normalized;

		m_cBuildDoneFX.transform.position = m_cTransform.position + direction * m_fBuildDoneFXOffset;

		m_cBuildDoneFX.Play(true);

		m_cProjector.SetActive(false);

		MeshRenderer blueprintMesh = m_cMeshRenderer[0];
		m_cTransform.position = blueprintMesh.transform.position;
		blueprintMesh.transform.localPosition = Vector3.zero;
    }
	#endregion

	#region NearestFeedback
	public override void SelectNearest(int playerIndex, MovableObject objectInHand)
	{
		base.SelectNearest(playerIndex, objectInHand);
	}
	#endregion

	#region Debug
	public void SetPieceDebugMovement()
    {
        m_eInteractEnum = E_INTERACT.End;
    }
    #endregion
}
