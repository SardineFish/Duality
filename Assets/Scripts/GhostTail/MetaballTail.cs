using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

namespace Ghost
{
    public enum GhostState
    {
        Relaxed,
        Alert,
        Attack
    }
    
    [ExecuteInEditMode]
    public class MetaballTail : MonoBehaviour
    {
        private int maxBlobs = 16;
        [SerializeField] private Material material;
        private Material matInstance;
        private List<Vector4> blobs;
        private List<Vector3> blobVel;

        public Animator anim;
        public Transform eyesTransform;

        public float emitInterval = 0.5f;
        public float maxInitialSize = 40;
        public float maxInitialVel = 1;
        public float maxLifetime = 3;
        public float eyesFollowCoeff = 20;
        public float shadowThreshold = 0.386f;
        public Color color = Color.white;
        public Color ShadowColor = Color.gray;
        public Color EdgeColor = Color.black;
        public float EdgeWidth = 4;
        public Vector2 wind = Vector2.zero;
        private float emitCountdown;
        private float maxVel;
        private Vector3 cachedEyePosition;

        [Space(10)]
        public Transform stopsObj;

        public int curStopIndex = 0;

        public float moveSpeed = 1;

        public GhostState curState = GhostState.Relaxed;
        
        [Space(10)]
        public float alertDuration = 1;
        public float curAlertAmount = 0;
        public float alertRestoreRate = 0.5f;

        [Space(10)]
        private float timeSinceAttackStart = 0;
        private Vector3 attackStartPos;
        public float attackDuration = 0.25f;
        
        void Awake()
        {
            // props: xy:=position; w:=radius
            emitCountdown = emitInterval;
            blobs = new List<Vector4>();
            blobVel = new List<Vector3>();
            maxVel = maxInitialVel;
            matInstance = Instantiate<Material>(material);
            GetComponent<SpriteRenderer>().material = matInstance;

            Reset();
        }

        private void OnEnable()
        {
            emitCountdown = emitInterval;
            blobs = new List<Vector4>();
            blobVel = new List<Vector3>();
            cachedEyePosition = eyesTransform.position;
        }

        public void OpenEyes()
        {
            if (anim) anim.SetTrigger("OpenEyes");
            maxVel = maxInitialVel;
        }

        public void CloseEyes()
        {
            if (anim) anim.SetTrigger("CloseEyes");
            maxVel = maxInitialVel * 0.4f;
        }

        void MoveGhost()
        {
            if (!stopsObj) return;
            var stops = stopsObj.gameObject.GetComponentsInChildren<GhostStop>();
            if (stops.Length == 0) return;
            if (stops[curStopIndex].timeTilDepart > 0 && (stops[curStopIndex].transform.position - transform.position).magnitude < 0.01f)
            {
                stops[curStopIndex].timeTilDepart -= Time.deltaTime;
            }
            else
            {
                if (stops[curStopIndex].timeTilDepart < 0) // hasn't transitioned yet. do it now
                {
                    stops[curStopIndex].timeTilDepart = stops[curStopIndex].stopDuration;
                    curStopIndex += 1;
                    if (curStopIndex >= stops.Length)
                    {
                        curStopIndex = 0;
                    }
                }
                
                // move
                // vec to destination
                var posDelta = stops[curStopIndex].transform.position - transform.position;//stops[prevStopIndex].transform.position;
                var maxStep = moveSpeed * Time.deltaTime;
                
                if (posDelta.magnitude > maxStep)
                {
                    transform.position += posDelta.normalized * maxStep;
                }
                else // arrived at stop.
                {
                    transform.position = stops[curStopIndex].transform.position;
                }
            }
        }

        void TransitionState(GhostState newState)
        {
            if (newState == GhostState.Relaxed)
            {
                curState = newState;
            }
            else if (newState == GhostState.Alert)
            {
                curState = newState;
                curAlertAmount = 0;
            }
            else if (newState == GhostState.Attack)
            {
                curState = newState;
                timeSinceAttackStart = 0;
                attackStartPos = transform.position;
            }

            if (newState != GhostState.Alert)
            {
                curAlertAmount = 0;
            }
        }

