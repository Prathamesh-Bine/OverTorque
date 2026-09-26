using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveForce = 15f;
    public float maxSpeed = 8f;
    public float fallThresholdY = 0f; 

    [Header("Attack Settings")]
    public float knockbackPower = 20f;
    
    [Header("Lifespan Settings")]
    public float lifespan = 20f;

    private Rigidbody rb;
    private Transform playerTransform;
    
    private float stunTimer = 0f; 
    private float lifeTimer = 0f;
    private bool isDespawning = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        // Apply Time-Based Global Difficulty Multiplier
        if (GameManager.Instance != null)
        {
            float diff = GameManager.Instance.globalDifficultyMultiplier;
            moveForce *= diff;
            maxSpeed *= diff;
            knockbackPower *= diff;
        }
    }

    private void FixedUpdate()
    {
        lifeTimer += Time.fixedDeltaTime;
        if (lifeTimer >= lifespan && !isDespawning)
        {
            isDespawning = true;
            Destroy(gameObject);
            return;
        }

        if (playerTransform == null || stunTimer > 0f || playerTransform.position.y < fallThresholdY)
        {
            if (stunTimer > 0f) stunTimer -= Time.fixedDeltaTime;
            return;
        }

        Vector3 directionToPlayer = playerTransform.position - transform.position;
        directionToPlayer.y = 0; 
        directionToPlayer = directionToPlayer.normalized;

        rb.AddForce(directionToPlayer * moveForce, ForceMode.Acceleration);

        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (horizontalVelocity.magnitude > maxSpeed)
        {
            horizontalVelocity = horizontalVelocity.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (stunTimer > 0f || isDespawning) return; 

        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                Vector3 pushDirection = collision.transform.position - transform.position;
                pushDirection.y = 0; 
                pushDirection = pushDirection.normalized;
                pushDirection.y = 0.2f; 

                playerRb.AddForce(pushDirection * knockbackPower, ForceMode.Impulse);
            }
        }
    }

    public void Stun(float time)
    {
        stunTimer = time;
    }

    private void OnDestroy()
    {
        // When this enemy dies, tell the GameManager to lower the active count
        // The GameManager's Update loop will automatically spawn a replacement
        if (GameManager.Instance != null && !gameObject.scene.isLoaded) return; // Prevent errors on quit
        if (GameManager.Instance != null)
        {
            GameManager.Instance.activeEnemyCount--;
        }
    }
}