using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player : WeightedObject
{
	[Header("Move")]
	[SerializeField]
	private float m_fSpeed = 8.0f;
	[SerializeField]
	private float m_fSpeedRotation = 0.15f;
	[SerializeField]
	private float m_fDistance = 1.0f;
	[SerializeField]
	private float m_fDistanceStopMoving = 0.1f;
	[SerializeField]
	private float m_fRunSpeed = 0.2f;
	[SerializeField]
	private float m_fMaxRunSpeed = 4f;
	[SerializeField]
	private float m_fRunFallOff = 0.01f;
	private float m_fCurrentRunSpeed = 0f;

	private bool m_bJumping = false;
	private bool m_bCharacterSelected = false;
	public bool CharacterSelected
	{
		get { return m_bCharacterSelected; }
	}

	private float m_fRadiusSphereCast = 0.2f;
	private float m_fHeightCapsuleHalf = 1f;
    public float HeightCapsuleHalf
    {
        get { return m_fHeightCapsuleHalf; }
    }
	private Vector3 m_vCapsuleHalf = Vector3.zero;

	public float MoveSpeed
	{
		get
		{
			return (m_fCurrentRunSpeed > 0f) ? m_fSpeed + Mathf.Min(m_fCurrentRunSpeed, m_fMaxRunSpeed) : m_fSpeed;
		}
	}

	private Vector3 m_vMoveDirection = Vector3.zero;
	public Vector3 MoveDirection
	{
		get { return m_vMoveDirection; }
	}

	private bool m_bMoving = false;
	private bool m_bRunning
	{
		get { return m_fCurrentRunSpeed > 1f; }
	}

	private CapsuleCollider m_cPlayerCollider = null;

	[Header("Interactions")]
	//[SerializeField]
	//private SphereCollider m_cInteractionCollider = null;
	[SerializeField, Range(-1f, 1f)]
	private float m_fInteractionAngle = 0f;
	[SerializeField]
	private int m_iHeavyObjectWeight = 4;
	[SerializeField]
	private string m_sArmsBonePath = "";
	private Transform m_cArms;
	public Transform Arms
	{
		get { return m_cArms; }
	}

	private List<GameObject> m_lcInteractableObjects = new List<GameObject>();

	private GameObject m_cNearestInteractableObject = null;

	private bool m_bCanInteractWithObject = false;
	public bool CanInteractWithObject
	{
		get { return m_bCanInteractWithObject; }
	}

	private MovableObject m_cObjectTaken = null;
	public bool HasObjectInHand
	{
		get { return m_cObjectTaken != null; }
	}

    private bool m_bIsInSpaceshipFinish = false;

	[Header("Sounds")]
	//[SerializeField]
	//private StudioEventEmitter m_cCharacterFallSound;
	[SerializeField]
	private StudioEventEmitter m_cCharacterRespawnSound = null;
	[SerializeField]
	private StudioEventEmitter m_cTakeObjectSound = null;
	[SerializeField]
	private StudioEventEmitter m_cDropObjectSound = null;

	public event Action<int> OnDeath = null;
	public event Action OnRespawn = null;
	public event Action<int> OnResourceAddedInPiece = null;
	public event Action<int> OnPieceCompleted = null;
	public event Action<int> OnPieceAddedInSpaceship = null;
	public event Action<int> OnEnterSpaceship = null;
	public event Action<int> OnInteractSuccessful = null;

	public int m_iIndex = 0;

	private bool m_bIsDead = false;
    public bool IsDead
    {
        get { return m_bIsDead; }
    }
	private bool m_bIsFalling = false;
	public bool IsFalling
	{
		get { return m_bIsFalling; }
	}

	private Animator m_cPlayerAnimator = null;

	private Transform m_cPlatformTransform = null;

	// layers
	private int m_iLayerResourceObject;
	private int m_iLayerBlueprint;
	private int m_iLayerSpaceship;
	private int m_iLayerPiece;
	private int m_iLayerPieceMachine;
	private int m_iLayerNoCollision;
	private int m_iLayerLiftedObject;
	private int m_iLayerCharacterSelector;
    private int m_iLayerRespawn;
    private int m_iLayerSpaceshipFinish;

	// masks
	private int m_iLayerMaskPlayerMove;
//	private int m_iLayerMaskPlatform;
	private int m_iLayerMaskInteraction;
	private int m_iLayerMaskPlatformBorder;
    private int m_iLayerMaskLand;
	private int m_iLayerMaskMovableObject;

	[Header("UI")]
	[SerializeField]
	private PlayerUI m_cPlayerUI = null;
	public PlayerUI PlayerUI
	{
		get { return m_cPlayerUI; }
	}

	[Header("Model")]
	public PlayerModel m_cPlayerModel = null;

	[SerializeField]
	private CharacterSelection.E_CHARACTER m_eBindedCharacter = CharacterSelection.E_CHARACTER.Raccoon;
	public CharacterSelection.E_CHARACTER BindedCharacter
	{
		get { return m_eBindedCharacter; }
	}

	[Header("FX")]
    [SerializeField]
    private ParticleSystem m_cFXFootStep = null;
    private bool m_bIsFootStep = false;
	[SerializeField]
	private ParticleSystem m_cFXScoreGained = null;
	[SerializeField]
	private ParticleSystem m_cFXSweat = null;

	private Vector3 m_vCastDirection = Vector3.zero;

    private bool m_bIsPlayerDisable = false;

    public Respawn m_cRespawn = null;
    private bool m_bIsRespawn = false;

	private int m_iFallingState = 0;


	#region MonoBehavior
	protected new void Awake()
	{
		base.Awake();

		m_cTransform = GetComponent<Transform>();
        m_cRespawn = GetComponent<Respawn>();

		m_iLayerResourceObject = LayerMask.NameToLayer("ResourceObject");
		m_iLayerBlueprint = LayerMask.NameToLayer("BlueprintObject");
		m_iLayerSpaceship = LayerMask.NameToLayer("Spaceship");
		m_iLayerPiece = LayerMask.NameToLayer("Piece");
		m_iLayerPieceMachine = LayerMask.NameToLayer("PieceMachine");
		m_iLayerNoCollision = LayerMask.NameToLayer("NoCollision");
		m_iLayerLiftedObject = LayerMask.NameToLayer("LiftedObject");
		m_iLayerCharacterSelector = LayerMask.NameToLayer("CharacterSelector");
        m_iLayerFalling = LayerMask.NameToLayer("Falling");
        m_iLayerRespawn = LayerMask.NameToLayer("Respawn");
        m_iLayerSpaceshipFinish = LayerMask.NameToLayer("SpaceshipFinish");

        m_iLayerMaskPlayerMove = (1 << m_iLayerResourceObject) | (1 << m_iLayerPiece);
        m_iLayerMaskLand = (1 << m_iLayerResourceObject) | (1 << m_iLayerPiece) | (1 << m_iLayerPlatform) | (1 << m_iLayerDefault);
		m_iLayerMaskInteraction = (1 << m_iLayerResourceObject) | (1 << m_iLayerBlueprint) | (1 << m_iLayerSpaceship) | (1 << m_iLayerPieceMachine) | (1 << m_iLayerPiece) | (1 << m_iLayerCharacterSelector) | (1 << m_iLayerLiftedObject);
		m_iLayerMaskPlatformBorder = 1 << m_iLayerPlatformBorder;
		m_iLayerMaskMovableObject = (1 << m_iLayerResourceObject) | (1 << m_iLayerLiftedObject) | (1 << m_iLayerBlueprint) | (1 << m_iLayerPiece);

		if (m_cRespawn)
            m_cRespawn.OnFinishRespawn += Respawn;
	}

	private void Start()
	{
		m_cPlayerAnimator = GetComponentInChildren<Animator>();

		m_cPlayerCollider = GetComponentInChildren<CapsuleCollider>();
		m_fRadiusSphereCast = m_cPlayerCollider.radius * m_cPlayerModel.Transform.lossyScale.x;
		m_fHeightCapsuleHalf = m_cPlayerCollider.height * m_cPlayerModel.Transform.lossyScale.y / 2f;
		m_vCapsuleHalf = new Vector3(0f, m_fHeightCapsuleHalf, 0f);

		m_cPlatformTransform = GameManager.Instance.PlatformTransform;

		m_cArms =  m_cPlayerModel.Transform.Find(m_sArmsBonePath);
	}

	private void Update()
	{
		m_fCurrentRunSpeed -= m_fRunFallOff * m_fCurrentRunSpeed;

        if (!m_bIsPlayerDisable && m_bIsDead && GameManager.Instance.IsGameFinish)
            DisablePlayer();
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();

		GameObject newNearest = GetNearestInteractableObject();

		if (m_cNearestInteractableObject != null && newNearest != m_cNearestInteractableObject && ((1 << m_cNearestInteractableObject.layer) & m_iLayerMaskMovableObject) != 0)
			m_cNearestInteractableObject.GetComponent<MovableObject>().UnselectNearest(m_iIndex);

		if (newNearest != null && ((1 << newNearest.layer) & m_iLayerMaskMovableObject) != 0)
			newNearest.GetComponent<MovableObject>().SelectNearest(m_iIndex, m_cObjectTaken);

		m_bCanInteractWithObject = newNearest != null;

		m_cNearestInteractableObject = newNearest;

		if (m_bMoving)
			ExecuteMove();

		Facing();

		if (m_cFXFootStep != null && m_bIsFootStep && !m_bRunning)
		{
			m_bIsFootStep = false;
			m_cFXFootStep.Stop(true);
			m_cFXSweat.Stop(true);
		}
	}

    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);

        int otherLayer = collision.gameObject.layer;
        if (m_bIsRespawn && m_bIsFalling && (((1 << otherLayer) & m_iLayerMaskLand) != 0))
        {
			m_iFallingState = 0;
			m_cPlayerModel.Animator.Play("Land");
            m_bIsFalling = false;
            m_bIsRespawn = false;
        }
    }

    protected override void OnCollisionExit(Collision collision)
	{
		base.OnCollisionExit(collision);
	}

	protected override void OnTriggerEnter(Collider collider)
	{
		GameObject otherGameObject = collider.gameObject;

		if (((1 << otherGameObject.layer) & m_iLayerMaskInteraction) != 0)
		{
			m_lcInteractableObjects.Add(otherGameObject);

			if (((1 << otherGameObject.layer) & m_iLayerMaskMovableObject) != 0)
				otherGameObject.GetComponent<MovableObject>().OnReturnToPool += OnInteractableDestroyed;
		}

        if (otherGameObject.layer == m_iLayerFalling && m_iFallingState == 0)
        {
            if (!m_bIsDead)
            {
				m_iFallingState = 1;
                m_bIsFalling = true;
                m_cPlayerModel.Fall();
                DisableVelocity();

                if (m_cObjectTaken != null)
                    Drop(m_cObjectTaken.ObjectType == MovableObject.E_OBJECT_TYPE.Resource);
            }
            else
                Revive();
        }

        m_cPlayerModel.UpdateMoveSound(otherGameObject.layer);
	}

	protected void OnTriggerExit(Collider collider)
	{
		GameObject otherGameObject = collider.gameObject;

		if (((1 << otherGameObject.layer) & m_iLayerMaskInteraction) != 0 || otherGameObject.layer == m_iLayerNoCollision)
			m_lcInteractableObjects.Remove(otherGameObject);

        m_cPlayerModel.LaunchSoundGrid(otherGameObject.layer);

        if (m_bIsInSpaceshipFinish && otherGameObject.layer == m_iLayerSpaceshipFinish)
            m_bIsInSpaceshipFinish = false;
	}

    private void OnTriggerStay(Collider collider)
    {
        if (!m_bIsInSpaceshipFinish)
        {
            int gameObjectLayer = collider.gameObject.layer;
            if (gameObjectLayer == m_iLayerSpaceshipFinish)
                m_bIsInSpaceshipFinish = true;       
        }
    }

    protected void OnDestroy()
	{
		OnDeath = null;
		OnRespawn = null;
		OnResourceAddedInPiece = null;
		OnPieceCompleted = null;
		OnPieceAddedInSpaceship = null;
        OnEnterSpaceship = null;
		OnInteractSuccessful = null;
    }
	#endregion

	#region Movement
	public void Move(Vector3 direction)
	{
		m_vMoveDirection = direction;

		m_bMoving = (direction.x != 0 || direction.z != 0);
		m_cPlayerAnimator.SetBool("IsWalking", m_bMoving);
        if (!CharacterSelected)
            m_cPlayerModel.MoveSound(m_bMoving);
	}

	public void Run()
	{
		if (m_fCurrentRunSpeed < 0f)
			m_fCurrentRunSpeed = 0f;

        if (m_cFXFootStep != null && !m_bIsFootStep && m_bMoving)
        {
            m_bIsFootStep = true;
            m_cFXFootStep.Play(true);
			m_cFXSweat.Play(true);
        }

        m_fCurrentRunSpeed += m_fRunSpeed;
	}

	private void ExecuteMove()
	{
		if (!m_bJumping && !m_bCharacterSelected)
		{
			RaycastHit hitRay;

			m_vCastDirection = m_cPlatformTransform.rotation * m_vMoveDirection;

			Collider[] hitColliders = Physics.OverlapCapsule(m_cTransform.position + m_vCapsuleHalf, m_cTransform.position - m_vCapsuleHalf, m_fRadiusSphereCast, m_iLayerMaskPlatformBorder);

			bool overlapCollider = false;

			for (int colliderIdx = 0; colliderIdx < hitColliders.Length; ++colliderIdx)
			{
				if (Vector3.Dot(-hitColliders[colliderIdx].transform.right, m_vCastDirection) > 0f)
				{
					overlapCollider = true;
					continue;
				}
			}

			if (!Physics.SphereCast(m_cTransform.position, m_fRadiusSphereCast, m_vCastDirection, out hitRay, m_fDistanceStopMoving, m_iLayerMaskPlayerMove)
				&& (hitColliders.Length == 0 || !overlapCollider))
			{
				Vector3 dir = m_vMoveDirection;
				float speed = MoveSpeed;

				if (m_cObjectTaken != null && m_cObjectTaken.TwoPlayersMode)
				{
					dir = m_cObjectTaken.ConstraintDir;
					speed = m_cObjectTaken.ConstraintSpeed;

					float forwardDotConstraintDir = Vector3.Dot(m_cTransform.forward, dir);

					if (forwardDotConstraintDir > 0.5f)
						m_cPlayerAnimator.SetInteger("LiftDirection", 0);
					else if (forwardDotConstraintDir < -0.5f)
						m_cPlayerAnimator.SetInteger("LiftDirection", 2);
					else
					{
						float rightDotConstraintDir = Vector3.Dot(m_cTransform.right, dir);
						if (rightDotConstraintDir > 0)
							m_cPlayerAnimator.SetInteger("LiftDirection", 1);
						else
							m_cPlayerAnimator.SetInteger("LiftDirection", 3);
					}
				}

				m_cTransform.position += (dir * Time.fixedDeltaTime * speed / Mathf.Max(1f, m_iCurrWeight * 0.6f));
			}
			else
				m_cRigidbody.velocity = Vector3.zero;
		}
	}

	private IEnumerator ExecuteJump(CharacterSelector selector)
	{
		m_bJumping = true;

		yield return new WaitForSeconds(0.375f);

		m_cRigidbody.useGravity = false;

		Vector3 endPosition = selector.AntenaPosition.position;
		Vector3 direction = (endPosition - m_cPlayerModel.Transform.position).normalized;

		while (m_cPlayerModel.Transform.position.y < endPosition.y)
		{
			Vector3 position = m_cPlayerModel.Transform.position;
			position += direction * Time.deltaTime * 4f;
			m_cPlayerModel.Transform.position = position;
			yield return null;
		}

		selector.SwitchMaterial(false, m_cPlayerModel.AntenaBall[0].material);

		yield return new WaitForSeconds(2f);

		m_bCharacterSelected = true;
		m_bJumping = false;
	}

	private void Facing()
	{
		if (!m_bJumping)
		{
			Vector3 lookRotation = m_cTransform.forward;

			if (m_cObjectTaken != null && m_cObjectTaken.TwoPlayersMode)
			{
				lookRotation = m_cObjectTaken.GetDirectionBetweenPlayers(this);
				lookRotation.y = 0f;
			}
			else if (m_bMoving)
				lookRotation = m_vMoveDirection;

			m_cTransform.rotation = Quaternion.Slerp(m_cTransform.rotation, Quaternion.LookRotation(lookRotation), m_fSpeedRotation);
		}
	}

	public void SetTwoPlayersModeAnim(bool twoPlayerMode)
	{
		m_cPlayerAnimator.SetBool("IsLiftingSmall", !m_cObjectTaken.TwoPlayersMode && m_cObjectTaken.Weight < m_iHeavyObjectWeight);
		m_cPlayerAnimator.SetBool("IsLiftingHeavy", !m_cObjectTaken.TwoPlayersMode && m_cObjectTaken.Weight >= m_iHeavyObjectWeight);
		m_cPlayerAnimator.SetBool("TwoPlayersLift", m_cObjectTaken.TwoPlayersMode);
	}
	#endregion

	#region Interactions
	public void CancelCharacterSelection()
	{
		if (!m_bJumping && m_bCharacterSelected)
		{
			m_bCharacterSelected = false;

			m_cPlayerAnimator.SetBool("AntenaJump", false);
			m_cPlayerAnimator.Play("CancelCharacter");

			m_cPlayerModel.Transform.localPosition = Vector3.zero;

			m_cRigidbody.useGravity = true;

			GameObject nearestInteractableObject = GetNearestInteractableObject();

			if (nearestInteractableObject.gameObject.layer == m_iLayerCharacterSelector)
			{
				CharacterSelector characterSelector = nearestInteractableObject.GetComponent<CharacterSelector>();

				characterSelector.SwitchMaterial(true);
				characterSelector.Deselect(this);
			}

			StartCoroutine(CancelCharacterWaitTime());
		}
	}

	private IEnumerator CancelCharacterWaitTime()
	{
		m_bJumping = true;

		yield return new WaitForSeconds(1.5f);

		m_bJumping = false;
	}

	public void Interact()
	{
        if (m_bIsInSpaceshipFinish)
            InteractWithSpaceship(GameManager.Instance.Spaceship);

		if (m_cObjectTaken != null)
		{
			if (m_cObjectTaken.ObjectType == MovableObject.E_OBJECT_TYPE.Resource)
			{
				PlaceResource(m_cNearestInteractableObject);
				return;
			}
			else if (m_cObjectTaken.ObjectType == MovableObject.E_OBJECT_TYPE.Piece)
			{
				PlacePiece(m_cNearestInteractableObject);
				return;
			}
		}

		if (m_cNearestInteractableObject != null)
			InteractWith(m_cNearestInteractableObject);
	}

	private void InteractWith(GameObject currentObject)
	{
		int objectLayer = currentObject.layer;

		if (objectLayer == m_iLayerResourceObject)
			InteractWithResourceObject(currentObject.GetComponent<MovableObject>());
		else if (objectLayer == m_iLayerPiece)
			InteractWithPiece(currentObject.GetComponent<Piece>());
		else if (objectLayer == m_iLayerLiftedObject)
		{
			Piece pieceObject = currentObject.GetComponent<Piece>();
			if (pieceObject != null)
				InteractWithPiece(pieceObject);
			else
				InteractWithResourceObject(currentObject.GetComponent<MovableObject>());
		}
		else if (objectLayer == m_iLayerPieceMachine)
			InteractWithPieceMachine(currentObject.GetComponent<PieceMachine>());
		else if (objectLayer == m_iLayerCharacterSelector)
			InteractWithCharacterSelector(currentObject.GetComponent<CharacterSelector>());

		m_cTakeObjectSound.Play();

		RaiseInteractSuccessfulEvent();
	}

	private void InteractWithResourceObject(MovableObject resourceObject)
	{
		m_cObjectTaken = resourceObject;
		m_cObjectTaken.OnPlaced += ObjectTakenPlacedByOtherPlayer;
		m_cObjectTaken.OnJointBreak += TwoPlayersJointBreak;

		m_lcInteractableObjects.Remove(resourceObject.GameObject);

		m_cObjectTaken.Take(this);

		Physics.IgnoreCollision(m_cPlayerCollider, m_cObjectTaken.BoxCollider, true);

		m_cPlayerAnimator.SetBool("IsLiftingSmall", !m_cObjectTaken.TwoPlayersMode && m_cObjectTaken.Weight < m_iHeavyObjectWeight);
		m_cPlayerAnimator.SetBool("IsLiftingHeavy", !m_cObjectTaken.TwoPlayersMode && m_cObjectTaken.Weight >= m_iHeavyObjectWeight);
		m_cPlayerAnimator.SetBool("TwoPlayersLift", m_cObjectTaken.TwoPlayersMode);
	}
	private void InteractWithPiece(Piece piece)
	{
		m_cObjectTaken = piece;
		m_cObjectTaken.OnPlaced += ObjectTakenPlacedByOtherPlayer;
		m_cObjectTaken.OnJointBreak += TwoPlayersJointBreak;

		m_lcInteractableObjects.Remove(m_cObjectTaken.GameObject);

		m_cObjectTaken.Take(this);

		Physics.IgnoreCollision(m_cPlayerCollider, m_cObjectTaken.BoxCollider, true);

		m_cPlayerAnimator.SetBool("IsLiftingSmall", false);
		m_cPlayerAnimator.SetBool("IsLiftingHeavy", !m_cObjectTaken.TwoPlayersMode);
		m_cPlayerAnimator.SetBool("TwoPlayersLift", m_cObjectTaken.TwoPlayersMode);
	}
	private void InteractWithPieceMachine(PieceMachine pieceMachine)
	{
		Piece piece = pieceMachine.GetPieceInMachine();

		if (piece != null)
		{
			m_cObjectTaken = piece;
			m_cObjectTaken.Transform.parent = m_cArms;
			piece.SetPieceRecoverFromTheMachine();

			m_cPlayerAnimator.SetBool("IsLiftingSmall", true);
		}
	}
	private void InteractWithSpaceship(Spaceship spaceship)
	{
        if (spaceship.SpaceshipFinish)
        {
			m_bJumping = true;

            m_cTransform.LookAt(spaceship.Transform);
            m_cPlayerAnimator.Play("JumpShip");
		    spaceship.AddPlayerInSpaceship();
            if (OnEnterSpaceship != null)
                OnEnterSpaceship(m_iIndex);
        }
	}
	private void InteractWithCharacterSelector(CharacterSelector characterSelector)
	{
		if (characterSelector.Select(m_iIndex))
		{
			m_cPlayerAnimator.SetBool("AntenaJump", true);

			StartCoroutine(ExecuteJump(characterSelector));
		}
	}

	private void PlaceResource(GameObject nearestInteractableObject)
	{
		if (nearestInteractableObject != null)
		{
			Blueprints blueprint = nearestInteractableObject.GetComponent<Blueprints>();

			if (blueprint != null)
			{
				bool isResourceAdded = blueprint.AddObject(m_cObjectTaken.GameObject);
				if (isResourceAdded)
				{
					m_cPlayerAnimator.SetBool("IsLiftingSmall", false);
					m_cPlayerAnimator.SetBool("IsLiftingHeavy", false);
					m_cPlayerAnimator.SetBool("TwoPlayersLift", false);

					m_cObjectTaken.Place();

					m_cFXScoreGained.Play(true);

					if (OnResourceAddedInPiece != null)
						OnResourceAddedInPiece(m_iIndex);

					if (OnPieceCompleted != null && blueprint.InteractEnum == Blueprints.E_INTERACT.End)
						OnPieceCompleted(m_iIndex);

					RaiseInteractSuccessfulEvent();
				}
			}
		}
		else
			Drop(true);
	}
	private void PlacePiece(GameObject nearestInteractableObject)
	{
		if (nearestInteractableObject != null)
		{
			Spaceship spaceship = nearestInteractableObject.GetComponent<Spaceship>();
			if (spaceship != null)
			{
				bool isPieceAdded = spaceship.AddObject(m_cObjectTaken.GameObject);
				if (isPieceAdded)
				{
					m_cPlayerAnimator.SetBool("IsLiftingSmall", false);
					m_cPlayerAnimator.SetBool("IsLiftingHeavy", false);
					m_cPlayerAnimator.SetBool("TwoPlayersLift", false);

					m_cObjectTaken.Place();

					m_cFXScoreGained.Play(true);

					if (OnPieceAddedInSpaceship != null)
						OnPieceAddedInSpaceship(m_iIndex);

					RaiseInteractSuccessfulEvent();
				}
			}
		}
		else
			Drop(false);
	}

	private void Drop(bool isResource)
	{
		Collider[] hitColliders = Physics.OverlapCapsule(m_cTransform.position + m_vCapsuleHalf, m_cTransform.position - m_vCapsuleHalf, m_fRadiusSphereCast, m_iLayerMaskInteraction);

		if (hitColliders.Length == 0 || (hitColliders.Length == 1 && hitColliders[0].gameObject == m_cObjectTaken.GameObject))
		{
			m_cObjectTaken.OnPlaced -= ObjectTakenPlacedByOtherPlayer;
			m_cObjectTaken.OnJointBreak -= TwoPlayersJointBreak;

			if (isResource)
			{
				if (!m_cObjectTaken.TwoPlayersMode || m_cObjectTaken.GameObject.layer != m_iLayerLiftedObject)
					m_lcInteractableObjects.Add(m_cObjectTaken.GameObject);

				Physics.IgnoreCollision(m_cPlayerCollider, m_cObjectTaken.BoxCollider, false);

				m_cObjectTaken.Drop(this);
				m_cObjectTaken = null;

				m_cDropObjectSound.Play();

				m_cPlayerAnimator.SetBool("IsLiftingSmall", false);
				m_cPlayerAnimator.SetBool("IsLiftingHeavy", false);
				m_cPlayerAnimator.SetBool("TwoPlayersLift", false);
			}
			else
			{
				if (m_cObjectTaken.GetComponent<Piece>().Place(this))
				{
					if (m_cObjectTaken.GameObject.layer == m_iLayerPiece)
					{
						m_lcInteractableObjects.Add(m_cObjectTaken.GameObject);

						Physics.IgnoreCollision(m_cPlayerCollider, m_cObjectTaken.BoxCollider, false);
					}

					m_cObjectTaken = null;

					RaiseInteractSuccessfulEvent();

					m_cDropObjectSound.Play();

					m_cPlayerAnimator.SetBool("IsLiftingSmall", false);
					m_cPlayerAnimator.SetBool("IsLiftingHeavy", false);
					m_cPlayerAnimator.SetBool("TwoPlayersLift", false);
				}
			}
		}
	}

	private void ObjectTakenPlacedByOtherPlayer()
	{
		m_iCurrWeight = m_iWeight;

		m_cObjectTaken.OnJointBreak -= TwoPlayersJointBreak;
		m_cObjectTaken = null;

		m_cPlayerAnimator.SetBool("TwoPlayersLift", false);
	}

	private GameObject GetNearestInteractableObject()
	{
		float nearestDistance = float.MaxValue;
		GameObject nearestObject = null;

		for (int idx = 0; idx < m_lcInteractableObjects.Count; ++idx)
		{
            GameObject interactableObject = m_lcInteractableObjects[idx];
            if (m_cObjectTaken != null && interactableObject == m_cObjectTaken.GameObject)
            {
                m_lcInteractableObjects.Remove(interactableObject);
                --idx;
            }

			Transform interactableObjectTransform = interactableObject.transform;

			Vector3 interactableObjectPos = interactableObjectTransform.position;
			Vector3 dir = interactableObjectPos - m_cTransform.position;
			dir.y = 0f;
			dir.Normalize();

			if (Vector3.Dot(m_cTransform.forward, dir) >= m_fInteractionAngle)
			{
				float distance = Vector3.Distance(m_cTransform.position, interactableObjectPos);

				if (distance < nearestDistance)
				{
					nearestDistance = distance;
					nearestObject = interactableObject;
				}
			}
		}

		return nearestObject;
	}
	#endregion

	#region Physics
	public void ApplyWeightedObject(bool applyWeight)
	{
		if (m_cObjectTaken != null)
		{
			m_iCurrWeight += (applyWeight) ? m_cObjectTaken.Weight : -m_cObjectTaken.Weight;
		}
	}

	private void TwoPlayersJointBreak(Player brokeByPlayer)
	{
		if (brokeByPlayer == this)
			Drop(m_cObjectTaken.ObjectType == MovableObject.E_OBJECT_TYPE.Resource);
	}
	#endregion

	#region Death
	public void Die()
	{
		m_iFallingState = 0;
		m_bIsDead = true;
        m_cRigidbody.useGravity = false;
        m_cRigidbody.velocity = Vector3.zero;
        m_vMoveDirection = Vector3.zero;

        m_cGameObject.layer = m_iLayerRespawn;

        if (m_cObjectTaken != null)
		{
			m_cObjectTaken.Drop(this);
			m_cObjectTaken = null;
		}

        m_lcInteractableObjects.Clear();

		if (OnDeath != null)
			OnDeath(m_iIndex);

        if (m_cRespawn)
            m_cRespawn.StartCraneWillTakePlayer();
	}

	private void Respawn()
	{
        m_bIsRespawn = true;
        m_cPlayerModel.Animator.SetBool("IsFalling", true);
        m_cGameObject.layer = m_iLayerDefault;

        m_vMoveDirection = Vector3.zero;
        m_cRigidbody.useGravity = true;
		m_cRigidbody.velocity = Vector3.zero;
		m_cObjectTaken = null;

		m_iCurrWeight = m_iWeight;

		m_lcInteractableObjects.Clear();

		m_cCharacterRespawnSound.Play();
	}

    public void Revive()
    {
		m_iFallingState = 1;
        m_bIsDead = false;
		if (OnRespawn != null)
			OnRespawn();
    }
	#endregion

	public void PauseGame()
	{
		GameManager.Instance.Pause();
	}

	private void OnInteractableDestroyed(GameObject destroyedObject)
	{
		m_lcInteractableObjects.Remove(destroyedObject);
	}

	private void RaiseInteractSuccessfulEvent()
	{
		if (OnInteractSuccessful != null)
			OnInteractSuccessful(m_iIndex);
	}

    public void DisableEvent()
    {
        OnDeath = null;
        OnRespawn = null;
        OnResourceAddedInPiece = null;
        OnPieceCompleted = null;
        OnPieceAddedInSpaceship = null;
        OnEnterSpaceship = null;
        OnInteractSuccessful = null;
    }

    private void DisablePlayer()
    {
        m_cPlayerModel.DisableSound();
		m_cPlayerModel.SetActive(false);
        CancelInvoke();
        m_cRigidbody.useGravity = false;
        m_bIsPlayerDisable = true;

        if (m_cRespawn)
        {
            m_cRespawn.Disable();
            m_cRespawn.enabled = false;
        }
        enabled = false;
    }

    public void DisableVelocity()
    {
        m_vMoveDirection = Vector3.zero;
        m_cRigidbody.velocity = Vector3.zero;
    }

    //#region UI

    //public void ActiveUI()
    //{
    //    if (m_cPlayerUI.Active)
    //    {
    //        m_cPlayerUI.SetActive(false);
    //        DespawnObjectTaken();
    //    }
    //    else
    //    {
    //        if (m_cObjectTaken)
    //            Drop();

    //        if (m_cObjectTaken == null)
    //        {
    //            SpawnObjectTaken();
    //            m_cPlayerUI.SetActive(true);
    //        }
    //    }
    //}

    //public bool GetIsUIActive()
    //{
    //    return m_cPlayerUI.Active;
    //}

    //public void ScrollLeft()
    //{
    //    m_cPlayerUI.ScrollLeft();
    //    DespawnObjectTaken();
    //    SpawnObjectTaken();
    //}

    //public void ScrollRight()
    //{
    //    m_cPlayerUI.ScrollRight();
    //    DespawnObjectTaken();
    //    SpawnObjectTaken();
    //}
    //#endregion

    //private void DespawnObjectTaken()
    //{
    //    if (m_cPieceSelected)
    //        m_cPieceMgrInstance.DespawnObject(m_cPieceSelected.gameObject);
    //}

    //private void SpawnObjectTaken()
    //{
    //    m_cPieceSelected = m_cPieceMgrInstance.SpawnObject(m_cPlayerUI.CurrentPieceIndex, m_cArms.transform).GetComponent<MovableObject>();
    //    m_cPieceSelected.DisablePhysics();
    //    m_cPieceSelected.gameObject.layer = m_iLayerNoCollision;

    //}

    #region Gizmos
    private void OnDrawGizmosSelected()
	{
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + new Vector3(0,0.4f,0), new Vector3(0.4f, 0.8f, 0.4f));

		Gizmos.color = Color.blue;
		Gizmos.DrawLine(transform.position, transform.position + m_vCastDirection);
	}
	#endregion
}