        void Reset()
        {
            OpenEyes();
            if (!stopsObj) return;
            var stops = stopsObj.gameObject.GetComponentsInChildren<GhostStop>();
            if (stops.Length > 0)
            {
                transform.position = stops[0].transform.position;
            }
            TransitionState(GhostState.Relaxed);
        }

        void Update()
        {
            if (Application.isPlaying)
            {
                if (curState == GhostState.Relaxed)
                {
                    MoveGhost();
                    curAlertAmount = Mathf.Max(0, curAlertAmount - Time.deltaTime * alertRestoreRate);
                }
                else if (curState == GhostState.Alert)
                {
                    curAlertAmount += Time.deltaTime;
                    if (curAlertAmount >= alertDuration)
                    {
                        TransitionState(GhostState.Attack);
                    }
                }
                else if (curState == GhostState.Attack)
                {
                    timeSinceAttackStart += Time.deltaTime;
                    if (timeSinceAttackStart > attackDuration) timeSinceAttackStart = attackDuration;
                    transform.position =
                        Vector3.Lerp(attackStartPos, new Vector3(), timeSinceAttackStart / attackDuration);
                }
            }
            
            //======== render ========
            
            if (!matInstance) return;
            if (!anim) return;
            if (!eyesTransform) return;

            var objectPos = transform.position;
            
            // emit
            if (blobs.Count < maxBlobs && emitCountdown < 0)
            {
                float size = Random.Range(maxInitialSize / 2, maxInitialSize);
                blobs.Add(new Vector4(objectPos.x, objectPos.y, 0, size));
                blobVel.Add(new Vector3(
                    Random.Range(-maxVel, maxVel),
                    Random.Range(-maxVel, maxVel),
                    size
                    ));
                emitCountdown = emitInterval;
            }
            emitCountdown -= Time.deltaTime;

            var newPos = Vector3.Lerp(cachedEyePosition, transform.position, Time.deltaTime * eyesFollowCoeff);
            newPos.z = -1;
            eyesTransform.position = newPos;
            
            // emit from a world position toward a random dir
            // each pt then update 
            for (int i = blobs.Count - 1; i >= 0; i--)
            {
                if (blobs[i].z > maxLifetime)
                {
                    blobs.RemoveAt(i);
                    blobVel.RemoveAt(i);
                    continue;
                }

                blobVel[i] = new Vector3(blobVel[i].x + wind.x, blobVel[i].y + wind.y, blobVel[i].z);
                var pos = blobs[i] + new Vector4(blobVel[i].x, blobVel[i].y, 0, 0) * Time.deltaTime;
                pos.z += Time.deltaTime;
                float life = blobs[i].z;
                float maxSize = blobVel[i].z;
                if (life < maxLifetime * 0.15)
                {
                    pos.w = Mathf.Lerp(0,maxSize, life / 0.15f);
                }
                else
                {
                    pos.w = Mathf.Lerp(maxSize, 0, (life - 0.15f) / (maxLifetime - 0.15f));
                }
                blobs[i] = pos;
            }

            if (blobs.Count == 0) return;
            
            for (int i = 0; i < blobs.Count; i++)
            {
                matInstance.SetVector("_P" + i, blobs[i]);
            }
            matInstance.SetInt("_NumPoints", Mathf.Min(blobs.Count, 16));
            matInstance.SetColor("_Color", color);
            matInstance.SetColor("_EdgeColor", EdgeColor);
            matInstance.SetColor("_ShadowColor", ShadowColor);
            matInstance.SetFloat("_EdgeWidth", EdgeWidth);
            matInstance.SetFloat("_ShadowThreshold", shadowThreshold);

            cachedEyePosition = eyesTransform.position;
            
            //---- DEBUG ----
            if (Input.GetKeyDown(KeyCode.Y))
            {
                OpenEyes();
            }

            if (Input.GetKeyDown(KeyCode.U))
            {
                CloseEyes();
            }

            if (Input.GetKeyDown(KeyCode.I))
            {
                TransitionState(GhostState.Alert);
            }

            if (Input.GetKeyDown(KeyCode.O))
            {
                Debug.Log("reset");
                Reset();
            }
        }
    }
}