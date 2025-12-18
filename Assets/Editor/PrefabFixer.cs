// 12/16/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;
using UnityEditor;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using System.Collections.Generic;

public class PrefabFixer : EditorWindow
{
    private GameObject _prefabToFix;
    private Transform _proxyParent; // Field for the proxy parent Transform

    [MenuItem("Tools/Prefab Fixer")]
    public static void ShowWindow()
    {
        GetWindow<PrefabFixer>("Prefab Fixer");
    }

    private void OnEnable()
    {
        titleContent = EditorGUIUtility.IconContent("d_Settings");
        titleContent.text = "Prefab Fixer";
    }

    private void OnGUI()
    {
        GUILayout.Label("Assign Prefab to Fix", EditorStyles.boldLabel);
        _prefabToFix = (GameObject)EditorGUILayout.ObjectField("Target Prefab", _prefabToFix, typeof(GameObject), false);

        // UI for assigning the proxy parent Transform
        _proxyParent = (Transform)EditorGUILayout.ObjectField("Proxy Parent (Optional)", _proxyParent, typeof(Transform), true);
        EditorGUILayout.HelpBox("Assign a Transform to parent the temporary grab objects. If left empty, the prefab's root will be used.", MessageType.Info);

        if (GUILayout.Button("Fix Prefab"))
        {
            if (_prefabToFix != null)
            {
                // If no parent is assigned, use the prefab's root transform.
                Transform parent = _proxyParent != null ? _proxyParent : _prefabToFix.transform;
                FixPrefab(_prefabToFix, parent);
            }
            else
            {
                Debug.LogError("Prefab Fixer: Please assign a prefab first.");
            }
        }
    }

    private static void FixPrefab(GameObject prefab, Transform proxyParent)
    {
        // Open the prefab for editing
        string path = AssetDatabase.GetAssetPath(prefab);
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(path);

        // Register the whole prefab root for Undo. This is important for prefab operations.
        Undo.RegisterFullObjectHierarchyUndo(prefabRoot, "Fix Prefab");

        // Find the correct proxy parent within the loaded prefab instance
        Transform proxyParentInPrefab = proxyParent;
        if (proxyParent != null && !proxyParent.IsChildOf(prefabRoot.transform))
        {
            // If the assigned parent is not part of the prefab, find it by name.
            // This handles cases where a scene object was dragged into the slot.
            var allTransforms = prefabRoot.GetComponentsInChildren<Transform>(true);
            foreach(var t in allTransforms)
            {
                if (t.name == proxyParent.name)
                {
                    proxyParentInPrefab = t;
                    break;
                }
            }
        }
        else if (proxyParent == null)
        {
            proxyParentInPrefab = prefabRoot.transform;
        }


        // Fix all Grabbable components within the prefab
        Grabbable[] grabbables = prefabRoot.GetComponentsInChildren<Grabbable>(true);
        if (grabbables.Length == 0)
        {
            Debug.LogWarning("Prefab Fixer: No 'Grabbable' components found in the prefab.");
            PrefabUtility.UnloadPrefabContents(prefabRoot);
            return;
        }

        foreach (Grabbable grabbable in grabbables)
        {
            GameObject obj = grabbable.gameObject;
            Debug.Log($"Prefab Fixer: Processing '{obj.name}'...");

            // 1. Clean up misplaced interactables from children
            // "Smart Cleanup" - preserves interactables if they are on a valid Grabbable child (Ragdoll support)
            CleanChildren(obj.transform);

            // 2. Ensure the main grabbable object has the correct, standard HandGrabInteractable
            // First, remove any TouchHandGrabInteractable on the main object to avoid conflicts
            TouchHandGrabInteractable touchInteractable = obj.GetComponent<TouchHandGrabInteractable>();
            if (touchInteractable != null)
            {
                Debug.Log($"Prefab Fixer: Removing conflicting 'TouchHandGrabInteractable' from '{obj.name}'.");
                Undo.DestroyObjectImmediate(touchInteractable);
            }

            // Now, ensure the standard HandGrabInteractable exists
            HandGrabInteractable handGrabInteractable = obj.GetComponent<HandGrabInteractable>();
            if (handGrabInteractable == null)
            {
                handGrabInteractable = Undo.AddComponent<HandGrabInteractable>(obj);
                Debug.Log($"Prefab Fixer: Added 'HandGrabInteractable' to '{obj.name}'.");
            }

            // 3. Ensure the physics transformer exists
            OneGrabPhysicsJointTransformer transformer = obj.GetComponent<OneGrabPhysicsJointTransformer>();
            if (transformer == null)
            {
                transformer = Undo.AddComponent<OneGrabPhysicsJointTransformer>(obj);
                Debug.Log($"Prefab Fixer: Added 'OneGrabPhysicsJointTransformer' to '{obj.name}'.");
            }

            // 4. Configure the transformer's Rigidbody root
            if (transformer != null)
            {
                SerializedObject transformerSO = new SerializedObject(transformer);
                SerializedProperty rigidbodiesRootProp = transformerSO.FindProperty("_rigidbodiesRoot");
                if (rigidbodiesRootProp != null)
                {
                    rigidbodiesRootProp.objectReferenceValue = proxyParentInPrefab;
                    transformerSO.ApplyModifiedProperties();
                }
            }

            // 5. Link the transformer to the Grabbable's serialized field
            SerializedObject grabbableSO = new SerializedObject(grabbable);
            SerializedProperty oneGrabTransformerProp = grabbableSO.FindProperty("_oneGrabTransformer");
            if (oneGrabTransformerProp != null)
            {
                oneGrabTransformerProp.objectReferenceValue = transformer;
                grabbableSO.ApplyModifiedProperties();
                Debug.Log($"Prefab Fixer: Linked physics transformer on '{obj.name}'.");
            }
        }

        // Save the changes back to the prefab asset on disk
        PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
        PrefabUtility.UnloadPrefabContents(prefabRoot);

        Debug.Log("Prefab Fixer: Prefab fixing process complete!");
    }

    private static void CleanChildren(Transform parent)
    {
        // Get all IHandGrabInteractable components in children, but not on the parent itself.
        var interactablesInChildren = parent.GetComponentsInChildren<IHandGrabInteractable>(true);

        foreach (var interactable in interactablesInChildren)
        {
            Component component = interactable as Component;
            if (component == null) continue;

            // Skip if it's on the parent object itself
            if (component.gameObject == parent.gameObject) continue;

            // SMART CHECK: Does this child object have its own Grabbable component?
            // If yes, it's a valid part of a ragdoll (like a Hand). Leave it alone.
            if (component.GetComponent<Grabbable>() != null)
            {
                continue;
            }

            // If no Grabbable, it's a misplaced interactable (like the Teddy Bear collider). Destroy it.
            Debug.Log($"Prefab Fixer: Removing misplaced '{component.GetType().Name}' from child '{component.name}' (No Grabbable found on child).");
            Undo.DestroyObjectImmediate(component);
        }
    }
}