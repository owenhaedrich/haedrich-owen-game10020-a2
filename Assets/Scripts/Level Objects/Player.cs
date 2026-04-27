using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour, IPushable
{
    public TimeSword timeSword;

    // Movement
    public float moveSpeed = 5.0f;
    public float gravity = -9.81f;

    // Pushing
    public float pushSpeed = 3.0f;
    public float pushBuildUpTime = 0.3f;
    public float pushResetDelay = 0.1f;
    public float pushStopTime = 0.1f;

    // Push Response (player reaction after pushing something)
    public float pushResponseSlowFactor = 0.05f;
    public float pushResponseRecoveryTime = 0.3f;

    // Getting Pushed (knockback)
    [Tooltip("Multiplier applied to incoming push speed to determine knockback strength")]
    public float knockbackForceMultiplier = 1.5f;
    [Tooltip("How quickly knockback velocity decays (higher = faster decay)")]
    public float knockbackDecaySpeed = 8.0f;
    
    [Header("Movement Feel")]
    [Tooltip("Time in seconds to reach full speed from standstill")]
    public float accelerationTime = 0.25f;
    [Tooltip("Time in seconds to stop from full speed")]
    public float decelerationTime = 0.12f;
    private CharacterController _controller;
    private Vector3 _velocity;
    private Vector3 _currentMoveVelocity; // smoothed horizontal velocity
    private Vector3 _moveDampVelocity;    // used by SmoothDamp

    public InputActionReference moveInput;
    public InputActionReference attackInput;
    public InputActionReference specialInput;

    private bool _isSlowedAfterPushing = false;
    private float _slowedTimer = 0f;

    private Vector3 _knockbackVelocity = Vector3.zero;

    private float _currentPushBuildUp = 0f;
    private float _lastPushTime = 0f;
    private bool _isPushingThisFrame = false;
    private bool _isPushing = false;
    private float _pushStopTimer = 0f;
    private IPushable _currentPushTarget = null;
    private Vector2 _lastMoveInput = Vector2.zero;

    private void OnEnable()
    {
        attackInput.action.performed += Attack;
        specialInput.action.performed += Special;
    }

    private void OnDisable()
    {
        attackInput.action.performed -= Attack;
        specialInput.action.performed -= Special;
    }

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    void FixedUpdate()
    {
        HandlePushing();
        Move();
        UpdatePushTimer();

        // Sustain _isPushing with a grace period to prevent jitter
        if (_isPushingThisFrame)
        {
            _isPushing = true;
            _pushStopTimer = pushStopTime;
        }
        else if (_pushStopTimer > 0f)
        {
            _pushStopTimer -= Time.deltaTime;
            if (_pushStopTimer <= 0f)
            {
                _isPushing = false;
            }
        }

        _isPushingThisFrame = false;
    }

    private void HandlePushing()
    {
        // Use the CharacterController's dimensions for the overlap check
        Vector3 point1 = transform.position + _controller.center + Vector3.up * (_controller.height / 2f - _controller.radius);
        Vector3 point2 = transform.position + _controller.center - Vector3.up * (_controller.height / 2f - _controller.radius);
        
        // Slightly expand the radius to detect objects we are touching
        float checkRadius = _controller.radius + 0.1f;
        Collider[] colliders = Physics.OverlapCapsule(point1, point2, checkRadius);

        IPushable bestPushable = null;
        float bestDot = -1f;
        Vector3 bestDirection = Vector3.zero;

        foreach (Collider other in colliders)
        {
            if (other.gameObject == gameObject) continue;

            IPushable pushable = other.GetComponent<IPushable>();
            if (pushable != null)
            {
                Vector3 direction = other.transform.position - transform.position;
                Vector3 moveDir = _currentMoveVelocity; // Use current velocity for direction check

                // Fallback to input direction if velocity is low
                if (moveDir.sqrMagnitude < 0.1f)
                {
                    Vector2 moveInputVec = moveInput.action.ReadValue<Vector2>();
                    moveDir = new Vector3(moveInputVec.x, 0, moveInputVec.y);
                }

                float dot = Vector3.Dot(moveDir.normalized, direction.normalized);
                // Only count as pushing if moving generally towards the object
                if (dot > 0.3f)
                {
                    // Prioritize existing target if it's still valid
                    if (pushable == _currentPushTarget)
                    {
                        bestPushable = pushable;
                        bestDirection = direction;
                        break; 
                    }

                    if (dot > bestDot)
                    {
                        bestDot = dot;
                        bestPushable = pushable;
                        bestDirection = direction;
                    }
                }
            }
        }

        // Handle target switching
        if (bestPushable != null)
        {
            if (bestPushable != _currentPushTarget)
            {
                _currentPushBuildUp = 0f;
                _currentPushTarget = bestPushable;
            }

            _isPushingThisFrame = true;
            _lastPushTime = Time.time;
            _currentPushBuildUp += Time.deltaTime;

            if (_currentPushBuildUp >= pushBuildUpTime)
            {
                // The player applies their push speed to the pushable
                PushType pushType = _currentPushTarget.Push(bestDirection, pushSpeed);
                if (pushType == PushType.Slow)
                {
                    _isSlowedAfterPushing = true;
                    _slowedTimer = pushResponseRecoveryTime;
                }
            }
        }
    }

    void UpdatePushTimer()
    {
        // If we are no longer pushing and enough time has passed, reset the build up
        if (!_isPushing && Time.time - _lastPushTime > pushResetDelay)
        {
            _currentPushBuildUp = 0f;
            _currentPushTarget = null;
        }
    }


    void Move()
    {
        // Decay knockback velocity over time
        if (_knockbackVelocity.sqrMagnitude > 0.01f)
        {
            _knockbackVelocity = Vector3.Lerp(_knockbackVelocity, Vector3.zero, knockbackDecaySpeed * Time.deltaTime);
        }
        else
        {
            _knockbackVelocity = Vector3.zero;
        }

        // Handle slowed state while moving
        if (_isSlowedAfterPushing && _slowedTimer > 0f) _slowedTimer -= Time.deltaTime;
        else _isSlowedAfterPushing = false;

        // snap the player to the ground if already grounded
        // when jumping, gravity takes over
        if (_controller.isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
        }

        Vector2 moveDirection = moveInput.action.ReadValue<Vector2>();

        // Reset push buildup if direction changes significantly
        // This makes the push feel "snappier" when trying to maneuver
        if (moveDirection.sqrMagnitude > 0.01f && _lastMoveInput.sqrMagnitude > 0.01f)
        {
            float dot = Vector2.Dot(moveDirection.normalized, _lastMoveInput.normalized);
            if (dot < 0.95f) // Roughly ~18 degrees change
            {
                _currentPushBuildUp = 0f;
            }
        }
        else if (moveDirection.sqrMagnitude > 0.01f && _lastMoveInput.sqrMagnitude <= 0.01f)
        {
            // Reset if we just started moving again
            _currentPushBuildUp = 0f;
        }

        Vector3 inputDir = Vector3.right * moveDirection.x + Vector3.forward * moveDirection.y;
        // Clamp so diagonal isn't faster
        if (inputDir.sqrMagnitude > 1f) inputDir.Normalize();

        float speed = moveSpeed;
        if (_isSlowedAfterPushing) speed *= pushResponseSlowFactor;

        Vector3 targetVelocity = inputDir * speed;

        bool hasInput = inputDir.sqrMagnitude > 0.01f;
        float smoothTime = hasInput ? accelerationTime : decelerationTime;

        // Smoothly interpolate the velocity vector toward the target velocity.
        // This handles both acceleration, deceleration, and smooth direction changes.
        _currentMoveVelocity = Vector3.SmoothDamp(
            _currentMoveVelocity,
            targetVelocity,
            ref _moveDampVelocity,
            smoothTime
        );

        // allow gravity to impact y velocity
        _velocity.y += gravity * Time.deltaTime;

        Vector3 finalVelocity = _currentMoveVelocity + _knockbackVelocity;
        finalVelocity.y = _velocity.y;

        // finally, Move the character
        _controller.Move(finalVelocity * Time.deltaTime);

        // rotate the character using Quaternion LookRotation()
        // slerp = Spherical Linear Interpolation. smoothly interpolates between Quaternion rotations
        Vector3 horizontalVelocity = new Vector3(finalVelocity.x, 0f, finalVelocity.z);
        if (horizontalVelocity.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(horizontalVelocity);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                15f * Time.deltaTime
            );
        }

        _lastMoveInput = moveDirection;
    }

    // Swing the Time Sword
    void Attack(InputAction.CallbackContext obj)
    {
        timeSword.Swing();
    }

    // Tell the Time Sword to tell the ISnapshottables to restore their snapshots
    void Special(InputAction.CallbackContext obj)
    {
        timeSword.Restore();
    }

    // IPushable usage: player gets knocked back when pushed
    public PushType Push(Vector3 pushDirection, float pushSpeed)
    {
        // Normalize to horizontal plane and apply knockback force
        Vector3 knockbackDir = new Vector3(pushDirection.x, 0f, pushDirection.z).normalized;
        Vector3 knockbackImpulse = knockbackDir * pushSpeed * knockbackForceMultiplier;

        // Add to existing knockback (supports continuous pushing)
        _knockbackVelocity += knockbackImpulse;

        return PushType.PushBack;
    }
}
