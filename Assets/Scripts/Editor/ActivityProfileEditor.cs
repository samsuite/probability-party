using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ActivityProfile))]
public class ActivityProfileEditor : Editor {

    public ActivityProfile activity {
        get {
            return target as ActivityProfile;
        }
    }

    public override void OnInspectorGUI () {

        SerializedProperty descriptionProperty = serializedObject.FindProperty(nameof(ActivityProfile.description));
        SerializedProperty exactPlayerCountProperty = serializedObject.FindProperty(nameof(ActivityProfile.exactPlayerCount));
        SerializedProperty minPlayerCountProperty = serializedObject.FindProperty(nameof(ActivityProfile.minPlayerCount));
        SerializedProperty maxPlayerCountProperty = serializedObject.FindProperty(nameof(ActivityProfile.maxPlayerCount));
        SerializedProperty requireExactPlayerCountProperty = serializedObject.FindProperty(nameof(ActivityProfile.requireExactPlayerCount));
        SerializedProperty hasMinPlayerCountProperty = serializedObject.FindProperty(nameof(ActivityProfile.hasMinPlayerCount));
        SerializedProperty hasMaxPlayerCountProperty = serializedObject.FindProperty(nameof(ActivityProfile.hasMaxPlayerCount));
        SerializedProperty requireEvenPlayerCountProperty = serializedObject.FindProperty(nameof(ActivityProfile.requireEvenPlayerCount));
        SerializedProperty requireOddPlayerCountProperty = serializedObject.FindProperty(nameof(ActivityProfile.requireOddPlayerCount));


        EditorGUI.BeginChangeCheck();
        GUILayout.Label("Description:");
        GUILayout.Space(-16);
        EditorGUILayout.PropertyField(descriptionProperty, new GUIContent(""));

        GUILayout.Space(16);
        EditorGUILayout.BeginHorizontal();
        if (hasMinPlayerCountProperty.boolValue ||
            hasMaxPlayerCountProperty.boolValue ||
            requireEvenPlayerCountProperty.boolValue ||
            requireOddPlayerCountProperty.boolValue) {

            GUI.enabled = false;
        }
        EditorGUILayout.PropertyField(requireExactPlayerCountProperty, new GUIContent("Require exact player count?"));
        if (requireExactPlayerCountProperty.boolValue) {
            EditorGUILayout.PropertyField(exactPlayerCountProperty, new GUIContent("Count:"));
        }
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();


        EditorGUILayout.BeginHorizontal();
        if (requireExactPlayerCountProperty.boolValue) {
            GUI.enabled = false;
        }
        EditorGUILayout.PropertyField(hasMinPlayerCountProperty, new GUIContent("Has minimum player count?"));
        if (hasMinPlayerCountProperty.boolValue) {
            EditorGUILayout.PropertyField(minPlayerCountProperty, new GUIContent("Count:"));
        }
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();


        EditorGUILayout.BeginHorizontal();
        if (requireExactPlayerCountProperty.boolValue) {
            GUI.enabled = false;
        }
        EditorGUILayout.PropertyField(hasMaxPlayerCountProperty, new GUIContent("Has maximum player count?"));
        if (hasMaxPlayerCountProperty.boolValue) {
            EditorGUILayout.PropertyField(maxPlayerCountProperty, new GUIContent("Count:"));
        }
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        if (requireOddPlayerCountProperty.boolValue || requireExactPlayerCountProperty.boolValue) {
            GUI.enabled = false;
        }
        EditorGUILayout.PropertyField(requireEvenPlayerCountProperty, new GUIContent("Require even player count?"));
        GUI.enabled = true;

        if (requireEvenPlayerCountProperty.boolValue || requireExactPlayerCountProperty.boolValue) {
            GUI.enabled = false;
        }
        EditorGUILayout.PropertyField(requireOddPlayerCountProperty, new GUIContent("Require odd player count?"));
        GUI.enabled = true;


        if (EditorGUI.EndChangeCheck()) {
            serializedObject.ApplyModifiedProperties();
        }

        string playerCountSummary = string.Empty;
        if (requireExactPlayerCountProperty.boolValue) {
            if (exactPlayerCountProperty.intValue == 1) {
                playerCountSummary = "Exactly 1 player";
            }
            else {
                playerCountSummary = $"Exactly {exactPlayerCountProperty.intValue} players";
            }
        }
        else if (hasMinPlayerCountProperty.boolValue && !hasMaxPlayerCountProperty.boolValue) {
            playerCountSummary = $"{minPlayerCountProperty.intValue} or more players";
        }
        else if (hasMaxPlayerCountProperty.boolValue && !hasMinPlayerCountProperty.boolValue) {
            playerCountSummary = $"1 to {maxPlayerCountProperty.intValue} players";
        }
        else if (hasMaxPlayerCountProperty.boolValue && hasMinPlayerCountProperty.boolValue) {
            playerCountSummary = $"{minPlayerCountProperty.intValue} to {maxPlayerCountProperty.intValue} players";
        }

        if (playerCountSummary == string.Empty) {
            if (requireEvenPlayerCountProperty.boolValue) {
                playerCountSummary += "Any even number of players";
            }
            if (requireOddPlayerCountProperty.boolValue) {
                playerCountSummary += "Any odd number of players";
            }
        }
        else {
            if (requireEvenPlayerCountProperty.boolValue) {
                playerCountSummary += " (must be even)";
            }
            if (requireOddPlayerCountProperty.boolValue) {
                playerCountSummary += " (must be odd)";
            }
        }

        if (playerCountSummary == string.Empty) {
            playerCountSummary += "Any number of players";
        }

        bool issueDetected = false;
        if (hasMinPlayerCountProperty.boolValue && hasMaxPlayerCountProperty.boolValue) {
            if (minPlayerCountProperty.intValue >= maxPlayerCountProperty.intValue) {
                issueDetected = true;
            }
        }

        GUILayout.Space(8);
        GUIStyle italicsLabel = new GUIStyle(GUI.skin.label);
        italicsLabel.fontStyle = FontStyle.Italic;

        GUILayout.BeginHorizontal();
        GUILayout.Space(16);
        if (issueDetected) {
            GUI.color = Color.red;
        }
        else {
            GUI.enabled = false;
        }
        GUILayout.Label(playerCountSummary, italicsLabel);
        GUILayout.EndHorizontal();

        GUI.color = Color.white;
        GUI.enabled = true;
    }

}
