using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneToggleSync : MonoBehaviour
{
    void Start()
    {
        var active = SceneManager.GetActiveScene().name;
        foreach (var t in GetComponentsInChildren<Toggle>(true))
        {
            var toggler = t.GetComponent<SceneLoadToggler>();
            if (!toggler) continue;

            // Use the public property instead of reflection.
            var sceneName = toggler.SceneName;

            if (sceneName == active)
                t.isOn = true;
        }
    }
}