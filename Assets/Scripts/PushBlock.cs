using UnityEngine;

// Slide when pushed.
// Take a snapshot of position when triggered by the Time Sword. Listen for the Time Sword's onRestore event to restore that snapshot.
public class PushBlock : MonoBehaviour, ISnapshottable, IPushable
{
    public float minimumPushSpeed = 1.0f;

    private Rigidbody _rigidbody;
    private Vector3 _snapshotPosition;
    private bool _hasSnapshot = false;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();

        // Listen for onRestore from the Time Sword to restore a snapshot
        TimeSword timeSword = FindFirstObjectByType<TimeSword>();
        timeSword.onRestore.AddListener(Restore);
    }

    // IPushable usage: only move up, down, left, or right - not diagonally
    public void Push(Vector3 pushDirection, float pushSpeed)
    {
        // Get the closest cardinal direction to the push direction
        Vector3 moveDirection = Vector3.zero;

        if (Mathf.Abs(pushDirection.x) > Mathf.Abs(pushDirection.z))
        {
            moveDirection = Vector3.right * Mathf.Sign(pushDirection.x);
            _rigidbody.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        }
        else
        {
            moveDirection = Vector3.forward * Mathf.Sign(pushDirection.z);
            _rigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezeRotation;
        }

        // Slide in that direction
        float finalSpeed = Mathf.Max(pushSpeed, minimumPushSpeed);

        _rigidbody.linearVelocity = finalSpeed * moveDirection;
    }

    // ISnapshottable usage: store position
    public void Snapshot()
    {
        _snapshotPosition = transform.position;
        _hasSnapshot = true;
    }

    // ISnapshottable usage: restore position
    public void Restore()
    {
        if (_hasSnapshot)
        {
            _rigidbody.position = _snapshotPosition;
            _hasSnapshot = false;
        }
    }

    // ISnapshottable usage: name is "Push Block"
    public string GetName()
    {
        return "Push Block";
    }
}
