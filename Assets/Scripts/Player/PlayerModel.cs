using UnityEngine;
using FMODUnity;


public class PlayerModel : MonoBehaviour
{
    private Animator m_cAnimator = null;
    public Animator Animator
    {
        get { return m_cAnimator; }
    }

    [SerializeField]
    private Renderer[] m_cAntennaBall = null;
	public Renderer[] AntenaBall
	{
		get { return m_cAntennaBall; }
	}

    [SerializeField]
    private GameObject m_cMesh = null;

	[SerializeField]
	private Renderer m_cCursor = null;

	private Renderer[] m_acRenderers;

    private GameObject m_cGameObject = null;

	private Transform m_cTransform = null;
	public Transform Transform
	{
		get { return m_cTransform; }
	}

    private bool m_bIsCharaModel = false;

    #region SOUNDS
    [Header("Voice Sounds")]
    [SerializeField]
    private StudioEventEmitter m_cFallSound = null;
    [SerializeField]
    private StudioEventEmitter m_cValidationSound = null;
    [SerializeField]
    private StudioEventEmitter m_cMoveSound = null;

    private enum E_FSTSOUND : int
    {
        Grid = 0,
        Metal,
        Grass,

        Count
    }

    private E_FSTSOUND m_eCurrFST;

    [System.Serializable]
    private struct FST
    {
#if UNITY_EDITOR
        public string m_sName;
#endif
        public int m_iPriority;
        public float m_fParameterValue;
        public E_FSTSOUND m_eFST;
    }

    [SerializeField]
    private FST[] m_acFST = new FST[(int)E_FSTSOUND.Count];

    private float m_fDistanceRaycast = 3.0f;
    [SerializeField]
    private string m_sParameterName = "Material";

    private int m_iPrioritySound = 0;
    private float m_fParameterValueSound = 0.0f;
    #endregion

    private int m_iLayerGrass;
    private int m_iLayerMetal;
    private int m_iLayerGrid;

    private int m_iLayerMaskFST;

    #region MonoBehavior
    private void Awake()
	{
		m_acRenderers = m_cMesh.GetComponentsInChildren<Renderer>();
        m_cGameObject = gameObject;

        if (gameObject.tag == "Chara")
            m_bIsCharaModel = true;

		m_cTransform = GetComponent<Transform>();
        m_cAnimator = GetComponent<Animator>();

        m_iLayerGrass = LayerMask.NameToLayer("Grass");
        m_iLayerMetal = LayerMask.NameToLayer("Plate");

        m_iLayerMaskFST = (1 << m_iLayerGrass) | (1 << m_iLayerMetal);

        m_eCurrFST = E_FSTSOUND.Grid;
    }
    #endregion

    public void UpdateMoveSound(int otherLayer)
    {
        if (m_bIsCharaModel)
        {
            if (((1 << otherLayer) & m_iLayerMaskFST) != 0)
            {
                if (((1 << otherLayer) & (1 << m_iLayerGrass)) != 0)
                    ChangeParameterSound(2);
                else
                    ChangeParameterSound(1);
            }
        }
    }

    public void LaunchSoundGrid(int otherLayer)
    {
        if (m_bIsCharaModel)
        {
            if (m_iPrioritySound == 2)
            {
                if (((1 << otherLayer) & (1 << m_iLayerGrass)) != 0)
                {
                    FST fstGrid = m_acFST[0];
                    UpdateValueSound(fstGrid);
                }
            }
            else if (m_iPrioritySound == 1)
            {
                if (((1 << otherLayer) & (1 << m_iLayerMetal)) != 0)
                {
                    FST fstGrid = m_acFST[0];
                    UpdateValueSound(fstGrid);
                }
            }
        }
    }

    private void ChangeParameterSound(int index)
    {
        FST fst = m_acFST[index];
        if (m_iPrioritySound < fst.m_iPriority)
            UpdateValueSound(fst);
    }

    private void UpdateValueSound(FST fst)
    {
        m_eCurrFST = fst.m_eFST;
        m_iPrioritySound = fst.m_iPriority;
        m_fParameterValueSound = fst.m_fParameterValue;
    }

    public void SetAntennaColor(Color color)
	{
        foreach (Renderer renderer in m_cAntennaBall)
            renderer.material.SetColor("_Albedo_BlendColor", color);

        if (m_cCursor)
		    m_cCursor.material.SetColor("_TintColor", color);
	}

	public void SetActive(bool active)
	{
		m_cMesh.SetActive(active);
	}

    public void SetVisible(bool visible)
    {
        foreach (Renderer renderer in m_acRenderers)
			renderer.enabled = visible;
    }

    public void Fall()
    {
        if (m_cFallSound != null)
            m_cFallSound.Play();

        m_cAnimator.Play("Falling");
    }

    public void MoveSound(bool isMoving)
    {
        if (m_cMoveSound != null)
        {
            if (isMoving && !m_cMoveSound.IsPlaying())
            {
                m_cMoveSound.Play();
                m_cMoveSound.SetParameter(m_sParameterName, m_fParameterValueSound);
            }
            //else if (!isMoving && m_cMoveSound.IsPlaying())
              //  m_cMoveSound.Stop();
        }
    }

	private void Show()
	{
		SetVisible(true);
	}

	private void Hide()
	{
		SetVisible(false);
	}

    public void DisableSound()
    {
        m_cFallSound.enabled = false;
        m_cValidationSound.enabled = false;
    }
}
