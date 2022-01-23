using System;
using Ghost;
using UnityEngine;

namespace Duality
{
    [RequireComponent(typeof(Ghost.MetaballTail), typeof(Rigidbody2D))]
    public class Fire : MonoBehaviour
    {
        private MetaballTail _metaballTail;
        private Rigidbody2D _rigidbody2D;

        private int _gravityDir = 1;
        public int GravityDirection
        {
            get => _gravityDir;
            set
            {
                _gravityDir = value;
                _metaballTail.wind.y = Mathf.Abs(_metaballTail.wind.y) * -_gravityDir;
                _rigidbody2D.gravityScale = Mathf.Abs(_rigidbody2D.gravityScale) * -_gravityDir;
            }
        }

        private void Awake()
        {
            _metaballTail = GetComponent<MetaballTail>();
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }
    }
}