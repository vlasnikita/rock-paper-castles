// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

using UnityEngine.Networking;

using FullSerializer;

namespace CCGKit
{
    /// <summary>
    /// This type is used to define the available zones in a CCG (e.g., deck, hand, board, etc.).
    /// </summary>
    public class GameZone
    {
        /// <summary>
        /// The name of this game zone.
        /// </summary>
        public string Name;

        /// <summary>
        /// True if the cards contained in this zone should not be sent across the network;
        /// false otherwise.
        /// </summary>
        public bool Private;
    }

    /// <summary>
    /// A static game zone is a zone which contains cards whose attributes never change.
    /// For example, cards in the deck or the hand usually never change so we can just
    /// store their unique identifiers.
    /// </summary>
    public class StaticGameZone : GameZone
    {
        /// <summary>
        /// The unique identifiers of the cards contained in this zone.
        /// </summary>
        [fsIgnore]
        public List<int> Cards = new List<int>();
    }

    /// <summary>
    /// A dynamic game zone is a zone which contains cards whose attributes can change.
    /// For example, cards in the board usually can change as the game progresses so we
    /// need to store their unique network identifiers in order to be able to point to
    /// the network objects containing their most up-to-date state.
    /// </summary>
    public class DynamicGameZone : GameZone
    {
        /// <summary>
        /// The unique network identifiers of the cards contained in this zone.
        /// </summary>
        [fsIgnore]
        public List<NetworkInstanceId> Cards = new List<NetworkInstanceId>();
    }

    /// <summary>
    /// The data describing a static zone that is sent across the network.
    /// </summary>
    public struct NetStaticGameZone
    {
        /// <summary>
        /// The name of this game zone.
        /// </summary>
        public string Name;

        /// <summary>
        /// The number of cards contained in this zone.
        /// </summary>
        public int NumCards;

        /// <summary>
        /// The unique identifiers of the cards contained in this zone.
        /// </summary>
        public int[] Cards;
    }

    /// <summary>
    /// The data describing a dynamic zone that is sent across the network.
    /// </summary>
    public struct NetDynamicGameZone
    {
        /// <summary>
        /// The name of this game zone.
        /// </summary>
        public string Name;

        /// <summary>
        /// The number of cards contained in this zone.
        /// </summary>
        public int NumCards;

        /// <summary>
        /// The unique network identifiers of the cards contained in this zone.
        /// </summary>
        public NetworkInstanceId[] Cards;
    }
}
