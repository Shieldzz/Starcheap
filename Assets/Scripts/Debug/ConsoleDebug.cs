using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Reflection;


public class ConsoleDebug : MonoBehaviour
{
    [SerializeField]
    private InputField m_cInputField = null;

    private string m_sSpawn = "spawn";

    private string m_sEngine = "engine";
    private string m_sTail = "tail";
    private string m_sWing = "wing";

    private string m_sBathub = "bathub";
    private string m_sCouch = "couch";
    private string m_sFridge = "fridge";
    private string m_sNut = "nut";
    private string m_sSrew = "screw";
    private string m_sTv = "tv";

    private string m_sDone = "done";

    private string m_sPlatform = "platform";

    private string m_sWin = "win";
    private string m_sLose = "lose";

    [Header("Resource")]
    [SerializeField]
    private ResourceSettings[] m_acResourceSettings = null;

    private PieceManager m_cPieceManager = null;

    private int m_iLayerBlueprint = 0;
    private int m_iLayerPiece = 0;

    private string[] stability;

	private PropertyInfo m_cStabilityProperty = null;


	#region MonoBehavior
	private void Start()
    {
        m_cPieceManager = PieceManager.Instance;
        m_iLayerPiece = LayerMask.NameToLayer("Piece");
        m_iLayerBlueprint = LayerMask.NameToLayer("BlueprintObject");
    }
	#endregion

	public void SetActive(bool active)
    {
        gameObject.SetActive(active);

        if (active)
        {
            EventSystem.current.SetSelectedGameObject(m_cInputField.gameObject);
            m_cInputField.OnSelect(null);
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(null);
            m_cInputField.OnDeselect(null);
        }
    }

    public void EnterDebugLine()
    {
        string text = m_cInputField.text;

        if (text.Contains(m_sSpawn))
        {
            if (text.Contains(m_sEngine))
                SpawnPiece(text, 0);
            else if (text.Contains(m_sTail))
                SpawnPiece(text, 1);
            else if (text.Contains(m_sWing))
                SpawnPiece(text, 2);
            else if (text.Contains(m_sBathub))
                SpawnResource(0);
            else if (text.Contains(m_sCouch))
                SpawnResource(1);
            else if (text.Contains(m_sFridge))
                SpawnResource(3);
            else if (text.Contains(m_sNut))
                SpawnResource(4);
            else if (text.Contains(m_sSrew))
                SpawnResource(5);
            else if (text.Contains(m_sTv))
                SpawnResource(6);
        }
        else if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Game"))
        {
            if (text.Contains(m_sWin))
                GameManager.Instance.Win();
            else if (text.Contains(m_sLose))
                GameManager.Instance.Lose();
            else if (text.Contains(m_sPlatform))
            {
                stability = text.Split(':');
                if (stability.Length == 2)
                {
                    int nbStability = int.Parse(stability[1]);

					if (m_cStabilityProperty != null)
						m_cStabilityProperty = GameManager.Instance.Platform.GetType().GetProperty("Stability", BindingFlags.Public | BindingFlags.Instance);

					m_cStabilityProperty.SetValue(GameManager.Instance.Platform, nbStability, null);
				}
            }
        }
    }

    private void SpawnPiece(string text, int index)
    {
        Piece newPiece = PieceManager.Instance.SpawnObject(index);
        gameObject.transform.position = new Vector3(2.0f, 2.0f, 0.0f);
        gameObject.transform.parent = null;

        UIManager.Instance.ShowNewRecipe(newPiece);

        if (text.Contains(m_sDone))
        {
            newPiece.BlueprintFinish();
            newPiece.SetPieceDebugMovement();
            gameObject.layer = m_iLayerPiece;
        }
        else
            gameObject.layer = m_iLayerBlueprint;
    }

    private void SpawnResource(int index)
    {
        ResourceObjectManager.Instance.Spawn(m_acResourceSettings[index].m_cPrefab, new Vector3(2.0f, 2.0f,0.0f), Quaternion.identity);
    }
}
