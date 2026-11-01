using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace StudyConstructor.Scenario.Editor
{
    [CustomPropertyDrawer(typeof(SelectTypeAttribute))]
    public class SelectTypeDrawer : PropertyDrawer
    {
        private const string NoneLabel = "— не выбрано —";

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            float labelWidth = EditorGUIUtility.labelWidth + 2f;
            var buttonRect = new Rect(position.x + labelWidth, position.y,
                position.width - labelWidth, EditorGUIUtility.singleLineHeight);
            var current = ParseType(property.managedReferenceFullTypename);
            if (EditorGUI.DropdownButton(buttonRect, new GUIContent(current == null ? NoneLabel : Label(current)), FocusType.Keyboard))
                ShowMenu(property, current);

            EditorGUI.PropertyField(position, property, label, true);
            EditorGUI.EndProperty();
        }

        private static void ShowMenu(SerializedProperty property, Type current)
        {
            var baseType = ParseType(property.managedReferenceFieldTypename);
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent(NoneLabel), current == null, () => Assign(property, null));

            var types = TypeCache.GetTypesDerivedFrom(baseType)
                .Where(t => !t.IsAbstract && !t.IsGenericType && t.IsSerializable
                            && !typeof(UnityEngine.Object).IsAssignableFrom(t)
                            && t.GetConstructor(Type.EmptyTypes) != null)
                .OrderBy(Label);
            foreach (var type in types)
                menu.AddItem(new GUIContent(Label(type)), type == current, () => Assign(property, type));

            menu.ShowAsContext();
        }

        private static void Assign(SerializedProperty property, Type type)
        {
            property.serializedObject.Update();
            property.managedReferenceValue = type == null ? null : Activator.CreateInstance(type);
            property.isExpanded = true;
            property.serializedObject.ApplyModifiedProperties();
        }

        private static string Label(Type type)
        {
            return type.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? ObjectNames.NicifyVariableName(type.Name);
        }

        private static Type ParseType(string typename)
        {
            if (string.IsNullOrEmpty(typename))
                return null;

            int space = typename.IndexOf(' ');
            return Type.GetType($"{typename.Substring(space + 1)}, {typename.Substring(0, space)}");
        }
    }
}
