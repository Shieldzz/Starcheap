using UnityEngine;


public class CameraOrientation : MonoBehaviour
{
	[SerializeField]
	private Transform m_cCameraTransform;

	[SerializeField]
	private Transform m_cPlatformTransform;

    private bool m_bIsFocusPlatformRotation = true;

    private Quaternion m_qStartRotation = Quaternion.identity;
    private Quaternion m_qCurrentRotation = Quaternion.identity;
    private float m_fTimeCountRotation = 0.0f;

    private void Start()
    {

        m_qStartRotation = m_cCameraTransform.rotation;
    }

    private void FixedUpdate()
	{
        if (m_bIsFocusPlatformRotation)
        {
            Vector3 platformRot = m_cPlatformTransform.rotation.eulerAngles;

            float angleX = (platformRot.x > 180f) ? platformRot.x - 360f : platformRot.x;
            float angleZ = (platformRot.z > 180f) ? Mathf.Abs(platformRot.z - 360f) : -platformRot.z;

            float cameraRotationY = (angleX + angleZ) / 2f;

            Vector3 cameraRot = m_cCameraTransform.rotation.eulerAngles;
            cameraRot.y = cameraRotationY;
            m_cCameraTransform.rotation = Quaternion.Euler(cameraRot);
        }
        else
        {
            if (m_fTimeCountRotation <= 1.0f)
            {
                m_fTimeCountRotation += Time.deltaTime;
                m_cCameraTransform.rotation = Quaternion.Lerp(m_qCurrentRotation, m_qStartRotation, m_fTimeCountRotation);
            }
        }
	}

    public void DisableFocusPlatform()
    {
        m_bIsFocusPlatformRotation = false;
        m_fTimeCountRotation = 0.0f;
        m_qCurrentRotation = m_cCameraTransform.transform.rotation;
    }

}
