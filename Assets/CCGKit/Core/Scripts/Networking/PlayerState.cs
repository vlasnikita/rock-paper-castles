// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Networking;

namespace CCGKit
{
    /// <summary>
    /// This class holds the information about a dynamic player attribute. Note we are only interested
    /// in numeric attributes for gameplay purposes.
    /// </summary>
    public struct PlayerAttribute
    {
        public string Name;
        public int Value;

        public PlayerAttribute(string name, int value)
        {
            Name = name;
            Value = value;
        }
    }

    /// <summary>
    /// Holds the data stored by the server for every player in a game. This is the server-authoritative data
    /// that eventually gets sent to the connected clients.
    /// </summary>
    public class PlayerState
    {
        /// <summary>
        /// Unique identifier of this player.
        /// </summary>
        public int Id;

        /// <summary>
        /// Network identifier of this player.
        /// </summary>
        public NetworkInstanceId NetId;

        /// <summary>
        /// Connection identifier of this player.
        /// </summary>
        public int ConnectionId;

        /// <summary>
        /// True if this player is currently connected to the server; false otherwise.
        /// </summary>
        public bool IsConnected;

        /// <summary>
        /// Name of this player.
        /// </summary>
        public string Name;

        /// <summary>
        /// True if this player is controlled by a human; false otherwise (AI).
        /// </summary>
        public bool IsHuman;

        /// <summary>
        /// List of attributes this player has.
        /// </summary>
        public List<PlayerAttribute> Attributes = new List<PlayerAttribute>();

        /// <summary>
        /// List of game zones this player owns.
        /// </summary>
        public List<GameZone> GameZones = new List<GameZone>();

        /// <summary>
        /// Identifier of the last card that was played by this player.
        /// </summary>
        public int LastCardPlayedId = -1;

        /// <summary>
        /// Колода, за которую играет игрок.
        /// </summary>
        public string ActiveDeck;

        /// <summary>
        /// Returns the player's attribute with the specified name.
        /// </summary>
        /// <param name="name">Name of the player's attribute to return.</param>
        /// <returns>The player's attribute with the specified name; false otherwise.</returns>
        public PlayerAttribute? GetAttribute(string name)
        {
            for (var i = 0; i < Attributes.Count; i++)
                if (Attributes[i].Name == name)
                    return Attributes[i];
            return null;
        }

        /// <summary>
        /// Sets the value of the player's attribute with the specified name to the specified value.
        /// </summary>
        /// <param name="name">Name of the player's attribute to update.</param>
        /// <param name="value">Value to which update the player's attribute.</param>
        public void SetAttribute(string name, int value)
        {
            for (var i = 0; i < Attributes.Count; i++)
            {
                if (Attributes[i].Name == name)
                {
                    // Check for triggered effects.
                    var oldValue = Attributes[i].Value;
                    if (value > oldValue)
                        TriggerOnPlayerIncreasedAttributeEffects(Attributes[i].Name);
                    else if (value < oldValue)
                        TriggerOnPlayerDecreasedAttributeEffects(Attributes[i].Name);

                    Attributes[i] = new PlayerAttribute(name, value);
                    return;
                }
            }
            Attributes.Add(new PlayerAttribute(name, value));
        }

        /// <summary>
        /// Returns the game zone with the specified name that is owned by this player.
        /// </summary>
        /// <param name="name">Name of the game zone to retrieve.</param>
        /// <returns>The game zone with the specified name owned by this player.</returns>
        public GameZone GetGameZone(string name)
        {
            return GameZones.Find(x => x.Name == name);
        }

        /// <summary>
        /// Returns the static game zone with the specified name that is owned by this player.
        /// </summary>
        /// <param name="name">Name of the static game zone to retrieve.</param>
        /// <returns>The static game zone with the specified name owned by this player.</returns>
        public StaticGameZone GetStaticGameZone(string name)
        {
            var zone = GameZones.Find(x => x.Name == name);
            if (zone != null)
            {
                return zone as StaticGameZone;
            }
            return null;
        }

        /// <summary>
        /// Returns the dynamic game zone with the specified name that is owned by this player.
        /// </summary>
        /// <param name="name">Name of the dynamic game zone to retrieve.</param>
        /// <returns>The dynamic game zone with the specified name owned by this player.</returns>
        public DynamicGameZone GetDynamicGameZone(string name)
        {
            var zone = GameZones.Find(x => x.Name == name);
            if (zone != null)
            {
                return zone as DynamicGameZone;
            }
            return null;
        }

        /// <summary>
        /// Trigger any effect that is a result of the player's attribute with the specified name being increased.
        /// </summary>
        /// <param name="attributeName">Name of the player's attribute that has increased.</param>
        public void TriggerOnPlayerIncreasedAttributeEffects(string attributeName)
        {
            foreach (var pair in NetworkServer.objects)
            {
                var netCard = pair.Value.gameObject.GetComponent<NetworkCard>();
                if (netCard != null && netCard.IsAlive && netCard.OwnerNetId == NetId)
                {
                    var card = GameManager.Instance.GetCard(netCard.CardId);
                    var cardDefinition = GameManager.Instance.Config.CardDefinitions.Find(x => x.Name == card.Definition);

                    var definitionEffects = cardDefinition.Effects.FindAll(x => x.Trigger.Type == EffectTriggerType.WhenPlayerAttributeIncreases);
                    definitionEffects = definitionEffects.FindAll(x => (x.Trigger as PlayerAttributeIncreasedTrigger).Attribute == attributeName);

                    var cardEffects = card.Effects.FindAll(x => x.Trigger.Type == EffectTriggerType.WhenPlayerAttributeIncreases);
                    cardEffects = cardEffects.FindAll(x => (x.Trigger as PlayerAttributeIncreasedTrigger).Attribute == attributeName);

                    definitionEffects.AddRange(cardEffects);
                    if (definitionEffects.Count > 0)
                    {
                        var server = GameObject.Find("Server").GetComponent<Server>();
                        server.OnPlayerIncreasedAttribute(netCard.netId);
                    }
                }
            }
        }

        /// <summary>
        /// Trigger any effect that is a result of the player's attribute with the specified name being decreased.
        /// </summary>
        /// <param name="attributeName">Name of the player's attribute that has decreased.</param>
        public void TriggerOnPlayerDecreasedAttributeEffects(string attributeName)
        {
            foreach (var pair in NetworkServer.objects)
            {
                var netCard = pair.Value.gameObject.GetComponent<NetworkCard>();
                if (netCard != null && netCard.IsAlive && netCard.OwnerNetId == NetId)
                {
                    var card = GameManager.Instance.GetCard(netCard.CardId);
                    var cardDefinition = GameManager.Instance.Config.CardDefinitions.Find(x => x.Name == card.Definition);

                    var definitionEffects = cardDefinition.Effects.FindAll(x => x.Trigger.Type == EffectTriggerType.WhenPlayerAttributeDecreases);
                    definitionEffects = definitionEffects.FindAll(x => (x.Trigger as PlayerAttributeDecreasedTrigger).Attribute == attributeName);

                    var cardEffects = card.Effects.FindAll(x => x.Trigger.Type == EffectTriggerType.WhenPlayerAttributeDecreases);
                    cardEffects = cardEffects.FindAll(x => (x.Trigger as PlayerAttributeDecreasedTrigger).Attribute == attributeName);

                    definitionEffects.AddRange(cardEffects);
                    if (definitionEffects.Count > 0)
                    {
                        var server = GameObject.Find("Server").GetComponent<Server>();
                        server.OnPlayerDecreasedAttribute(netCard.netId);
                    }
                }
            }
        }
    }
}
