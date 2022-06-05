// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

namespace CCGKit
{
    public enum GameActionTarget
    {
        CurrentPlayer,
        CurrentOpponents,
        AllPlayers
    }

    public class GameAction
    {
        public string Name { get; private set; }

        public GameActionTarget Target;

        public GameAction(string name)
        {
            Name = name;
        }
    }

    public class SetPlayerAttributeAction : GameAction
    {
        public string Attribute;
        public int Value;

        public SetPlayerAttributeAction() : base("Set player attribute")
        {
        }
    }

    public class IncreasePlayerAttributeAction : GameAction
    {
        public string Attribute;
        public int Value;
        public int Max;

        public IncreasePlayerAttributeAction() : base("Increase player attribute")
        {
        }
    }

    public class ShuffleCardsAction : GameAction
    {
        public string Zone;

        public ShuffleCardsAction() : base("Shuffle cards")
        {
        }
    }

    public class MoveCardsAction : GameAction
    {
        public string OriginZone;
        public string DestinationZone;
        public int NumCards;

        public MoveCardsAction() : base("Move cards")
        {
        }
    }
}
