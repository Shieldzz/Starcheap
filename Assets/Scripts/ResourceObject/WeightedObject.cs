using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class WeightedObject : MonoBehaviour
{
	[Header("Physics properties")]
	[SerializeField]
	protected int m_iWeight = 1;

    protected int m_iCurrWeight = 1;
    public int Weight
	{
		get { return m_iCurrWeight; }
	}

	[SerializeField, Range(0f, 1f)]
	protected float m_fSlopeCoeff = 1f;

	public bool m_bActivateSlope = true;

	protected bool m_bIsOnGroud = false;
	public bool IsOnGround
	{
		get { return m_bIsOnGroud; }
	}

	public Cell m_cCurrCell = null;
	public List<Cell> m_acPrevCells = new List<Cell>();

	public Vector3 m_vSlope = Vector3.zero;

    protected Transform m_cTransform = null;
    public Transform Transform { get { return m_cTransform; } }

	protected GameObject m_cGameObject = null;
	public GameObject GameObject
	{
		get { return m_cGameObject; }
	}

    protected Rigidbody m_cRigidbody;
    public Rigidbody Rigidbody
    {
        get { return m_cRigidbody; }
    }

	protected int m_iLayerPlatform = 0;
	protected int m_iLayerPlatformBorder = 0;
	protected int m_iLayerFalling = 0;
	protected int m_iLayerDefault = 0;

    protected int m_iLayerMaskPlatform = 0;


    #region MonoBehavior
    virtual protected void Awake()
    {
        m_cTransform = transform;
		m_cGameObject = gameObject;
        m_cRigidbody = GetComponent<Rigidbody>();
        m_iCurrWeight = m_iWeight;

		m_iLayerPlatform = LayerMask.NameToLayer("Platform");
		m_iLayerPlatformBorder = LayerMask.NameToLayer("PlatformBorder");
		m_iLayerFalling = LayerMask.NameToLayer("Falling");
		m_iLayerDefault = LayerMask.NameToLayer("Default");

		m_iLayerMaskPlatform = 1 << m_iLayerPlatform;
    }

    virtual protected void FixedUpdate()
	{
		if (m_bActivateSlope && m_cCurrCell != null && m_bIsOnGroud)
			m_cTransform.position += m_vSlope / m_iWeight * m_fSlopeCoeff * Time.fixedDeltaTime;
	}

	virtual protected void OnCollisionEnter(Collision collision)
	{
		int otherLayer = collision.gameObject.layer;

		if (otherLayer == m_iLayerPlatform)
			m_bIsOnGroud = true;
		else if (otherLayer == m_iLayerPlatformBorder)
		{
			m_cRigidbody.constraints = RigidbodyConstraints.FreezePositionY;
			m_cRigidbody.velocity = Vector3.zero;

			PlatformBorder border = collision.gameObject.GetComponent<PlatformBorder>();

			if (border != null)
				border.OnDestroyed += ExitPlatformBorder;
		}
	}

	virtual protected void OnCollisionExit(Collision collision)
	{
		int otherLayer = collision.gameObject.layer;

		if (otherLayer == m_iLayerPlatform)
		{
			m_cRigidbody.velocity = Vector3.zero;
		}
		else if (otherLayer == m_iLayerPlatformBorder)
		{
			PlatformBorder border = collision.gameObject.GetComponent<PlatformBorder>();

			if (border != null)
				border.OnDestroyed -= ExitPlatformBorder;

			ExitPlatformBorder();
		}
	}

	virtual protected void OnTriggerEnter(Collider collider)
	{
		if (collider.gameObject.layer == m_iLayerFalling)
			m_bIsOnGroud = false;
	}
	#endregion

	#region Physics
    public virtual void EnablePhysics()
    {
        m_cRigidbody.useGravity = true;
        m_cRigidbody.isKinematic = false;
        m_cRigidbody.constraints = RigidbodyConstraints.None;
    }

	public virtual void DisablePhysics()
    {
        m_cRigidbody.useGravity = false;
        m_cRigidbody.isKinematic = true;
        m_cRigidbody.constraints = RigidbodyConstraints.FreezeAll;
    }

	private void ExitPlatformBorder()
	{
		if (m_cGameObject.layer != m_iLayerDefault)
			m_cRigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
		else
			m_cRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
	}
	#endregion
}
