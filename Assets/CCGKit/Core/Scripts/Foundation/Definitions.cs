// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

namespace CCGKit
{
    /// <summary>
    /// Contains the player definition for a game. For example, in the accompanying demo game
    /// a player has numeric life and mana attributes.
    /// </summary>
    public class PlayerDefinition : Entity
    {
    }

    /// <summary>
    /// Contains the information that defines a type of card available in a game. For example, in
    /// the accompanying demo game there are two card definitions (two types of cards): spells and
    /// creatures.
    /// </summary>
    public class CardDefinition : Entity
    {
        /// <summary>
        /// The name of this card definition.
        /// </summary>
        public string Name;

        /// <summary>
        /// True if the card should be destroyed after its effect has been triggered; false otherwise.
        /// </summary>
        public bool DestroyAfterTriggeringEffect;

        /// <summary>
        /// List of effects for this card definition.
        /// </summary>
        public List<Effect> Effects = new List<Effect>();
    }
}
