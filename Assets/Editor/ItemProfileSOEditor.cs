// File: Assets/Editor/ItemProfileSOEditor.cs
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemProfileSO))]
public class ItemProfileSOEditor : Editor
{
    SerializedProperty useTypeProp;
    SerializedProperty healAmountProp;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Hiển thị toàn bộ các field mặc định
        DrawPropertiesExcluding(serializedObject, "m_Script", "healAmount");

        useTypeProp = serializedObject.FindProperty("useType");
        healAmountProp = serializedObject.FindProperty("healAmount");

        // Chỉ hiện healAmount nếu useType == Heal
        if ((ItemUseType)useTypeProp.enumValueIndex == ItemUseType.Heal)
        {
            EditorGUILayout.PropertyField(healAmountProp);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
