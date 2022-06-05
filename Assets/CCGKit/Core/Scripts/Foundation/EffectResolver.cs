// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

namespace CCGKit
{
    /// <summary>
    /// This class is responsible for actually resolving an effect's action over its intended target,
    /// be it a player or a card.
    /// </summary>
    public static class EffectResolver
    {
        /// <summary>
        /// Resolves the specified effect on the specified target player.
        /// </summary>
        /// <param name="effect">Effect to resolve.</param>
        /// <param name="playerState">The specified effect's player target.</param>
        /// <param name="server">The current server.</param>
        /// <param name="card">The card with the effect.</param>
        public static void ResolvePlayerTargetedEffect(PlayerEffect effect, PlayerState playerState, Server server, NetworkCard card)
        {
            if (!IsValidTrigger(server, effect, card) || !IsValidTarget(playerState, effect))
                return;

            var effectDefinition = GameManager.Instance.Config.EffectDefinitions.Find(x => x.Name == effect.Definition) as PlayerEffectDefinition;
            var effectAction = effectDefinition.Action;
            var effectAttribute = effectDefinition.Attribute;
            var effectValue = effect.Value;
            switch (effectAction)
            {
                case PlayerEffectActionType.IncreaseAttribute:
                    {
                        var attribute = playerState.GetAttribute(effectAttribute);
                        if (attribute.HasValue)
                            playerState.SetAttribute(attribute.Value.Name, attribute.Value.Value + effectValue);
                    }
                    break;

                case PlayerEffectActionType.DecreaseAttribute:
                    {
                        var attribute = playerState.GetAttribute(effectAttribute);
                        if (attribute.HasValue)
                            playerState.SetAttribute(attribute.Value.Name, attribute.Value.Value - effectValue);
                    }
                    break;

                case PlayerEffectActionType.SetAttribute:
                    {
                        var attribute = playerState.GetAttribute(effectAttribute);
                        if (attribute.HasValue)
                            playerState.SetAttribute(attribute.Value.Name, effectValue);
                    }
                    break;

                case PlayerEffectActionType.MoveCards:
                    {
                        server.MoveCards(playerState, effectAttribute, effectDefinition.AttributeExtra, effectValue);
                    }
                    break;
            }
        }

        /// <summary>
        /// Resolves the specified effect on the specified target card.
        /// </summary>
        /// <param name="effect">Effect to resolve.</param>
        /// <param name="card">The specified effect's target card.</param>
        /// <param name="server">The current server.</param>
        public static void ResolveCardTargetedEffect(CardEffect effect, NetworkCard card, Server server)
        {
            var effectDefinition = GameManager.Instance.Config.EffectDefinitions.Find(x => x.Name == effect.Definition) as CardEffectDefinition;
            var effectAction = effectDefinition.Action;
            var effectAttribute = effectDefinition.Attribute;
            var effectValue = effect.Value;
            // Note the effect is only applied if the intended target can be the target of such an effect.
            switch (effectAction)
            {
                case CardEffectActionType.AddCounter:
                    {
                        if (IsValidTrigger(server, effect, card) && IsValidTarget(card, effect) && card.CanBeTargetOfEffect(effect))
                        {
                            var attribute = card.GetAttribute(effectAttribute);
                            if (attribute.HasValue)
                                card.SetAttribute(attribute.Value.Name, attribute.Value.Value + effectValue);
                        }
                    }
                    break;

                case CardEffectActionType.RemoveCounter:
                    {
                        if (IsValidTrigger(server, effect, card) && IsValidTarget(card, effect) && card.CanBeTargetOfEffect(effect))
                        {
                            var attribute = card.GetAttribute(effectAttribute);
                            if (attribute.HasValue)
                                card.SetAttribute(attribute.Value.Name, attribute.Value.Value - effectValue);
                        }
                    }
                    break;

                case CardEffectActionType.SetAttribute:
                    {
                        if (IsValidTrigger(server, effect, card) && IsValidTarget(card, effect) && card.CanBeTargetOfEffect(effect))
                        {
                            var attribute = card.GetAttribute(effectAttribute);
                            if (attribute.HasValue)
                                card.SetAttribute(attribute.Value.Name, effectValue);
                        }
                    }
                    break;

                case CardEffectActionType.Kill:
                    if (IsValidTrigger(server, effect, card) && IsValidTarget(card, effect) && card.CanBeTargetOfEffect(effect))
                        server.DestroyCard(card.netId);
                    break;

                case CardEffectActionType.Transform:
                    if (IsValidTrigger(server, effect, card) && IsValidTarget(card, effect) && card.CanBeTargetOfEffect(effect))
                        server.TransformCard(card.netId, (effect as TransformEffect).Card);
                    break;
            }
        }

