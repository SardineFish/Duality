using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostStop : MonoBehaviour
{
    public float stopDuration = 2;
    public float timeTilDepart;

    void Start()
    {
        timeTilDepart = stopDuration;
    }

    //public float debugRadius = 0.1f;
    // Start is called before the first frame update
    // Update is called once per frame
    private void OnDrawGizmos()
    {
        Debug.DrawLine(transform.position, transform.position + transform.right, Color.blue);
    }
}
