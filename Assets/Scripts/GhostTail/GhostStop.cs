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
    void Update()
    {
        /*
        var r = new Vector2(debugRadius, debugRadius);
        var pos = new Vector2(transform.position.x - debugRadius, transform.position.y - debugRadius);
        var size = new Vector2(debugRadius*2, debugRadius*2);
        SardineFish.Utils.Utility.DebugDrawRect(new Rect(pos, size));
        */
    }
}
