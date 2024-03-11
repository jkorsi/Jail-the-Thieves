using UnityEngine;
using UnityEditor;

public class MultiScript : EditorWindow
{
    private MonoScript scriptToAdd;
    private bool addToChildren = false;

    [MenuItem("Window/MultiScript")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(MultiScript), false, "MultiScript");
    }

    void OnGUI()
    {
        scriptToAdd = EditorGUILayout.ObjectField("Script to add", scriptToAdd, typeof(MonoScript), false) as MonoScript;

        if (scriptToAdd != null)
        {
            // Display selected objects
            GUILayout.Label("Selected Objects:", EditorStyles.boldLabel);
            if (Selection.gameObjects.Length > 0)
            {
                string selectedObjectsSummary = "";
                for (int i = 0; i < Mathf.Min(10, Selection.gameObjects.Length); i++)
                {
                    selectedObjectsSummary += Selection.gameObjects[i].name + "\n";
                }

                if (Selection.gameObjects.Length > 10)
                {
                    selectedObjectsSummary += $"...and {Selection.gameObjects.Length - 10} more objects";
                }

                GUILayout.Label(selectedObjectsSummary);
            }
            else
            {
                GUILayout.Label("No objects selected.");
            }

            addToChildren = EditorGUILayout.Toggle("Add to all children (recursive)", addToChildren);

            if (GUILayout.Button($"Add {scriptToAdd.name} to selected objects"))
            {
                AddScriptToSelectedObjects();
            }
        }
        else
        {
            GUILayout.Label("Select a script to add.");
        }
    }

    void AddScriptToSelectedObjects()
    {
        if (scriptToAdd != null && Selection.gameObjects.Length > 0)
        {
            System.Type scriptType = scriptToAdd.GetClass();
            foreach (GameObject obj in Selection.gameObjects)
            {
                AddComponentToGameObject(obj, scriptType);

                if (addToChildren)
                {
                    RecursiveAddToChildren(obj.transform, scriptType);
                }
            }
        }
    }

    void AddComponentToGameObject(GameObject obj, System.Type scriptType)
    {
        if (obj != null && scriptType != null && scriptType.IsSubclassOf(typeof(Component)))
        {
            obj.AddComponent(scriptType);
        }
    }

    void RecursiveAddToChildren(Transform parent, System.Type scriptType)
    {
        foreach (Transform child in parent)
        {
            AddComponentToGameObject(child.gameObject, scriptType);

            if (child.childCount > 0)
            {
                RecursiveAddToChildren(child, scriptType);
            }
        }
    }
}
