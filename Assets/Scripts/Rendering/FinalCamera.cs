using System;
using UnityEngine;
using UnityEngine.UI;

namespace Rendering
{
    [RequireComponent(typeof(Camera))]
    public class FinalCamera : MonoBehaviour
    {
        [SerializeField] private PlayerCamera LeftCamera;
        [SerializeField] private PlayerCamera RightCamera;

        [SerializeField] private RawImage LeftImage;
        [SerializeField] private RawImage RightImage;

        private void Start()
        {
            LeftImage.texture = LeftCamera?.OutputRT;
            RightImage.texture = RightCamera?.OutputRT;
        }
    }
}