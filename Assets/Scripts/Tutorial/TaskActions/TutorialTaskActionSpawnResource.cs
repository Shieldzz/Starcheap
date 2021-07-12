using UnityEngine;


public class TutorialTaskActionSpawnResource : TutorialTaskAction
{
	[Header("Spawn parameters")]
	[SerializeField]
	private GameObject m_cResourcePrefab = null;

	[SerializeField]
	private Vector3 m_vPosition = Vector3.zero;

	[SerializeField]
	private Transform m_cParent = null;

	[SerializeField]
	private bool m_bSpawnInWorldPos = true;


	protected override void SetTaskAction()
	{
		TaskAction += () => { ResourceObjectManager.Instance.Spawn(m_cResourcePrefab, m_vPosition, Quaternion.identity, m_cParent, m_bSpawnInWorldPos, false); };
	}

	#region Gizmos
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.blue;

		if (m_bSpawnInWorldPos)
			Gizmos.DrawCube(m_vPosition, new Vector3(0.3f, 0.3f, 0.3f));
		else if (m_cParent != null)
			Gizmos.DrawCube(m_cParent.position + m_vPosition, new Vector3(0.3f, 0.3f, 0.3f));
	}
	#endregion
}
