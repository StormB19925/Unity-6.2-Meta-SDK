using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Connects a UI Toggle's OnValueChanged event to unload a scene.
/// When the toggle is switched on, it unloads the specified scene,
/// or its own scene if none is specified.
/// </summary>
[RequireComponent(typeof(Toggle))]
public class SceneUnloadToggler : MonoBehaviour
{
    [Tooltip("Optional: Specify the exact name of the scene to unload. If left empty, it will unload the scene this component is in.")]
    [SerializeField]
    private string _sceneNameToUnload;

    private Toggle _toggle;

    void Awake()
    {
        _toggle = GetComponent<Toggle>();
    }

    void OnEnable()
    {
        _toggle.onValueChanged.AddListener(HandleToggleValueChanged);
    }

    void OnDisable()
    {
        _toggle.onValueChanged.RemoveListener(HandleToggleValueChanged);
    }

    /// <summary>
    /// Called when the toggle's value changes.
    /// </summary>
    /// <param name="isOn">The new state of the toggle.</param>
    private void HandleToggleValueChanged(bool isOn)
    {
        // Unload the scene only when the toggle is activated (checked).
        if (isOn)
        {
            // Determine which scene to unload. Default to the scene this GameObject is part of.
            string sceneToUnload = string.IsNullOrEmpty(_sceneNameToUnload)
                ? gameObject.scene.name
                : _sceneNameToUnload;

            Debug.Log($"[SceneUnloadToggler] Toggle is on. Unloading scene '{sceneToUnload}'.", this);

            if (SceneManager.GetSceneByName(sceneToUnload).isLoaded)
            {
                SceneManager.UnloadSceneAsync(sceneToUnload);
            }
            else
            {
                Debug.LogWarning($"[SceneUnloadToggler] Scene '{sceneToUnload}' is not loaded or does not exist.", this);
            }
        }
    }
}