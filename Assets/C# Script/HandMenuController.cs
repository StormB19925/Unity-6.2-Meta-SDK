using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.Input;

/// <summary>
/// This script manages a UI menu that is attached to the user's hand.
/// It shows the menu when the palm is facing upwards (towards the user's face)
/// and hides it otherwise. This provides an intuitive way to access a menu in VR.
///
/// To use this script:
/// 1. Create a new GameObject in your scene to act as the controller (e.g., "HandMenuController").
/// 2. Attach this script to the GameObject.
/// 3. Create your UI menu as a world-space Canvas and assign it to the `_handMenu` field.
/// 4. In your scene, locate your OVRCameraRig or equivalent XR setup. Find the `OVRHand` components
///    for both the left and right hands and assign them to the corresponding fields.
/// 5. The script will automatically handle showing, hiding, and positioning the menu on the correct hand.
/// </summary>
public class HandMenuController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The UI GameObject to be shown or hidden. This should be a world-space canvas.")]
    [SerializeField]
    private GameObject _handMenu;

    [Tooltip("The left hand to track for the menu gesture.")]
    [SerializeField]
    private OVRHand _leftHand;

    [Tooltip("The right hand to track for the menu gesture.")]
    [SerializeField]
    private OVRHand _rightHand;

    [Header("Settings")]
    [Tooltip("The angle in degrees from the 'up' direction to trigger the menu's visibility. A lower value means the palm must be more directly facing up.")]
    [SerializeField]
    private float _showAngleThreshold = 45.0f;

    [Tooltip("The angle in degrees from the 'up' direction to trigger hiding the menu. This should be larger than the show threshold to prevent flickering.")]
    [SerializeField]
    private float _hideAngleThreshold = 65.0f;

    [Tooltip("The offset from the hand's position to place the menu.")]
    [SerializeField]
    private Vector3 _positionOffset = new Vector3(0, 0.1f, 0.1f);

    [Tooltip("The speed at which the menu follows the hand's position and rotation.")]
    [SerializeField]
    private float _followSpeed = 8.0f;

    private bool _isMenuVisible = false;
    private OVRHand _activeHand = null;

    void Start()
    {
        if (_handMenu == null)
        {
            Debug.LogError("[HandMenuController] Hand Menu reference is not set in the inspector.", this);
            enabled = false;
            return;
        }

        if (_leftHand == null || _rightHand == null)
        {
            Debug.LogError("[HandMenuController] One or both hand references are not set. Please assign both OVRHand components.", this);
            enabled = false;
            return;
        }

        // Start with the menu hidden
        _handMenu.SetActive(false);
        _isMenuVisible = false;
    }

    void Update()
    {
        // If the menu is visible, check if the active hand should hide it
        if (_isMenuVisible && _activeHand != null)
        {
            if (ShouldHideMenu(_activeHand))
            {
                HideMenu();
            }
            else
            {
                UpdateMenuTransform(_activeHand);
            }
            return;
        }

        // If the menu is not visible, check if either hand should show it
        if (!_isMenuVisible)
        {
            if (ShouldShowMenu(_leftHand))
            {
                ShowMenu(_leftHand);
            }
            else if (ShouldShowMenu(_rightHand))
            {
                ShowMenu(_rightHand);
            }
        }
    }

    private bool ShouldShowMenu(OVRHand hand)
    {
        if (!hand.IsTracked) return false;
        return Vector3.Angle(hand.transform.up, Camera.main.transform.up) < _showAngleThreshold;
    }

    private bool ShouldHideMenu(OVRHand hand)
    {
        if (!hand.IsTracked) return true;
        return Vector3.Angle(hand.transform.up, Camera.main.transform.up) > _hideAngleThreshold;
    }

    private void ShowMenu(OVRHand hand)
    {
        _activeHand = hand;
        _isMenuVisible = true;
        _handMenu.SetActive(true);
        UpdateMenuTransform(_activeHand);
    }

    private void HideMenu()
    {
        _activeHand = null;
        _isMenuVisible = false;
        _handMenu.SetActive(false);
    }

    /// <summary>
    /// Smoothly updates the menu's position and rotation to follow the hand.
    /// </summary>
    private void UpdateMenuTransform(OVRHand hand)
    {
        // Calculate the target position with the offset relative to the hand's rotation
        Vector3 targetPosition = hand.transform.position + (hand.transform.rotation * _positionOffset);

        // The menu should look away from the palm
        Quaternion targetRotation = hand.transform.rotation * Quaternion.LookRotation(Vector3.up, Vector3.forward);

        // Smoothly move the menu to the target position and rotation
        _handMenu.transform.position = Vector3.Lerp(_handMenu.transform.position, targetPosition, Time.deltaTime * _followSpeed);
        _handMenu.transform.rotation = Quaternion.Slerp(_handMenu.transform.rotation, targetRotation, Time.deltaTime * _followSpeed);
    }
}