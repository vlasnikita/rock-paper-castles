// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CCGKit
{
    [Serializable]
    public struct ExportedCardCollection
    {
        public List<int> cards;
    }

    partial class CCGKitEditor
    {
        private ReorderableList cardCollectionList;
        private ReorderableList cardSetList;
        private ReorderableList cardSubtypeList;

        private CardSet currentCardSet;

        private Card currentCard;

        private int cardTypeIndex = 0;
        private List<string> cardTypeNames = new List<string>();

        private List<string> cardRarities = new List<string>();
        private int currentCardRarityIndex;

        private Vector2 cardCollectionScrollPos;

        private int startingCardId;

        private EffectEditor effectEditor = new EffectEditor();

        private void InitCardCollectionEditor()
        {
            foreach (var card in gameConfig.CardDefinitions)
                cardTypeNames.Add(card.Name);

            foreach (var rarity in gameConfig.CardRarities)
                cardRarities.Add(rarity.Name);

            effectEditor.LoadEditorData(gameConfig);

            currentCard = null;
            currentCardSet = null;

            cardCollectionList = EditorUtils.SetupReorderableList("Card sets", gameConfig.CardCollection, ref currentCardSet, (rect, x) =>
            {
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight), x.Name);
            },
            (x) =>
            {
                currentCard = null;
                currentCardSet = x;
                CreateCardSetList(currentCardSet);
                effectEditor.Clear();
            },
            () =>
            {
                var cardSet = new CardSet();
                gameConfig.CardCollection.Add(cardSet);
            },
            (x) =>
            {
                currentCard = null;
                currentCardSet = null;
                effectEditor.Clear();
            });
        }

        private void CreateCardSetList(CardSet cardSet)
        {
            cardSetList = EditorUtils.SetupReorderableList("Cards", cardSet.Cards, ref currentCard, (rect, x) =>
            {
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight), x.Name);
            },
            (x) =>
            {
                currentCard = x;
                CreateCardSubtypeList(currentCard);
                effectEditor.Clear();
                effectEditor.SetCurrentCard(x);
                effectEditor.CreateEffectList(currentCard);
            },
            () =>
            {
                var menu = new GenericMenu();
                foreach (var cardType in cardTypeNames)
                    menu.AddItem(new GUIContent(cardType), false, CreateCardCallback, cardType);
                menu.ShowAsContext();
            },
            (x) =>
            {
                currentCard = null;
                effectEditor.Clear();
            });
        }

        private void CreateCardCallback(object obj)
        {
            var card = new Card();

            // Generate a unique numeric identifier for this card.
            if (currentCardSet.Cards.Count > 0)
            {
                var higherIdCard = currentCardSet.Cards.Aggregate((x, y) => x.Id > y.Id ? x : y);
                if (higherIdCard != null)
                    card.Id = higherIdCard.Id + 1;
            }
            else
            {
                card.Id = 0;
            }

            card.Definition = (string)obj;

            var cardDefinition = gameConfig.CardDefinitions.Find(x => x.Name == (string)obj);
            if (cardDefinition != null)
            {
                foreach (var attribute in cardDefinition.Attributes)
                {
                    if (attribute is BoolAttribute)
                        card.SetBoolAttribute(attribute.Name, false);
                    else if (attribute is IntAttribute)
                        card.SetIntegerAttribute(attribute.Name, 0);
                    else if (attribute is StringAttribute)
                        card.SetStringAttribute(attribute.Name, string.Empty);
                }

                currentCardSet.Cards.Add(card);
            }
        }

        private void DrawCardCollectionEditor()
        {
            var oldLabelWidth = EditorGUIUtility.labelWidth;

            EditorGUIUtility.labelWidth = 80;

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.MaxWidth(150));
            if (cardCollectionList != null)
                cardCollectionList.DoLayoutList();
            if (GUILayout.Button("Regenerate ids", GUILayout.MaxWidth(150)))
                RegenerateIds();
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Starting id");
            startingCardId = EditorGUILayout.IntField(startingCardId, GUILayout.MaxWidth(30));
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            EditorGUIUtility.labelWidth = 60;

            if (currentCardSet != null)
            {
                GUILayout.BeginVertical(GUILayout.MaxWidth(250));
                GUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Name");
                currentCardSet.Name = GUILayout.TextField(currentCardSet.Name, GUILayout.MaxWidth(150));
                GUILayout.EndHorizontal();
                cardCollectionScrollPos = GUILayout.BeginScrollView(cardCollectionScrollPos, false, false, GUILayout.Width(250), GUILayout.Height(600));
                if (cardSetList != null)
                    cardSetList.DoLayoutList();
                GUILayout.EndScrollView();
                GUILayout.EndVertical();
            }

            EditorGUIUtility.labelWidth = oldLabelWidth;

            if (currentCard != null)
                DrawCard(currentCard);

            GUILayout.EndHorizontal();
        }

        private void RegenerateIds()
        {
            if (currentCardSet != null)
            {
                var id = startingCardId;
                foreach (var card in currentCardSet.Cards)
                {
                    card.Id = id;
                    id += 1;
                }
            }
        }

        private void DrawCard(Card card)
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Label(card.Definition, EditorStyles.boldLabel);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Max copies allowed");
            card.MaxCopies = EditorGUILayout.IntField(card.MaxCopies, GUILayout.MaxWidth(25));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Rarity");
            if (card.Rarity != null)
                currentCardRarityIndex = cardRarities.FindIndex(x => x == card.Rarity);
            if (currentCardRarityIndex == -1)
                currentCardRarityIndex = 0;
            currentCardRarityIndex = EditorGUILayout.Popup(currentCardRarityIndex, cardRarities.ToArray(), GUILayout.MaxWidth(EditorSettings.RegularComboBoxWidth));
            if (cardRarities.Count > 0)
                card.Rarity = cardRarities[currentCardRarityIndex];
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Name");
            card.Name = EditorGUILayout.TextField(card.Name, GUILayout.MaxWidth(150));
            GUILayout.EndHorizontal();

            foreach (var attribute in card.Attributes)
                DrawCardAttribute(attribute);

            DrawCardSubtypes(card);

            DrawCardEffects(card);

            GUILayout.EndVertical();
        }

        private void DrawCardTypeSelector()
        {
            GUILayout.BeginVertical();
            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            cardTypeIndex = EditorGUILayout.Popup(cardTypeIndex, cardTypeNames.ToArray(), GUILayout.MaxWidth(100));
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void DrawCardAttribute(Attribute attribute)
        {
            if (attribute is BoolAttribute)
                DrawBoolAttribute(attribute as BoolAttribute);
            else if (attribute is IntAttribute)
                DrawIntegerAttribute(attribute as IntAttribute);
            else if (attribute is StringAttribute)
                DrawStringAttribute(attribute as StringAttribute);
        }

        private void DrawBoolAttribute(BoolAttribute attribute)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(attribute.Name);
            attribute.Value = EditorGUILayout.Toggle(attribute.Value, GUILayout.MaxWidth(50));
            GUILayout.EndHorizontal();
        }

        private void DrawIntegerAttribute(IntAttribute attribute)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(attribute.Name);
            attribute.Value = EditorGUILayout.IntField(attribute.Value, GUILayout.MaxWidth(30));
            GUILayout.EndHorizontal();
        }

        private void DrawStringAttribute(StringAttribute attribute)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(attribute.Name);
            // If the attribute name contains the word 'text', display its contents via a text area for convenience.
            // This is particularly useful for the body or flavor text present in most CCG cards.
            if (attribute.Name.Contains("Text") || attribute.Name.Contains("text"))
            {
                EditorStyles.textField.wordWrap = true;
                attribute.Value = EditorGUILayout.TextArea(attribute.Value, GUILayout.MaxWidth(200), GUILayout.MaxHeight(100));
            }
            else
            {
                attribute.Value = EditorGUILayout.TextField(attribute.Value, GUILayout.MaxWidth(150));
            }
            GUILayout.EndHorizontal();
        }

        private void DrawCardSubtypes(Card card)
        {
            GUILayout.BeginVertical(GUILayout.MaxWidth(250));
            if (cardSubtypeList != null)
                cardSubtypeList.DoLayoutList();
            GUILayout.EndVertical();
        }

        private void DrawCardEffects(Card card)
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.MaxWidth(250));
            effectEditor.DrawEffectList();
            GUILayout.EndVertical();

            effectEditor.DrawCurrentEffect();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void CreateCardSubtypeList(Card card)
        {
            cardSubtypeList = new ReorderableList(card.Subtypes, typeof(string), true, true, true, true);

            cardSubtypeList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Subtypes");
            };

            cardSubtypeList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                card.Subtypes[index] = EditorGUI.TextField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight), card.Subtypes[index]);
            };

            cardSubtypeList.onAddCallback = (ReorderableList list) =>
            {
                card.Subtypes.Add("Subtype");
            };
        }
    }
}
