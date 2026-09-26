using UnityEngine;

public class CollectableEXP : MonoBehaviour
{
    public int expValue = 10;
    public Vector3 rotationSpeed = new Vector3(0, 90f, 0);

    private void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CollectionSuccessful();
        }
    }

    private void CollectionSuccessful()
    {
        // 1. Send the score to the GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(expValue);
        }
        else
        {
            Debug.LogWarning("GameManager not found in scene!");
        }

        // 2. Destroy the shard so it disappears from the map
        Destroy(gameObject);
    }
}