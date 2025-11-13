using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This script manages the spawning of specified prefabs at a designated spawn point.
/// It is designed to be controlled by UI buttons or other game events, providing a centralized
/// and reusable system for object instantiation.
/// </summary>
public class AssetSpawner : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("A list of all the prefabs that are allowed to be spawned by this system. This is used for validation.")]
    [SerializeField]
    private List<GameObject> _spawnablePrefabs = new();    

    /// <summary>
    /// A public accessor to get a read-only version of the spawnable prefabs list.
    /// This allows other scripts to see what can be spawned without allowing them to modify the list.
    /// </summary>
    public IReadOnlyList<GameObject> SpawnablePrefabs => _spawnablePrefabs;

    [Header("Spawning Surface")]
    [Tooltip("The collider of the surface where objects will be spawned (e.g., a table).")]
    [SerializeField]
    private Collider _spawnSurface;

    [Tooltip("The layers that can block spawning. This should include other spawned objects.")]
    [SerializeField]
    private LayerMask _blockingLayers;

    [Tooltip("The maximum number of attempts to find an empty spawn position before giving up.")]
    [SerializeField]
    private int _maxSpawnAttempts = 50;

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Validates that essential references have been assigned in the Inspector.
    /// </summary>
    private void Awake()
    {
        if (_spawnSurface == null)
        {
            Debug.LogError("[AssetSpawner] Spawn Surface collider is not assigned.", this);
        }
    }

    /// <summary>
    /// Spawns a specified prefab at a valid position on the designated spawn surface.
    /// </summary>
    /// <param name="prefabToSpawn">The GameObject prefab to instantiate.</param>
    public void SpawnAsset(GameObject prefabToSpawn)
    {
        // Using '== null' is the most reliable pattern for Unity objects
        // to satisfy analyzers and handle destroyed objects correctly.
        if (prefabToSpawn == null || !_spawnablePrefabs.Contains(prefabToSpawn))
        {
            Debug.LogWarning($"[AssetSpawner] The prefab '{prefabToSpawn?.name}' is not in the list of spawnable prefabs.", this);
            return;
        }

        if (TryFindPositionOnSurface(prefabToSpawn, out Vector3 position, out Quaternion rotation))
        {
            Instantiate(prefabToSpawn, position, rotation);
            Debug.Log($"[AssetSpawner] Spawned '{prefabToSpawn.name}' successfully.", this);
        }
        else
        {
            Debug.LogError($"[AssetSpawner] Failed to find a valid spawn position for '{prefabToSpawn.name}' after {_maxSpawnAttempts} attempts.", this);
        }
    }

    /// <summary>
    /// Attempts to find a random, unoccupied position on the spawn surface.
    /// </summary>
    /// <param name="prefabToSpawn">The prefab to be spawned, used to determine its size.</param>
    /// <param name="position">The found position on the surface.</param>
    /// <param name="rotation">The appropriate rotation for the object to sit flat on the surface.</param>
    /// <returns>True if a valid position was found, otherwise false.</returns>
    private bool TryFindPositionOnSurface(GameObject prefabToSpawn, out Vector3 position, out Quaternion rotation)
    {
        Bounds surfaceBounds = _spawnSurface.bounds;
        Renderer renderer = prefabToSpawn.GetComponentInChildren<Renderer>();
        Bounds prefabBounds = renderer != null ? renderer.bounds : new();

        for (int i = 0; i < _maxSpawnAttempts; i++)
        {
            float randomX = Random.Range(surfaceBounds.min.x, surfaceBounds.max.x);
            float randomZ = Random.Range(surfaceBounds.min.z, surfaceBounds.max.z);
            Vector3 rayStart = new(randomX, surfaceBounds.max.y + 1f, randomZ);

            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 2f, 1 << _spawnSurface.gameObject.layer))
            {
                position = hit.point;
                rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

                // Check for overlaps before confirming the position
                if (!Physics.CheckBox(position + prefabBounds.center, prefabBounds.extents, rotation, _blockingLayers))
                {
                    return true;
                }
            }
        }

        // Default values if no position is found
        position = Vector3.zero;
        rotation = Quaternion.identity;
        return false;
    }
}
