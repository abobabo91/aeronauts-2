using UnityEngine;

namespace Aeronauts2.Combat
{
    public sealed class TargetMover : MonoBehaviour
    {
        [SerializeField] private Vector3 pointA;
        [SerializeField] private Vector3 pointB;
        [SerializeField, Min(0f)] private float speed = 10f;

        private float distanceTravelled;

        public void Configure(Vector3 firstPoint, Vector3 secondPoint, float movementSpeed)
        {
            pointA = firstPoint;
            pointB = secondPoint;
            speed = movementSpeed;
            transform.position = pointA;
        }

        private void FixedUpdate()
        {
            float pathLength = Vector3.Distance(pointA, pointB);
            if (pathLength <= 0.001f || speed <= 0f)
            {
                return;
            }

            distanceTravelled += speed * Time.fixedDeltaTime;
            float interpolation = Mathf.PingPong(distanceTravelled, pathLength) / pathLength;
            transform.position = Vector3.Lerp(pointA, pointB, interpolation);
        }
    }
}
