using UnityEngine;
using FMODUnity;

[RequireComponent(typeof(SphereCollider))]
public class CharacterSelector : MonoBehaviour
{
	[SerializeField]
	private CharacterSelection.E_CHARACTER m_eCharacter = CharacterSelection.E_CHARACTER.Raccoon;

	[SerializeField]
	private Renderer[] m_acProjectedCharacter = null;

	[SerializeField]
	private Renderer[] m_acProjectedAntena = null;

	[SerializeField]
	private Material m_cSelectedMaterial = null;
	private Material m_cOriginalMaterial;

	[SerializeField]
	private Transform m_cAntenaPosition = null;
	public Transform AntenaPosition
	{
		get { return m_cAntenaPosition; }
	}

	public event System.Action<int, CharacterSelection.E_CHARACTER> OnStateChange;

	private int m_iPlayerInSelector = -1;

	private int m_iLayerPlayer;

    [Header("Sounds")]
    [SerializeField]
    private StudioEventEmitter m_cValidSound = null;
    [SerializeField]
    private StudioEventEmitter m_cCharaSelect = null;

    #region MonoBehavior
    private void Awake()
	{
		m_cOriginalMaterial = m_acProjectedCharacter[0].material;

		m_iLayerPlayer = LayerMask.NameToLayer("Player");
	}

	private void OnTriggerEnter(Collider other)
	{
		Player player = other.GetComponent<Player>();

		if (player != null)
			player.m_bActivateSlope = false;
	}

	//private void OnTriggerExit(Collider other)
	//{
	//	Player player = other.GetComponent<Player>();

	//	if (player != null)
	//	{
	//		player.m_bActivateSlope = true;

	//		if (m_iPlayerInSelector != -1)
	//		{
	//			m_iPlayerInSelector = -1;

	//			if (OnStateChange != null)
	//				OnStateChange(m_iPlayerInSelector, m_eCharacter);
	//		}
	//	}
	//}

	private void OnDestroy()
	{
		OnStateChange = null;
	}
	#endregion

	public bool Select(int playerIndex)
	{
		if (m_iPlayerInSelector == -1)
		{
			m_iPlayerInSelector = playerIndex;
            m_cValidSound.Play();
            m_cCharaSelect.Play();

			if (OnStateChange != null)
				OnStateChange(m_iPlayerInSelector, m_eCharacter);

			return true;
		}

		return false;
	}

	public void Deselect(Player player)
	{
		player.m_bActivateSlope = true;

		if (m_iPlayerInSelector != -1)
		{
			m_iPlayerInSelector = -1;

			if (OnStateChange != null)
				OnStateChange(m_iPlayerInSelector, m_eCharacter);
		}
	}

	public void SwitchMaterial(bool original, Material material = null)
	{
		if (original)
		{
			foreach (Renderer renderer in m_acProjectedCharacter)
				renderer.material = m_cOriginalMaterial;

			foreach (Renderer renderer in m_acProjectedAntena)
				renderer.material = m_cOriginalMaterial;
		}
		else
		{
			foreach (Renderer renderer in m_acProjectedCharacter)
				renderer.material = m_cSelectedMaterial;

			if (material != null)
			{
				foreach (Renderer renderer in m_acProjectedAntena)
					renderer.material = material;
			}
		}
	}
}
