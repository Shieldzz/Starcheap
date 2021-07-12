using UnityEngine;


[RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(BoxCollider))]
public class DustCloud : MonoBehaviour
{
	[SerializeField]
	private ParticleSystem m_cDustFX = null;

	[SerializeField]
	private ParticleSystem m_cDustReleaseFX = null;

	private float m_fDustReleaseDuration;

	private Transform m_cTransform;
	private Rigidbody m_cRigidbody;
	private GameObject m_cGameObject;

	private float m_fNegativeGravityY;

	private bool m_bHit = false;

	private int m_iLayerPlatform;


	#region MonoBehavior
	private void Awake()
	{
		m_fDustReleaseDuration = m_cDustReleaseFX.main.duration;

		m_cTransform = transform;
		m_cRigidbody = GetComponent<Rigidbody>();
		m_cGameObject = gameObject;

		m_fNegativeGravityY = -Physics.gravity.y;

		m_iLayerPlatform = LayerMask.NameToLayer("Platform");
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!m_bHit)
		{
			m_bHit = true;

			if (other.gameObject.layer == m_iLayerPlatform)
				Impact();
			else
				Hide();
		}
	}

	private void OnDestroy()
	{
		CancelInvoke();
	}
	#endregion

	public void Init(Vector3 position, Vector3 target, float angle)
	{
		m_bHit = false;

		float distance = Vector3.Distance(position, target);

		m_cTransform.LookAt(target);

		float velocity = Mathf.Sqrt(distance * m_fNegativeGravityY / (Mathf.Sin(angle * 2f)));
		float yVelocity = Mathf.Sin(angle) * velocity;
		float zVelocity = Mathf.Cos(angle) * velocity;

		Vector3 globalVelocity = m_cTransform.TransformVector(new Vector3(0f, yVelocity, zVelocity));

		m_cRigidbody.velocity = globalVelocity;
		m_cRigidbody.useGravity = true;

		m_cDustFX.Play(true);
	}

	private void Impact()
	{
		Hide();

		ResourceObjectManager.Instance.Spawn(RandomEventManager.Instance.GetRandomGeneratedResource(), m_cTransform.position, GameManager.Instance.PlatformTransform.rotation);
	}

	private void Hide()
	{
		m_cRigidbody.velocity = Vector3.zero;
		m_cRigidbody.useGravity = false;

		m_cDustFX.Stop();
		m_cDustReleaseFX.Play();

		Invoke("Despawn", m_fDustReleaseDuration);
	}

	private void Despawn()
	{
		PoolManager.Despawn(m_cGameObject);
	}
}
