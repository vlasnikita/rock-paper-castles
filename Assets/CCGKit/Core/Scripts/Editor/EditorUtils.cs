// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CCGKit
{
    public class EditorUtils
    {
        public static ReorderableList SetupReorderableList<T>(string headerText, List<T> elements, ref T currentElement, Action<Rect, T> drawElement, Action<T> selectElement, Action createElement, Action<T> removeElement)
        {
            var list = new ReorderableList(elements, typeof(T), true, true, true, true);

            list.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, headerText);
            };

            list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = elements[index];
                drawElement(rect, element);
            };

            list.onSelectCallback = (ReorderableList l) =>
            {
                var selectedElement = elements[list.index];
                selectElement(selectedElement);
            };

            if (createElement != null)
            {
                list.onAddDropdownCallback = (Rect buttonRect, ReorderableList l) =>
                {
                    createElement();
                };
            }

            list.onRemoveCallback = (ReorderableList l) =>
            {
                if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete this item?", "Yes", "No"))
                {
                    var element = elements[l.index];
                    removeElement(element);
                    ReorderableList.defaultBehaviours.DoRemoveButton(l);
                }
            };

            return list;
        }

        private static string[] intValueOptions = new string[] { "Constant value", "Random value" };

        public static void DrawIntValue(ref IntValue value)
        {
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 40;

            GUILayout.BeginVertical();
            var index = EditorGUILayout.Popup(value is ConstantIntValue ? 0 : 1, intValueOptions, GUILayout.MaxWidth(100));
            if (index == 0)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Value");
                if (value is ConstantIntValue)
                {
                    var newValue = EditorGUILayout.IntField((value as ConstantIntValue).Constant, GUILayout.MaxWidth(30));
                    (value as ConstantIntValue).Constant = newValue;
                }
                else
                {
                    var newValue = EditorGUILayout.IntField(0, GUILayout.MaxWidth(30));
                    value = new ConstantIntValue(newValue);
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginHorizontal();
                if (value is RandomIntValue)
                {
                    EditorGUILayout.PrefixLabel("Min");
                    var newMin = EditorGUILayout.IntField((value as RandomIntValue).Min, GUILayout.MaxWidth(30));
                    EditorGUILayout.PrefixLabel("Max");
                    var newMax = EditorGUILayout.IntField((value as RandomIntValue).Max, GUILayout.MaxWidth(30));
                    (value as RandomIntValue).Min = newMin;
                    (value as RandomIntValue).Max = newMax;
                }
                else
                {
                    EditorGUILayout.PrefixLabel("Min");
                    var newMin = EditorGUILayout.IntField(0, GUILayout.MaxWidth(30));
                    EditorGUILayout.PrefixLabel("Max");
                    var newMax = EditorGUILayout.IntField(0, GUILayout.MaxWidth(30));
                    value = new RandomIntValue(newMin, newMax);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            EditorGUIUtility.labelWidth = oldLabelWidth;
        }

        public static void DrawEffectTrigger(Effect effect, List<string> attributes = null)
        {
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 100;

            if (effect.Trigger is EffectPlayerTrigger)
            {
                GUILayout.BeginHorizontal();
                var playerTrigger = effect.Trigger as EffectPlayerTrigger;
                EditorGUILayout.PrefixLabel("Source");
                playerTrigger.Source = (EffectPlayerTriggerSource)EditorGUILayout.EnumPopup(playerTrigger.Source, GUILayout.MaxWidth(100));
                GUILayout.EndHorizontal();
            }

            switch (effect.Trigger.Type)
            {
                case EffectTriggerType.WhenPlayerAttributeIncreases:
                case EffectTriggerType.WhenPlayerAttributeDecreases:
                    {
                        var playerTrigger = effect.Trigger as EffectPlayerAttributeChangeTrigger;
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel("Attribute");
                        var attributeIndex = 0;
                        if (playerTrigger.Attribute != null)
                            attributeIndex = attributes.FindIndex(x => x == playerTrigger.Attribute);
                        if (attributeIndex == -1)
                            attributeIndex = 0;
                        attributeIndex = EditorGUILayout.Popup(attributeIndex, attributes.ToArray(), GUILayout.MaxWidth(200));
                        playerTrigger.Attribute = attributes[attributeIndex];
                        GUILayout.EndHorizontal();
                    }
                    break;

                case EffectTriggerType.WhenPlayerPlaysACard:
                    {
                        var trigger = effect.Trigger as PlayerPlayedCardTrigger;
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel("Card type");
                        var cardDefinitionIndex = 0;
                        if (trigger.CardDefinition != null)
                            cardDefinitionIndex = attributes.FindIndex(x => x == trigger.CardDefinition);
                        if (cardDefinitionIndex == -1)
                            cardDefinitionIndex = 0;
                        cardDefinitionIndex = EditorGUILayout.Popup(cardDefinitionIndex, attributes.ToArray(), GUILayout.MaxWidth(150));
                        trigger.CardDefinition = attributes[cardDefinitionIndex];
                        GUILayout.EndHorizontal();
                    }
                    break;

                case EffectTriggerType.WhenCardEntersZone:
                case EffectTriggerType.WhenCardLeavesZone:
                    {
                        var cardTrigger = effect.Trigger as EffectCardZoneTrigger;
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel("Zone");
                        var attributeIndex = 0;
                        if (cardTrigger.Zone != null)
                            attributeIndex = attributes.FindIndex(x => x == cardTrigger.Zone);
                        if (attributeIndex == -1)
                            attributeIndex = 0;
                        attributeIndex = EditorGUILayout.Popup(attributeIndex, attributes.ToArray(), GUILayout.MaxWidth(200));
                        cardTrigger.Zone = attributes[attributeIndex];
                        GUILayout.EndHorizontal();
                    }
                    break;

                case EffectTriggerType.WhenCardAttributeIncreases:
                case EffectTriggerType.WhenCardAttributeDecreases:
                    {
                        var cardTrigger = effect.Trigger as EffectCardAttributeChangeTrigger;
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel("Attribute");
                        var attributeIndex = 0;
                        if (cardTrigger.Attribute != null)
                            attributeIndex = attributes.FindIndex(x => x == cardTrigger.Attribute);
                        if (attributeIndex == -1)
                            attributeIndex = 0;
                        attributeIndex = EditorGUILayout.Popup(attributeIndex, attributes.ToArray(), GUILayout.MaxWidth(200));
                        cardTrigger.Attribute = attributes[attributeIndex];
                        GUILayout.EndHorizontal();
                    }
                    break;

                case EffectTriggerType.WhenCardAttributeIsLessThan:
                case EffectTriggerType.WhenCardAttributeIsLessThanOrEqualTo:
                case EffectTriggerType.WhenCardAttributeIsEqualTo:
                case EffectTriggerType.WhenCardAttributeIsGreaterThanOrEqualTo:
                case EffectTriggerType.WhenCardAttributeIsGreaterThan:
                    {
                        var cardTrigger = effect.Trigger as EffectCardAttributeComparisonTrigger;
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel("Attribute");
                        var attributeIndex = 0;
                        if (cardTrigger.Attribute != null)
                            attributeIndex = attributes.FindIndex(x => x == cardTrigger.Attribute);
                        if (attributeIndex == -1)
                            attributeIndex = 0;
                        attributeIndex = EditorGUILayout.Popup(attributeIndex, attributes.ToArray(), GUILayout.MaxWidth(200));
                        cardTrigger.Attribute = attributes[attributeIndex];
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel("Value");
                        cardTrigger.Value = EditorGUILayout.IntField(cardTrigger.Value, GUILayout.MaxWidth(30));
                        GUILayout.EndHorizontal();
                    }
                    break;

                case EffectTriggerType.AfterNumberOfTurns:
                case EffectTriggerType.EveryNumberOfTurns:
                    {
                        var trigger = effect.Trigger as EffectTurnTrigger;
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel("Turns");
                        trigger.NumTurns = EditorGUILayout.IntField(trigger.NumTurns, GUILayout.MaxWidth(30));
                        GUILayout.EndHorizontal();
                    }
                    break;
            }

            EditorGUIUtility.labelWidth = oldLabelWidth;
        }
    }
}
