using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour, IPushable
{
    public TimeSword timeSword;

    public float moveSpeed = 5.0f;
    public float gravity = -9.81f;
    public float pushSlowFactor = 0.3f;
    public float pushRecoveryTime = 0.1f;

    private CharacterController _controller;
    private Vector3 _velocity;

    public InputActionReference moveInput;
    public InputActionReference attackInput;
    public InputActionReference specialInput;

    private bool _pushed = false;
    private float _pushTimer = 0f;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();

        attackInput.action.performed += Attack;
        specialInput.action.performed += Special;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    private void FixedUpdate()
    {
        
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<IPushable>() != null)
        {
            Vector3 direction = other.transform.position - transform.position;

            // The player applies the minimum push speed to the pushable
            other.GetComponent<IPushable>().Push(direction, 0f);
        }
    }

    void Move()
    {
        // Handle pushed state while moving
        if (_pushed && _pushTimer > 0f) _pushTimer -= Time.deltaTime;
        else _pushed = false;

        // the following is pretty standard character controller code

        // snap the player to the ground if already grounded
        // when jumping, gravity takes over
        if (_controller.isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
        }

        Vector2 moveDirection = moveInput.action.ReadValue<Vector2>();

        Vector3 move = Vector3.right * moveDirection.x + Vector3.forward * moveDirection.y;
        Vector3 moveVelocity = move * moveSpeed;

        if (_pushed) moveVelocity *= pushSlowFactor;

        // allow gravity to impact y velocity
        _velocity.y += gravity * Time.deltaTime;

        moveVelocity.y = _velocity.y;

        // finally, Move the character
        _controller.Move(moveVelocity * Time.deltaTime);


        // rotate the character using Quaternion LookRotation()
        // slerp = Spherical Linear Interpolation. smoothly interpolates between Quaternion rotations
        Vector3 horizontalVelocity = new Vector3(moveVelocity.x, 0f, moveVelocity.z);
        if (horizontalVelocity.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(horizontalVelocity);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                15f * Time.deltaTime
            );
        }
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

    // IPushable usage: walk speed is slowed when being pushed
    public void Push(Vector3 pushDirection, float pushSpeed)
    {
        _pushed = true;
        _pushTimer = pushRecoveryTime;
    }
}
