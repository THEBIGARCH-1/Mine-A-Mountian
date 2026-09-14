using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class WobblyCharacterController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float acceleration = 10f;

    [Header("Mouse Look Settings (Right-Click)")]
    public Transform cameraTransform; // Drag your Camera here
    public float mouseSensitivity = 2f;
    private float yaw = 0f;

    [Header("Jump Settings")]
    public float jumpForce = 5f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    [Header("Wobbly Physics Effect")]
    public Transform characterVisuals; 
    public float maxTiltAngle = 15f;
    public float wobbleSpeed = 8f;

    private Rigidbody rb;
    private Vector3 movementInput;
    private bool isGrounded;
    private bool jumpRequested;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        if (characterVisuals == null && transform.childCount > 0)
        {
            characterVisuals = transform.GetChild(0);
        }

        // Hide cursor initially if desired, or keep visible for right-click hold
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        // 1. Mouse Look with Right-Click
        if (Input.GetMouseButton(1)) // 1 is Right Mouse Button
        {
            Cursor.lockState = CursorLockMode.Locked; // Lock cursor while holding right click
            Cursor.visible = false;

            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            yaw += mouseX;
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // 2. WASD Input (Horizontal = A/D, Vertical = W/S)
        float moveX = Input.GetAxisRaw("Horizontal"); // A and D keys
        float moveZ = Input.GetAxisRaw("Vertical");   // W and S keys

        // Calculate movement direction relative to the character's facing direction
        Vector3 rawInput = new Vector3(moveX, 0f, moveZ).normalized;
        movementInput = transform.TransformDirection(rawInput);

        // 3. Ground Check
        if (groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
        }
        else
        {
            isGrounded = true;
        }

        // 4. Jump Input (Spacebar)
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            jumpRequested = true;
        }
    }

    void FixedUpdate()
    {
        // --- Movement Physics ---
        Vector3 targetVelocity = movementInput * moveSpeed;
        targetVelocity.y = rb.velocity.y; // Retain vertical velocity

        rb.velocity = Vector3.Lerp(rb.velocity, targetVelocity, acceleration * Time.fixedDeltaTime);

        // --- Wobbly Visuals Tilt ---
        if (movementInput.magnitude >= 0.1f && characterVisuals != null)
        {
            // Calculate local lean based on movement
            Vector3 localMove = transform.InverseTransformDirection(movementInput);
            float tiltZ = -localMove.x * maxTiltAngle + Mathf.Sin(Time.time * wobbleSpeed) * 3f;
            float tiltX = localMove.z * (maxTiltAngle / 2f);
            
            Quaternion targetVisualRotation = Quaternion.Euler(tiltX, 0f, tiltZ);
            characterVisuals.localRotation = Quaternion.Slerp(characterVisuals.localRotation, targetVisualRotation, 10f * Time.fixedDeltaTime);
        }
        else if (characterVisuals != null)
        {
            float idleSway = Mathf.Sin(Time.time * (wobbleSpeed * 0.5f)) * 2f;
            Quaternion idleRotation = Quaternion.Euler(0f, 0f, idleSway);
            characterVisuals.localRotation = Quaternion.Slerp(characterVisuals.localRotation, idleRotation, 5f * Time.fixedDeltaTime);
        }

        // --- Jumping Execution ---
        if (jumpRequested)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpRequested = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
