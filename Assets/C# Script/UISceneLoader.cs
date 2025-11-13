using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UISceneLoader : MonoBehaviour
{
    [Header("Loading Options")]
    [SerializeField] private bool _useAsync = true;
    [SerializeField] private LoadSceneMode _loadMode = LoadSceneMode.Single;
    [SerializeField] private bool _allowReloadCurrent = false;

    /// <summary>
    /// Load a scene by name. Scenes must be added to Build Settings.
    /// </summary>
    public void LoadSceneByName(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[UISceneLoader] Scene name is null or empty.", this);
            return;
        }

        var active = SceneManager.GetActiveScene().name;
        if (!_allowReloadCurrent && active == sceneName)
        {
            Debug.Log($"[UISceneLoader] Scene '{sceneName}' already active. Skipping.", this);
            return;
        }

        if (_useAsync)
            StartCoroutine(LoadRoutine(sceneName));
        else
            SceneManager.LoadScene(sceneName, _loadMode);
    }

    private IEnumerator LoadRoutine(string sceneName)
    {
        var op = SceneManager.LoadSceneAsync(sceneName, _loadMode);
        if (op == null)
        {
            Debug.LogError($"[UISceneLoader] Failed to start async load for '{sceneName}'.", this);
            yield break;
        }
        yield return op;
        // Post-load hooks can go here.
    }
}