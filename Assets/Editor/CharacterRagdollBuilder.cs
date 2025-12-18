using UnityEngine;
using UnityEditor;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;

public class CharacterRagdollBuilder : EditorWindow
{
    private GameObject _characterRoot;
    private Transform _proxyParent;

    [MenuItem("Tools/Character Ragdoll Builder")]
    public static void ShowWindow()
    {
        GetWindow<CharacterRagdollBuilder>("Ragdoll Builder");
    }

    private void OnEnable()
    {
        titleContent = EditorGUIUtility.IconContent("d_Avatar Icon");
        titleContent.text = "Ragdoll Builder";
    }

    private void OnGUI()
    {
        GUILayout.Label("Character Ragdoll Setup", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("1. Assign the Character Root (must have an Animator and Unity Ragdoll Wizard already applied).\n2. This tool adds the Meta Interaction components to every limb.", MessageType.Info);

        _characterRoot = (GameObject)EditorGUILayout.ObjectField("Character Root", _characterRoot, typeof(GameObject), true);
        
        _proxyParent = (Transform)EditorGUILayout.ObjectField("Proxy Parent (Optional)", _proxyParent, typeof(Transform), true);
        EditorGUILayout.HelpBox("Assign a Transform (e.g., 'Interaction Root') to hold the temporary grab joints. If empty, the Character Root is used.", MessageType.None);

        if (GUILayout.Button("Build Ragdoll Interaction"))
        {
            if (_characterRoot != null)
            {
                Transform parent = _proxyParent != null ? _proxyParent : _characterRoot.transform;
                BuildRagdoll(_characterRoot, parent);
            }
            else
            {
                Debug.LogError("Ragdoll Builder: Please assign a Character Root GameObject.");
            }
        }
    }

    private static void BuildRagdoll(GameObject root, Transform proxyParent)
    {
        // Find all Rigidbodies (limbs)
        Rigidbody[] limbs = root.GetComponentsInChildren<Rigidbody>(true);

        if (limbs.Length == 0)
        {
            Debug.LogWarning("Ragdoll Builder: No Rigidbodies found. Please use the Unity 'Ragdoll Wizard' first to set up the basic physics.");
            return;
        }

        foreach (Rigidbody limb in limbs)
        {
            GameObject obj = limb.gameObject;
            
            // 1. Add Grabbable
            Grabbable grabbable = obj.GetComponent<Grabbable>();
            if (grabbable == null) grabbable = Undo.AddComponent<Grabbable>(obj);

            // Link Rigidbody to Grabbable explicitly
            SerializedObject grabbableSO = new SerializedObject(grabbable);
            SerializedProperty rbProp = grabbableSO.FindProperty("_rigidbody");
            if (rbProp != null)
            {
                rbProp.objectReferenceValue = limb;
                grabbableSO.ApplyModifiedProperties();
            }

            // 2. Add HandGrabInteractable
            HandGrabInteractable handGrab = obj.GetComponent<HandGrabInteractable>();
            if (handGrab == null) handGrab = Undo.AddComponent<HandGrabInteractable>(obj);

            // 3. Add Physics Transformer
            OneGrabPhysicsJointTransformer transformer = obj.GetComponent<OneGrabPhysicsJointTransformer>();
            if (transformer == null) transformer = Undo.AddComponent<OneGrabPhysicsJointTransformer>(obj);

            // Configure Transformer
            SerializedObject transformerSO = new SerializedObject(transformer);
            
            // Set Proxy Parent
            SerializedProperty rootProp = transformerSO.FindProperty("_rigidbodiesRoot");
            if (rootProp != null)
            {
                rootProp.objectReferenceValue = proxyParent;
            }

            // IMPORTANT: For ragdolls, we usually want Physics Grabs (not Kinematic) so the limb dangles
            SerializedProperty kinematicGrabProp = transformerSO.FindProperty("_isKinematicGrab");
            if (kinematicGrabProp != null)
            {
                kinematicGrabProp.boolValue = false; // Use physics joints, don't snap to hand
            }

            transformerSO.ApplyModifiedProperties();

            // 4. Link Transformer to Grabbable
            SerializedProperty transformerLinkProp = grabbableSO.FindProperty("_oneGrabTransformer");
            if (transformerLinkProp != null)
            {
                transformerLinkProp.objectReferenceValue = transformer;
                grabbableSO.ApplyModifiedProperties();
            }
        }

        Debug.Log($"Ragdoll Builder: Successfully configured {limbs.Length} limbs on '{root.name}'.");
    }
}