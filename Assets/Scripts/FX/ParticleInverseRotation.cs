using UnityEngine;


public class ParticleInverseRotation : MonoBehaviour
{
	[SerializeField]
	private GameObject m_cParticle = null;

	[SerializeField]
	private GameObject m_cReferenceObject = null;

	private Transform m_cParticleTransform;
	private Transform m_cReferenceObjectTransform;


	#region MonoBehavior
	private void Awake()
	{
		m_cParticleTransform = m_cParticle.transform;
		m_cReferenceObjectTransform = m_cReferenceObject.transform;
	}

	private void Update()
	{
		m_cParticleTransform.localRotation = Quaternion.Euler(0f, -(m_cReferenceObjectTransform.rotation.eulerAngles.y - 360f), 0f);
	}
	#endregion
}
