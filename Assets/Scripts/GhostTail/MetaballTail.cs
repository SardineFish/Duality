using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

namespace Ghost
{
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
        private float emitCountdown;
        private float maxVel;
        private Vector3 cachedEyePosition;
        void Awake()
        {
            // props: xy:=position; w:=radius
            emitCountdown = emitInterval;
            blobs = new List<Vector4>();
            blobVel = new List<Vector3>();
            maxVel = maxInitialVel;
            matInstance = Instantiate<Material>(material);
            GetComponent<SpriteRenderer>().material = matInstance;
        }

        private void OnEnable()
        {
            emitCountdown = emitInterval;
            blobs = new List<Vector4>();
            blobVel = new List<Vector3>();
            cachedEyePosition = eyesTransform.position;
            CloseEyes();
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

        void Update()
        {
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
        }
    }
}