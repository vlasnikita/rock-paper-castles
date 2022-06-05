// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

namespace CCGKit
{
    /// <summary>
    /// Available effect types.
    /// </summary>
    public enum EffectType
    {
        Permanent,
        TargetPlayer,
        TargetCard,
        General
    }

    /// <summary>
    /// Available action types for player effects.
    /// </summary>
    public enum PlayerEffectActionType
    {
        IncreaseAttribute,
        DecreaseAttribute,
        SetAttribute,
        MoveCards
    }

    /// <summary>
    /// Available action types for card effects.
    /// </summary>
    public enum CardEffectActionType
    {
        AddCounter,
        RemoveCounter,
        SetAttribute,
        Kill,
        Transform
    }

    /// <summary>
    /// Available action types for general effects.
    /// </summary>
    public enum GeneralEffectActionType
    {
        CreateToken
    }

    /// <summary>
    /// Contains the information that defines a type of effect available in a game. You can think
    /// of effects as the 'abilities' that may be present in a given card.
    /// </summary>
    public class EffectDefinition
    {
        /// <summary>
        /// The name of this effect definition.
        /// </summary>
        public string Name;

        /// <summary>
        /// The type of effect for this effect definition.
        /// </summary>
        public EffectType Type;
    }

    /// <summary>
    /// The definition of a permanent effect.
    /// </summary>
    public class PermanentEffectDefinition : EffectDefinition
    {
        public PermanentEffectDefinition()
        {
            Type = EffectType.Permanent;
        }
    }

    /// <summary>
    /// The definition of an effect that targets a player or set of players.
    /// </summary>
    public class PlayerEffectDefinition : EffectDefinition
    {
        /// <summary>
        /// The action type for this effect definition.
        /// </summary>
        public PlayerEffectActionType Action;

        /// <summary>
        /// The player attribute that is modified by this effect.
        /// </summary>
        public string Attribute;

        /// <summary>
        /// Additional field needed by the move cards effect.
        /// </summary>
        public string AttributeExtra;

        public PlayerEffectDefinition()
        {
            Type = EffectType.TargetPlayer;
        }
    }

    /// <summary>
    /// The definition of an effect that targets a card or set of cards.
    /// </summary>
    public class CardEffectDefinition : EffectDefinition
    {
        /// <summary>
        /// The action type for this effect definition.
        /// </summary>
        public CardEffectActionType Action;

        /// <summary>
        /// The card type that can be the target of this effect.
        /// </summary>
        public string Card;

        /// <summary>
        /// The card attribute that is modified by this effect.
        /// </summary>
        public string Attribute;

        public CardEffectDefinition()
        {
            Type = EffectType.TargetCard;
        }
    }

    /// <summary>
    /// The definition of a effect that cannot be categorized as permanent nor targetable.
    /// </summary>
    public class GeneralEffectDefinition : EffectDefinition
    {
        /// <summary>
        /// The action type for this effect definition.
        /// </summary>
        public GeneralEffectActionType Action;

        public GeneralEffectDefinition()
        {
            Type = EffectType.General;
        }
    }

    /// <summary>
    /// Available condition types for player effects.
    /// </summary>
    public enum PlayerEffectConditionType
    {
        AttributeLessThan,
        AttributeLessThanOrEqualTo,
        AttributeEqualTo,
        AttributeGreaterThanOrEqualTo,
        AttributeGreaterThan
    }

    /// <summary>
    /// Available condition types for card effects.
    /// </summary>
    public enum CardEffectConditionType
    {
        WithPermanentEffect,
        WithoutPermanentEffect,
        WithSubtype,
        WithoutSubtype,
        AttributeLessThan,
        AttributeLessThanOrEqualTo,
        AttributeEqualTo,
        AttributeGreaterThanOrEqualTo,
        AttributeGreaterThan
    }

    /// <summary>
    /// Contains the information that defines an effect condition.
    /// </summary>
    public class EffectCondition
    {
        /// <summary>
        /// String payload of this effect condition.
        /// </summary>
        public string Attribute;

