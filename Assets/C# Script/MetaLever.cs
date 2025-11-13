using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Assertions;
using Oculus.Interaction;
using System;

/// <summary>
/// This script acts as a bridge between the Meta Interaction SDK's Grabbable and Transformer components
/// and a custom lever system. It reads the rotation value from a OneGrabRotateTransformer and
/// translates it into a normalized 0-1 float value, which is then broadcast through a UnityEvent.
/// This allows for easy integration with Animators, UI sliders, or any other system that needs
/// to react to the lever's state.
/// </summary>
[RequireComponent(typeof(Grabbable), typeof(OneGrabRotateTransformer))]
public class MetaLever : MonoBehaviour
{
    [Header("Component References")]
    [Tooltip("The transformer that constrains the grab movement to a rotation.")]
    [SerializeField]
    private OneGrabRotateTransformer _rotateTransformer;

    [Header("Events")]
    [Tooltip("This event is invoked every frame the lever's value changes. It sends a float from 0.0 to 1.0.")]
    [SerializeField]
    private UnityEvent<float> _onValueChanged;

    private Vector3 _initialRotation;

    private void Awake()
    {
        // Ensure the transformer has been assigned in the Inspector.
        Assert.IsNotNull(_rotateTransformer, $"The {nameof(MetaLever)} script requires a {nameof(_rotateTransformer)} to be assigned in the Inspector.");
        _initialRotation = transform.localEulerAngles;
    }

    private void Update()
    {
        var constraints = _rotateTransformer.Constraints;
        if (!constraints.MinAngle.Constrain || !constraints.MaxAngle.Constrain)
        {
            // We cannot calculate a normalized value without both min and max constraints.
            return;
        }

        float minAngle = constraints.MinAngle.Value;
        float maxAngle = constraints.MaxAngle.Value;

        // Get the current angle based on the transformer's specified axis.
        float currentAngle = GetAngleForAxis(_rotateTransformer.RotationAxis);

        // Normalize the value from the angle range to a 0-1 range.
        float normalizedValue = Mathf.InverseLerp(minAngle, maxAngle, currentAngle);

        // Invoke the event and pass the current normalized value.
        _onValueChanged.Invoke(normalizedValue);
    }

    /// <summary>
    /// Gets the current rotation angle for the specified axis, handling Euler angle wrapping.
    /// </summary>
    private float GetAngleForAxis(OneGrabRotateTransformer.Axis axis)
    {
        // We use localEulerAngles, as the rotation is applied in local space.
        Vector3 eulerAngles = transform.localEulerAngles;
        float angle = 0;

        switch (axis)
        {
            case OneGrabRotateTransformer.Axis.Right: // X-axis
                angle = eulerAngles.x;
                break;
            case OneGrabRotateTransformer.Axis.Up: // Y-axis
                angle = eulerAngles.y;
                break;
            case OneGrabRotateTransformer.Axis.Forward: // Z-axis
                angle = eulerAngles.z;
                break;
        }

        // Convert the 0-360 degree angle from Euler angles to a -180 to 180 range
        // to correctly handle negative constraints.
        if (angle > 180)
        {
            angle -= 360;
        }

        return angle;
    }

    #region Inject
    /// <summary>
    /// Injects the OneGrabRotateTransformer dependency.
    /// </summary>
    public void InjectRotateTransformer(OneGrabRotateTransformer rotateTransformer)
    {
        _rotateTransformer = rotateTransformer;
    }
    #endregion
}
    