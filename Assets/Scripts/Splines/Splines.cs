using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Catmull-Rom spline
public class Splines : MonoBehaviour
{
    [SerializeField]
    private Transform[] m_cTransformPoint = new Transform[4];
    private int m_iTransformPointSize = 0;
    public int TransformPointSize { get { return m_iTransformPointSize; } }

    private List<Vector3> m_lvAllPointInTheSpline = new List<Vector3>();
    public List<Vector3> AllPointInTheSpline { get { return m_lvAllPointInTheSpline; } }

    private float m_fResolution = 0.01f;

    [SerializeField]
    private bool m_bIsLooping = false;

    private int m_iNbOfLoop = 0;

#if UNITY_EDITOR
    [SerializeField]
    [Header("Debug")]
    private bool m_bDrawSpline = true;
#endif

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        m_iTransformPointSize = m_cTransformPoint.Length;
        m_iNbOfLoop = Mathf.FloorToInt(1 / m_fResolution);
        m_lvAllPointInTheSpline.Clear();
    }

    private Vector3 CalculeSplineBtwn4Point(float resolution, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        Vector3 a = 2f * p2;
        Vector3 b = p3 - p1;
        Vector3 c = (2f * p1) - (5f * p2) + (4f * p3) - p4;
        Vector3 d = -p1 + (3f * p2) - (3f * p3) + p4;

        //The cubic polynomial: a + b * t + c * t^2 + d * t^3
        Vector3 pos = 0.5f * (a + (b * resolution) + (c * resolution * resolution) + (d * resolution * resolution * resolution));

        return pos;
    }

    public Vector3 GetPointInTheCurve(int idxCurve, float time)
    {
        return FindPosInCurve(m_cTransformPoint[idxCurve].position, idxCurve, time);
    }

    public Vector3 GetPointInTheCurve(Vector3 position, int idxCurve, float time)
    {
        return FindPosInCurve(position, idxCurve, time);
    }

    private Vector3 FindPosInCurve(Vector3 position, int idxCurve, float time)
    {
        Vector3 p1 = m_cTransformPoint[(m_iTransformPointSize + idxCurve - 1) % m_iTransformPointSize].position;
        Vector3 p2 = position;
        Vector3 p3 = m_cTransformPoint[(idxCurve + 1) % m_iTransformPointSize].position;
        Vector3 p4 = m_cTransformPoint[(idxCurve + 2) % m_iTransformPointSize].position;

        float resolution = time; //(time * 10) * m_fResolution;
        Vector3 pos = CalculeSplineBtwn4Point(resolution, p1, p2, p3, p4);
        return CalculeSplineBtwn4Point(resolution, p1, p2, p3, p4);
    }

#if UNITY_EDITOR
    private void DisplaySpline(int index)
    {
        Vector3 p1 = m_cTransformPoint[(m_iTransformPointSize + index - 1) % m_iTransformPointSize].position;
        Vector3 p2 = m_cTransformPoint[index].position;
        Vector3 p3 = m_cTransformPoint[(index + 1) % m_iTransformPointSize].position;
        Vector3 p4 = m_cTransformPoint[(index + 2) % m_iTransformPointSize].position;

        Vector3 lastPos = p2;

        for (int idx = 0; idx < m_iNbOfLoop; idx++)
        {
            float resolution = idx * m_fResolution;

            Vector3 newPos = CalculeSplineBtwn4Point(resolution, p1, p2, p3, p4);

            m_lvAllPointInTheSpline.Add(newPos);
            Gizmos.DrawLine(lastPos,newPos);
            lastPos = newPos;
        }
    }

    private void DrawAllSplines()
    {
        for (int idx = 0; idx < m_iTransformPointSize; idx++)
        {
            if (!m_bIsLooping && (idx == 0 || idx == m_iTransformPointSize - 1))
                continue;

            DisplaySpline(idx);
        }
    }

    private void OnDrawGizmos()
    {
        if (m_bDrawSpline)
        {
            Gizmos.color = Color.white;
            Init();
            DrawAllSplines();
        }
    }
    #endif


}
