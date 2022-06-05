// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;

namespace CCGKit
{
    /// <summary>
    /// This server handler manages the registration of new players into an open game.
    /// </summary>
    public class PlayerRegistrationHandler : ServerHandler
    {
        public PlayerRegistrationHandler(Server server) : base(server)
        {
        }

        public override void RegisterNetworkHandlers()
        {
            base.RegisterNetworkHandlers();
            NetworkServer.RegisterHandler(NetworkProtocol.RegisterPlayer, OnRegisterPlayer);
        }

        public override void UnregisterNetworkHandlers()
        {
            base.UnregisterNetworkHandlers();
            NetworkServer.UnregisterHandler(NetworkProtocol.RegisterPlayer);
        }

        protected virtual void OnRegisterPlayer(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<RegisterPlayerMessage>();

            // Create a new state for the registered player.
            var playerState = server.Players.Find(x => x.NetId == msg.NetId);
            if (playerState != null)
                return;

            playerState = new PlayerState();
            playerState.Id = server.Players.Count;
            playerState.ConnectionId = netMsg.conn.connectionId;
            playerState.IsConnected = true;
            playerState.NetId = msg.NetId;
            playerState.Name = msg.Name;
            playerState.IsHuman = msg.IsHuman;
            playerState.ActiveDeck = msg.ActiveDeck;
            foreach (var configZone in GameManager.Instance.Config.Zones)
            {
                GameZone zone = null;
                if (configZone is StaticGameZone)
                    zone = new StaticGameZone();
                else if (configZone is DynamicGameZone)
                    zone = new DynamicGameZone();
                zone.Name = configZone.Name;
                playerState.GameZones.Add(zone);
            }
            playerState.GetStaticGameZone("Deck").Cards.AddRange(msg.Deck);
            server.Players.Add(playerState);
            Logger.Log("Player with id " + playerState.Id + " has joined the game.");

            // Set the player attributes based on the generic player definition. Note we only
            // take into account the integer attributes, as they are the ones that we are
            // interested in from a gameplay perspective.
            var playerDefinition = GameManager.Instance.Config.PlayerDefinition;
            foreach (var playerAttribute in playerDefinition.Attributes)
            {
                if (playerAttribute is IntAttribute)
                    playerState.Attributes.Add(new PlayerAttribute(playerAttribute.Name, (playerAttribute as IntAttribute).Value));
            }

            // When the appropriate number of players is registered, the game can start.
            if (server.Players.Count == server.MaxPlayers)
                server.StartGame();
        }
    }
}
