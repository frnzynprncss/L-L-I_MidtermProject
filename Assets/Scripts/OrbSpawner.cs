using UnityEngine;

public class OrbSpawner : MonoBehaviour
{
    public GameObject orbPrefab;
    public Vector3 spawnAreaSize = new Vector3(20f, 0f, 20f); // How wide the spawn zone is
    public float spawnInterval = 3f; // Spawn a new orb every 3 seconds

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnOrb();
            timer = 0f;
        }
    }

    void SpawnOrb()
    {
        // Pick a random X and Z coordinate within our spawn box
        float randomX = Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2);
        float randomZ = Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2);

        // Calculate the final position (we add 1f to Y so it hovers slightly above the floor)
        Vector3 spawnPosition = transform.position + new Vector3(randomX, 1f, randomZ);

        Instantiate(orbPrefab, spawnPosition, Quaternion.identity);
    }

    // This draws a helpful green box in the Unity Editor so you can see your spawn zone!
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawCube(transform.position, spawnAreaSize);
    }
}