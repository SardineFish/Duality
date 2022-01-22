using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SardineFish.Utils;

public class GhostViewDetector : MonoBehaviour
{
    [SerializeField] private LayerMask m_viewDetectLayer;

    [SerializeField] [Range(0.0f, 180.0f)] private float m_viewAngle;

    [SerializeField] [Range(2, 10)] private int m_detectRayNum;

    [SerializeField] [Range(0.0f, 5.0f)] private float m_detectLength;

    private float _castIntervalRadian => Mathf.Deg2Rad * m_viewAngle / m_detectRayNum;

    private float _startRadian => -Mathf.Deg2Rad * m_viewAngle / 2.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (PlayerInView())
        {
            Debug.Log("Ghost Sees the Player");
        }
    }

    bool PlayerInView()
    {
        Vector3 worldPos = transform.position;
        Vector3 worldRight = Vector3.Scale(transform.parent.right, Vector3.one * transform.lossyScale.x).normalized;
        bool viewResult = false;
        for (int i = 0; i < m_detectRayNum; i++)
        {
            RaycastHit2D ithHit = RaycastWithDebug(worldPos,
                                                    MathUtility.Rotate(worldRight,
                                                                              _startRadian + i * _castIntervalRadian),
                                                    m_detectLength, m_viewDetectLayer);

            if (ithHit && ithHit.transform.CompareTag("Player"))
                viewResult = true;
        }
        
        return viewResult;
    }

    RaycastHit2D RaycastWithDebug(Vector2 origin, Vector2 direction, float detectLength, LayerMask layer)
    {
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, detectLength, layer);
        Debug.DrawRay(origin, direction * (hit ? hit.distance : detectLength), Color.magenta);
        return hit;
    }
}