        /// <summary>
        /// Returns true if the specified effect's trigger is valid.
        /// </summary>
        /// <param name="server">Reference to the server.</param>
        /// <param name="effect">Effect to be applied.</param>
        /// <param name="card">Card containing the effect to be applied.</param>
        /// <returns>True if the specified effect's trigger is valid; false otherwise.</returns>
        public static bool IsValidTrigger(Server server, Effect effect, NetworkCard card)
        {
            if (effect.Trigger is EffectPlayerTrigger)
            {
                var playerTrigger = effect.Trigger as EffectPlayerTrigger;
                var players = new List<PlayerState>();
                switch (playerTrigger.Source)
                {
                    case EffectPlayerTriggerSource.OwnerPlayer:
                        players.Add(server.Players.Find(x => x.NetId == card.OwnerNetId));
                        break;

                    case EffectPlayerTriggerSource.AnyOpponent:
                        players.AddRange(server.Players.FindAll(x => x.NetId != card.OwnerNetId));
                        break;

                    case EffectPlayerTriggerSource.AnyPlayer:
                        players = server.Players;
                        break;
                }

                foreach (var player in players)
                {
                    var conditionsFullfilled = true;
                    foreach (var condition in effect.TriggerConditions)
                    {
                        if (condition is PlayerEffectCondition)
                        {
                            if (!FulfillCondition(player, condition as PlayerEffectCondition))
                            {
                                conditionsFullfilled = false;
                                break;
                            }
                        }
                        // Check the last card played in 'played played a card' effects.
                        else if (condition is CardEffectCondition)
                        {
                            if (player.LastCardPlayedId != -1)
                            {
                                var lastPlayedCard = GameManager.Instance.GetCard(player.LastCardPlayedId);
                                if (!FulfillCondition(lastPlayedCard, condition as CardEffectCondition))
                                {
                                    conditionsFullfilled = false;
                                    break;
                                }
                            }
                            else
                            {
                                conditionsFullfilled = false;
                                break;
                            }
                        }
                    }
                    if (conditionsFullfilled)
                        return true;
                }
                return false;
            }
            else if (effect.Trigger is EffectCardTrigger)
            {
                foreach (var condition in effect.TriggerConditions)
                {
                    if (!FulfillCondition(card, condition as CardEffectCondition))
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns true if the specified player is currently a valid target of the specified effect.
        /// </summary>
        /// <param name="player">Player to check for.</param>
        /// <param name="effect">Effect to be applied to the specified player.</param>
        /// <returns>True if the specified player is currently a valid target of the specified effect; false otherwise.</returns>
        public static bool IsValidTarget(PlayerState player, PlayerEffect effect)
        {
            foreach (var condition in effect.TargetConditions)
            {
                if (!FulfillCondition(player, condition as PlayerEffectCondition))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Returns true if the specified effect trigger is of player type.
        /// </summary>
        /// <param name="trigger">Effect trigger to check.</param>
        /// <returns>True if the specified effect trigger is of player type; false otherwise.</returns>
        private static bool IsPlayerTrigger(EffectTrigger trigger)
        {
            return trigger is EffectPlayerTrigger;
        }

        /// <summary>
        /// Returns true if the specified effect trigger is of card type.
        /// </summary>
        /// <param name="trigger">Effect trigger to check.</param>
        /// <returns>True if the specified effect trigger is of card type; false otherwise.</returns>
        private static bool IsCardTrigger(EffectTrigger trigger)
        {
            return trigger is EffectCardTrigger;
        }

        /// <summary>
        /// Returns true if the specified player fulfills the specified condition from the specified effect.
        /// </summary>
        /// <param name="player">Player to check for.</param>
        /// <param name="condition">Condition to check for.</param>
        /// <returns>True if the specified player fulfills the specified condition from the specified effect; false otherwise.</returns>
        public static bool FulfillCondition(PlayerState player, PlayerEffectCondition condition)
        {
            var conditionFulfilled = false;
            switch (condition.Type)
            {
                case PlayerEffectConditionType.AttributeLessThan:
                    {
                        var attribute = player.GetAttribute(condition.Attribute);
                        conditionFulfilled = attribute.HasValue && attribute.Value.Value < condition.Value;
                    }
                    break;

                case PlayerEffectConditionType.AttributeLessThanOrEqualTo:
                    {
                        var attribute = player.GetAttribute(condition.Attribute);
                        conditionFulfilled = attribute.HasValue && attribute.Value.Value <= condition.Value;
                    }
                    break;

                case PlayerEffectConditionType.AttributeEqualTo:
                    {
                        var attribute = player.GetAttribute(condition.Attribute);
                        conditionFulfilled = attribute.HasValue && attribute.Value.Value == condition.Value;
                    }
                    break;

                case PlayerEffectConditionType.AttributeGreaterThanOrEqualTo:
                    {
                        var attribute = player.GetAttribute(condition.Attribute);
                        conditionFulfilled = attribute.HasValue && attribute.Value.Value >= condition.Value;
                    }
                    break;

                case PlayerEffectConditionType.AttributeGreaterThan:
                    {
                        var attribute = player.GetAttribute(condition.Attribute);
                        conditionFulfilled = attribute.HasValue && attribute.Value.Value > condition.Value;
                    }
                    break;
            }
            return conditionFulfilled;
        }

        /// <summary>
        /// Returns true if the specified card is currently a valid target of the specified effect.
        /// </summary>
        /// <param name="card">Card to check for.</param>
        /// <param name="effect">Effect to be applied to the specified card.</param>
        /// <returns>True if the specified card is currently a valid target of the specified effect; false otherwise.</returns>
        public static bool IsValidTarget(NetworkCard card, CardEffect effect)
        {
            foreach (var condition in effect.TargetConditions)
            {
                if (!FulfillCondition(card, condition as CardEffectCondition))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Returns true if the specified card fulfills the specified condition from the specified effect.
        /// </summary>
        /// <param name="card">Card to check for.</param>
        /// <param name="condition">Condition to check for.</param>
        /// <returns>True if the specified card fulfills the specified condition from the specified effect; false otherwise.</returns>
        public static bool FulfillCondition(NetworkCard netCard, CardEffectCondition condition)
        {
            var conditionFulfilled = false;
            var card = GameManager.Instance.GetCard(netCard.CardId);
            switch (condition.Type)
            {
                case CardEffectConditionType.WithPermanentEffect:
                    {
                        var permanentEffect = card.Effects.Find(x => x is PermanentEffect && (x as PermanentEffect).Definition == condition.Attribute);
                        conditionFulfilled = permanentEffect != null;
                    }
                    break;

                case CardEffectConditionType.WithoutPermanentEffect:
                    {
                        var permanentEffect = card.Effects.Find(x => x is PermanentEffect && (x as PermanentEffect).Definition == condition.Attribute);
                        conditionFulfilled = permanentEffect == null;
                    }
                    break;

                case CardEffectConditionType.WithSubtype:
                    {
                        var subtype = card.Subtypes.Find(x => x == condition.Attribute);
                        conditionFulfilled = subtype != null;
                    }
                    break;

                case CardEffectConditionType.WithoutSubtype:
                    {
                        var subtype = card.Subtypes.Find(x => x == condition.Attribute);
                        conditionFulfilled = subtype == null;
                    }
                    break;

                case CardEffectConditionType.AttributeLessThan:
                    {
                        var attribute = netCard.GetAttribute(condition.Attribute);
                        conditionFulfilled = attribute.HasValue && attribute.Value.Value < condition.Value;
                    }
                    break;

                case CardEffectConditionType.AttributeLessThanOrEqualTo:
                    {
                        var attribute = netCard.GetAttribute(condition.Attribute);
                        conditionFulfilled = attribute.HasValue && attribute.Value.Value <= condition.Value;
                    }
                    break;

                case CardEffectConditionType.AttributeEqualTo:
                    {
                        var attribute = netCard.GetAttribute(condition.Attribute);
                        conditionFulfilled = attribute.HasValue && attribute.Value.Value == condition.Value;
                    }
                    break;

                case CardEffectConditionType.AttributeGreaterThanOrEqualTo:
                    {
                        var attribute = netCard.GetAttribute(condition.Attribute);
                        conditionFulfilled = attribute.HasValue && attribute.Value.Value >= condition.Value;
                    }
                    break;

                case CardEffectConditionType.AttributeGreaterThan:
                    {
                        var attribute = netCard.GetAttribute(condition.Attribute);
                        conditionFulfilled = attribute.HasValue && attribute.Value.Value > condition.Value;
                    }
                    break;
            }
            return conditionFulfilled;
        }

        /// <summary>
        /// Returns true if the specified card fulfills the specified condition from the specified effect.
        /// </summary>
        /// <param name="card">Card to check for.</param>
        /// <param name="condition">Condition to check for.</param>
        /// <returns>True if the specified card fulfills the specified condition from the specified effect; false otherwise.</returns>
        public static bool FulfillCondition(Card card, CardEffectCondition condition)
        {
            var conditionFulfilled = false;
            switch (condition.Type)
            {
                case CardEffectConditionType.WithPermanentEffect:
                    {
                        var permanentEffect = card.Effects.Find(x => x is PermanentEffect && (x as PermanentEffect).Definition == condition.Attribute);
                        conditionFulfilled = permanentEffect != null;
                    }
                    break;

                case CardEffectConditionType.WithoutPermanentEffect:
                    {
                        var permanentEffect = card.Effects.Find(x => x is PermanentEffect && (x as PermanentEffect).Definition == condition.Attribute);
                        conditionFulfilled = permanentEffect == null;
                    }
                    break;

                case CardEffectConditionType.WithSubtype:
                    {
                        var subtype = card.Subtypes.Find(x => x == condition.Attribute);
                        conditionFulfilled = subtype != null;
                    }
                    break;

                case CardEffectConditionType.WithoutSubtype:
                    {
                        var subtype = card.Subtypes.Find(x => x == condition.Attribute);
                        conditionFulfilled = subtype == null;
                    }
                    break;

                case CardEffectConditionType.AttributeLessThan:
                    {
                        var attribute = card.GetIntegerAttribute(condition.Attribute);
                        conditionFulfilled = attribute < condition.Value;
                    }
                    break;

                case CardEffectConditionType.AttributeLessThanOrEqualTo:
                    {
                        var attribute = card.GetIntegerAttribute(condition.Attribute);
                        conditionFulfilled = attribute <= condition.Value;
                    }
                    break;

                case CardEffectConditionType.AttributeEqualTo:
                    {
                        var attribute = card.GetIntegerAttribute(condition.Attribute);
                        conditionFulfilled = attribute == condition.Value;
                    }
                    break;

                case CardEffectConditionType.AttributeGreaterThanOrEqualTo:
                    {
                        var attribute = card.GetIntegerAttribute(condition.Attribute);
                        conditionFulfilled = attribute >= condition.Value;
                    }
                    break;

                case CardEffectConditionType.AttributeGreaterThan:
                    {
                        var attribute = card.GetIntegerAttribute(condition.Attribute);
                        conditionFulfilled = attribute > condition.Value;
                    }
                    break;
            }
            return conditionFulfilled;
        }

        /// <summary>
        /// Resolves the specified general effect.
        /// </summary>
        /// <param name="effect">Effect to resolve.</param>
        /// <param name="card">Card that has the effect.</param>
        /// <param name="server">The current server.</param>
        public static void ResolveGeneralEffect(GeneralEffect effect, NetworkCard card, Server server)
        {
            var effectDefinition = GameManager.Instance.Config.EffectDefinitions.Find(x => x.Name == effect.Definition) as GeneralEffectDefinition;
            switch (effectDefinition.Action)
            {
                case GeneralEffectActionType.CreateToken:
                    var effectCardName = (effect as GeneralEffect).Card;
                    var effectCard = GameManager.Instance.GetCard(effectCardName);
                    server.PlayCard(effectCard.Id, card.OwnerNetId);
                    break;

                default:
                    break;
            }
        }
    }
}
