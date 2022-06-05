// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

namespace CCGKit
{
    /// <summary>
    /// Available effect triggers.
    /// </summary>
    public enum EffectTriggerType
    {
        WhenPlayerTurnStarts,
        WhenPlayerTurnEnds,
        WhenPlayerAttributeIncreases,
        WhenPlayerAttributeDecreases,
        WhenPlayerPlaysACard,
        WhenCardEntersZone,
        WhenCardLeavesZone,
        WhenCardAttributeIncreases,
        WhenCardAttributeDecreases,
        WhenCardAttributeIsLessThan,
        WhenCardAttributeIsLessThanOrEqualTo,
        WhenCardAttributeIsEqualTo,
        WhenCardAttributeIsGreaterThanOrEqualTo,
        WhenCardAttributeIsGreaterThan,
        WhenCardAttacks,
        AfterNumberOfTurns,
        EveryNumberOfTurns
    }

    public enum EffectPlayerTriggerSource
    {
        OwnerPlayer,
        AnyOpponent,
        AnyPlayer
    }

    //-------------------------------------------------------------------------
    // Base classes for defining effect triggers.
    //-------------------------------------------------------------------------
    /// <summary>
    /// Contains the information that defines an effect trigger.
    /// </summary>
    public class EffectTrigger
    {
        public EffectTriggerType Type;
    }

    public class EffectPlayerTrigger : EffectTrigger
    {
        public EffectPlayerTriggerSource Source;
    }

    public class EffectCardTrigger : EffectTrigger
    {
    }

    //-------------------------------------------------------------------------
    // Player triggers.
    //-------------------------------------------------------------------------
    public class PlayerTurnStartedTrigger : EffectPlayerTrigger
    {
        public PlayerTurnStartedTrigger()
        {
            Type = EffectTriggerType.WhenPlayerTurnStarts;
        }
    }

    public class PlayerTurnEndedTrigger : EffectPlayerTrigger
    {
        public PlayerTurnEndedTrigger()
        {
            Type = EffectTriggerType.WhenPlayerTurnEnds;
        }
    }

    public class EffectPlayerAttributeChangeTrigger : EffectPlayerTrigger
    {
        public string Attribute;
    }

    public class PlayerAttributeIncreasedTrigger : EffectPlayerAttributeChangeTrigger
    {
        public PlayerAttributeIncreasedTrigger()
        {
            Type = EffectTriggerType.WhenPlayerAttributeIncreases;
        }
    }

    public class PlayerAttributeDecreasedTrigger : EffectPlayerAttributeChangeTrigger
    {
        public PlayerAttributeDecreasedTrigger()
        {
            Type = EffectTriggerType.WhenPlayerAttributeDecreases;
        }
    }

    public class PlayerPlayedCardTrigger : EffectPlayerTrigger
    {
        public PlayerPlayedCardTrigger()
        {
            Type = EffectTriggerType.WhenPlayerPlaysACard;
        }

        public string CardDefinition;
    }

    //-------------------------------------------------------------------------
    // Card triggers.
    //-------------------------------------------------------------------------
    public class EffectCardZoneTrigger : EffectCardTrigger
    {
        public string Zone;
    }

    public class CardEnteredZoneTrigger : EffectCardZoneTrigger
    {
        public CardEnteredZoneTrigger()
        {
            Type = EffectTriggerType.WhenCardEntersZone;
        }
    }

    public class CardLeftZoneTrigger : EffectCardZoneTrigger
    {
        public CardLeftZoneTrigger()
        {
            Type = EffectTriggerType.WhenCardLeavesZone;
        }
    }

    public class EffectCardAttributeChangeTrigger : EffectCardTrigger
    {
        public string Attribute;
    }

    public class CardAttributeIncreasedTrigger : EffectCardAttributeChangeTrigger
    {
        public CardAttributeIncreasedTrigger()
        {
            Type = EffectTriggerType.WhenCardAttributeIncreases;
        }
    }

    public class CardAttributeDecreasedTrigger : EffectCardAttributeChangeTrigger
    {
        public CardAttributeDecreasedTrigger()
        {
            Type = EffectTriggerType.WhenCardAttributeDecreases;
        }
    }

    public class EffectCardAttributeComparisonTrigger : EffectCardTrigger
    {
        public string Attribute;
        public int Value;
    }

    public class CardAttributeLessThanTrigger : EffectCardAttributeComparisonTrigger
    {
        public CardAttributeLessThanTrigger()
        {
            Type = EffectTriggerType.WhenCardAttributeIsLessThan;
        }
    }

    public class CardAttributeLessThanOrEqualToTrigger : EffectCardAttributeComparisonTrigger
    {
        public CardAttributeLessThanOrEqualToTrigger()
        {
            Type = EffectTriggerType.WhenCardAttributeIsLessThanOrEqualTo;
        }
    }

    public class CardAttributeEqualToTrigger : EffectCardAttributeComparisonTrigger
    {
        public CardAttributeEqualToTrigger()
        {
            Type = EffectTriggerType.WhenCardAttributeIsEqualTo;
        }
    }

    public class CardAttributeGreaterThanOrEqualToTrigger : EffectCardAttributeComparisonTrigger
    {
        public CardAttributeGreaterThanOrEqualToTrigger()
        {
            Type = EffectTriggerType.WhenCardAttributeIsGreaterThanOrEqualTo;
        }
    }

    public class CardAttributeGreaterThanTrigger : EffectCardAttributeComparisonTrigger
    {
        public CardAttributeGreaterThanTrigger()
        {
            Type = EffectTriggerType.WhenCardAttributeIsGreaterThan;
        }
    }

    public class CardAttackedTrigger : EffectCardTrigger
    {
        public CardAttackedTrigger()
        {
            Type = EffectTriggerType.WhenCardAttacks;
        }
    }

    //-------------------------------------------------------------------------
    // Miscellaneous triggers.
    //-------------------------------------------------------------------------
    public class EffectTurnTrigger : EffectTrigger
    {
        public int NumTurns;
    }

    public class AfterNumberOfTurnsTrigger : EffectTurnTrigger
    {
        public AfterNumberOfTurnsTrigger()
        {
            Type = EffectTriggerType.AfterNumberOfTurns;
        }
    }

    public class EveryNumberOfTurnsTrigger : EffectTurnTrigger
    {
        public EveryNumberOfTurnsTrigger()
        {
            Type = EffectTriggerType.EveryNumberOfTurns;
        }
    }
}
