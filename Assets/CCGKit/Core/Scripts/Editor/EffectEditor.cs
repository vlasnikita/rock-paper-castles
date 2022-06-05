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
    public class EffectEditor
    {
        private GameConfiguration gameConfig;

        private ReorderableList effectList;
        private ReorderableList effectTriggerConditionList;
        private ReorderableList effectTargetConditionList;

        private Effect currentEffect;
        private EffectCondition currentEffectTriggerCondition;
        private EffectCondition currentEffectTargetCondition;

        private int currentGeneralEffectCard;
        private int currentPlayerEffectTriggerConditionAttribute;
        private int currentCardEffectTriggerConditionAttribute;
        private int currentPlayerEffectTargetConditionAttribute;
        private int currentCardEffectTargetConditionAttribute;

        private List<string> cardDefinitionNames = new List<string>();
        private List<string> effectNames = new List<string>();
        private List<string> cardNames = new List<string>();
        private List<string> playerAttributeNames = new List<string>();
        private List<string> gameZoneNames = new List<string>();

        private List<string> effectTriggerConditionAttributeOptions = new List<string>();
        private List<string> effectTargetConditionAttributeOptions = new List<string>();

        private Card currentCard;
        private CardDefinition currentCardDefinition;

        public void LoadEditorData(GameConfiguration gameConfig)
        {
            this.gameConfig = gameConfig;

            cardDefinitionNames.Clear();
            foreach (var definition in gameConfig.CardDefinitions)
                cardDefinitionNames.Add(definition.Name);

            effectNames.Clear();
            foreach (var effect in gameConfig.EffectDefinitions)
                effectNames.Add(effect.Name);

            cardNames.Clear();
            foreach (var set in gameConfig.CardCollection)
                foreach (var card in set.Cards)
                    cardNames.Add(card.Name);

            playerAttributeNames.Clear();
            foreach (var attribute in gameConfig.PlayerDefinition.Attributes)
                playerAttributeNames.Add(attribute.Name);

            gameZoneNames.Clear();
            foreach (var zone in gameConfig.Zones)
                gameZoneNames.Add(zone.Name);
        }

        public void SetCurrentCard(Card card)
        {
            currentCard = card;
        }

        public void SetCurrentCardDefinition(CardDefinition cardDefinition)
        {
            currentCardDefinition = cardDefinition;
        }

        public void Clear()
        {
            currentCard = null;
            currentCardDefinition = null;
            currentEffect = null;
        }

        public void CreateEffectList(Card card)
        {
            effectList = EditorUtils.SetupReorderableList("Effects", card.Effects, ref currentEffect, (rect, x) =>
            {
                var effectDefinitionName = x.Definition;
                if (effectDefinitionName != null)
                {
                    var effectDefinition = gameConfig.EffectDefinitions.Find(attr => attr.Name == effectDefinitionName);
                    if (effectDefinition != null)
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight), effectDefinition.Name);
                }
            },
            (x) =>
            {
                currentEffect = x;
                if (currentEffect is TargetableEffect)
                    CreateEffectTriggerConditionList(currentEffect as TargetableEffect);
                if (currentEffect is TargetableEffect)
                    CreateEffectTargetConditionList(currentEffect as TargetableEffect);
            },
            () =>
            {
                foreach (var effect in gameConfig.EffectDefinitions)
                    effectNames.Add(effect.Name);

                var menu = new GenericMenu();
                foreach (var cardEffect in effectNames)
                    menu.AddItem(new GUIContent(cardEffect), false, CreateEffectCallback, cardEffect);
                menu.ShowAsContext();
            },
            (x) =>
            {
                currentEffect = null;
            });
        }

        public void CreateEffectList(CardDefinition cardDefinition)
        {
            effectList = EditorUtils.SetupReorderableList("Effects", cardDefinition.Effects, ref currentEffect, (rect, x) =>
            {
                var effectDefinitionName = x.Definition;
                if (effectDefinitionName != null)
                {
                    var effectDefinition = gameConfig.EffectDefinitions.Find(attr => attr.Name == effectDefinitionName);
                    if (effectDefinition != null)
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight), effectDefinition.Name);
                }
            },
            (x) =>
            {
                currentEffect = x;
                CreateEffectTriggerConditionList(currentEffect as TargetableEffect);
                if (currentEffect is TargetableEffect)
                    CreateEffectTargetConditionList(currentEffect as TargetableEffect);
            },
            () =>
            {
                foreach (var effect in gameConfig.EffectDefinitions)
                    effectNames.Add(effect.Name);

                var menu = new GenericMenu();
                foreach (var cardEffect in effectNames)
                    menu.AddItem(new GUIContent(cardEffect), false, CreateEffectCallback, cardEffect);
                menu.ShowAsContext();
            },
            (x) =>
            {
                currentEffect = null;
                currentGeneralEffectCard = 0;
                currentPlayerEffectTriggerConditionAttribute = 0;
                currentCardEffectTriggerConditionAttribute = 0;
                currentPlayerEffectTargetConditionAttribute = 0;
                currentCardEffectTargetConditionAttribute = 0;
            });
        }

        public void DrawEffectList()
        {
            if (effectList != null)
                effectList.DoLayoutList();
        }

        private void CreateEffectCallback(object obj)
        {
            var effectDefinition = gameConfig.EffectDefinitions.Find(x => x.Name == (string)obj);
            if (effectDefinition != null)
            {
                Effect effect = null;
                switch (effectDefinition.Type)
                {
                    case EffectType.Permanent:
                        effect = new PermanentEffect();
                        break;

                    case EffectType.TargetPlayer:
                        effect = new PlayerEffect();
                        break;

                    case EffectType.TargetCard:
                        var cardEffectDefinition = effectDefinition as CardEffectDefinition;
                        if (cardEffectDefinition != null)
                        {
                            switch (cardEffectDefinition.Action)
                            {
                                case CardEffectActionType.Transform:
                                    effect = new TransformEffect();
                                    break;

                                default:
                                    effect = new CardEffect();
                                    break;
                            }
                        }
                        break;

                    case EffectType.General:
                        effect = new GeneralEffect();
                        break;
                }
                effect.Definition = effectDefinition.Name;
                effect.Trigger = CreateEffectTrigger(0);
                if (currentCard != null)
                    currentCard.Effects.Add(effect);
                else if (currentCardDefinition != null)
                    currentCardDefinition.Effects.Add(effect);
            }
        }

        public void DrawCurrentEffect()
        {
            if (currentEffect != null)
                DrawEffect(currentEffect);
        }

        private void DrawEffect(Effect effect)
        {
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 100;

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Label(effect.Definition, EditorStyles.boldLabel);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Unavoidable");
            effect.Unavoidable = EditorGUILayout.Toggle(effect.Unavoidable);
            GUILayout.EndHorizontal();

            var effectDefinition = gameConfig.EffectDefinitions.Find(x => x.Name == effect.Definition);
            if (effectDefinition.Type != EffectType.Permanent)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Trigger");
                var triggerType = (EffectTriggerType)EditorGUILayout.EnumPopup((effect.Trigger != null) ? effect.Trigger.Type : 0, GUILayout.MaxWidth(270));
                if (effect.Trigger == null || effect.Trigger.Type != triggerType)
                {
                    effect.Trigger = CreateEffectTrigger(triggerType);
                }
                GUILayout.EndHorizontal();

                if (effect.Trigger is PlayerPlayedCardTrigger)
                {
                    EditorUtils.DrawEffectTrigger(effect, cardDefinitionNames);
                }
                else if (effect.Trigger is EffectPlayerTrigger)
                {
                    EditorUtils.DrawEffectTrigger(effect, playerAttributeNames);
                }
                else if (effect.Trigger is EffectCardZoneTrigger)
                {
                    EditorUtils.DrawEffectTrigger(effect, gameZoneNames);
                }
                else if (effect.Trigger is EffectCardTrigger)
                {
                    var cardAttributes = new List<string>();
                    if (currentCard != null)
                    {
                        foreach (var attribute in currentCard.Attributes)
                            cardAttributes.Add(attribute.Name);
                    }
                    else if (currentCardDefinition != null)
                    {
                        foreach (var attribute in currentCardDefinition.Attributes)
                            cardAttributes.Add(attribute.Name);
                    }
                    EditorUtils.DrawEffectTrigger(effect, cardAttributes);
                }
                else
                {
                    EditorUtils.DrawEffectTrigger(effect);
                }

                DrawEffectTriggerConditions();

                if (effect.Trigger.Type == EffectTriggerType.AfterNumberOfTurns || effect.Trigger.Type == EffectTriggerType.EveryNumberOfTurns)
                    EditorUtils.DrawEffectTrigger(effect);

                if (effectDefinition.Type == EffectType.TargetPlayer)
                {
                    var playerEffect = effect as PlayerEffect;

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("Target");
                    playerEffect.Target = (PlayerEffectTargetType)EditorGUILayout.EnumPopup(playerEffect.Target, GUILayout.MaxWidth(200));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("Value");
                    EditorUtils.DrawIntValue(ref playerEffect.Value);
                    GUILayout.EndHorizontal();
                }
                else if (effectDefinition.Type == EffectType.TargetCard)
                {
                    var cardEffect = effect as CardEffect;

                    var cardEffectDefinition = effectDefinition as CardEffectDefinition;
                    if (cardEffectDefinition != null)
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel("Target");
                        cardEffect.Target = (CardEffectTargetType)EditorGUILayout.EnumPopup(cardEffect.Target, GUILayout.MaxWidth(200));
                        GUILayout.EndHorizontal();

                        if (cardEffectDefinition.Action == CardEffectActionType.Transform)
                        {
                            var transformEffect = cardEffect as TransformEffect;
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.PrefixLabel("Card");
                            if (transformEffect.Card != null)
                                currentGeneralEffectCard = cardNames.FindIndex(x => x == transformEffect.Card);
                            if (currentGeneralEffectCard == -1)
                                currentGeneralEffectCard = 0;
                            currentGeneralEffectCard = EditorGUILayout.Popup(currentGeneralEffectCard, cardNames.ToArray(), GUILayout.MaxWidth(200));
                            transformEffect.Card = cardNames[currentGeneralEffectCard];
                            GUILayout.EndHorizontal();
                        }
                        else if (cardEffectDefinition.Action != CardEffectActionType.Kill)
                        {
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.PrefixLabel("Value");
                            EditorUtils.DrawIntValue(ref cardEffect.Value);
                            GUILayout.EndHorizontal();
                        }
                    }
                }
                else if (effectDefinition.Type == EffectType.General)
                {
                    var generalEffect = effect as GeneralEffect;

                    var generalEffectDefinition = effectDefinition as GeneralEffectDefinition;
                    if (generalEffectDefinition.Action == GeneralEffectActionType.CreateToken)
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel("Card");
                        if (generalEffect.Card != null)
                            currentGeneralEffectCard = cardNames.FindIndex(x => x == generalEffect.Card);
                        if (currentGeneralEffectCard == -1)
                            currentGeneralEffectCard = 0;
                        currentGeneralEffectCard = EditorGUILayout.Popup(currentGeneralEffectCard, cardNames.ToArray(), GUILayout.MaxWidth(200));
                        generalEffect.Card = cardNames[currentGeneralEffectCard];
                        GUILayout.EndHorizontal();
                    }
                }

                DrawEffectTargetConditions();
            }

            GUILayout.EndVertical();

            EditorGUIUtility.labelWidth = oldLabelWidth;
        }

        private EffectTrigger CreateEffectTrigger(EffectTriggerType type)
        {
            EffectTrigger trigger = null;
            switch (type)
            {
                case EffectTriggerType.WhenPlayerTurnStarts:
                    trigger = new PlayerTurnStartedTrigger();
                    break;

                case EffectTriggerType.WhenPlayerTurnEnds:
                    trigger = new PlayerTurnEndedTrigger();
                    break;

                case EffectTriggerType.WhenPlayerAttributeIncreases:
                    trigger = new PlayerAttributeIncreasedTrigger();
                    break;

                case EffectTriggerType.WhenPlayerAttributeDecreases:
                    trigger = new PlayerAttributeDecreasedTrigger();
                    break;

                case EffectTriggerType.WhenPlayerPlaysACard:
                    trigger = new PlayerPlayedCardTrigger();
                    break;

                case EffectTriggerType.WhenCardEntersZone:
                    trigger = new CardEnteredZoneTrigger();
                    break;

                case EffectTriggerType.WhenCardLeavesZone:
                    trigger = new CardLeftZoneTrigger();
                    break;

                case EffectTriggerType.WhenCardAttributeIncreases:
                    trigger = new CardAttributeIncreasedTrigger();
                    break;

                case EffectTriggerType.WhenCardAttributeDecreases:
                    trigger = new CardAttributeDecreasedTrigger();
                    break;

                case EffectTriggerType.WhenCardAttributeIsLessThan:
                    trigger = new CardAttributeLessThanTrigger();
                    break;

                case EffectTriggerType.WhenCardAttributeIsLessThanOrEqualTo:
                    trigger = new CardAttributeLessThanOrEqualToTrigger();
                    break;

                case EffectTriggerType.WhenCardAttributeIsEqualTo:
                    trigger = new CardAttributeEqualToTrigger();
                    break;

                case EffectTriggerType.WhenCardAttributeIsGreaterThanOrEqualTo:
                    trigger = new CardAttributeGreaterThanOrEqualToTrigger();
                    break;

                case EffectTriggerType.WhenCardAttributeIsGreaterThan:
                    trigger = new CardAttributeGreaterThanTrigger();
                    break;

                case EffectTriggerType.WhenCardAttacks:
                    trigger = new CardAttackedTrigger();
                    break;

                case EffectTriggerType.AfterNumberOfTurns:
                    trigger = new AfterNumberOfTurnsTrigger();
                    break;

                case EffectTriggerType.EveryNumberOfTurns:
                    trigger = new EveryNumberOfTurnsTrigger();
                    break;

                default:
                    break;
            }
            return trigger;
        }

        private void DrawEffectTriggerConditions()
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.MaxWidth(380));
            if (effectTriggerConditionList != null)
                effectTriggerConditionList.DoLayoutList();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void DrawEffectTargetConditions()
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.MaxWidth(380));
            if (effectTargetConditionList != null)
                effectTargetConditionList.DoLayoutList();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void CreateEffectTriggerConditionList(TargetableEffect effect)
        {
            effectTriggerConditionList = EditorUtils.SetupReorderableList("Trigger conditions", effect.TriggerConditions, ref currentEffectTriggerCondition, (rect, x) =>
            {
                EditorGUI.LabelField(rect, "Conditions");
            },
            (x) =>
            {
                currentEffectTriggerCondition = x;
            },
            () =>
            {
                var menu = new GenericMenu();
                if (IsPlayerTrigger(currentEffect.Trigger))
                {
                    foreach (var option in Enum.GetValues(typeof(PlayerEffectConditionType)))
                        menu.AddItem(new GUIContent(GetEditorFriendlyName((PlayerEffectConditionType)option)), false, CreatePlayerEffectTriggerConditionCallback, option);
                }
                else if (IsCardTrigger(currentEffect.Trigger))
                {
                    foreach (var option in Enum.GetValues(typeof(CardEffectConditionType)))
                        menu.AddItem(new GUIContent(GetEditorFriendlyName((CardEffectConditionType)option)), false, CreateCardEffectTriggerConditionCallback, option);
                }
                if (currentEffect.Trigger is PlayerPlayedCardTrigger)
                {
                    foreach (var option in Enum.GetValues(typeof(CardEffectConditionType)))
                        menu.AddItem(new GUIContent(GetEditorFriendlyName((CardEffectConditionType)option)), false, CreateCardEffectTriggerConditionCallback, option);
                }
                menu.ShowAsContext();

                currentPlayerEffectTriggerConditionAttribute = 0;
                currentCardEffectTriggerConditionAttribute = 0;
            },
            (x) =>
            {
                currentPlayerEffectTriggerConditionAttribute = 0;
                currentCardEffectTriggerConditionAttribute = 0;
            });

            effectTriggerConditionList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                DrawEffectTriggerCondition(rect, index);
            };
        }

        private void CreatePlayerEffectTriggerConditionCallback(object obj)
        {
            var effectCondition = new PlayerEffectCondition();
            effectCondition.Type = (PlayerEffectConditionType)obj;
            currentEffect.TriggerConditions.Add(effectCondition);
        }

        private void CreateCardEffectTriggerConditionCallback(object obj)
        {
            var effectCondition = new CardEffectCondition();
            effectCondition.Type = (CardEffectConditionType)obj;
            currentEffect.TriggerConditions.Add(effectCondition);
        }

        private void DrawEffectTriggerCondition(Rect rect, int index)
        {
            var currentEffectCondition = currentEffect.TriggerConditions[index];

            if (currentEffectCondition is PlayerEffectCondition)
            {
                if (currentEffectCondition.Attribute != null)
                    currentPlayerEffectTriggerConditionAttribute = playerAttributeNames.FindIndex(x => x == currentEffectCondition.Attribute);
                if (currentPlayerEffectTriggerConditionAttribute == -1)
                    currentPlayerEffectTriggerConditionAttribute = 0;
                currentPlayerEffectTriggerConditionAttribute = EditorGUI.Popup(new Rect(rect.x, rect.y, 100, EditorGUIUtility.singleLineHeight), currentPlayerEffectTriggerConditionAttribute, playerAttributeNames.ToArray());
                currentEffectCondition.Attribute = playerAttributeNames[currentPlayerEffectTriggerConditionAttribute];

                if (currentEffectCondition is PlayerEffectCondition)
                    EditorGUI.LabelField(new Rect(rect.x + 100, rect.y, 100, EditorGUIUtility.singleLineHeight), GetEditorFriendlyName((currentEffectCondition as PlayerEffectCondition).Type));
                else
                    EditorGUI.LabelField(new Rect(rect.x + 100, rect.y, 100, EditorGUIUtility.singleLineHeight), GetEditorFriendlyName((currentEffectCondition as CardEffectCondition).Type));

                currentEffectCondition.Value = EditorGUI.IntField(new Rect(rect.x + 180, rect.y, 30, EditorGUIUtility.singleLineHeight), currentEffectCondition.Value);
            }
            else if (currentEffectCondition is CardEffectCondition)
            {
                effectTriggerConditionAttributeOptions.Clear();
                if (currentEffect.Trigger is PlayerPlayedCardTrigger)
                {
                    var trigger = currentEffect.Trigger as PlayerPlayedCardTrigger;
                    var cardDefinition = gameConfig.CardDefinitions.Find(x => x.Name == trigger.CardDefinition);
                    foreach (var attribute in cardDefinition.Attributes)
                        effectTriggerConditionAttributeOptions.Add(attribute.Name);
                }
                else
                {
                    if (currentCard != null)
                    {
                        foreach (var attribute in currentCard.Attributes)
                            effectTriggerConditionAttributeOptions.Add(attribute.Name);
                    }
                    else if (currentCardDefinition != null)
                    {
                        foreach (var attribute in currentCardDefinition.Attributes)
                            effectTriggerConditionAttributeOptions.Add(attribute.Name);
                    }
                }

                var cardEffectCondition = currentEffectCondition as CardEffectCondition;
                if (cardEffectCondition.Type == CardEffectConditionType.WithPermanentEffect || cardEffectCondition.Type == CardEffectConditionType.WithoutPermanentEffect)
                {
                    var permanentEffects = new List<string>();
                    foreach (var definition in gameConfig.EffectDefinitions)
                        if (definition.Type == EffectType.Permanent)
                            permanentEffects.Add(definition.Name);

                    EditorGUI.LabelField(new Rect(rect.x, rect.y, 180, EditorGUIUtility.singleLineHeight), GetEditorFriendlyName(cardEffectCondition.Type));

                    if (cardEffectCondition.Attribute != null)
                        currentCardEffectTriggerConditionAttribute = permanentEffects.FindIndex(x => x == cardEffectCondition.Attribute);
                    if (currentCardEffectTriggerConditionAttribute == -1)
                        currentCardEffectTriggerConditionAttribute = 0;
                    currentCardEffectTriggerConditionAttribute = EditorGUI.Popup(new Rect(rect.x + 180, rect.y, 100, EditorGUIUtility.singleLineHeight), currentCardEffectTriggerConditionAttribute, permanentEffects.ToArray());
                    cardEffectCondition.Attribute = permanentEffects[currentCardEffectTriggerConditionAttribute];
                }
                else if (cardEffectCondition.Type == CardEffectConditionType.WithSubtype || cardEffectCondition.Type == CardEffectConditionType.WithoutSubtype)
                {
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, 180, EditorGUIUtility.singleLineHeight), GetEditorFriendlyName(cardEffectCondition.Type));

                    var currentCardEffectConditionSubtype = cardEffectCondition.Attribute;
                    cardEffectCondition.Attribute = EditorGUI.TextField(new Rect(rect.x + 180, rect.y, 100, EditorGUIUtility.singleLineHeight), currentCardEffectConditionSubtype);
                }
                else
                {
                    if (cardEffectCondition.Attribute != null)
                        currentCardEffectTriggerConditionAttribute = effectTriggerConditionAttributeOptions.FindIndex(x => x == cardEffectCondition.Attribute);
                    if (currentCardEffectTriggerConditionAttribute == -1)
                        currentCardEffectTriggerConditionAttribute = 0;
                    currentCardEffectTriggerConditionAttribute = EditorGUI.Popup(new Rect(rect.x, rect.y, 100, EditorGUIUtility.singleLineHeight), currentCardEffectTriggerConditionAttribute, effectTriggerConditionAttributeOptions.ToArray());
                    cardEffectCondition.Attribute = effectTriggerConditionAttributeOptions[currentCardEffectTriggerConditionAttribute];

                    EditorGUI.LabelField(new Rect(rect.x + 100, rect.y, 100, EditorGUIUtility.singleLineHeight), GetEditorFriendlyName(cardEffectCondition.Type));

                    cardEffectCondition.Value = EditorGUI.IntField(new Rect(rect.x + 180, rect.y, 30, EditorGUIUtility.singleLineHeight), cardEffectCondition.Value);
                }
            }
        }

        private bool IsPlayerTrigger(EffectTrigger trigger)
        {
            return trigger is EffectPlayerTrigger;
        }

        private bool IsCardTrigger(EffectTrigger trigger)
        {
            return trigger is EffectCardTrigger;
        }

        private void CreateEffectTargetConditionList(TargetableEffect effect)
        {
            effectTargetConditionList = EditorUtils.SetupReorderableList("Target conditions", effect.TargetConditions, ref currentEffectTargetCondition, (rect, x) =>
            {
                EditorGUI.LabelField(rect, "Conditions");
            },
            (x) =>
            {
                currentEffectTargetCondition = x;
            },
            () =>
            {
                var menu = new GenericMenu();
                if (currentEffect.Type == EffectType.TargetPlayer)
                {
                    foreach (var option in Enum.GetValues(typeof(PlayerEffectConditionType)))
                        menu.AddItem(new GUIContent(GetEditorFriendlyName((PlayerEffectConditionType)option)), false, CreatePlayerEffectTargetConditionCallback, option);
                }
                else if (currentEffect.Type == EffectType.TargetCard)
                {
                    foreach (var option in Enum.GetValues(typeof(CardEffectConditionType)))
                        menu.AddItem(new GUIContent(GetEditorFriendlyName((CardEffectConditionType)option)), false, CreateCardEffectTargetConditionCallback, option);
                }
                menu.ShowAsContext();

                currentPlayerEffectTargetConditionAttribute = 0;
                currentCardEffectTargetConditionAttribute = 0;
            },
            (x) =>
            {
                currentPlayerEffectTargetConditionAttribute = 0;
                currentCardEffectTargetConditionAttribute = 0;
            });

            effectTargetConditionList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                DrawEffectTargetCondition(rect, index);
            };
        }

        private void CreatePlayerEffectTargetConditionCallback(object obj)
        {
            var effectCondition = new PlayerEffectCondition();
            effectCondition.Type = (PlayerEffectConditionType)obj;
            currentEffect.TargetConditions.Add(effectCondition);
        }

        private void CreateCardEffectTargetConditionCallback(object obj)
        {
            var effectCondition = new CardEffectCondition();
            effectCondition.Type = (CardEffectConditionType)obj;
            currentEffect.TargetConditions.Add(effectCondition);
        }

        private void DrawEffectTargetCondition(Rect rect, int index)
        {
            var currentEffectCondition = currentEffect.TargetConditions[index];

            if (currentEffect.Type == EffectType.TargetPlayer)
            {
                var playerEffectCondition = currentEffectCondition as PlayerEffectCondition;

                if (playerEffectCondition.Attribute != null)
                    currentPlayerEffectTargetConditionAttribute = playerAttributeNames.FindIndex(x => x == playerEffectCondition.Attribute);
                if (currentPlayerEffectTargetConditionAttribute == -1)
                    currentPlayerEffectTargetConditionAttribute = 0;
                currentPlayerEffectTargetConditionAttribute = EditorGUI.Popup(new Rect(rect.x, rect.y, 100, EditorGUIUtility.singleLineHeight), currentPlayerEffectTargetConditionAttribute, playerAttributeNames.ToArray());
                playerEffectCondition.Attribute = playerAttributeNames[currentPlayerEffectTargetConditionAttribute];

                EditorGUI.LabelField(new Rect(rect.x + 100, rect.y, 100, EditorGUIUtility.singleLineHeight), GetEditorFriendlyName(playerEffectCondition.Type));

                playerEffectCondition.Value = EditorGUI.IntField(new Rect(rect.x + 180, rect.y, 30, EditorGUIUtility.singleLineHeight), playerEffectCondition.Value);
            }
            else if (currentEffect.Type == EffectType.TargetCard)
            {
                effectTargetConditionAttributeOptions.Clear();
                var effectDefinition = gameConfig.EffectDefinitions.Find(x => x.Name == currentEffect.Definition);
                if (effectDefinition != null)
                {
                    var targetCardType = (effectDefinition as CardEffectDefinition).Card;
                    var cardDefinition = gameConfig.CardDefinitions.Find(x => x.Name == targetCardType);
                    if (cardDefinition != null)
                    {
                        foreach (var attribute in cardDefinition.Attributes)
                            effectTargetConditionAttributeOptions.Add(attribute.Name);
                    }
                }

                var cardEffectCondition = currentEffectCondition as CardEffectCondition;
                if (cardEffectCondition.Type == CardEffectConditionType.WithPermanentEffect || cardEffectCondition.Type == CardEffectConditionType.WithoutPermanentEffect)
                {
                    var permanentEffects = new List<string>();
                    foreach (var definition in gameConfig.EffectDefinitions)
                        if (definition.Type == EffectType.Permanent)
                            permanentEffects.Add(definition.Name);

                    EditorGUI.LabelField(new Rect(rect.x, rect.y, 180, EditorGUIUtility.singleLineHeight), GetEditorFriendlyName(cardEffectCondition.Type));

                    if (cardEffectCondition.Attribute != null)
                        currentCardEffectTargetConditionAttribute = permanentEffects.FindIndex(x => x == cardEffectCondition.Attribute);
                    if (currentCardEffectTargetConditionAttribute == -1)
                        currentCardEffectTargetConditionAttribute = 0;
                    currentCardEffectTargetConditionAttribute = EditorGUI.Popup(new Rect(rect.x + 180, rect.y, 100, EditorGUIUtility.singleLineHeight), currentCardEffectTargetConditionAttribute, permanentEffects.ToArray());
                    cardEffectCondition.Attribute = permanentEffects[currentCardEffectTargetConditionAttribute];
                }
                else if (cardEffectCondition.Type == CardEffectConditionType.WithSubtype || cardEffectCondition.Type == CardEffectConditionType.WithoutSubtype)
                {
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, 180, EditorGUIUtility.singleLineHeight), GetEditorFriendlyName(cardEffectCondition.Type));

                    var currentCardEffectConditionSubtype = cardEffectCondition.Attribute;
                    cardEffectCondition.Attribute = EditorGUI.TextField(new Rect(rect.x + 180, rect.y, 100, EditorGUIUtility.singleLineHeight), currentCardEffectConditionSubtype);
                }
                else
                {
                    if (cardEffectCondition.Attribute != null)
                        currentCardEffectTargetConditionAttribute = effectTargetConditionAttributeOptions.FindIndex(x => x == cardEffectCondition.Attribute);
                    if (currentCardEffectTargetConditionAttribute == -1)
                        currentCardEffectTargetConditionAttribute = 0;
                    currentCardEffectTargetConditionAttribute = EditorGUI.Popup(new Rect(rect.x, rect.y, 100, EditorGUIUtility.singleLineHeight), currentCardEffectTargetConditionAttribute, effectTargetConditionAttributeOptions.ToArray());
                    cardEffectCondition.Attribute = effectTargetConditionAttributeOptions[currentCardEffectTargetConditionAttribute];

                    EditorGUI.LabelField(new Rect(rect.x + 100, rect.y, 100, EditorGUIUtility.singleLineHeight), GetEditorFriendlyName(cardEffectCondition.Type));

                    cardEffectCondition.Value = EditorGUI.IntField(new Rect(rect.x + 180, rect.y, 30, EditorGUIUtility.singleLineHeight), cardEffectCondition.Value);
                }
            }
        }

        private string GetEditorFriendlyName(PlayerEffectConditionType type)
        {
            switch (type)
            {
                case PlayerEffectConditionType.AttributeLessThan:
                    return "Player attribute <";

                case PlayerEffectConditionType.AttributeLessThanOrEqualTo:
                    return "Player attribute <=";

                case PlayerEffectConditionType.AttributeEqualTo:
                    return "Playe attribute ==";

                case PlayerEffectConditionType.AttributeGreaterThanOrEqualTo:
                    return "Player attribute >=";

                case PlayerEffectConditionType.AttributeGreaterThan:
                    return "Player attribute >";
            }

            return "";
        }

        private string GetEditorFriendlyName(CardEffectConditionType type)
        {
            switch (type)
            {
                case CardEffectConditionType.WithPermanentEffect:
                    return "Card with permanent effect";

                case CardEffectConditionType.WithoutPermanentEffect:
                    return "Card without permanent effect";

                case CardEffectConditionType.WithSubtype:
                    return "Card with subtype";

                case CardEffectConditionType.WithoutSubtype:
                    return "Card without subtype";

                case CardEffectConditionType.AttributeLessThan:
                    return "Card attribute <";

                case CardEffectConditionType.AttributeLessThanOrEqualTo:
                    return "Card attribute <=";

                case CardEffectConditionType.AttributeEqualTo:
                    return "Card attribute ==";

                case CardEffectConditionType.AttributeGreaterThanOrEqualTo:
                    return "Card attribute >=";

                case CardEffectConditionType.AttributeGreaterThan:
                    return "Card attribute >";
            }

            return "";
        }
    }
}