        /// <summary>
        /// Integer payload of this effect condition.
        /// </summary>
        public int Value;
    }

    /// <summary>
    /// Contains the information that defines a player effect condition.
    /// </summary>
    public class PlayerEffectCondition : EffectCondition
    {
        /// <summary>
        /// The type of this player effect condition.
        /// </summary>
        public PlayerEffectConditionType Type;
    }

    /// <summary>
    /// Contains the information that defines a card effect condition.
    /// </summary>
    public class CardEffectCondition : EffectCondition
    {
        /// <summary>
        /// The types of this card effect condition.
        /// </summary>
        public CardEffectConditionType Type;
    }

    /// <summary>
    /// Contains the information that defines a specific effect in a card.
    /// </summary>
    public class Effect
    {
        /// <summary>
        /// The type of this effect.
        /// </summary>
        public EffectType Type;

        /// <summary>
        /// The definition of this effect.
        /// </summary>
        public string Definition;

        /// <summary>
        /// The trigger of this effect.
        /// </summary>
        public EffectTrigger Trigger = new EffectTrigger();

        /// <summary>
        /// The conditions that need to be fulfilled by the trigger in order for this effect to be applied.
        /// </summary>
        public List<EffectCondition> TriggerConditions = new List<EffectCondition>();

        /// <summary>
        /// The conditions that need to be fulfilled by the target in order for this effect to be applied.
        /// </summary>
        public List<EffectCondition> TargetConditions = new List<EffectCondition>();

        /// <summary>
        /// True if this effect should always be applied; false otherwise.
        /// </summary>
        public bool Unavoidable;
    }

    /// <summary>
    /// Available target types for player effects.
    /// </summary>
    public enum PlayerEffectTargetType
    {
        TargetPlayer,
        CurrentPlayer,
        CurrentOpponent,
        AllPlayers,
        RandomPlayer
    }

    /// <summary>
    /// Available target types for card effects.
    /// </summary>
    public enum CardEffectTargetType
    {
        TargetCard,
        ThisCard,
        CurrentPlayerCard,
        CurrentOpponentCard,
        AllCurrentPlayerCards,
        AllCurrentOpponentCards,
        AllCards,
        RandomCard
    }

    /// <summary>
    /// Contains the information that defines a permanent effect.
    /// </summary>
    public class PermanentEffect : Effect
    {
        public PermanentEffect()
        {
            Type = EffectType.Permanent;
        }
    }

    /// <summary>
    /// Contains the information that defines a targetable effect.
    /// </summary>
    public class TargetableEffect : Effect
    {
        /// <summary>
        /// The value associated to this effect.
        /// </summary>
        public IntValue Value;
    }

    /// <summary>
    /// Contains the information that defines a player effect.
    /// </summary>
    public class PlayerEffect : TargetableEffect
    {
        /// <summary>
        /// The target type of this player effect.
        /// </summary>
        public PlayerEffectTargetType Target;

        public PlayerEffect()
        {
            Type = EffectType.TargetPlayer;
        }
    }

    /// <summary>
    /// Contains the information that defines a card effect.
    /// </summary>
    public class CardEffect : TargetableEffect
    {
        /// <summary>
        /// The target type of this card effect.
        /// </summary>
        public CardEffectTargetType Target;

        public CardEffect()
        {
            Type = EffectType.TargetCard;
        }
    }

    /// <summary>
    /// Contains the information that defines a transform effect.
    /// </summary>
    public class TransformEffect : CardEffect
    {
        /// <summary>
        /// The card to transform to.
        /// </summary>
        public string Card;
    }

    /// <summary>
    /// Contains the information that defines an effect that cannot be categorized as permanent
    /// nor targetable.
    /// </summary>
    public class GeneralEffect : Effect
    {
        /// <summary>
        /// The card of this card effect.
        /// </summary>
        public string Card;

        public GeneralEffect()
        {
            Type = EffectType.General;
        }
    }
}
