using System;
using System.Collections.Generic;
using UnityEngine;


public class MovableObject : WeightedObject
{
	public event Action OnPlaced = null;
	public event Action<Player> OnJointBreak = null;
	public event Action<GameObject> OnReturnToPool = null;

    protected enum E_MOVEMENT
    {
        Movable,
		Fixed
    };

    protected E_MOVEMENT m_eMovement = E_MOVEMENT.Movable;

	public enum E_OBJECT_TYPE
	{
		Resource,
		Piece
	}

	protected E_OBJECT_TYPE m_eObjectType = E_OBJECT_TYPE.Resource;
	public E_OBJECT_TYPE ObjectType
	{
		get { return m_eObjectType; }
	}

	[SerializeField]
	private float m_fJointElasticity = 0.2f;
	private float m_fMaxJointDistance;

	[Header("Nearest object feedback")]
	[SerializeField]
	private MeshRenderer m_cNearestFeedback = null;

	[SerializeField]
	private Material[] m_acNearestMaterial = new Material[4];

	private Material[] m_acFeedbackMaterials = new Material[4];

	private bool[] m_abPlayersNearest = new bool[4] { false, false, false, false };

	protected BoxCollider m_cBoxCollider = null;
	public BoxCollider BoxCollider
	{
		get { return m_cBoxCollider; }
	}

	private List<Player> m_lcPlayersHolding = new List<Player>();

	private bool m_bTwoPlayersMode = false;
	public bool TwoPlayersMode
	{
		get { return m_bTwoPlayersMode; }
	}

	private Vector3 m_vConstraintDir = Vector3.zero;
	public Vector3 ConstraintDir
	{
		get { return m_vConstraintDir; }
	}

	private float m_fConstraintSpeed = 0f;
	public float ConstraintSpeed
	{
		get { return m_fConstraintSpeed; }
	}

	private float m_fYRotationAngle;

	protected int m_iLayerResourceObject;
	protected int m_iLayerBlueprintObject;
	protected int m_iLayerLiftedObject;
	protected int m_iLayerPiece;
	protected int m_iLayerSpaceship;
	protected int m_iLayerSpaceshipFinish;
	protected int m_iLayerNoCollision;


	#region MonoBehavior
	protected override void Awake()
    {
        base.Awake();

        m_cBoxCollider = GetComponent<BoxCollider>();
		m_cRigidbody = GetComponent<Rigidbody>();

		m_fYRotationAngle = m_cTransform.rotation.eulerAngles.y;

		if (m_cRigidbody != null)
			m_cRigidbody.constraints = RigidbodyConstraints.FreezeRotation;

		m_iLayerResourceObject = LayerMask.NameToLayer("ResourceObject");
		m_iLayerBlueprintObject = LayerMask.NameToLayer("BlueprintObject");
		m_iLayerLiftedObject = LayerMask.NameToLayer("LiftedObject");
		m_iLayerPiece = LayerMask.NameToLayer("Piece");
		m_iLayerSpaceship = LayerMask.NameToLayer("Spaceship");
		m_iLayerSpaceshipFinish = LayerMask.NameToLayer("SpaceshipFinish");
		m_iLayerNoCollision = LayerMask.NameToLayer("NoCollision");
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();

		if (m_bTwoPlayersMode)
			HandleTwoPlayer();

		int layer = m_cGameObject.layer;

		if (layer != m_iLayerSpaceship && layer != m_iLayerSpaceshipFinish && layer != m_iLayerLiftedObject && layer != m_iLayerNoCollision)
		{
			m_cTransform.rotation = Quaternion.identity;

			m_cTransform.Rotate(GameManager.Instance.PlatformTransform.up, m_fYRotationAngle);

			Quaternion previousRotation = m_cTransform.rotation;

			m_cTransform.up = GameManager.Instance.PlatformTransform.up;

			// /!\ the order of the multiply is important /!\
			m_cTransform.rotation = previousRotation * m_cTransform.rotation;
		}
	}

	protected virtual void OnDestroy()
	{
		OnPlaced = null;
		OnJointBreak = null;
		OnReturnToPool = null;
	}
	#endregion

	public void SetPosition(Transform parent)
    {
        m_cTransform.SetParent(parent);
        m_cTransform.localPosition = Vector3.zero;
        m_cTransform.localRotation = Quaternion.identity;
    }

	public void AddPlayer(Player player)
	{
		m_lcPlayersHolding.Add(player);

		m_bTwoPlayersMode = (m_lcPlayersHolding.Count > 1);
	}

	public void RemovePlayer(Player player)
	{
		m_lcPlayersHolding.Remove(player);

		m_bTwoPlayersMode = (m_lcPlayersHolding.Count > 1);
	}

