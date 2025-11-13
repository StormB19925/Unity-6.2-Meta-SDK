using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// This script dynamically populates a UGUI layout with buttons based on a list of spawnable prefabs
/// from an AssetSpawner. It is designed to work with Unity's UGUI system and is a replacement
/// for the UI Toolkit-based AssetMenuController.
/// </summary>
public class UGUIAssetSpawnerMenu : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The AssetSpawner that holds the list of prefabs and the spawn logic.")]
    [SerializeField]
    private AssetSpawner _assetSpawner;

    [Tooltip("The UGUI Button prefab to use for each spawnable asset.")]
    [SerializeField]
    private GameObject _buttonPrefab;

    [Tooltip("The parent transform where the buttons will be instantiated (e.g., a panel with a Layout Group).")]
    [SerializeField]
    private Transform _buttonContainer;

    void Start()
    {
        if (_assetSpawner == null || _buttonPrefab == null || _buttonContainer == null)
        {
            Debug.LogError("[UGUIAssetSpawnerMenu] Required references are not assigned in the inspector.", this);
            return;
        }

        PopulateMenu();
    }

    /// <summary>
    /// Clears any existing buttons and creates new ones for each spawnable prefab.
    /// </summary>
    private void PopulateMenu()
    {
        // Clear any old buttons
        foreach (Transform child in _buttonContainer)
        {
            Destroy(child.gameObject);
        }

        // Get the list of prefabs from the spawner
        IReadOnlyList<GameObject> spawnableItems = _assetSpawner.SpawnablePrefabs;

        // Create a button for each prefab
        foreach (GameObject prefab in spawnableItems)
        {
            // Create a local copy of the prefab variable for the closure
            GameObject prefabToSpawn = prefab;

            // Instantiate the button and parent it to the container
            GameObject buttonGO = Instantiate(_buttonPrefab, _buttonContainer);
            buttonGO.name = $"Button_{prefabToSpawn.name}";

            // Set the button's text. Assumes TextMeshPro is used.
            // If using legacy Text, change to GetComponentInChildren<Text>()
            TextMeshProUGUI buttonText = buttonGO.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = prefabToSpawn.name;
            }

            // Add the OnClick listener
            Button button = buttonGO.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() =>
                {
                    _assetSpawner.SpawnAsset(prefabToSpawn);
                    Debug.Log($"[UGUIAssetSpawnerMenu] Requested to spawn '{prefabToSpawn.name}'.");
                });
            }
            else
            {
                Debug.LogWarning($"[UGUIAssetSpawnerMenu] The button prefab '{_buttonPrefab.name}' does not have a Button component.", this);
            }
        }
    }
}
