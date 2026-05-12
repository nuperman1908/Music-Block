using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(PortalScript)), CanEditMultipleObjects]
public class PortalEditor : Editor
{
    public enum DisplayCategory
    {
        Gamemode,
        Speed,
        Gravity
    }
    public DisplayCategory category;
    bool FirstTime = true;
    public override void OnInspectorGUI()
    {
        SerializedProperty isSpeedPortalProp = serializedObject.FindProperty("isSpeedPortal");
        if (FirstTime)
        {
            switch (serializedObject.FindProperty("state").intValue)
            {
                case 0:
                    category = DisplayCategory.Speed;
                    if (isSpeedPortalProp != null)
                    {
                        isSpeedPortalProp.boolValue = true;
                    }
                    break;
                case 1:
                    category = DisplayCategory.Gamemode;
                    if (isSpeedPortalProp != null)
                    {
                        isSpeedPortalProp.boolValue = false;
                    }
                    break;
                case 2:
                    category = DisplayCategory.Gravity;
                    if (isSpeedPortalProp != null)
                    {
                        isSpeedPortalProp.boolValue = false;
                    }
                    break;
            }
        }
        else category = (DisplayCategory)EditorGUILayout.EnumPopup("Display", category);

        EditorGUILayout.PropertyField(isSpeedPortalProp);

        EditorGUILayout.Space();

        switch (category)
        {
            case DisplayCategory.Gamemode:
                DisplayProperty("gamemodes", 1);
                if (isSpeedPortalProp != null)
                {
                    isSpeedPortalProp.boolValue = false;
                }
                break;
            case DisplayCategory.Speed:
                DisplayProperty("speed", 0);
                if (isSpeedPortalProp != null)
                {
                    isSpeedPortalProp.boolValue = true;
                }
                break;
            case DisplayCategory.Gravity:
                DisplayProperty("gravity", 2);
                if (isSpeedPortalProp != null)
                {
                    isSpeedPortalProp.boolValue = false;
                }
                break;
        }

        FirstTime = false;
        serializedObject.ApplyModifiedProperties();
    }
    void DisplayProperty(string property, int PropNumb)
    {
        try
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty(property));
        }
        catch
        {
            Debug.LogError("Property " + property + " not found in PortalScript. Make sure it is spelled correctly and is public.");
        }
        serializedObject.FindProperty("state").intValue = PropNumb;
    }
}
