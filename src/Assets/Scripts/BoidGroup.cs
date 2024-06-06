using UnityEngine;

public class BoidGroup : MonoBehaviour
{
    private Boid[] _boids;

    [SerializeField] private int maxFlockSize;
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

            Color avgFlockColor = Color.magenta;

            foreach (var boidB in _boids)
            {
                if (boidA == boidB)
                    continue;

                Vector2 offset = boidB.transform.position - boidA.transform.position;
                float distance = offset.magnitude;

                if (distance <= boidA.perceptionRadius && Vector2.Angle(boidA.velocity, offset) <= boidA.fieldOfView && numPerceivedFlockmates <= maxFlockSize)
                {
                    numPerceivedFlockmates++;
                    centerOfFlock += (Vector2)boidB.transform.position;
                    avgFlockHeading += boidB.velocity;

                    if (distance <= boidA.avoidanceRadius)
                    {
                        numDangerFlockmates++;
                        avgSeparationHeading -= offset;
                    }

                    avgFlockColor += boidB.color;
                }
            }

            centerOfFlock /= numPerceivedFlockmates;
            offsetToCenterOfFlock = centerOfFlock - (Vector2)boidA.transform.position;

            avgFlockHeading /= numPerceivedFlockmates;
            avgSeparationHeading /= numDangerFlockmates;

            avgFlockColor /= numPerceivedFlockmates;

            boidA.UpdateBoid(offsetToCenterOfFlock, avgFlockHeading, avgSeparationHeading, offsetToTarget);
            boidA.ChangeColor(avgFlockColor);
        }
    }
}
