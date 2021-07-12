using Cinemachine;
using System.Collections;
using UnityEngine;


public class CameraEffects : MonoBehaviour
{
	[System.Serializable]
	public struct CameraShakeData
	{
		public float m_fAmplitude;
		public float m_fFrequency;
		public float m_fDuration;
	}

	[SerializeField]
	private CinemachineVirtualCamera m_cCamera = null;

	private CinemachineBasicMultiChannelPerlin m_cCameraNoise = null;

	private Coroutine m_cShakeCoroutine = null;


	#region MonoBehavior
	private void Awake()
	{
		m_cCameraNoise = (CinemachineBasicMultiChannelPerlin)m_cCamera.GetCinemachineComponent(CinemachineCore.Stage.Noise);
		m_cCameraNoise.m_AmplitudeGain = m_cCameraNoise.m_FrequencyGain = 0f;
	}
	#endregion

	public void PlayCameraShake(CameraShakeData cameraShake)
	{
		m_cCameraNoise.m_AmplitudeGain = cameraShake.m_fAmplitude;
		m_cCameraNoise.m_FrequencyGain = cameraShake.m_fFrequency;

		m_cShakeCoroutine = StartCoroutine(CameraShakerTimer(cameraShake.m_fDuration));
	}

	public void StopCameraShake()
	{
		if (m_cShakeCoroutine != null)
		{
			StopCoroutine(m_cShakeCoroutine);
			m_cShakeCoroutine = null;
		}

		m_cCameraNoise.m_AmplitudeGain = m_cCameraNoise.m_FrequencyGain = 0f;
	}

	private IEnumerator CameraShakerTimer(float duration)
	{
		yield return new WaitForSeconds(duration);

		StopCameraShake();
	}
}
