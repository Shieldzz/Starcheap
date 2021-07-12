using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUI : MonoBehaviour {

    [SerializeField]
    private Camera m_cCamera;

    private Canvas m_cCanvas;
    private bool m_bIsActive = false;
    public bool Active
    {
        get { return m_bIsActive; }
    }

    private int m_iCurrentSpriteIndex = 0;
    public int CurrentPieceIndex
    {
        get { return m_iCurrentSpriteIndex; }
    }

    private List<GameObject> m_lcSpriteUI = new List<GameObject>();
    private Animator[] m_cSpriteUIAnimator;

    private int m_iSpriteListSize = 0;

    private void Awake()
    {
        m_cCanvas = GetComponent<Canvas>();
        m_cCanvas.enabled = false;
    }

    private void LateUpdate()
    {
        transform.LookAt(transform.position + m_cCamera.transform.rotation * Vector3.forward,
            m_cCamera.transform.rotation * Vector3.up);
    }

    public void SetActive(bool enable)
    {
        m_cCanvas.enabled = enable;
        m_bIsActive = enable;
        if (!m_bIsActive)
            m_iCurrentSpriteIndex = 0;

        for (int i = 0; i < m_iSpriteListSize; i++)
            m_cSpriteUIAnimator[i].SetBool("Visible", (i == m_iCurrentSpriteIndex));
    }

    public void ScrollLeft()
    {
        int prevIndex = (m_iCurrentSpriteIndex + 1) % m_iSpriteListSize;

        m_cSpriteUIAnimator[m_iCurrentSpriteIndex].SetTrigger("CenterToLeft");
        m_cSpriteUIAnimator[m_iCurrentSpriteIndex].SetBool("Visible", false);

        m_cSpriteUIAnimator[prevIndex].SetTrigger("RightToCenter");
        m_cSpriteUIAnimator[prevIndex].SetBool("Visible", true);

        m_iCurrentSpriteIndex = prevIndex;
    }

    public void ScrollRight()
    {
        int prevIndex = (m_iCurrentSpriteIndex + (m_iSpriteListSize -1)) % m_iSpriteListSize;

        m_cSpriteUIAnimator[m_iCurrentSpriteIndex].SetTrigger("CenterToRight");
        m_cSpriteUIAnimator[m_iCurrentSpriteIndex].SetBool("Visible", false);

        m_cSpriteUIAnimator[prevIndex].SetTrigger("LeftToCenter");
        m_cSpriteUIAnimator[prevIndex].SetBool("Visible", true);

        m_iCurrentSpriteIndex = prevIndex;
    }

    public void InstanciateSprite(List<GameObject> spriteList)
    {
        m_lcSpriteUI = spriteList;

        m_iSpriteListSize = m_lcSpriteUI.Count;
        m_cSpriteUIAnimator = new Animator[m_iSpriteListSize];
        for (int i = 0; i < m_iSpriteListSize; i++)
            m_cSpriteUIAnimator[i] = m_lcSpriteUI[i].GetComponent<Animator>();
    }

}
