using System.Reflection;
using UnityEngine;


public class TutorialTaskActionPlatform : TutorialTaskAction
{
	[SerializeField]
	private float m_fStability = 80f;
	[SerializeField]
	private float m_fAcceleration = 0.98f;

	private Platform m_cPlatform;

	private PropertyInfo m_cStabilityProperty;
	private PropertyInfo m_cAccelerationProperty;


	#region MonoBehavior
	private void Start()
	{
		m_cPlatform = GameManager.Instance.Platform;
		System.Type platformType = m_cPlatform.GetType();
		m_cStabilityProperty = platformType.GetProperty("Stability", BindingFlags.Public | BindingFlags.Instance);
		m_cAccelerationProperty = platformType.GetProperty("Acceleration", BindingFlags.Public | BindingFlags.Instance);
	}
	#endregion

	protected override void SetTaskAction()
	{
		TaskAction += () => { m_cStabilityProperty.SetValue(m_cPlatform, m_fStability, null); m_cAccelerationProperty.SetValue(m_cPlatform, m_fAcceleration, null); };
	}
}
