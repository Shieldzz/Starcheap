using UnityEngine;


public class TutorialTaskActionRotatePlayer : TutorialTaskAction
{
    [SerializeField]
    private Vector3 m_vRotationPlayer = Vector3.zero; 

    private Controller[] m_acControllers = new Controller[4];

    private int m_iLayerMaskPlayer;


    #region MonoBeheviour
    private void Start()
    {
        m_acControllers = GameManager.Instance.Controllers;

        m_iLayerMaskPlayer = (1 << LayerMask.NameToLayer("Player"));
    }

    #endregion

    protected override void SetTaskAction()
    {
        TaskAction += () => { RotatePlayer(); };
    }

    private void RotatePlayer()
    {
        int idxPlayer = 0;
        foreach (Controller controller in m_acControllers)
        {
            if (controller.LockGameInputs)
            {
                idxPlayer = controller.PlayerNumber;
                break;
            }
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, GetComponent<SphereCollider>().radius, m_iLayerMaskPlayer);

        foreach (Collider collider in  colliders)
        {
            Player player = collider.GetComponentInParent<Player>();

            if (player != null && idxPlayer == player.m_iIndex)
            {
                player.Transform.rotation = Quaternion.Euler(m_vRotationPlayer);
                break;
            }
        }
    }


}