	public virtual void ReturnToPool()
	{
		m_cTransform.rotation = Quaternion.identity;
		m_fYRotationAngle = 0f;

		m_cRigidbody.constraints = RigidbodyConstraints.FreezeRotation;

		if (OnReturnToPool != null)
			OnReturnToPool(m_cGameObject);

		OnReturnToPool = null;
	}

	#region Movement
	public virtual void Take(Player player)
	{
		AddPlayer(player);

		if (!m_bTwoPlayersMode)
		{
			SetPosition(player.Arms);
			player.ApplyWeightedObject(true);
		}
		else
		{
			Player otherPlayer = m_lcPlayersHolding[0];
			otherPlayer.ApplyWeightedObject(false);
			otherPlayer.SetTwoPlayersModeAnim(true);

			Vector3 objectDistanceFirstPlayer = (otherPlayer.Arms.position - otherPlayer.Transform.position);
			objectDistanceFirstPlayer.y = 0f;

			Vector3 objectDistanceSecondPlayer = (player.Arms.position - player.Transform.position);
			objectDistanceSecondPlayer.y = 0f;

			m_fMaxJointDistance = objectDistanceFirstPlayer.magnitude + objectDistanceSecondPlayer.magnitude + m_fJointElasticity;
		}

		DisablePhysics();
		gameObject.layer = m_iLayerLiftedObject;
	}

	public virtual void Drop(Player player)
    {
		if (m_bTwoPlayersMode)
		{
			RemovePlayer(player);

			Player otherPlayer = m_lcPlayersHolding[0];
			m_cTransform.parent = otherPlayer.Arms;
			otherPlayer.ApplyWeightedObject(true);
			otherPlayer.SetTwoPlayersModeAnim(false);
		}
		else
		{
			if (m_lcPlayersHolding.Count > 0)
				m_lcPlayersHolding[0].ApplyWeightedObject(false);

			m_lcPlayersHolding.Remove(player);

			m_cTransform.parent = null;
			gameObject.layer = m_iLayerResourceObject;

			m_fYRotationAngle = m_cTransform.rotation.eulerAngles.y;

			EnablePhysics();
		}
	}

	public virtual void Place()
    {
		m_lcPlayersHolding.Clear();
		m_bTwoPlayersMode = false;

		if (OnPlaced != null)
			OnPlaced();

		OnPlaced = null;
    }
	#endregion

	#region Physics
	public override void EnablePhysics()
	{
		base.EnablePhysics();

		m_cRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
	}

	public override void DisablePhysics()
	{
		base.DisablePhysics();

		if (m_cCurrCell != null)
		{
			m_cCurrCell.RemoveWeightedObject(this);
			m_cCurrCell = null;
			m_acPrevCells.Clear();
		}
	}
	#endregion

	#region TwoPlayerHold Joint
	public Vector3 GetDirectionBetweenPlayers(Player player)
	{
		return ((player == m_lcPlayersHolding[0]) ? m_lcPlayersHolding[1].Transform.position - player.Transform.position : m_lcPlayersHolding[0].Transform.position - player.Transform.position).normalized;
	}
	
	private void BreakJoint()
	{
		Debug.Log("Break joint");
		if (OnJointBreak != null)
			OnJointBreak(m_lcPlayersHolding[1]);
	}

	private void HandleTwoPlayer()
	{
		if (m_lcPlayersHolding[0].MoveDirection == Vector3.zero)
			HandleOnePlayerMoving(m_lcPlayersHolding[1], m_lcPlayersHolding[0]);
		else if (m_lcPlayersHolding[1].MoveDirection == Vector3.zero)
			HandleOnePlayerMoving(m_lcPlayersHolding[0], m_lcPlayersHolding[1]);
		else
			HandleTwoPlayerMoving();
	}

	private void HandleOnePlayerMoving(Player movingPlayer, Player otherPlayer)
	{
		Vector3 directionToOtherPlayer = (otherPlayer.Transform.position - movingPlayer.Transform.position);
		directionToOtherPlayer.y = 0f;

		if (directionToOtherPlayer.magnitude > m_fMaxJointDistance)
		{
			BreakJoint();
			return;
		}

		directionToOtherPlayer.Normalize();

		Vector3 desiredMoveDirection = movingPlayer.MoveDirection.normalized;

		if (Vector3.Dot(directionToOtherPlayer, desiredMoveDirection) > 0f)
			m_vConstraintDir = Vector3.zero;
		else
		{
			Vector3 toOtherPlayerNormal1 = new Vector3(-directionToOtherPlayer.z, directionToOtherPlayer.y, directionToOtherPlayer.x);
			Vector3 toOtherPlayerNormal2 = new Vector3(directionToOtherPlayer.z, directionToOtherPlayer.y, -directionToOtherPlayer.x);

			float dotFirstNormal = Vector3.Dot(desiredMoveDirection, toOtherPlayerNormal1);
			float dotSecondNormal = Vector3.Dot(desiredMoveDirection, toOtherPlayerNormal2);

			m_vConstraintDir = (dotFirstNormal < dotSecondNormal) ? toOtherPlayerNormal2 : toOtherPlayerNormal1;
			m_vConstraintDir.y = 0f;
		}

		m_fConstraintSpeed = otherPlayer.MoveSpeed;
	}

