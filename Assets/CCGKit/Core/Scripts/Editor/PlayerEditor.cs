// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CCGKit
{
    partial class CCGKitEditor
    {
        private ReorderableList playerDefinitionList;
        private Attribute currentPlayerAttribute;

        private bool boolValue;
        private int intValue;
        private string stringValue;

        private void InitPlayerEditor()
        {
            currentPlayerAttribute = null;

            playerDefinitionList = EditorUtils.SetupReorderableList("Player attributes", gameConfig.PlayerDefinition.Attributes, ref currentPlayerAttribute, (rect, x) =>
            {
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight), x.Name);
            },
            (x) =>
            {
                currentPlayerAttribute = x;
            },
            () =>
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Bool"), false, CreatePlayerAttributeCallback, "Bool");
                menu.AddItem(new GUIContent("Integer"), false, CreatePlayerAttributeCallback, "Integer");
                menu.AddItem(new GUIContent("String"), false, CreatePlayerAttributeCallback, "String");
                menu.ShowAsContext();
            },
            (x) =>
            {
                currentPlayerAttribute = null;
            });
        }

        private void CreatePlayerAttributeCallback(object obj)
        {
            Attribute attribute = null;
            switch ((string)obj)
            {
                case "Bool":
                    attribute = new BoolAttribute();
                    break;

                case "Integer":
                    attribute = new IntAttribute();
                    break;

                case "String":
                    attribute = new StringAttribute();
                    break;
            }
            gameConfig.PlayerDefinition.Attributes.Add(attribute);
        }

        private void DrawPlayerEditor()
        {
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.MaxWidth(250));
            if (playerDefinitionList != null)
                playerDefinitionList.DoLayoutList();
            GUILayout.EndVertical();

            if (currentPlayerAttribute != null)
                DrawAttribute(currentPlayerAttribute, true);

            GUILayout.EndHorizontal();
        }

        private void DrawAttribute(Attribute attribute, bool displayDefaultValue)
        {
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = EditorSettings.RegularLabelWidth;

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            if (attribute is BoolAttribute)
                GUILayout.Label("Bool", EditorStyles.boldLabel);
            else if (attribute is IntAttribute)
                GUILayout.Label("Integer", EditorStyles.boldLabel);
            else if (attribute is StringAttribute)
                GUILayout.Label("String", EditorStyles.boldLabel);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Name");
            attribute.Name = EditorGUILayout.TextField(attribute.Name, GUILayout.MaxWidth(EditorSettings.RegularTextFieldWidth));
            GUILayout.EndHorizontal();

            if (displayDefaultValue)
            {
                GUILayout.BeginHorizontal();
                DrawAttributeValue(attribute);
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();

            EditorGUIUtility.labelWidth = oldLabelWidth;
        }

        private void DrawAttributeValue(Attribute attribute)
        {
            EditorGUILayout.PrefixLabel("Value");
            if (attribute is BoolAttribute)
            {
                var boolAttribute = attribute as BoolAttribute;
                boolAttribute.Value = EditorGUILayout.Toggle(boolAttribute.Value, GUILayout.MaxWidth(50));
            }
            else if (attribute is IntAttribute)
            {
                var intAttribute = attribute as IntAttribute;
                intAttribute.Value = EditorGUILayout.IntField(intAttribute.Value, GUILayout.MaxWidth(50));
            }
            else if (attribute is StringAttribute)
            {
                var stringAttribute = attribute as StringAttribute;
                stringAttribute.Value = EditorGUILayout.TextField(stringAttribute.Value, GUILayout.MaxWidth(100));
            }
        }
    }
}
