using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowSplines : MonoBehaviour
{
    public event System.Action OnFinishSpline = null;
    public event System.Action OnAlmostFinished = null;

    private Transform m_cTransform = null;

    [Header("Anim Ending")]
    private Vector3 m_vTargetPosition = Vector3.zero;
    [SerializeField]
    private Splines m_cSplines = null;
    private int m_iIndexSpline = 1;
    private float m_fCurrTimeInTheSpline = 0.0f;
    [SerializeField]
    private float m_fSpeedFlying = 2.0f;
    private float m_fAccelerationFlying = 0.0f;
    [SerializeField]
    private float m_fSpeedMax = 3.0f;
    [SerializeField]
    private float m_fSpeedRotate = 1.0f;
    [SerializeField]
    private float m_fReachedDistance = 0.5f;

    [SerializeField]
    private Vector3 m_vAxisRotation = Vector3.zero;
    private Quaternion m_qAxisRotation = Quaternion.identity;

    private Quaternion m_qStartRotationSpaceship;
    private Quaternion m_qEndRotationSpaceship;

    private float m_fTimeCountRotation = 0.0f;

    private Vector3 m_vDir;

    private bool m_bIsFollowSpline = false;

    private bool m_bIsRotate = true;

    private void Awake()
    {
        m_cTransform = GetComponent<Transform>();
        m_qAxisRotation = Quaternion.Euler(m_vAxisRotation);
    }

	private void LateUpdate ()
    {
        if (m_bIsFollowSpline)
            Move();
    }

    public void StartMoving(bool rotate = true)
    {
        m_bIsFollowSpline = true;
        m_bIsRotate = rotate;
        m_cSplines.transform.position = m_cTransform.position;
        m_vTargetPosition = m_cSplines.GetPointInTheCurve(m_iIndexSpline, m_fCurrTimeInTheSpline);
        m_qStartRotationSpaceship = m_cTransform.rotation;
    }

    private void Move()
    {
        if (m_bIsRotate)
        {
            m_cTransform.LookAt(m_vTargetPosition);

            Quaternion targetRot = m_cTransform.rotation;
            targetRot *= m_qAxisRotation;
            m_cTransform.rotation = targetRot;

            m_qEndRotationSpaceship = m_cTransform.rotation;
            m_cTransform.rotation = m_qStartRotationSpaceship;

            m_fTimeCountRotation += Time.deltaTime * m_fSpeedRotate;
            m_cTransform.rotation = Quaternion.Lerp(m_qStartRotationSpaceship, m_qEndRotationSpaceship, m_fTimeCountRotation);
        }

        m_fAccelerationFlying += m_fSpeedFlying * Time.deltaTime;
        m_fAccelerationFlying = Mathf.Clamp(m_fAccelerationFlying, 0.0f, m_fSpeedMax);

        m_cTransform.position = Vector3.MoveTowards(m_cTransform.position, m_vTargetPosition, m_fAccelerationFlying * Time.deltaTime);
        //m_cPlatformTransform.position += (m_vTargetPosition - m_cPlatformTransform.position).normalized * m_fAccelerationFlying;

        if (HasReachedCrtTarget())
        {
            m_qStartRotationSpaceship = m_cTransform.rotation;
            m_fTimeCountRotation = 0.0f;
            m_fCurrTimeInTheSpline += Time.deltaTime * m_fSpeedFlying;
            m_vTargetPosition = m_cSplines.GetPointInTheCurve(m_iIndexSpline, m_fCurrTimeInTheSpline);

            if (m_fCurrTimeInTheSpline >= 1.0f)
            {
                m_fCurrTimeInTheSpline = 0.0f;
                m_iIndexSpline++;

                if (m_iIndexSpline == m_cSplines.TransformPointSize - 3)
                {
                    if (OnAlmostFinished != null)
                        OnAlmostFinished();
                }

                if (m_iIndexSpline == m_cSplines.TransformPointSize - 2)
                {
                    m_bIsFollowSpline = false;
                    if (OnFinishSpline != null)
                        OnFinishSpline();
                }
            }
        }
    }

    private bool HasReachedCrtTarget()
    {
        if ((m_vTargetPosition - m_cTransform.position).magnitude <= m_fReachedDistance)
            return true;
        return false;
    }
}
