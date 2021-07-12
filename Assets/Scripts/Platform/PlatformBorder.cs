using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class PlatformBorder : MonoBehaviour
{
	public event System.Action OnDestroyed = null;

	[SerializeField]
	private float m_fMinDurability = 1f;

	[SerializeField]
	private float m_fMaxDurability = 2f;

	[SerializeField]
	private float m_fBreakForce = 10f;

	[SerializeField]
	private float m_fAngle = 30f;

	[SerializeField]
	private Vector3 m_vRotationBreakForce = new Vector3(90f, 0f, 0f);

	[SerializeField]
	private GameObject[] m_acRopes = new GameObject[2];
	private BoxCollider[] m_acRopesCollider = new BoxCollider[2];
	private Renderer[] m_acRopesRenderer = new Renderer[2];

	private float m_fCurrDurability;

	private bool m_bLoseDurability = false;

	private GameObject m_cGameObject;
	private Transform m_cTransform;
	private Rigidbody m_cRigidbody;

	private int m_iLayerNoCollision;


	#region MonoBehavior
	private void Awake()
	{
		m_fCurrDurability = Random.Range(m_fMinDurability, m_fMaxDurability);

		m_cGameObject = gameObject;
		m_cTransform = transform;

		m_cRigidbody = GetComponent<Rigidbody>();
		m_cRigidbody.constraints = RigidbodyConstraints.FreezeAll;

		m_fAngle *= Mathf.Deg2Rad;

		m_iLayerNoCollision = LayerMask.NameToLayer("NoCollision");

		m_acRopesCollider[0] = m_acRopes[0].GetComponent<BoxCollider>();
		m_acRopesCollider[1] = m_acRopes[1].GetComponent<BoxCollider>();

		m_acRopesRenderer[0] = m_acRopes[0].GetComponent<Renderer>();
		m_acRopesRenderer[1] = m_acRopes[1].GetComponent<Renderer>();
	}

	private void Update()
	{
		if (m_bLoseDurability)
		{
			m_fCurrDurability -= Time.deltaTime;

			if (m_fCurrDurability <= 0f)
			{
				m_bLoseDurability = false;

				Destroyed();
			}
		}
	}

	private void OnDestroy()
	{
		OnDestroyed = null;
	}
	#endregion

	public void StartDeterioration()
	{
		m_bLoseDurability = true;
	}

	private void Destroyed()
	{
		m_cGameObject.layer = m_iLayerNoCollision;
		m_cRigidbody.constraints = RigidbodyConstraints.None;

		Vector3 breakTarget = m_cTransform.position + new Vector3(m_fBreakForce, 0f, 0f);

		float distance = Vector3.Distance(m_cTransform.position, breakTarget);

		m_cTransform.LookAt(breakTarget);

		float velocity = Mathf.Sqrt(distance * -Physics.gravity.y / (Mathf.Sin(m_fAngle * 2f)));
		float yVelocity = Mathf.Sin(m_fAngle) * velocity;
		float zVelocity = Mathf.Cos(m_fAngle) * velocity;

		Vector3 globalVelocity = m_cTransform.TransformVector(new Vector3(0f, yVelocity, zVelocity));

		m_cRigidbody.velocity = globalVelocity;
		m_cRigidbody.useGravity = true;

		m_cRigidbody.AddTorque(m_vRotationBreakForce, ForceMode.Impulse);

		m_acRopes[0].layer = m_iLayerNoCollision;
		m_acRopes[1].layer = m_iLayerNoCollision;

		m_acRopesCollider[0].isTrigger = true;
		m_acRopesCollider[1].isTrigger = true;

		m_acRopesRenderer[0].enabled = false;
		m_acRopesRenderer[1].enabled = false;

		if (OnDestroyed != null)
			OnDestroyed();
	}
}