	private void HandleTwoPlayerMoving()
	{
		Player player1 = m_lcPlayersHolding[0];
		Player player2 = m_lcPlayersHolding[1];

		Vector3 directionToOtherPlayer = (player1.Transform.position - player2.Transform.position);
		directionToOtherPlayer.y = 0f;

		if (directionToOtherPlayer.magnitude > m_fMaxJointDistance)
		{
			BreakJoint();
			return;
		}

		m_vConstraintDir = (player1.MoveDirection + player2.MoveDirection).normalized;
		m_fConstraintSpeed = (player1.MoveSpeed + player2.MoveSpeed) / 2f;
	}
	#endregion

	#region NearestFeedback
	public virtual void SelectNearest(int playerIndex, MovableObject objectInHand)
	{
		if (!m_bTwoPlayersMode && IsFeedbackVisible(objectInHand))
		{
			bool prevState = m_abPlayersNearest[playerIndex];

			m_abPlayersNearest[playerIndex] = true;

			if (prevState != m_abPlayersNearest[playerIndex])
				UpdateNearestFeedback();
		}
		else
			m_cNearestFeedback.enabled = false;
	}

	public void UnselectNearest(int playerIndex)
	{
		bool prevState = m_abPlayersNearest[playerIndex];

		m_abPlayersNearest[playerIndex] = false;

		if (prevState != m_abPlayersNearest[playerIndex])
			UpdateNearestFeedback();
	}

	private void UpdateNearestFeedback()
	{
		List<Material> nearestMat = new List<Material>();

		for (int idx = 0; idx < 4; ++idx)
		{
			if (m_abPlayersNearest[idx])
				nearestMat.Add(m_acNearestMaterial[idx]);
		}

		m_cNearestFeedback.enabled = nearestMat.Count != 0;

		if (nearestMat.Count != 0)
		{
			m_acFeedbackMaterials = m_cNearestFeedback.materials;

			switch (nearestMat.Count)
			{
				case 1:
					Material mat = nearestMat[0];
					m_acFeedbackMaterials[0] = mat;
					m_acFeedbackMaterials[1] = mat;
					m_acFeedbackMaterials[2] = mat;
					m_acFeedbackMaterials[3] = mat;
					break;
				case 2:
					m_acFeedbackMaterials[0] = nearestMat[0];
					m_acFeedbackMaterials[1] = nearestMat[0];
					m_acFeedbackMaterials[2] = nearestMat[1];
					m_acFeedbackMaterials[3] = nearestMat[1];
					break;
				case 3:
					m_acFeedbackMaterials[0] = nearestMat[0];
					m_acFeedbackMaterials[1] = nearestMat[1];
					m_acFeedbackMaterials[2] = nearestMat[2];
					m_acFeedbackMaterials[3] = nearestMat[2];
					break;
				case 4:
					m_acFeedbackMaterials[0] = nearestMat[0];
					m_acFeedbackMaterials[1] = nearestMat[1];
					m_acFeedbackMaterials[2] = nearestMat[2];
					m_acFeedbackMaterials[3] = nearestMat[3];
					break;
				default:
					break;
			}

			m_cNearestFeedback.materials = m_acFeedbackMaterials;
		}
	}

	private bool IsFeedbackVisible(MovableObject objectInHand)
	{
		int objectInHandLayer = (objectInHand != null) ? objectInHand.GameObject.layer : -1;

		return ((objectInHandLayer == -1 && m_cGameObject.layer == m_iLayerResourceObject || m_cGameObject.layer == m_iLayerPiece || m_cGameObject.layer == m_iLayerLiftedObject)
			|| (objectInHandLayer == m_iLayerLiftedObject && objectInHand.ObjectType == E_OBJECT_TYPE.Resource && m_cGameObject.layer == m_iLayerBlueprintObject)
			&& (objectInHandLayer != m_iLayerBlueprintObject)
			&& (objectInHandLayer != m_iLayerPiece));
	}
	#endregion
}
