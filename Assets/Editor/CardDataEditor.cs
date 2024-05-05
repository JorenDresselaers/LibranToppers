using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

[CustomEditor(typeof(CardData))]
public class CardDataEditor : Editor
{
    private Type selectedAbilityType;
    private List<Type> abilityTypes;
    private Dictionary<int, bool> foldouts = new Dictionary<int, bool>(); // Track foldout states

    private void OnEnable()
    {
        // Load all CardAbility types
        abilityTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsSubclassOf(typeof(CardAbility)) && !type.IsAbstract)
            .ToList();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space(25f);
        EditorGUILayout.LabelField("Create New Ability:", EditorStyles.boldLabel);
        DrawSeparator(Color.gray);
        EditorGUILayout.Space();

        CardData cardData = (CardData)target;

        if (abilityTypes != null && abilityTypes.Count > 0)
        {
            int selected = abilityTypes.IndexOf(selectedAbilityType);
            string[] options = abilityTypes.Select(type => type.Name).ToArray();
            int choice = EditorGUILayout.Popup("Select Ability Type", selected, options);

            if (choice != selected)
            {
                selectedAbilityType = abilityTypes[choice];
            }

            if (GUILayout.Button("Add"))
            {
                AddAbilityOfType(cardData, selectedAbilityType);
            }
        }

        EditorGUILayout.Space(25f);
        EditorGUILayout.LabelField("Current Abilities:", EditorStyles.boldLabel);
        DrawSeparator(Color.gray);

        if (cardData.abilities != null)
        {
            for (int i = 0; i < cardData.abilities.Count; i++)
            {
                SerializedObject serializedAbility = new SerializedObject(cardData.abilities[i]);
                SerializedProperty propertyIterator = serializedAbility.GetIterator();

                if (!foldouts.ContainsKey(i))
                    foldouts[i] = true;

                EditorGUILayout.BeginVertical("box");
                foldouts[i] = EditorGUILayout.Foldout(foldouts[i], cardData.abilities[i].GetType().Name, true);

                if (foldouts[i])
                {
                    bool enterChildren = true;
                    while (propertyIterator.NextVisible(enterChildren))
                    {
                        enterChildren = false;

                        if (propertyIterator.name == "m_Script") continue; // Skip showing the script reference

                        if (propertyIterator.isArray && propertyIterator.propertyType == SerializedPropertyType.Generic)
                        {
                            DrawList(propertyIterator);
                        }
                        else
                        {
                            EditorGUILayout.PropertyField(propertyIterator, true);
                        }
                    }
                    serializedAbility.ApplyModifiedProperties();

                    if (GUILayout.Button("Remove"))
                    {
                        RemoveAbility(cardData, i);
                    }
                }

                EditorGUILayout.EndVertical();
                DrawSeparator(Color.gray);
            }
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(cardData);
        }
    }

    private void DrawList(SerializedProperty property)
    {
        // Explicitly manage the foldout state
        bool wasExpanded = property.isExpanded;
        //EditorGUILayout.PropertyField(property, new GUIContent(property.displayName), true);  // Allow automatic handling when expanded
        EditorGUILayout.LabelField(property.displayName, EditorStyles.boldLabel);

        // Ensure the property is expanded if performing modifications
        property.isExpanded = true;

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;
            int toRemove = -1;  // Index of element to remove, if any
            for (int i = 0; i < property.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(property.GetArrayElementAtIndex(i), new GUIContent("Element " + i));
                if (GUILayout.Button("-", GUILayout.Width(30)))
                {
                    toRemove = i;  // Mark this element for removal
                }
                EditorGUILayout.EndHorizontal();
            }

            // Handle removal outside the loop to avoid modifying the list during iteration
            if (toRemove != -1)
            {
                property.DeleteArrayElementAtIndex(toRemove);
                if (property.GetArrayElementAtIndex(toRemove) != null &&
                    property.GetArrayElementAtIndex(toRemove).propertyType == SerializedPropertyType.ObjectReference &&
                    property.GetArrayElementAtIndex(toRemove).objectReferenceValue != null)
                {
                    property.DeleteArrayElementAtIndex(toRemove);  // Double removal for object references
                }
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Element"))
            {
                property.arraySize++;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel--;

            // Apply modifications immediately
            property.serializedObject.ApplyModifiedProperties();
        }

        // Restore the original expansion state if necessary
        property.isExpanded = wasExpanded;
    }


    private void AddAbilityOfType(CardData cardData, Type abilityType)
    {
        CardAbility newAbility = (CardAbility)CreateInstance(abilityType);

        // Generate a unique name based on existing abilities
        int count = 1;
        string baseName = cardData.cardName + " " + abilityType.Name;
        string newName = baseName + " " + count;

        // Count how many of this type already exist to ensure the name is unique
        while (cardData.abilities.Any(a => a.name == newName))
        {
            count++;
            newName = baseName + " " + count;
        }

        newAbility.name = newName;

        string assetPath = AssetDatabase.GetAssetPath(cardData);
        string folderPath = assetPath.Substring(0, assetPath.LastIndexOf('/'));

        // Updated to include a general 'Abilities' folder followed by a card-specific subfolder
        string abilitiesFolderPath = folderPath + "/Abilities";
        if (!AssetDatabase.IsValidFolder(abilitiesFolderPath))
        {
            AssetDatabase.CreateFolder(folderPath, "Abilities");
        }

        string newFolderPath = abilitiesFolderPath + "/" + cardData.name;
        if (!AssetDatabase.IsValidFolder(newFolderPath))
        {
            AssetDatabase.CreateFolder(abilitiesFolderPath, cardData.name);
        }

        string finalPath = newFolderPath + "/" + newAbility.name + ".asset";
        AssetDatabase.CreateAsset(newAbility, finalPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        cardData.abilities.Add(newAbility);
        EditorUtility.SetDirty(cardData);
    }

    private void RemoveAbility(CardData cardData, int index)
    {
        CardAbility abilityToRemove = cardData.abilities[index];
        if (abilityToRemove != null)
        {
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(abilityToRemove));
            cardData.abilities.RemoveAt(index);
            AssetDatabase.SaveAssets();
        }
    }

    // Utility method to draw a separator line
    private void DrawSeparator(Color color)
    {
        Rect rect = GUILayoutUtility.GetRect(1f, 1f);
        rect.height = 3;
        //rect.y += 10;
        rect.x -= 2;
        rect.width += 6;
        EditorGUI.DrawRect(rect, color);
    }
}
