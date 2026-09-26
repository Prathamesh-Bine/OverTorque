using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class KillZoneReset : MonoBehaviour
{
    [Header("Player Reset Settings")]
    [Tooltip("Create an Empty GameObject in the center of your arena and assign it here.")]
    public Transform playerSpawnPoint;
    public float failureEXPPenalty = 50f;

    [Header("SFX (Audio)")]
    public AudioClip fallClip;
    public AudioClip respawnClip;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (fallClip != null) audioSource.PlayOneShot(fallClip);
            PlayerResetSequence(other.gameObject);
        }
        else if (other.TryGetComponent<EnemyAI>(out _))
        {
            Destroy(other.gameObject);
        }
    }

    private void PlayerResetSequence(GameObject playerObj)
    {
        // 1. Deduct EXP (which automatically handles Game Over if it drops to 0)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(Mathf.RoundToInt(-failureEXPPenalty));
        }

        // 2. Kill momentum
        if (playerObj.TryGetComponent<Rigidbody>(out Rigidbody playerRb))
        {
            playerRb.linearVelocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
        }

        // 3. Relocate to center
        if (playerSpawnPoint != null)
        {
            playerObj.transform.position = playerSpawnPoint.position;
        }
        else
        {
            // Changed fallback from (7, 10, 7) to perfectly centered (0, 10, 0)
            playerObj.transform.position = new Vector3(0f, 10f, 0f);
        }

        if (respawnClip != null) audioSource.PlayOneShot(respawnClip);

        // 4. --- NEW: STUN ALL ACTIVE ENEMIES FOR 2 SECONDS ---
        EnemyAI[] activeEnemies = Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        foreach (EnemyAI enemy in activeEnemies)
        {
            enemy.Stun(2.0f);
        }
    }
}