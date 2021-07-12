using FMOD.Studio;
using FMODUnity;
using System;
using UnityEngine;


[RequireComponent(typeof(SphereCollider))]
public class Meteor : WeightedObject
{
	private enum E_METEOR_MESH_ROTATION
	{
		Random,
		Identity,
		Direction
	}

	[Serializable]
	private struct MeteorMesh
	{
		public GameObject[] m_acMeshes;
		public E_METEOR_MESH_ROTATION m_eMeshRotation;
	}

	[Header("Meteor")]
	[SerializeField]
	private MeteorMesh[] m_acThemedMeshes = null;

	[SerializeField]
	private float m_fKineticEnergy = 1f;

	[Header("Sounds")]
	[SerializeField]
	private StudioEventEmitter m_cMeteorFallEvent = null;

	[SerializeField]
	private StudioEventEmitter m_cMeteorImpactEvent = null;

	public event Action OnImpact = null;
	public event Action<Vector3> OnImpactPlatform = null;
	public event Action<GameObject> OnDespawn = null;

	private GameObject m_cCurrentMesh = null;
	private ParticleSystem m_cImpactFeedbackFX = null;
	private GameObject m_cImpactFeedbackGameObject = null;

	private Vector3 m_vDirection = Vector3.down;
	private float m_fSpeed = 1f;
	private float m_fRadius = 1f;

	private bool m_bIsFalling = false;

	private int m_iLayerMeteor = 0;
	private int m_iLayerSpaceship = 0;
	private int m_iLayerPlayer = 0;
    private int m_iLayerResource = 0;
	private int m_iLayerPiece = 0;
    private int m_iLayerNoCollision = 0;

	private int m_iMaskImpact = 0;
	private int m_iMaskPlatform = 0;

    [Header("FX")]
    [SerializeField]
    private ParticleSystem m_cMeteorFX = null;

	[SerializeField]
	private ParticleSystem m_cImpactFeedbackFXPrefab = null;
	private float m_fImpactFeedbackHeight = 0f;

    [SerializeField]
    private ParticleSystem m_cMeteorExplosionFX = null;
    private float m_fDurationExplosionFX = 0.0f;


    #region MonoBehavior
    override protected void Awake()
	{
		base.Awake();

		foreach (MeteorMesh meteorMesh in m_acThemedMeshes)
		{
			foreach (GameObject mesh in meteorMesh.m_acMeshes)
				mesh.SetActive(false);
		}

		m_cImpactFeedbackFX = Instantiate(m_cImpactFeedbackFXPrefab, GameObject.FindGameObjectWithTag("Platform").transform);
		m_cImpactFeedbackGameObject = m_cImpactFeedbackFX.gameObject;

		m_cImpactFeedbackGameObject.SetActive(false);

		m_fRadius = GetComponent<SphereCollider>().radius * m_cTransform.lossyScale.x;

		ParticleSystem.MainModule explosionFXMainModule = m_cMeteorExplosionFX.main;
		m_fDurationExplosionFX = explosionFXMainModule.duration + explosionFXMainModule.startLifetime.constant;

		m_iLayerMeteor = LayerMask.NameToLayer("Meteor");
		m_iLayerSpaceship = LayerMask.NameToLayer("Spaceship");
		m_iLayerPlayer = LayerMask.NameToLayer("Player");
        m_iLayerResource = LayerMask.NameToLayer("ResourceObject");
		m_iLayerPiece = LayerMask.NameToLayer("Piece");
        m_iLayerNoCollision = LayerMask.NameToLayer("NoCollision");

		m_iMaskImpact = (1 << m_iLayerPlatform) | (1 << m_iLayerSpaceship) | (1 << m_iLayerPlayer) | (1 << m_iLayerResource) | (1 << m_iLayerPiece) | (1 << m_iLayerPlatformBorder);
		m_iMaskPlatform = 1 << m_iLayerPlatform;
	}

	protected override void FixedUpdate()
	{
		bool showFeedback = false;

		if (m_bIsFalling)
		{
			m_cTransform.position += m_vDirection * m_fSpeed * Time.fixedDeltaTime;

			RaycastHit hitInfo;
			if (Physics.SphereCast(m_cTransform.position, m_fRadius, m_vDirection, out hitInfo, 100f, m_iMaskPlatform))
			{
				showFeedback = true;

				Vector3 feedbackPosition = hitInfo.point;
				feedbackPosition.y += m_cImpactFeedbackFXPrefab.transform.position.y;
				m_cImpactFeedbackFX.transform.position = feedbackPosition;
			}
		}

		m_cImpactFeedbackGameObject.SetActive(showFeedback);

		if (showFeedback && !m_cImpactFeedbackFX.isPlaying)
			m_cImpactFeedbackFX.Play(true);
	}

