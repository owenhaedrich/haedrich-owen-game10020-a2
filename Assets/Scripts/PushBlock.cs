using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

// Slide when pushed.
// Take a snapshot of position when triggered by the Time Sword. Listen for the Time Sword's onRestore event to restore that snapshot.
public class PushBlock : MonoBehaviour, ISnapshottable, IPushable
{
    public float minimumPushSpeed = 1.0f;
    public float pushMultiplier = 1.0f;
    public bool sliding = false;
    public float slideBoost = 1.0f;

    private Rigidbody _rigidbody;
    private bool _isCurrentlySliding = false;
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
    public PushType Push(Vector3 pushDirection, float pushSpeed)
    {
        if (sliding)
        {
            SlidingPush(pushDirection, pushSpeed * pushMultiplier * slideBoost);
            return PushType.Slow;
        }
        else
        {
            DefaultPush(pushDirection, pushSpeed * pushMultiplier);
            return PushType.Default;
        }
    }

    public void DefaultPush(Vector3 pushDirection, float pushSpeed)
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

    public void SlidingPush(Vector3 pushDirection, float pushSpeed)
    {
        StartCoroutine(Slide(pushDirection, pushSpeed));
    }

    public IEnumerator Slide(Vector3 slideDirection, float slideSpeed)
    {
        if (_isCurrentlySliding) yield break;
        _isCurrentlySliding = true;

        // Get the closest cardinal direction to the push direction
        Vector3 moveDirection = Vector3.zero;

        if (Mathf.Abs(slideDirection.x) > Mathf.Abs(slideDirection.z))
        {
            moveDirection = Vector3.right * Mathf.Sign(slideDirection.x);
            _rigidbody.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        }
        else
        {
            moveDirection = Vector3.forward * Mathf.Sign(slideDirection.z);
            _rigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezeRotation;
        }

        // Slide in that direction until something is hit
        float finalSpeed = Mathf.Max(slideSpeed, minimumPushSpeed);

        RaycastHit hit;
        // Use SweepTest to check if there is an obstacle in the way.
        // We use a small distance buffer (0.1f) to detect collision slightly before/at contact.
        while (!_rigidbody.SweepTest(moveDirection, out hit, 0.1f))
        {
            _rigidbody.linearVelocity = finalSpeed * moveDirection;
            yield return new WaitForFixedUpdate();
        }

        // If we hit something and it's a pushable, push it
        if (hit.collider != null)
        {
            IPushable other = hit.collider.GetComponent<IPushable>();
            if (other != null)
            {
                other.Push(moveDirection, finalSpeed);
            }
        }

        // Stop the block once an obstacle is detected
        _rigidbody.linearVelocity = Vector3.zero;
        _isCurrentlySliding = false;
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
            StopAllCoroutines();
            _isCurrentlySliding = false;
            _rigidbody.position = _snapshotPosition;
            _rigidbody.linearVelocity = Vector3.zero;
            _hasSnapshot = false;
        }
    }

    // ISnapshottable usage: name is "Push Block"
    public string GetName()
    {
        return "Push Block";
    }

    // Push anything we run into while moving
    private void OnCollisionStay(Collision collision)
    {
        // Only propagate if we are moving significantly
        if (_rigidbody.linearVelocity.sqrMagnitude > 0.1f)
        {
            Vector3 movementDir = _rigidbody.linearVelocity.normalized;

            // Check if any contact point is on the front side
            foreach (ContactPoint contact in collision.contacts)
            {
                // contact.normal points from the other object towards us.
                // If we are moving towards the other object, movementDir and contact.normal should be roughly opposite.
                if (Vector3.Dot(movementDir, contact.normal) < -0.5f)
                {
                    IPushable other = collision.collider.GetComponent<IPushable>();
                    if (other != null)
                    {
                        // Push in the direction we are moving
                        other.Push(movementDir, _rigidbody.linearVelocity.magnitude);
                        break; // Only push once per frame per object
                    }
                }
            }
        }
    }
}
