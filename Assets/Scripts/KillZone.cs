using UnityEngine;

public class KillZone : MonoBehaviour
{
    [SerializeField] private Vector3 spawnPosition = new Vector3(0f, 1f, 0f);

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Rigidbody>(out var rb))
        {
            // Reset velocity so momentum does not carry over
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            other.transform.position = spawnPosition;
        }
    }
}