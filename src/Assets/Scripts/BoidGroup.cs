using UnityEngine;

public class BoidGroup : MonoBehaviour
{
    private Boid[] _boids;

    [SerializeField] private Transform _targetTransform;

    private void Start()
    {
        _boids = FindObjectsByType<Boid>(FindObjectsSortMode.None);
    }


    private void Update()
    {
        foreach (var boidA in _boids)
        {
            int numPerceivedFlockmates = 0;
            int numDangerFlockmates = 0;
            Vector2 centerOfFlock = Vector2.zero;

            Vector2 offsetToCenterOfFlock = Vector2.zero;
            Vector2 avgFlockHeading = Vector2.zero;
            Vector2 avgSeparationHeading = Vector2.zero;

            Vector2 offsetToTarget = Vector2.zero;
            if (_targetTransform != null)
                offsetToTarget = (_targetTransform.position - boidA.transform.position);

            foreach (var boidB in _boids)
            {
                if (boidA == boidB)
                    continue;

                Vector2 offset = boidB.transform.position - boidA.transform.position;
                float distance = offset.magnitude;

                if (distance <= boidA.perceptionRadius)
                {
                    numPerceivedFlockmates++;

                    if (Vector2.Angle(boidA.velocity, offset) <= boidA.fieldOfView)
                    {
                        centerOfFlock += (Vector2)boidB.transform.position;
                        avgFlockHeading += boidB.velocity;
                    }

                    if (distance <= boidA.avoidanceRadius)
                    {
                        numDangerFlockmates++;

                        float inverseFract = 1f - (distance / boidA.avoidanceRadius);
                        avgSeparationHeading -= offset * inverseFract;
                    }
                }
            }

            centerOfFlock /= numPerceivedFlockmates;
            offsetToCenterOfFlock = centerOfFlock - (Vector2)boidA.transform.position;

            avgFlockHeading /= numPerceivedFlockmates;
            //avgSeparationHeading /= numDangerFlockmates;

            boidA.UpdateBoid(offsetToCenterOfFlock, avgFlockHeading, avgSeparationHeading, offsetToTarget);
        }
    }
}