using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Unloads a scene, typically the one it resides in.
/// Designed to be called from a UI Button's OnClick event.
/// </summary>
public class SceneUnloader : MonoBehaviour
{
    [Tooltip("Optional: Specify the exact name of the scene to unload. If left empty, it will unload the scene this component is in.")]
    [SerializeField]
    private string _sceneNameToUnload;

    /// <summary>
    /// Unloads the specified scene, or the current scene if none is specified.
    /// </summary>
    public void UnloadScene()
    {
        // Determine which scene to unload. Default to the scene this GameObject is part of.
        string sceneToUnload = string.IsNullOrEmpty(_sceneNameToUnload)
            ? gameObject.scene.name
            : _sceneNameToUnload;

        // Check if the scene is actually loaded before trying to unload it.
        if (SceneManager.GetSceneByName(sceneToUnload).isLoaded)
        {
            Debug.Log($"[SceneUnloader] Unloading scene '{sceneToUnload}'.", this);
            SceneManager.UnloadSceneAsync(sceneToUnload);
        }
        else
        {
            Debug.LogWarning($"[SceneUnloader] Scene '{sceneToUnload}' is not loaded or does not exist.", this);
        }
    }
}