using UnityEngine;
using UnityEngine.SceneManagement;
using Oculus.Interaction.Input;

/// <summary>
/// This script manages a pause menu that can be toggled with a hand gesture.
/// When activated, it pauses the game and displays a UI menu in front of the player.
///
/// To use this script:
/// 1. Create an empty GameObject in your scene (e.g., "PauseMenuController").
/// 2. Attach this script to the GameObject.
/// 3. Assign your main UI canvas to the `_pauseMenuCanvas` field.
/// 4. Assign both the left and right hand OVRHand components to the `_leftHand` and `_rightHand` fields.
/// 5. Assign your main camera (usually the CenterEyeAnchor in an OVRCameraRig) to the `_mainCamera` field.
/// 6. Connect the `Resume()` and `QuitGame()` methods to buttons on your UI canvas.
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The main UI canvas for the pause menu.")]
    [SerializeField]
    private GameObject _pauseMenuCanvas;

    [Tooltip("The left hand to track for the pause gesture.")]
    [SerializeField]
    private OVRHand _leftHand;

    [Tooltip("The right hand to track for the pause gesture.")]
    [SerializeField]
    private OVRHand _rightHand;

    [Tooltip("The player's main camera. Used to position the menu in front of the player.")]
    [SerializeField]
    private Camera _mainCamera;

    [Header("Settings")]
    [Tooltip("The distance from the camera where the menu will appear.")]
    [SerializeField]
    private float _menuDistance = 2.0f;

    private bool _isPaused = false;
    private bool _wasLeftPinching = false;
    private bool _wasRightPinching = false;

    void Start()
    {
        if (_pauseMenuCanvas == null)
        {
            Debug.LogError("[PauseMenuController] Pause Menu Canvas reference is not set.", this);
            enabled = false;
            return;
        }

        if (_leftHand == null || _rightHand == null)
        {
            Debug.LogError("[PauseMenuController] One or both hand references are not set. Please assign both OVRHand components.", this);
            enabled = false;
            return;
        }

        if (_mainCamera == null)
        {
            Debug.LogError("[PauseMenuController] Main Camera reference is not set.", this);
            // Try to find it automatically as a fallback
            _mainCamera = Camera.main;
            if (_mainCamera == null)
            {
                enabled = false;
                return;
            }
        }

        // Start with the menu hidden and the game running
        _pauseMenuCanvas.SetActive(false);
        Time.timeScale = 1f;
    }

    void Update()
    {
        HandleGesture(_leftHand, ref _wasLeftPinching);
        HandleGesture(_rightHand, ref _wasRightPinching);
    }

    private void HandleGesture(OVRHand hand, ref bool wasPinching)
    {
        // Only proceed if the hand is being tracked with high confidence
        if (!hand.IsTracked || hand.HandConfidence != OVRHand.TrackingConfidence.High)
        {
            wasPinching = false; // Reset state if tracking is lost
            return;
        }

        bool isPinching = hand.GetFingerIsPinching(OVRHand.HandFinger.Index);

        // Check if the pinch has just started (was not pinching last frame, but is now)
        if (isPinching && !wasPinching)
        {
            TogglePauseMenu();
        }

        wasPinching = isPinching;
    }

    /// <summary>
    /// Toggles the pause state of the game and the visibility of the menu.
    /// </summary>
    public void TogglePauseMenu()
    {
        _isPaused = !_isPaused;
        _pauseMenuCanvas.SetActive(_isPaused);

        if (_isPaused)
        {
            // Pause the game
            Time.timeScale = 0f;

            // Position the menu in front of the camera
            PositionMenu();
        }
        else
        {
            // Resume the game
            Time.timeScale = 1f;
        }
    }

    /// <summary>
    /// Positions the menu in front of the player's camera.
    /// </summary>
    private void PositionMenu()
    {
        Transform cameraTransform = _mainCamera.transform;
        _pauseMenuCanvas.transform.position = cameraTransform.position + (cameraTransform.forward * _menuDistance);
        _pauseMenuCanvas.transform.rotation = Quaternion.LookRotation(cameraTransform.forward);
    }

    /// <summary>
    /// Public method to be called by a "Resume" button on the UI.
    /// </summary>
    public void Resume()
    {
        if (_isPaused)
        {
            TogglePauseMenu();
        }
    }

    /// <summary>
    /// Public method to be called by a "Quit" button on the UI.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("[PauseMenuController] Quitting application...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}   