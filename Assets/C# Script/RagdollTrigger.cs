using UnityEngine;
using Oculus.Interaction;

public class RagdollTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator _animator;
    
    // We cache these so we can toggle them
    private Rigidbody[] _limbs;
    private Grabbable[] _grabbables;

    private void Awake()
    {
        if (_animator == null) _animator = GetComponent<Animator>();

        // Find all components created by the Builder
        _limbs = GetComponentsInChildren<Rigidbody>(true);
        _grabbables = GetComponentsInChildren<Grabbable>(true);
    }

    private void Start()
    {
        // Start in "Alive" state
        SetRagdollState(false);
    }

    /// <summary>
    /// Call this method when the character dies (e.g., HP <= 0)
    /// </summary>
    public void TriggerDeath()
    {
        SetRagdollState(true);
    }

    private void SetRagdollState(bool isDead)
    {
        // 1. Toggle Animator
        if (_animator != null)
        {
            _animator.enabled = !isDead;
        }

        // 2. Toggle Physics
        foreach (var rb in _limbs)
        {
            // When alive, limbs should be kinematic (controlled by animation)
            // When dead, they should be non-kinematic (controlled by physics)
            rb.isKinematic = !isDead;
            
            // Optional: Enable/Disable collision detection modes for performance
            rb.collisionDetectionMode = isDead ? CollisionDetectionMode.Continuous : CollisionDetectionMode.Discrete;
        }

        // 3. Toggle Grabbability
        // We don't want the player grabbing the NPC while they are alive and fighting
        foreach (var grabbable in _grabbables)
        {
            grabbable.enabled = isDead;
        }
    }
}