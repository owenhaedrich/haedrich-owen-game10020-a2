using UnityEngine;

public class RotatingBarrier : MonoBehaviour, ISnapshottable
{
    public ToggleColour toggleColour = ToggleColour.Red;
    public float rotationSpeed = 1.0f;

    private Rigidbody _rigidbody;
    private BoxCollider _barrierCollider;

    private bool _active = false;
    private bool _inPosition = false;
    private Quaternion _targetRotation;
    private Quaternion _closedRotation;
    private Quaternion _openRotation;
    private Quaternion _snapshotRotation;
    private bool _hasSnapshot = false;

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

        PressurePlate[] pressurePlates = FindObjectsByType<PressurePlate>(FindObjectsSortMode.None);
        
        foreach (PressurePlate pressurePlate in pressurePlates)
        {
            if (pressurePlate.toggleColour == toggleColour)
            pressurePlate.onToggle.AddListener(Toggle);
        }

        TimeSword timeSword = FindFirstObjectByType<TimeSword>();
        timeSword.onRestore.AddListener(Restore);
    }

    private void OnTriggerStay(Collider other)
    {
        if (_inPosition) return;

        if (other.GetComponent<IPushable>() != null)
        {
            Vector3 collisionPoint = _barrierCollider.ClosestPoint(other.transform.position);
            Vector3 direction = (other.transform.position - collisionPoint).normalized;
            
            // Calculate linear velocity at the collision point: angular velocity * radius
            float angularSpeedRad = rotationSpeed * Mathf.Deg2Rad;
            float radius = Vector3.Distance(transform.position, collisionPoint);
            float pushSpeed = angularSpeedRad * radius;

            other.GetComponent<IPushable>().Push(direction, pushSpeed);
        }
    }

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

    private void Toggle(bool active, ToggleColour incomingToggleColour)
    {
        if (incomingToggleColour != toggleColour) return;

        _active = active;
        _inPosition = false ;

        if (_active)
        {
            _targetRotation = _openRotation;
        }
        else
        {
            _targetRotation = _closedRotation;
        }
    }

    public void Snapshot()
    {
        _snapshotRotation = transform.rotation;
        _hasSnapshot = true;
    }

    public void Restore()
    {
        if (_hasSnapshot)
        {
            _rigidbody.rotation = _snapshotRotation;
            _hasSnapshot = false;
            _inPosition = false;
        }
    }

    public string GetName()
    {
        return "Rotating Barrier";
    }
}
