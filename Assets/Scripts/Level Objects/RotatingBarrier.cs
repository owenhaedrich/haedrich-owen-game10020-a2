using UnityEngine;

// Listen for the pressure plate's onToggle event and rotate between two positions 90 degrees apart. Push IPushables when rotating.
// Take a snapshot of rotation when triggered by the Time Sword. Listen for the Time Sword's onRestore event to restore that snapshot.
public class RotatingBarrier : MonoBehaviour, ISnapshottable
{
    public ToggleColour toggleColour = ToggleColour.Red;
    public float rotationSpeed = 1.0f;

    private Rigidbody _rigidbody;
    private BoxCollider _barrierCollider;

    private bool _inPosition = false;
    private bool _hasSnapshot = false;
    private Quaternion _targetRotation;
    private Quaternion _closedRotation;
    private Quaternion _openRotation;
    private Quaternion _snapshotRotation;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        BoxCollider[] boxColliders = GetComponents<BoxCollider>();
        foreach (BoxCollider boxCollider in boxColliders)
        {
            if (boxCollider.isTrigger) _barrierCollider = boxCollider;
        }

        _closedRotation = Quaternion.Euler(0, 90, 0);
        _openRotation = Quaternion.Euler(0, 0, 0);
        _targetRotation = _closedRotation;
    }

    private void Start()
    {
        // Listen to global color state changes from the PressurePlateManager
        PressurePlateManager.Instance.onColorStateChanged.AddListener(OnColorStateChanged);
        // Initialize with current state
        Toggle(PressurePlateManager.Instance.IsColorActive(toggleColour));

        // Listen for onRestore from the Time Sword to restore a snapshot
        TimeSword timeSword = FindFirstObjectByType<TimeSword>();
        timeSword.onRestore.AddListener(Restore);
    }

    private void OnColorStateChanged(ToggleColour colour, bool active)
    {
        if (colour == toggleColour)
        {
            Toggle(active);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (_inPosition) return;

        IPushable pushable = other.GetComponent<IPushable>();
        if (pushable != null)
        {
            Vector3 collisionPoint = _barrierCollider.ClosestPoint(other.transform.position);
            Vector3 direction = (other.transform.position - collisionPoint).normalized;
            
            // Fallback if the object's center is inside the collider (ClosestPoint returns the point itself)
            if (direction == Vector3.zero)
            {
                Vector3 boxCenter = transform.TransformPoint(_barrierCollider.center);
                direction = (other.transform.position - boxCenter).normalized;
                if (direction == Vector3.zero) direction = transform.forward;
            }

            // Determine current rotation direction relative to target
            Vector3 currentForward = transform.forward;
            Vector3 targetForward = _targetRotation * Vector3.forward;
            float signedAngle = Vector3.SignedAngle(currentForward, targetForward, Vector3.up);
            
            // If we are extremely close to the target, don't push (should be caught by _inPosition but added for safety)
            if (Mathf.Abs(signedAngle) < 0.1f) return;

            float rotationSign = Mathf.Sign(signedAngle);
            float angularSpeedRad = rotationSpeed * Mathf.Deg2Rad;
            
            // Calculate tangential velocity at the collision point: v = ω × (P - C)
            // In Unity's coordinate system, a positive rotation around Y is clockwise.
            Vector3 angularVelocity = Vector3.up * (rotationSign * angularSpeedRad);
            Vector3 leverArm = collisionPoint - transform.position;
            Vector3 tangentialVelocity = Vector3.Cross(angularVelocity, leverArm);

            // Only push if the barrier is moving towards the object (positive dot product)
            // This prevents pushing from the "backside" of the barrier.
            if (Vector3.Dot(direction, tangentialVelocity) > 0)
            {
                float radius = leverArm.magnitude;
                float pushSpeed = angularSpeedRad * radius;
                pushable.Push(direction, pushSpeed);
            }
        }
    }

    // Rotate towards the target rotation then stop when the target rotation is reached
    private void FixedUpdate()
    {
        if (!_inPosition)
        {
            Quaternion nextRotation = Quaternion.RotateTowards(_rigidbody.rotation, _targetRotation, rotationSpeed * Time.fixedDeltaTime);
            _rigidbody.MoveRotation(nextRotation.normalized);

            if (Quaternion.Angle(_rigidbody.rotation, _targetRotation) < 0.1f)
            {
                _inPosition = true;
            }
        }
    }

    // Set target rotation when the pressure plate fires onToggle
    private void Toggle(bool active)
    {
        _inPosition = false ;

        if (active)
        {
            _targetRotation = _openRotation;
        }
        else
        {
            _targetRotation = _closedRotation;
        }
    }

    // ISnapshottable usage: store rotation
    public void Snapshot()
    {
        _snapshotRotation = transform.rotation;
        _hasSnapshot = true;
    }

    // ISnapshottable usage: restore rotation
    public void Restore()
    {
        if (_hasSnapshot)
        {
            _rigidbody.rotation = _snapshotRotation;
            _hasSnapshot = false;
            _inPosition = false;
        }
    }

    // ISnapshottable usage: name is "Rotating Barrier"
    public string GetName()
    {
        return "Rotating Barrier";
    }
}
