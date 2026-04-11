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
        if (FirstTime)
        {
            switch (serializedObject.FindProperty("state").intValue)
            {
                case 0:
                    category = DisplayCategory.Speed;
                    break;
                case 1:
                    category = DisplayCategory.Gamemode;
                    break;
                case 2:
                    category = DisplayCategory.Gravity;
                    break;
            }
        }
        else category = (DisplayCategory)EditorGUILayout.EnumPopup("Display", category);

        EditorGUILayout.Space();

        switch (category)
        {
            case DisplayCategory.Gamemode:
                DisplayProperty("gamemode", 1);
                break;
            case DisplayCategory.Speed:
                DisplayProperty("speed", 0);
                break;
            case DisplayCategory.Gravity:
                DisplayProperty("gravity", 2);
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
