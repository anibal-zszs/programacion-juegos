using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento físico")]
    public float speed = 8f;
    public float sprintMultiplier = 2f;
    public float acceleration = 45f;
    public float jumpImpulse = 8f;
    public LayerMask groundMask = ~0;
    public Transform cameraTransform;
    public Transform groundCheck;
    public float groundCheckRadius = 0.34f;
    public float coyoteTime = 0.12f;
    public float jumpBufferTime = 0.12f;

    private Rigidbody body;
    private Collider playerCollider;
    private bool isGrounded;
    private float lastGroundedTime = -10f;
    private float jumpPressedTime = -10f;
    private bool jumpQueued;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        playerCollider = GetComponent<Collider>();
        body.constraints = RigidbodyConstraints.FreezeRotation;
        body.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void Update()
    {
        UpdateGroundedState();
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            jumpPressedTime = Time.time;
            jumpQueued = true;
        }
    }

    private void FixedUpdate()
    {
        UpdateGroundedState();
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        float x = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1f : 0f)
                - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1f : 0f);
        float z = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1f : 0f)
                - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1f : 0f);
        Vector3 forward = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
        Vector3 right = cameraTransform != null ? cameraTransform.right : Vector3.right;
        forward.y = 0f;
        right.y = 0f;
        Vector3 direction = (forward.normalized * z + right.normalized * x).normalized;
        bool sprinting = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
        float currentSpeed = speed * (sprinting ? sprintMultiplier : 1f);
        Vector3 desired = direction * currentSpeed;
        Vector3 horizontal = new Vector3(body.linearVelocity.x, 0f, body.linearVelocity.z);
        Vector3 change = Vector3.ClampMagnitude(desired - horizontal, acceleration * Time.fixedDeltaTime);
        body.AddForce(change, ForceMode.VelocityChange);

        bool jumpBuffered = jumpQueued && Time.time - jumpPressedTime <= jumpBufferTime;
        bool canUseGroundJump = isGrounded || Time.time - lastGroundedTime <= coyoteTime;
        if (jumpBuffered && canUseGroundJump)
        {
            Vector3 velocity = body.linearVelocity;
            velocity.y = 0f;
            body.linearVelocity = velocity;
            body.AddForce(Vector3.up * jumpImpulse, ForceMode.Impulse);
            jumpQueued = false;
            lastGroundedTime = -10f;
        }
        else if (jumpQueued && Time.time - jumpPressedTime > jumpBufferTime)
        {
            jumpQueued = false;
        }
    }

    private void UpdateGroundedState()
    {
        if (groundCheck == null) return;
        Vector3 origin = playerCollider != null ? playerCollider.bounds.center : transform.position;
        float distance = playerCollider != null ? playerCollider.bounds.extents.y + groundCheckRadius + 0.12f : 1.5f;
        if (Physics.SphereCast(origin, groundCheckRadius, Vector3.down, out RaycastHit hit, distance, groundMask, QueryTriggerInteraction.Ignore)
            && hit.collider != playerCollider && !hit.collider.transform.IsChildOf(transform))
        {
            isGrounded = Vector3.Angle(hit.normal, Vector3.up) < 65f;
            if (isGrounded) lastGroundedTime = Time.time;
        }
        else
        {
            isGrounded = false;
        }
    }

    public void ResetMotion(Vector3 position)
    {
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        body.position = position;
        isGrounded = false;
        lastGroundedTime = -10f;
        jumpQueued = false;
        jumpPressedTime = -10f;
        body.Sleep();
        body.WakeUp();
    }
}
