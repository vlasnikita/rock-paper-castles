// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

namespace CCGKit
{
    /// <summary>
    /// Contains the information that defines a concrete card. Cards have an associated card definition,
    /// which defines the general properties that constitute it. You can think of card definitions as
    /// "classes" and of cards as "objects".
    /// </summary>
    public class Card : Entity
    {
        /// <summary>
        /// The unique identifier of this card.
        /// </summary>
        public int Id;

        /// <summary>
        /// The name of this card.
        /// </summary>
        public string Name;

        /// <summary>
        /// The definition (type) of this card.
        /// </summary>
        public string Definition;

        /// <summary>
        /// Maximum number of copies of this card that can be in a deck.
        /// </summary>
        public int MaxCopies;

        /// <summary>
        /// The rarity of this card.
        /// </summary>
        public string Rarity;

        /// <summary>
        /// List of subtypes for this card.
        /// </summary>
        public List<string> Subtypes = new List<string>();

        /// <summary>
        /// List of effects for this card.
        /// </summary>
        public List<Effect> Effects = new List<Effect>();
    }

    /// <summary>
    /// Contains a named list of cards. This is useful in order to have different sets of cards rather than
    /// a single, monolithic one (e.g., expansion packs).
    /// </summary>
    public class CardSet
    {
        /// <summary>
        /// The name of this card collection.
        /// </summary>
        public string Name = string.Empty;

        /// <summary>
        /// List of cards that belong to this set.
        /// </summary>
        public List<Card> Cards = new List<Card>();
    }
}
