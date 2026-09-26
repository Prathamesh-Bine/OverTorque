using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

[RequireComponent(typeof(Rigidbody))]
public class KineticPlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveForce = 25f;
    public float maxSpeed = 15f;

    [Header("Energy & Cooldown System")]
    public float maxEnergy = 100f;
    public float energyRegenRate = 5f;
    public float collisionEnergyMultiplier = 0.5f; 
    public float actionCooldown = 1.5f;

    [Header("Linear Anchor Dash")]
    public float anchorMassMultiplier = 10f;
    public float dashChargeRate = 50f; 
    public float burstForceMultiplier = 40f;
    public float dashTrailDuration = 0.5f;

    [Header("Sonic Anchor (AoE)")]
    public float sonicRadius = 15f;
    public float sonicChargeRate = 100f; 
    public float sonicExplosionMultiplier = 60f;
    private bool isSonicCharge;

    [Header("Input Action References")]
    public InputActionReference moveAction;
    public InputActionReference anchorAction;
    public InputActionReference modifierAction;

    [Header("UI Elements")]
    public Slider energySlider;
    public Slider cooldownSlider;

    [Header("VFX")]
    public ParticleSystem sonicVFX;
    public ParticleSystem chargeVFX;
    public ParticleSystem dashVFX;
    public TrailRenderer dashTrail;

    [Header("SFX (Audio Clips)")]
    public AudioClip chargeClip;
    public AudioClip dashClip;
    public AudioClip sonicClip;
    public AudioClip heavyImpactClip;
    public AudioClip lightImpactClip;
    public AudioClip normalImpactClip;
    public AudioClip rollingClip;

    [Header("SFX (Audio Sources)")]
    public AudioSource mainAudioSource; 
    public AudioSource rollingAudioSource; 

    private Rigidbody rb;
    private CinemachineImpulseSource impulseSource;
    private Vector2 moveInput;
    private Vector3 lastMoveDirection = Vector3.forward;
    private bool isAnchored;
    private float currentCharge;
    private float currentEnergy;
    private float cooldownTimer;
    private float baseMass;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        impulseSource = GetComponent<CinemachineImpulseSource>();
        baseMass = rb.mass;
        currentEnergy = maxEnergy;

        if (dashTrail != null) dashTrail.emitting = false;
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
        if (moveInput != Vector2.zero) lastMoveDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;

        if (isAnchored)
        {
            float activeChargeRate = isSonicCharge ? sonicChargeRate : dashChargeRate;
            float chargeAmount = activeChargeRate * Time.deltaTime;
            float maxAbilityCharge = isSonicCharge ? 100f : 50f;
            float availableCharge = Mathf.Min(chargeAmount, currentEnergy, maxAbilityCharge - currentCharge);
            currentCharge += availableCharge;
            currentEnergy -= availableCharge;
        }
        else
        {
            currentEnergy += energyRegenRate * Time.deltaTime;
            currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
        }

        if (energySlider != null) energySlider.value = currentEnergy;
        if (cooldownSlider != null) cooldownSlider.value = Mathf.Clamp01(cooldownTimer / actionCooldown) * 100f; 

        // --- ROLLING AUDIO LOGIC ---
        if (rollingClip != null && rollingAudioSource != null)
        {
            if (rollingAudioSource.clip == null) rollingAudioSource.clip = rollingClip;

            float currentSpeed = rb.linearVelocity.magnitude;
            
            // Only play if moving faster than 0.5f and not actively charging an anchor
            if (currentSpeed > 0.5f && !isAnchored)
            {
                if (!rollingAudioSource.isPlaying) rollingAudioSource.Play();
                
                // Dynamically adjust volume and pitch based on speed
                rollingAudioSource.volume = Mathf.Clamp01(currentSpeed / maxSpeed) * 0.7f; 
                rollingAudioSource.pitch = 0.8f + (currentSpeed / maxSpeed) * 0.4f; 
            }
            else
            {
                if (rollingAudioSource.isPlaying) rollingAudioSource.Pause();
            }
        }
    }

    private void FixedUpdate()
    {
        if (isAnchored) return;

        if (moveInput == Vector2.zero)
        {
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, new Vector3(0f, rb.linearVelocity.y, 0f), Time.fixedDeltaTime * 6f);
            rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, Time.fixedDeltaTime * 6f);
            return;
        }

        Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);
        rb.AddForce(moveDirection * moveForce, ForceMode.Acceleration);
        Vector3 torqueAxis = new Vector3(moveDirection.z, 0f, -moveDirection.x);
        rb.AddTorque(torqueAxis * moveForce, ForceMode.Acceleration);

        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }

    private void OnAnchorPressed(InputAction.CallbackContext context)
    {
        if (cooldownTimer > 0f || currentEnergy < 1f) return;

        isAnchored = true;
        isSonicCharge = modifierAction.action.IsPressed();
        currentCharge = 0f;

        if (chargeVFX != null) chargeVFX.Play();
        
        if (chargeClip != null && mainAudioSource != null) 
        {
            mainAudioSource.clip = chargeClip;
            mainAudioSource.volume = 0.2f; 
            mainAudioSource.Play();
        }

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.mass = baseMass * anchorMassMultiplier;
    }

    private void OnAnchorReleased(InputAction.CallbackContext context)
    {
        if (!isAnchored) return; 

        isAnchored = false;
        rb.mass = baseMass;
        cooldownTimer = actionCooldown; 

        if (chargeVFX != null) chargeVFX.Stop();
        
        if (mainAudioSource != null)
        {
            mainAudioSource.Stop();
            mainAudioSource.volume = 1.0f; 
        }
        
        float expPowerMultiplier = 1f;
        if (GameManager.Instance != null) expPowerMultiplier = 1f + (GameManager.Instance.currentEXP * 0.01f);

        float chargeRatio = isSonicCharge ? (currentCharge / 100f) : (currentCharge / 50f);
        float impulseForce = Mathf.Clamp(chargeRatio, 0.1f, 1f);

        if (isSonicCharge)
        {
            if (sonicClip != null && mainAudioSource != null) mainAudioSource.PlayOneShot(sonicClip);
            float finalExplosionForce = currentCharge * sonicExplosionMultiplier * expPowerMultiplier;

            if (sonicVFX != null) sonicVFX.Play();
            if (impulseSource != null) impulseSource.GenerateImpulse(impulseForce * 1.5f);

            Collider[] colliders = Physics.OverlapSphere(transform.position, sonicRadius);
            foreach (Collider hit in colliders)
            {
                if (hit.TryGetComponent<Rigidbody>(out Rigidbody targetRb) && targetRb != rb)
                {
                    targetRb.AddExplosionForce(finalExplosionForce, transform.position, sonicRadius, 1f, ForceMode.Impulse);
                    if (hit.TryGetComponent<EnemyAI>(out EnemyAI enemyScript)) enemyScript.Stun(2.5f);
                }
            }
        }
        else
        {
            if (dashClip != null && mainAudioSource != null) mainAudioSource.PlayOneShot(dashClip);
            if (dashVFX != null) dashVFX.Play();
            if (dashTrail != null)
            {
                dashTrail.emitting = true;
                Invoke(nameof(StopDashTrail), dashTrailDuration);
            }

            if (impulseSource != null) impulseSource.GenerateImpulse(impulseForce);

            Vector3 burstDir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
            if (burstDir == Vector3.zero) burstDir = lastMoveDirection;

            float impulseMagnitude = currentCharge * burstForceMultiplier * expPowerMultiplier;
            rb.AddForce(burstDir * impulseMagnitude, ForceMode.Impulse);
        }

        currentCharge = 0f;
        isSonicCharge = false;
    }

    private void StopDashTrail()
    {
        if (dashTrail != null) dashTrail.emitting = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.rigidbody != null)
        {
            float momentumTransferred = collision.impulse.magnitude;
            
            currentEnergy += momentumTransferred * collisionEnergyMultiplier;
            currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);

            if (momentumTransferred > 10f) 
            {
                if (heavyImpactClip != null && mainAudioSource != null) mainAudioSource.PlayOneShot(heavyImpactClip);
                if (collision.gameObject.TryGetComponent<EnemyAI>(out EnemyAI enemyScript)) enemyScript.Stun(2.0f);
            }
            else if (momentumTransferred > 2f)
            {
                if (lightImpactClip != null && mainAudioSource != null) mainAudioSource.PlayOneShot(lightImpactClip);
            }
            else if (momentumTransferred > 0.2f) 
            {
                if (normalImpactClip != null && mainAudioSource != null) mainAudioSource.PlayOneShot(normalImpactClip, 0.5f);
            }
        }
    }
}