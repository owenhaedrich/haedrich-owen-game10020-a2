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

        _rigidbody.linearVelocity = finalSpeed * moveDirection;
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
