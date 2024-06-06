using UnityEngine;

public class Boid : MonoBehaviour
{
    public Color color { get { return _cachedMeshRenderer.material.color; } }

    public float perceptionRadius, fieldOfView, avoidanceRadius;

    [SerializeField]
    private float _minSpeed, _maxSpeed, _maxSteerForce;
    [SerializeField]
    private float _cohesionWeight, _alignmentWeight, _separationWeight, _followWeight;

    // Object Avoidance
    [SerializeField]
    private int _resolution;
    [SerializeField]
    private float _detectionDist, _objectAvoidanceWeight;
    [SerializeField]
    private LayerMask _objectLayer;

    public Vector2 velocity { get; private set; }

    private Transform _cachedTransform;
    private Rigidbody2D _cachedRb;
    private MeshRenderer _cachedMeshRenderer;

    private void Awake()
    {
        _cachedTransform = transform;
        _cachedRb = GetComponent<Rigidbody2D>();
        _cachedMeshRenderer = GetComponent<MeshRenderer>();
        Material materialInstance = Instantiate(_cachedMeshRenderer.material) as Material;
        materialInstance.color = Random.ColorHSV();
        _cachedMeshRenderer.material = materialInstance;
    }

    public void Start()
    {
        float startSpeed = (_minSpeed + _maxSpeed) / 2;
        Vector2 forward = Random.insideUnitCircle;
        velocity = forward * startSpeed;
    }

    public void UpdateBoid(Vector2 offsetToCenterOfFlock, Vector2 avgFlockHeading, Vector2 avgSeparationHeading, Vector2 offsetToTarget)
    {
        Vector2 acceleration = Vector2.zero;

        Vector2 cohesionForce = SteerTowards(offsetToCenterOfFlock) * _cohesionWeight;
        Vector2 alignmentForce = SteerTowards(avgFlockHeading) * _alignmentWeight;
        Vector2 separationForce = SteerTowards(avgSeparationHeading) * _separationWeight;

        Vector2 objectAvoidanceForce = SteerTowards(AvoidObjects()) * _objectAvoidanceWeight;

        Vector2 followTargetForce = SteerTowards(offsetToTarget) * _followWeight;

        acceleration = cohesionForce + alignmentForce + separationForce + objectAvoidanceForce + followTargetForce;
        velocity += acceleration * Time.fixedDeltaTime;

        float speed = Mathf.Clamp(velocity.magnitude, _minSpeed, _maxSpeed);
        velocity = velocity.normalized * speed;

        _cachedRb.velocity = velocity;
    }

    public void ChangeColor(Color color)
    {
        _cachedMeshRenderer.material.color = color;
    }

    private Vector2 AvoidObjects()
    {
        Vector2 force = Vector2.zero;

        float angle = (180f / _resolution) * Mathf.Deg2Rad;
        bool impendingCollision = false;
        float currentHeadingAngle = Mathf.Atan2(velocity.y, velocity.x);
        for (int i = 0; i < _resolution; i++)
        {

            Vector2 dir = new Vector2(Mathf.Cos(currentHeadingAngle + angle * i - Mathf.PI / 2), Mathf.Sin(currentHeadingAngle + angle * i - Mathf.PI / 2));
            RaycastHit2D hitInfo = Physics2D.Raycast(_cachedTransform.position, dir, _detectionDist, _objectLayer);
            if (hitInfo.collider != null)
            {
                impendingCollision = true;
                force -= dir * Mathf.Max(_detectionDist - hitInfo.distance, _detectionDist * 0.5f);
            }
            else
            {
                force += dir *= _detectionDist;
            }
        }

        force /= _resolution;
        return impendingCollision ? force : Vector2.zero;
    }

    private Vector2 SteerTowards(Vector2 vector)
    {
        Vector2 force = vector.normalized * _maxSpeed - velocity;
        force = Vector2.ClampMagnitude(force, _maxSteerForce);

        return force;
    }

    private void OnDrawGizmos()
    {
        if (_cachedTransform != null)
        {
            Gizmos.DrawRay(_cachedTransform.position, velocity.normalized * 1.5f);

            //float angle = (180f / _resolution) * Mathf.Deg2Rad;
            //float currentHeadingAngle = Mathf.Atan2(velocity.y, velocity.x);
            //for (int i = 0; i < _resolution; i++)
            //{
            //    Vector2 dir = new Vector2(Mathf.Cos(currentHeadingAngle + angle * i - Mathf.PI / 2), Mathf.Sin(currentHeadingAngle + angle * i - Mathf.PI / 2));
            //    Gizmos.DrawRay(_cachedTransform.position, dir * _detectionDist);
            //}
        }
    }
}

public struct BoidSettings
{
    public float perceptionRadius;
    public float minSpeed, maxSpeed;
}