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
        private List<Vector4> blobs;
        private List<Vector3> blobVel;

        public Camera cam;

        public float emitInterval = 0.5f;
        public float maxInitialSize = 40;
        public float maxInitialVel = 1;
        public float maxLifetime = 3;
        private float emitCountdown;
        void Awake()
        {
            // props: xy:=position; w:=radius
            emitCountdown = emitInterval;
            blobs = new List<Vector4>();
            blobVel = new List<Vector3>();
        }

        private void OnEnable()
        {
            emitCountdown = emitInterval;
            blobs = new List<Vector4>();
            blobVel = new List<Vector3>();
        }

        void Update()
        {
            if (!material) return;
            if (!cam) return;
            
            var objectPos = cam.WorldToScreenPoint(transform.position);//transform.position;
            
            // emit
            if (blobs.Count < maxBlobs && emitCountdown < 0)
            {
                float size = Random.Range(maxInitialSize / 2, maxInitialSize);
                blobs.Add(new Vector4(objectPos.x, objectPos.y, 0, size));
                blobVel.Add(new Vector3(
                    Random.Range(-maxInitialVel, maxInitialVel),
                    Random.Range(-maxInitialVel, maxInitialVel),
                    size
                    ));
                emitCountdown = emitInterval;
            }
            emitCountdown -= Time.deltaTime;
            
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
                //pos.w = Mathf.Lerp(blobVel[i].z, 0, (life) / (maxLifetime));
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
            
            //material.SetVectorArray("_Points", blobs);
            for (int i = 0; i < blobs.Count; i++)
            {
                material.SetVector("_P" + i, blobs[i]);
            }
            material.SetInt("_NumPoints", Mathf.Min(blobs.Count, 16));
        }
    }
}