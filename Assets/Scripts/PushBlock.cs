using UnityEngine;

public class PushBlock : MonoBehaviour, ISnapshottable, IPushable
{
    public float minimumPushSpeed = 1.0f;

    private Rigidbody _rigidbody;
    private Vector3 _snapshotPosition;
    private bool _hasSnapshot = false;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        TimeSword timeSword = FindFirstObjectByType<TimeSword>();
        timeSword.onRestore.AddListener(Restore);
    }

    public void Push(Vector3 pushDirection, float pushSpeed)
    {
        // Get the closest cardinal direction to the push direction
        Vector3 moveDirection = Vector3.zero;

        if (Mathf.Abs(pushDirection.x) > Mathf.Abs(pushDirection.z))
            moveDirection = Vector3.right * Mathf.Sign(pushDirection.x);
        else
            moveDirection = Vector3.forward * Mathf.Sign(pushDirection.z);

        // Slide in that direction
        float finalSpeed = Mathf.Max(pushSpeed, minimumPushSpeed);
        float moveDistance = finalSpeed * Time.fixedDeltaTime;

        RaycastHit[] hits = _rigidbody.SweepTestAll(moveDirection, moveDistance);
        RaycastHit closestHit = new RaycastHit();
        float minDistance = float.MaxValue;
        bool hitFound = false;

        foreach (var hit in hits)
        {
            // Check if the hit object's layer is in the collisionLayers mask

            bool includedInCollisionLayer = (_rigidbody.includeLayers.value & (1 << hit.collider.gameObject.layer)) != 0;
            if (includedInCollisionLayer && !hit.collider.isTrigger)
            {
                if (hit.distance < minDistance)
                {
                    minDistance = hit.distance;
                    closestHit = hit;
                    hitFound = true;
                }
            }
        }

        if (!hitFound)
        {
            _rigidbody.MovePosition(transform.position + moveDirection * moveDistance);
        }
        else
        {
            // If there's an obstacle, move as close as possible
            _rigidbody.MovePosition(transform.position + moveDirection * (closestHit.distance - 0.01f));
        }
    }
    
    public void Snapshot()
    {
        _snapshotPosition = transform.position;
        _hasSnapshot = true;
    }

    public void Restore()
    {
        if (_hasSnapshot)
        {
            _rigidbody.position = _snapshotPosition;
            _hasSnapshot = false;
        }
    }

    public string GetName()
    {
        return "Push Block";
    }
}
