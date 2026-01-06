using UnityEngine;
using Meta.XR.MRUtilityKit;
using System.Collections.Generic;

/// <summary>
/// This script manages the spawning of specified prefabs at a designated spawn point.
/// It is designed to be controlled by UI buttons or other game events, providing a centralized
/// and reusable system for object instantiation.
/// </summary>
public class AssetSpawner : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("The Character Prefab to spawn.")]
    [SerializeField]
    private GameObject _characterPrefab;

    [Tooltip("List of prefabs available to spawn via UI menu.")]
    [SerializeField]
    private List<GameObject> _spawnablePrefabs = new List<GameObject>();

    public IReadOnlyList<GameObject> SpawnablePrefabs => _spawnablePrefabs;

    [Header("Spawn Rules")]
    [Tooltip("Where should the character spawn?")]
    [SerializeField] 
    private MRUKAnchor.SceneLabels _spawnLabels = MRUKAnchor.SceneLabels.FLOOR | MRUKAnchor.SceneLabels.TABLE;
    
    [SerializeField]
    private float _spawnHeightOffset = 0.1f;

    /// <summary>
    /// Call this via a UI Button or Event to spawn the character safe in the room.
    /// </summary>
    public void SpawnCharacter()
    {
        SpawnAsset(_characterPrefab);
    }

    public void SpawnAsset(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("AssetSpawner: No Prefab assigned.");
            return;
        }

        if (MRUK.Instance == null || MRUK.Instance.GetCurrentRoom() == null)
        {
            Debug.LogError("AssetSpawner: No Room Data loaded yet. Wait for LocalRoomLoader.");
            return;
        }

        // 1. Get the current room
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();

        // 2. Try to find a random position on the allowed surfaces (Table or Floor)
        // This function automatically handles "Is the point inside the room?" and "Is it on a valid surface?"
        if (room.GenerateRandomPositionOnSurface(MRUK.SurfaceType.FACING_UP | MRUK.SurfaceType.VERTICAL | MRUK.SurfaceType.FACING_DOWN, 0.1f, new LabelFilter(_spawnLabels), out Vector3 pos, out Vector3 normal))
        {
            // 3. Spawn the character
            Instantiate(prefab, pos + (Vector3.up * _spawnHeightOffset), Quaternion.LookRotation(Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up)));
            Debug.Log($"AssetSpawner: {prefab.name} spawned successfully.");
        }
        else
        {
            Debug.LogWarning("AssetSpawner: Could not find a valid surface to spawn on.");
        }
    }
}