	override protected void OnCollisionEnter(Collision collision)
	{
		base.OnCollisionEnter(collision);

		int otherLayer = collision.gameObject.layer;

		if (((1 << otherLayer) & m_iMaskImpact) != 0)
			Impact(collision, otherLayer);
	}

	override protected void OnCollisionExit(Collision collision)
	{

	}

	private void OnDestroy()
	{
		OnImpact = null;
		OnImpactPlatform = null;
		OnDespawn = null;
	}
	#endregion

	public void Init(Vector3 direction, float speed, int theme)
	{
		MeteorMesh currentTheme = m_acThemedMeshes[theme];
		m_cCurrentMesh = currentTheme.m_acMeshes[UnityEngine.Random.Range(0, currentTheme.m_acMeshes.Length)];

		Quaternion meshRotation;
		switch (currentTheme.m_eMeshRotation)
		{
			case E_METEOR_MESH_ROTATION.Random:
				meshRotation = UnityEngine.Random.rotation;
				break;
			case E_METEOR_MESH_ROTATION.Identity:
				meshRotation = Quaternion.identity;
				break;
			case E_METEOR_MESH_ROTATION.Direction:
				meshRotation = Quaternion.LookRotation(direction);
				break;
			default:
				meshRotation = Quaternion.identity;
				break;
		}

		m_cCurrentMesh.transform.rotation = meshRotation;
		m_cCurrentMesh.SetActive(true);

		m_vDirection = direction;
		m_fSpeed = speed;

        m_bIsFalling = true;

		m_cMeteorFallEvent.Play();
        m_cMeteorFX.Play(true);

		if (m_cCurrCell != null)
		{
			m_cCurrCell.RemoveWeightedObject(this);
			m_cCurrCell = null;
		}

		m_acPrevCells.Clear();
	}

	public void Despawn()
	{
		if (OnDespawn != null)
		{
			gameObject.layer = m_iLayerMeteor;

			OnDespawn(gameObject);

			OnImpact = null;
			OnImpactPlatform = null;
			OnDespawn = null;
		}
	}

	public void HideMeteor()
	{
        if (m_cCurrentMesh != null)
        {
			m_cCurrentMesh.SetActive(false);
            m_cCurrentMesh = null;
        }
	}

	#region Impacts
	private void Impact(Collision collision, int otherLayer)
	{
		m_bIsFalling = false;

		HideMeteor();

		gameObject.layer = m_iLayerNoCollision;

		m_cImpactFeedbackGameObject.SetActive(false);

		m_cMeteorFallEvent.Stop();
		m_cMeteorImpactEvent.Play();

        m_cMeteorFX.Stop(true);
        m_cMeteorExplosionFX.Play(true);

		if (OnImpact != null)
			OnImpact();

		if (otherLayer == m_iLayerPlatform)
			ImpactOnPlatform();
		else if (otherLayer == m_iLayerPlayer)
			ImpactOnPlayer(collision.gameObject.GetComponentInParent<Player>());
		else if (otherLayer == m_iLayerSpaceship)
			ImpactOnSpaceship();
		else if (otherLayer == m_iLayerPlatformBorder)
			ImpactOnPlatformBorder();
		else
			Invoke("Despawn", m_fDurationExplosionFX);
	}

	private void ImpactOnPlatform()
	{
		if (OnImpactPlatform != null)
			OnImpactPlatform(m_cTransform.position);

		Invoke("RemoveMeteorCell", m_fKineticEnergy);
        Invoke("Despawn", (m_fKineticEnergy > m_fDurationExplosionFX) ? m_fKineticEnergy : m_fDurationExplosionFX);
	}

	private void ImpactOnPlatformBorder()
	{
		Invoke("Despawn", m_fDurationExplosionFX);
	}

	private void ImpactOnPlayer(Player playerHit)
	{
        Invoke("Despawn", m_fDurationExplosionFX);
	}

	private void ImpactOnSpaceship()
	{
        Invoke("Despawn", m_fDurationExplosionFX);
	}
	#endregion

	private void RemoveMeteorCell()
    {
        if (m_cCurrCell != null)
        {
            m_cCurrCell.RemoveWeightedObject(this);
            m_cCurrCell = null;
        }

        m_bIsOnGroud = false;
        m_acPrevCells.Clear();
    }

	#region Gizmos
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(transform.position, transform.position + (m_vDirection * 100f));
	}
	#endregion
}
