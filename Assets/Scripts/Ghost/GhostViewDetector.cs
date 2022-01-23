using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SardineFish.Utils;

public class GhostViewDetector : MonoBehaviour
{
    [SerializeField]
    private LayerMask m_playerLayer;

    [SerializeField]
    private LayerMask m_occlusionLayer;

    [SerializeField] [Range(0.0f, 180.0f)] private float m_viewAngle;

    [SerializeField] [Range(2, 10)] private int m_detectRayNum;

    [SerializeField] [Range(0.0f, 5.0f)] private float m_detectLength;

    private float _castIntervalRadian => Mathf.Deg2Rad * m_viewAngle / m_detectRayNum;

    private float _startRadian => -Mathf.Deg2Rad * m_viewAngle / 2.0f;

    private readonly HashSet<Collider2D> RayHitColliders = new HashSet<Collider2D>();
    private Mesh mesh;

    private void Awake()
    {
        mesh = new Mesh();
    }

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

    private void Update()
    {
        UpdateViewMesh();
    }

    public Transform PlayerInView()
    {
        Debug.DrawLine(transform.position, transform.position + transform.right * m_detectLength, Color.blue);
        RayHitColliders.Clear();
        Vector3 worldPos = transform.position;
        // Vector3 worldRight = Vector3.Scale(transform.parent.right, Vector3.one * transform.lossyScale.x).normalized;
        var worldRight = transform.right;
        Transform viewResult = null;
        for (int i = 0; i < m_detectRayNum; i++)
        {
            RaycastHit2D ithHit = RaycastWithDebug(worldPos,
                                                    MathUtility.Rotate(worldRight,
                                                                              _startRadian + i * _castIntervalRadian),
                                                    m_detectLength, m_playerLayer | m_occlusionLayer);

            if (ithHit && ithHit.collider)
            {
                RayHitColliders.Add(ithHit.collider);
            }

            if (ithHit && ithHit.transform.CompareTag("Player"))
            {
                viewResult = ithHit.transform;   
            }
        }
        
        return viewResult;
    }

    RaycastHit2D RaycastWithDebug(Vector2 origin, Vector2 direction, float detectLength, LayerMask layer)
    {
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, detectLength, layer);
        Debug.DrawRay(origin, direction * (hit ? hit.distance : detectLength), Color.magenta);
        return hit;
    }

    void UpdateViewMesh()
    {
        var points = new List<Vector2>();
        mesh.Clear();
        foreach (var collider in RayHitColliders)
        {
            switch (collider)
            {
                case CompositeCollider2D composite:
                    for (var i = 0; i < composite.pathCount; ++i)
                    {
                        var list = new List<Vector2>();
                        composite.GetPath(i, list);
                        points.AddRange(list.Select(point => composite.transform.localToWorldMatrix.MultiplyPoint(point).ToVector2())
                            .Where(point => InViewRange(point) && CheckOcclusion(point)));
                    }
                    break;
                case BoxCollider2D box:
                {
                    var list = new List<Vector2>();
                    list.Add(collider.transform.localToWorldMatrix.MultiplyPoint(
                        new Vector3(-box.size.x + box.offset.x, -box.size.y + box.offset.y, 0)));
                    list.Add(collider.transform.localToWorldMatrix.MultiplyPoint(
                        new Vector3(box.size.x + box.offset.x, -box.size.y + box.offset.y, 0)));
                    list.Add(collider.transform.localToWorldMatrix.MultiplyPoint(
                        new Vector3(box.size.x + box.offset.x, box.size.y + box.offset.y, 0)));
                    list.Add(collider.transform.localToWorldMatrix.MultiplyPoint(
                        new Vector3(-box.size.x + box.offset.x, box.size.y + box.offset.y, 0)));
                    
                    points.AddRange(list
                        .Where(point => InViewRange(point) && CheckOcclusion(point)));
                    break;
                }
            }
        }
    }
    
    bool InViewRange(Vector2 point)
    {
        var worldRight = transform.right;
        var dir = point - transform.position.ToVector2();
        var dot = Vector2.Dot(worldRight, dir.normalized);
        if (dot > Mathf.Cos(Mathf.Deg2Rad * m_viewAngle / 2))
        {
            return true;
        }

        return false;
    }

    bool CheckOcclusion(Vector2 point)
    {
        var dir = (point - transform.position.ToVector2());
        RaycastHit2D ithHit = RaycastWithDebug(transform.position, dir.normalized,
            dir.magnitude + 1, m_occlusionLayer);
        return ithHit && Mathf.Abs(ithHit.distance - dir.magnitude) < 0.1;
    }
    
}
