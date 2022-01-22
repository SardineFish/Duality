using System;
using UnityEngine;

namespace SardineFish.Utils
{
    public class CriticalDamping : MonoBehaviour
    {
        public Transform Target;
        [SerializeField] private float SmoothTime = 0.5f;
        [SerializeField] private float AngularSmoothTime = 0.1f;
        
        public Vector3 velocity = Vector3.zero;
        Quaternion angularVelocity = Quaternion.identity;

        private Vector3 CriticalDamp(Vector3 from, Vector3 to, ref Vector3 velocity, float smoothTime, float dt)
        {
            float omega = 2.0f / smoothTime;
            float x = omega * dt;
            float exp = 1.0f / (1.0f + x + 0.48f * x * x + 0.235f * x * x * x);
            var change = from - to;
            var temp = (velocity + omega * change) * dt;
            velocity = (velocity - omega * temp) * exp;
            return to + (change + temp) * exp;
        }

        private Quaternion AngularVelocity(Quaternion from, Quaternion to, float dt)
        {
            Quaternion delta = (to * Quaternion.Inverse(from)).normalized;
            delta.ToAngleAxis(out var angle, out var axis);
            axis *= angle * Mathf.Deg2Rad / dt;
            return new Quaternion(axis.x, axis.y, axis.z, 0);
        }

        private Quaternion CriticalDamp(Quaternion from, Quaternion to, ref Quaternion velocityQuat, float smoothTime,
            float dt)
        {
            float omega = 2.0f / smoothTime;
            float x = omega * dt;
            float exp = 1.0f / (1.0f + x + 0.48f * x * x + 0.235f * x * x * x);

            var change = AngularVelocity(to, from, 1).ToVec4();
            var velocity = velocityQuat.ToVec4();
            var temp = (velocity + omega * change) * dt;
            velocity = (velocity - omega * temp) * exp;

            return (to.ToVec4() + (((change + temp) * exp / 2).ToQuat() * to).ToVec4()).ToQuat().normalized;
        }
        
        private void Update()
        {
            if(!Target || SmoothTime <= 0)
                return;

            transform.position = CriticalDamp(transform.position, Target.position, ref velocity, SmoothTime,
                Time.deltaTime);
            transform.rotation = CriticalDamp(transform.rotation, Target.rotation, ref angularVelocity, AngularSmoothTime, Time.deltaTime);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, transform.right);
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, transform.up);
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, transform.forward);
            
        }
    }
}