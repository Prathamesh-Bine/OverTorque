using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class KineticPlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveForce = 25f;
    public float maxSpeed = 15f;

    [Header("Linear Anchor Dash")]
    public float anchorMassMultiplier = 10f;
    public float chargeRate = 25f;
    public float maxCharge = 100f;
    public float burstForceMultiplier = 40f;

    [Header("Sonic Anchor (AoE)")]
    public float sonicRadius = 15f;
    public float sonicExplosionMultiplier = 60f;
    private bool isSonicCharge;

    [Header("Input Action References")]
    public InputActionReference moveAction;
    public InputActionReference anchorAction;
    public InputActionReference modifierAction;

    // Internal components and state
    private Rigidbody rb;
    private Vector2 moveInput;
    private bool isAnchored;
    private float currentCharge;
    private float baseMass;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        baseMass = rb.mass;
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        anchorAction.action.Enable();
        modifierAction.action.Enable();

        anchorAction.action.started += OnAnchorPressed;
        anchorAction.action.canceled += OnAnchorReleased;
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        anchorAction.action.Disable();
        modifierAction.action.Disable();

        anchorAction.action.started -= OnAnchorPressed;
        anchorAction.action.canceled -= OnAnchorReleased;
    }

    private void Update()
    {
        moveInput = moveAction.action.ReadValue<Vector2>();

        if (isAnchored)
        {
            currentCharge += chargeRate * Time.deltaTime;
            currentCharge = Mathf.Clamp(currentCharge, 0f, maxCharge);
        }
    }

    private void FixedUpdate()
    {
        if (isAnchored) return;

        // Active braking when no input is provided
        if (moveInput == Vector2.zero)
        {
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, new Vector3(0f, rb.linearVelocity.y, 0f), Time.fixedDeltaTime * 6f);
            rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, Time.fixedDeltaTime * 6f);
            return;
        }

        // Map 2D input (X, Y) to 3D world space (X, 0, Z)
        Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);

        // Apply pushing force
        rb.AddForce(moveDirection * moveForce, ForceMode.Acceleration);

        // Apply rolling torque (Z and X inverted/swapped for correct rolling direction)
        Vector3 torqueAxis = new Vector3(moveDirection.z, 0f, -moveDirection.x);
        rb.AddTorque(torqueAxis * moveForce, ForceMode.Acceleration);

        // Cap maximum linear velocity
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }

    private void OnAnchorPressed(InputAction.CallbackContext context)
    {
        isAnchored = true;

        // Check if Shift is currently held down when Spacebar is pressed
        isSonicCharge = modifierAction.action.IsPressed();

        // Immediately stop all velocity
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Spike mass to resist incoming knockback
        rb.mass = baseMass * anchorMassMultiplier;
        currentCharge = 0f;
    }

    private void OnAnchorReleased(InputAction.CallbackContext context)
    {
        isAnchored = false;
        rb.mass = baseMass; // Restore normal mass

        if (isSonicCharge)
        {
            // --- SONIC WAVE (AoE) ---
            float finalExplosionForce = currentCharge * sonicExplosionMultiplier;
            
            // Find all colliders within the blast radius
            Collider[] colliders = Physics.OverlapSphere(transform.position, sonicRadius);
            foreach (Collider hit in colliders)
            {
                // If it has a rigidbody and is NOT the player, blast it away
                if (hit.TryGetComponent<Rigidbody>(out Rigidbody targetRb) && targetRb != rb)
                {
                    // The '1f' parameter adds a slight upward lift to the knockback
                    targetRb.AddExplosionForce(finalExplosionForce, transform.position, sonicRadius, 1f, ForceMode.Impulse);
                }
            }
        }
        else
        {
            // --- LINEAR DASH ---
            // Determine dash direction from current WASD direction; default forward if neutral
            Vector3 burstDir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
            if (burstDir == Vector3.zero)
            {
                burstDir = Vector3.forward;
            }

            // Apply explosive impulse based on accumulated charge
            float impulseMagnitude = currentCharge * burstForceMultiplier;
            rb.AddForce(burstDir * impulseMagnitude, ForceMode.Impulse);
        }

        currentCharge = 0f;
        isSonicCharge = false;
    }
    private void OnDrawGizmosSelected()
    {
        // Set the color of the wireframe
        Gizmos.color = Color.cyan;
        
        // Draw the sphere at the player's current position using the sonicRadius variable
        Gizmos.DrawWireSphere(transform.position, sonicRadius);
    }
}