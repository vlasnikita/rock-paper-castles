// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

namespace CCGKit
{
    public class GameProperties
    {
        /// <summary>
        /// Number of players allowed in a game.
        /// </summary>
        public int NumPlayers;

        /// <summary>
        /// Duration of a game turn (in seconds).
        /// </summary>
        public int TurnDuration;

        /// <summary>
        /// Minimum number of cards that need to be in a deck.
        /// </summary>
        public int MinDeckSize;

        /// <summary>
        /// Maximum number of cards that can be in a deck.
        /// </summary>
        public int MaxDeckSize;

        /// <summary>
        /// Maximum number of cards that can be in a hand.
        /// </summary>
        public int MaxHandSize;

        /// <summary>
        /// List of actions to perform when a game starts.
        /// </summary>
        public List<GameAction> GameStartActions = new List<GameAction>();

        /// <summary>
        /// List of actions to perform when a turn starts.
        /// </summary>
        public List<GameAction> TurnStartActions = new List<GameAction>();

        /// <summary>
        /// List of actions to perform when a turn ends.
        /// </summary>
        public List<GameAction> TurnEndActions = new List<GameAction>();
    }
}
