using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// This helper script connects a UGI Toggle's OnValueChanged event to the UISceneLoader.
/// It is designed to be placed on a GameObject that has a Toggle component.
/// When the toggle is switched on, it calls the UISceneLoader to load a specified scene.
/// </summary>
[RequireComponent(typeof(Toggle))]
public class SceneLoadToggler : MonoBehaviour
{
    [Tooltip("The scene to load when this toggle is activated. Drag a scene asset here from the Project window.")]
    [SerializeField]
    private Object _sceneAsset;

    [Tooltip("Reference to the UISceneLoader in the scene. If left null, it will try to find it automatically.")]
    [SerializeField]
    private UISceneLoader _sceneLoader;

    // The path of the scene to load. This is what's used at runtime.
    [HideInInspector]
    [SerializeField]
    private string _scenePath;

    private Toggle _toggle;

    /// <summary>
    /// Public accessor for the scene name, derived from the scene path.
    /// </summary>
    public string SceneName => string.IsNullOrEmpty(_scenePath) ? string.Empty : System.IO.Path.GetFileNameWithoutExtension(_scenePath);

#if UNITY_EDITOR
    /// <summary>
    /// This function is called when the script is loaded or a value is changed in the Inspector.
    /// It's used here to update the scene path whenever a new scene asset is assigned.
    /// </summary>
    void OnValidate()
    {
        if (_sceneAsset != null && _sceneAsset is SceneAsset)
        {
            // Get the path of the scene asset. This path is valid for loading with SceneManager.
            _scenePath = AssetDatabase.GetAssetPath(_sceneAsset);
        }
        else
        {
            _scenePath = string.Empty;
        }
    }
#endif

    void Awake()
    {
        _toggle = GetComponent<Toggle>();

        // Find the UISceneLoader if it's not assigned in the inspector
        if (_sceneLoader == null)
        {
#if UNITY_2023_1_OR_NEWER
            _sceneLoader = FindFirstObjectByType<UISceneLoader>();
#else
            _sceneLoader = FindObjectOfType<UISceneLoader>();
#endif
        }

        if (_sceneLoader == null)
        {
            Debug.LogError("[SceneLoadToggler] UISceneLoader not found in the scene. Please add it to a GameObject.", this);
            enabled = false; // Disable the script if the loader is missing
            return;
        }

        if (string.IsNullOrEmpty(_scenePath))
        {
            Debug.LogError("[SceneLoadToggler] Scene Asset is not assigned. Please assign a scene in the inspector.", this);
            enabled = false;
            return;
        }
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
        // We only want to load the scene when the toggle is turned ON
        if (isOn)
        {
            if (!string.IsNullOrEmpty(_scenePath))
            {
                // Get scene name from path to display in the log
                string sceneName = SceneName;
                Debug.Log($"[SceneLoadToggler] Toggle for scene '{sceneName}' is on. Requesting scene load.", this);
                _sceneLoader.LoadSceneByName(sceneName);
            }
            else
            {
                Debug.LogWarning("[SceneLoadToggler] Cannot load scene because no scene asset is assigned.", this);
            }
        }
    }
}
