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
        SerializedProperty qrLinkProperty = serializedObject.FindProperty(nameof(ActivityProfile.qrLink));
        SerializedProperty imageProperty = serializedObject.FindProperty(nameof(ActivityProfile.image));
        SerializedProperty tagsProperty = serializedObject.FindProperty(nameof(ActivityProfile.tags));
        SerializedProperty exactPlayerCountProperty = serializedObject.FindProperty(nameof(ActivityProfile.exactPlayerCount));
        SerializedProperty minPlayerCountProperty = serializedObject.FindProperty(nameof(ActivityProfile.minPlayerCount));
        SerializedProperty maxPlayerCountProperty = serializedObject.FindProperty(nameof(ActivityProfile.maxPlayerCount));
        SerializedProperty requireExactPlayerCountProperty = serializedObject.FindProperty(nameof(ActivityProfile.requireExactPlayerCount));
        SerializedProperty hasMinPlayerCountProperty = serializedObject.FindProperty(nameof(ActivityProfile.hasMinPlayerCount));
        SerializedProperty hasMaxPlayerCountProperty = serializedObject.FindProperty(nameof(ActivityProfile.hasMaxPlayerCount));
        SerializedProperty requireEvenPlayerCountProperty = serializedObject.FindProperty(nameof(ActivityProfile.requireEvenPlayerCount));
        SerializedProperty requireOddPlayerCountProperty = serializedObject.FindProperty(nameof(ActivityProfile.requireOddPlayerCount));
        SerializedProperty weightProperty = serializedObject.FindProperty(nameof(ActivityProfile.weight));
        SerializedProperty unavailableBeforeTimeProperty = serializedObject.FindProperty(nameof(ActivityProfile.unavailableBeforeTime));
        SerializedProperty hoursProperty = serializedObject.FindProperty(nameof(ActivityProfile.hours));
        SerializedProperty minutesProperty = serializedObject.FindProperty(nameof(ActivityProfile.minutes));


        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(descriptionProperty, new GUIContent("Description:"));
        EditorGUILayout.PropertyField(qrLinkProperty, new GUIContent("QR Link:"));
        EditorGUILayout.PropertyField(imageProperty, new GUIContent("Image:"));
        GUILayout.Space(16);

        EditorGUILayout.PropertyField(tagsProperty, new GUIContent("Tags:"));
        EditorGUILayout.PropertyField(weightProperty, new GUIContent("Weight:"));
        GUILayout.Space(16);

        EditorGUILayout.PropertyField(unavailableBeforeTimeProperty, new GUIContent("Unavailable before a certain time?"));
        if (unavailableBeforeTimeProperty.boolValue) {
            EditorGUI.indentLevel += 1;
            EditorGUILayout.PropertyField(hoursProperty, new GUIContent("Hours (0-23):"));
            EditorGUILayout.PropertyField(minutesProperty, new GUIContent("Minutes (0-59):"));
            EditorGUI.indentLevel -= 1;
        }

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

        bool issueDetected = false;
        if (hasMinPlayerCountProperty.boolValue && hasMaxPlayerCountProperty.boolValue) {
            if (minPlayerCountProperty.intValue >= maxPlayerCountProperty.intValue) {
                issueDetected = true;
            }
        }

        if (hasMinPlayerCountProperty.boolValue) {
            if (minPlayerCountProperty.intValue <= 0) {
                issueDetected = true;
            }
        }

        if (hasMaxPlayerCountProperty.boolValue) {
            if (maxPlayerCountProperty.intValue <= 0) {
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

        string playerCountSummary = "an activity for "+GameLogic.GetPlayerCountSummary(activity);

        GUILayout.Label(playerCountSummary, italicsLabel);
        GUILayout.EndHorizontal();

        GUI.color = Color.white;
        GUI.enabled = true;

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Test Print", GUILayout.Width(100))) {
            PrintManager.PrintActivityReceipt(activity);
        }
        GUILayout.EndHorizontal();
    }

}
