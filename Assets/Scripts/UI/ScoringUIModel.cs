using UnityEngine;
using FMODUnity;

public class ScoringUIModel : MonoBehaviour
{
    private Animator m_cAnimator = null;
    private Transform m_cTransform = null;
    public Transform Transform { get { return m_cTransform; } }

    [Header("Voice Sounds")]
    [SerializeField]
    private StudioEventEmitter m_cWinSound = null;
    [SerializeField]
    private StudioEventEmitter m_cLoseSound = null;

    private void Awake()
    {
        m_cTransform = GetComponent<Transform>();
        m_cAnimator = GetComponent<Animator>();
    }


    public void Win()
    {
        m_cAnimator.enabled = true;
        m_cWinSound.Play();
    }

    public void Lose()
    {
        m_cLoseSound.Play();
    }

}
