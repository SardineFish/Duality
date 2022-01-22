using System;
using UnityEngine;

namespace Rendering
{
    [RequireComponent(typeof(Camera))]
    public class PlayerCamera : MonoBehaviour
    {
        private Camera _camera;
        private RenderTexture outputRT;

        public RenderTexture OutputRT => outputRT;
        
        private void Awake()
        {
            _camera = GetComponent<Camera>();

            outputRT = new RenderTexture(Screen.width / 2, Screen.height, 0);
            _camera.targetTexture = outputRT;
        }
    }
}