using UnityEngine;


public class CountDown : Timer
{
    [SerializeField]
    private Camera m_cCamera = null;


    #region MonoBehaviour
    protected override void Update()
    {
        base.Update();
	}

    private void LateUpdate()
    {
        transform.LookAt(transform.position + m_cCamera.transform.rotation * Vector3.forward,
            m_cCamera.transform.rotation * Vector3.up);
    }
    #endregion

    protected override void SetFormeTimer()
    {
        m_cTimerLabel.text = m_fCurrTime.ToString("0.0");
    }
}
