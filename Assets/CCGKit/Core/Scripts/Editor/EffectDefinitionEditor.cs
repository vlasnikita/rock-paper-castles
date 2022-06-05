// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CCGKit
{
    partial class CCGKitEditor
    {
        private ReorderableList effectList;
        private EffectDefinition currentEffectDefinition;

        private List<string> playerAttributeNames = new List<string>();
        private int currentPlayerEffectAttribute;

        private List<string> cardDefinitionNames = new List<string>();
        private int currentEffectCard;
        private int currentCardEffectAttribute;

        private List<string> gameZoneNames = new List<string>();
        private int currentFromGameZone;
        private int currentToGameZone;

        private void InitEffectEditor()
        {
            currentEffectDefinition = null;

            foreach (var cardType in gameConfig.CardDefinitions)
                cardDefinitionNames.Add(cardType.Name);

            foreach (var attribute in gameConfig.PlayerDefinition.Attributes)
                playerAttributeNames.Add(attribute.Name);

            foreach (var zone in gameConfig.Zones)
                gameZoneNames.Add(zone.Name);

            effectList = EditorUtils.SetupReorderableList("Effect types", gameConfig.EffectDefinitions, ref currentEffectDefinition, (rect, x) =>
            {
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight), x.Name);
            },
            (x) =>
            {
                currentEffectDefinition = x;
                currentEffectCard = 0;
                currentPlayerEffectAttribute = 0;
                currentCardEffectAttribute = 0;
                currentFromGameZone = 0;
                currentToGameZone = 0;
            },
            () =>
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Permanent"), false, CreateEffectDefinitionCallback, "Permanent");
                menu.AddItem(new GUIContent("Target/Player"), false, CreateEffectDefinitionCallback, "TargetPlayer");
                menu.AddItem(new GUIContent("Target/Card"), false, CreateEffectDefinitionCallback, "TargetCard");
                menu.AddItem(new GUIContent("General"), false, CreateEffectDefinitionCallback, "General");
                menu.ShowAsContext();
            },
            (x) =>
            {
                currentEffectDefinition = null;
                currentEffectCard = 0;
                currentPlayerEffectAttribute = 0;
                currentCardEffectAttribute = 0;
                currentFromGameZone = 0;
                currentToGameZone = 0;
            });
        }

        private void CreateEffectDefinitionCallback(object obj)
        {
            EffectDefinition effectDefinition = null;
            switch ((string)obj)
            {
                case "Permanent":
                    effectDefinition = new PermanentEffectDefinition();
                    break;

                case "TargetPlayer":
                    effectDefinition = new PlayerEffectDefinition();
                    break;

                case "TargetCard":
                    effectDefinition = new CardEffectDefinition();
                    break;

                case "General":
                    effectDefinition = new GeneralEffectDefinition();
                    break;
            }
            if (effectDefinition != null)
                gameConfig.EffectDefinitions.Add(effectDefinition);
        }

        private void DrawEffectEditor()
        {
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.MaxWidth(250));
            if (effectList != null)
                effectList.DoLayoutList();
            GUILayout.EndVertical();

            if (currentEffectDefinition != null)
                DrawEffectDefinition(currentEffectDefinition);

            GUILayout.EndHorizontal();
        }

        private void DrawEffectDefinition(EffectDefinition effectDefinition)
        {
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 60;

            GUILayout.BeginVertical();
            switch (effectDefinition.Type)
            {
                case EffectType.Permanent:
                    DrawEffectDefinition(effectDefinition as PermanentEffectDefinition);
                    break;

                case EffectType.TargetPlayer:
                    DrawEffectDefinition(effectDefinition as PlayerEffectDefinition);
                    break;

                case EffectType.TargetCard:
                    DrawEffectDefinition(effectDefinition as CardEffectDefinition);
                    break;

                case EffectType.General:
                    DrawEffectDefinition(effectDefinition as GeneralEffectDefinition);
                    break;
            }
            GUILayout.EndVertical();

            EditorGUIUtility.labelWidth = oldLabelWidth;
        }

        private void DrawEffectDefinition(PermanentEffectDefinition effectDefinition)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Permanent", EditorStyles.boldLabel);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Name");
            effectDefinition.Name = EditorGUILayout.TextField(effectDefinition.Name, GUILayout.MaxWidth(EditorSettings.LargeTextFieldWidth));
            GUILayout.EndHorizontal();
        }

        private void DrawEffectDefinition(PlayerEffectDefinition effectDefinition)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Target player", EditorStyles.boldLabel);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Name");
            effectDefinition.Name = EditorGUILayout.TextField(effectDefinition.Name, GUILayout.MaxWidth(EditorSettings.LargeTextFieldWidth));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Action");
            effectDefinition.Action = (PlayerEffectActionType)EditorGUILayout.EnumPopup(effectDefinition.Action, GUILayout.MaxWidth(EditorSettings.RegularComboBoxWidth));
            GUILayout.EndHorizontal();

            if (effectDefinition.Action != PlayerEffectActionType.MoveCards)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Attribute");
                var effectAttribute = effectDefinition.Attribute;
                if (effectAttribute != null)
                    currentPlayerEffectAttribute = playerAttributeNames.IndexOf(effectAttribute);
                currentPlayerEffectAttribute = EditorGUILayout.Popup(currentPlayerEffectAttribute, playerAttributeNames.ToArray(), GUILayout.MaxWidth(EditorSettings.RegularComboBoxWidth));
                effectDefinition.Attribute = playerAttributeNames[currentPlayerEffectAttribute];
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("From");
                var fromZone = effectDefinition.Attribute;
                if (fromZone != null)
                    currentFromGameZone = gameZoneNames.IndexOf(fromZone);
                if (currentFromGameZone == -1)
                    currentFromGameZone = 0;
                currentFromGameZone = EditorGUILayout.Popup(currentFromGameZone, gameZoneNames.ToArray(), GUILayout.MaxWidth(EditorSettings.RegularComboBoxWidth));
                effectDefinition.Attribute = gameZoneNames[currentFromGameZone];
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("To");
                var toZone = effectDefinition.AttributeExtra;
                if (toZone != null)
                    currentToGameZone = gameZoneNames.IndexOf(toZone);
                if (currentToGameZone == -1)
                    currentToGameZone = 0;
                currentToGameZone = EditorGUILayout.Popup(currentToGameZone, gameZoneNames.ToArray(), GUILayout.MaxWidth(EditorSettings.RegularComboBoxWidth));
                effectDefinition.AttributeExtra = gameZoneNames[currentToGameZone];
                GUILayout.EndHorizontal();
            }
        }

        private void DrawEffectDefinition(CardEffectDefinition effectDefinition)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Target card", EditorStyles.boldLabel);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Name");
            effectDefinition.Name = EditorGUILayout.TextField(effectDefinition.Name, GUILayout.MaxWidth(EditorSettings.LargeTextFieldWidth));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Card");
            var effectCard = effectDefinition.Card;
            if (effectCard != null)
                currentEffectCard = cardDefinitionNames.IndexOf(effectCard);
            currentEffectCard = EditorGUILayout.Popup(currentEffectCard, cardDefinitionNames.ToArray(), GUILayout.MaxWidth(EditorSettings.RegularComboBoxWidth));
            effectDefinition.Card = cardDefinitionNames[currentEffectCard];
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Action");
            effectDefinition.Action = (CardEffectActionType)EditorGUILayout.EnumPopup(effectDefinition.Action, GUILayout.MaxWidth(EditorSettings.RegularComboBoxWidth));
            GUILayout.EndHorizontal();

            if (effectDefinition.Action != CardEffectActionType.Kill && effectDefinition.Action != CardEffectActionType.Transform)
            {
                var cardAttributes = new List<string>();
                var cardDefinition = gameConfig.CardDefinitions.Find(x => x.Name == effectDefinition.Card);
                foreach (var attribute in cardDefinition.Attributes)
                    cardAttributes.Add(attribute.Name);

                if (cardAttributes.Count > 0)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("Attribute");
                    var effectAttribute = effectDefinition.Attribute;
                    if (effectAttribute != null)
                        currentCardEffectAttribute = cardAttributes.IndexOf(effectAttribute);
                    if (currentCardEffectAttribute == -1)
                        currentCardEffectAttribute = 0;
                    currentCardEffectAttribute = EditorGUILayout.Popup(currentCardEffectAttribute, cardAttributes.ToArray(), GUILayout.MaxWidth(EditorSettings.RegularComboBoxWidth));
                    effectDefinition.Attribute = cardAttributes[currentCardEffectAttribute];
                    GUILayout.EndHorizontal();
                }
            }
        }

        private void DrawEffectDefinition(GeneralEffectDefinition effectDefinition)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("General", EditorStyles.boldLabel);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Name");
            effectDefinition.Name = EditorGUILayout.TextField(effectDefinition.Name, GUILayout.MaxWidth(EditorSettings.LargeTextFieldWidth));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Action");
            effectDefinition.Action = (GeneralEffectActionType)EditorGUILayout.EnumPopup(effectDefinition.Action, GUILayout.MaxWidth(EditorSettings.RegularComboBoxWidth));
            GUILayout.EndHorizontal();
        }
    }
}
