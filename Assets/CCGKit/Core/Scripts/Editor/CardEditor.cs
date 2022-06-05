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
        private ReorderableList cardDefinitionList;
        private ReorderableList cardAttributeList;
        private ReorderableList cardDefinitionEffectList;

        private CardDefinition currentCardDefinition;
        private Attribute currentCardAttribute;

        private EffectEditor cardDefinitionEffectEditor = new EffectEditor();

        private void InitCardEditor()
        {
            currentCardDefinition = null;
            currentCardAttribute = null;

            cardDefinitionEffectEditor.LoadEditorData(gameConfig);

            cardDefinitionList = EditorUtils.SetupReorderableList("Card types", gameConfig.CardDefinitions, ref currentCardDefinition, (rect, x) =>
            {
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight), x.Name);
            },
            (x) =>
            {
                currentCardDefinition = x;
                currentCardAttribute = null;
                CreateCardAttributeList(currentCardDefinition);
                cardDefinitionEffectEditor.Clear();
                cardDefinitionEffectEditor.SetCurrentCardDefinition(x);
                cardDefinitionEffectEditor.CreateEffectList(currentCardDefinition);
            },
            () =>
            {
                var cardDefinition = new CardDefinition();
                gameConfig.CardDefinitions.Add(cardDefinition);
            },
            (x) =>
            {
                currentCardDefinition = null;
                currentCardAttribute = null;
                cardDefinitionEffectEditor.Clear();
            });
        }

        private void CreateCardAttributeList(CardDefinition cardDefinition)
        {
            cardAttributeList = EditorUtils.SetupReorderableList("Card attributes", cardDefinition.Attributes, ref currentCardAttribute, (rect, x) =>
            {
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight), x.Name);
            },
            (x) =>
            {
                currentCardAttribute = x;
            },
            () =>
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Bool"), false, CreateCardDefinitionAttributeCallback, "Bool");
                menu.AddItem(new GUIContent("Integer"), false, CreateCardDefinitionAttributeCallback, "Integer");
                menu.AddItem(new GUIContent("String"), false, CreateCardDefinitionAttributeCallback, "String");
                menu.ShowAsContext();
            },
            (removedAttribute) =>
            {
                currentCardAttribute = null;
                // If an attribute is removed, update all the cards using that attribute as appropriate.
                foreach (var set in gameConfig.CardCollection)
                {
                    var cardsToUpdate = set.Cards.FindAll(x => x.Definition == currentCardDefinition.Name);
                    foreach (var card in cardsToUpdate)
                    {
                        var index = card.Attributes.FindIndex(x => x.Name == removedAttribute.Name);
                        if (index != -1)
                            card.Attributes.RemoveAt(index);
                    }
                }
            });
        }

        private void CreateCardDefinitionAttributeCallback(object obj)
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
            currentCardDefinition.Attributes.Add(attribute);

            // If an attribute is created, update all the cards as appropriate.
            foreach (var set in gameConfig.CardCollection)
            {
                var cardsToUpdate = set.Cards.FindAll(x => x.Definition == currentCardDefinition.Name);
                foreach (var card in cardsToUpdate)
                {
                    if (attribute is BoolAttribute)
                        card.SetBoolAttribute(attribute.Name, false);
                    else if (attribute is IntAttribute)
                        card.SetIntegerAttribute(attribute.Name, 0);
                    else if (attribute is StringAttribute)
                        card.SetStringAttribute(attribute.Name, string.Empty);
                }
            }
        }

        private void DrawCardEditor()
        {
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.MaxWidth(250));
            if (cardDefinitionList != null)
                cardDefinitionList.DoLayoutList();
            GUILayout.EndVertical();

            if (currentCardDefinition != null)
                DrawItem(currentCardDefinition);

            GUILayout.EndHorizontal();
        }

        private void DrawItem(CardDefinition cardDefinition)
        {
            GUILayout.BeginVertical();

            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = EditorSettings.RegularLabelWidth * 3;

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Name");
            cardDefinition.Name = EditorGUILayout.TextField(cardDefinition.Name, GUILayout.MaxWidth(EditorSettings.RegularTextFieldWidth));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Destroy after triggering effect");
            cardDefinition.DestroyAfterTriggeringEffect = EditorGUILayout.Toggle(cardDefinition.DestroyAfterTriggeringEffect, GUILayout.MaxWidth(150));
            GUILayout.EndHorizontal();

            EditorGUIUtility.labelWidth = oldLabelWidth;

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.MaxWidth(250));
            if (cardAttributeList != null)
                cardAttributeList.DoLayoutList();
            GUILayout.EndVertical();

            if (currentCardAttribute != null)
            {
                var oldAttributeName = currentCardAttribute.Name;
                DrawAttribute(currentCardAttribute, false);
                // If the attribute is renamed, update all the cards using that attribute as appropriate.
                if (currentCardAttribute.Name != oldAttributeName)
                {
                    foreach (var set in gameConfig.CardCollection)
                    {
                        var cardsToUpdate = set.Cards.FindAll(x => x.Definition == cardDefinition.Name);
                        foreach (var card in cardsToUpdate)
                        {
                            var attribute = card.Attributes.Find(x => x.Name == oldAttributeName);
                            if (attribute != null)
                                attribute.Name = currentCardAttribute.Name;
                        }
                    }
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.MaxWidth(250));
            cardDefinitionEffectEditor.DrawEffectList();
            GUILayout.EndVertical();

            cardDefinitionEffectEditor.DrawCurrentEffect();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }
    }
}
