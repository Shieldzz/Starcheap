using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class LoseUI : MonoBehaviour {

    public event Action OnShowUI = null;

    private GameObject m_cGameObject = null;

    [SerializeField]
    private Transform m_cCrane = null;
    private Vector3 m_cStartPositionCrane = Vector3.zero;


    [SerializeField]
    private Transform m_cTarget = null;

    private bool m_bIsMoving = false;

    [SerializeField]
    private float m_fSpeedCrane = 1.0f;

    #region MonoBehaviour
    void Start ()
    {
        m_cGameObject = gameObject;
        m_cStartPositionCrane = m_cCrane.position;
	}
	
	void Update ()
    {
        if (m_bIsMoving)
        {
            Move(m_cCrane, m_cTarget.position);
        }
	}
    #endregion

    public void LaunchCrane()
    {
        m_bIsMoving = true;
    }

    private void Move(Transform transformCrane, Vector3 position)
    {
        Vector3 dir = position - transformCrane.position;

        if (dir.magnitude < 0.2f)
        {
            m_bIsMoving = false;
            OnShowUI();
        }

        Vector3 dirNormalize = dir.normalized;
        transformCrane.position += dirNormalize * m_fSpeedCrane * Time.deltaTime;
    }

    public void SetActive(bool active)
    {
        m_cGameObject.SetActive(active);
    }

    public void Restart()
    {
        m_cCrane.position = m_cStartPositionCrane;
    }
}
