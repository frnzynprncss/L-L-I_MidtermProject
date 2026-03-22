using UnityEngine;

public class OrbSpawner : MonoBehaviour
{
    public GameObject orbPrefab;
    public Vector3 spawnAreaSize = new Vector3(20f, 0f, 20f);
    public float spawnInterval = 3f;

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
        // 1. Pick a random X and Z coordinate
        float randomX = Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2);
        float randomZ = Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2);

        // 2. Create the random position offset (local space)
        Vector3 randomOffset = new Vector3(randomX, 1f, randomZ);

        // ---> CHANGED: Multiply the offset by the spawner's rotation so it turns with the object! <---
        Vector3 spawnPosition = transform.position + (transform.rotation * randomOffset);

        Instantiate(orbPrefab, spawnPosition, Quaternion.identity);
    }

    // ---> CHANGED: Updated Gizmos so the green box actually rotates in the editor <---
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);

        // This tells Unity to draw the Gizmo using the exact position, rotation, and scale of this GameObject
        Gizmos.matrix = transform.localToWorldMatrix;

        // Because the matrix handles the position, we just draw the cube at "zero" (the center of the object)
        Gizmos.DrawCube(Vector3.zero, spawnAreaSize);
    }
}