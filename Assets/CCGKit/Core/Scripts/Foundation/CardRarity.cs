// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

namespace CCGKit
{
    /// <summary>
    /// The rarity of a card represents the chance to get a copy of it in a pack.
    /// </summary>
    public class CardRarity
    {
        /// <summary>
        /// Name of the rarity.
        /// </summary>
        public string Name;

        /// <summary>
        /// Chance of the rarity. Must be a value in the [0, 100] range and indicates the probability
        /// to get a copy of a card with this rarity in a pack.
        /// </summary>
        public int Chance;
    }
}
