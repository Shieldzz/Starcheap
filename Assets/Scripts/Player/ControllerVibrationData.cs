using UnityEngine;

[System.Serializable]
public struct ControllerVibrationData
{
	public AnimationCurve m_cLeftMotorCurve;
	public AnimationCurve m_cRightMotorCurve;

	public float m_fDuration;
}
