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
    /// This class is responsible for handling the reception of custom network messages from the
    /// game server and routing them to the appropriate local player. Single-player mode is
    /// implemented via a second local player that uses the same system as normal human players
    /// in multiplayer modes; which is specially convenient implementation-wise (as there is no
    /// special case for it).
    /// </summary>
    public class GameNetworkClient : NetworkBehaviour
    {
        /// <summary>
        /// Cached reference to the local network client.
        /// </summary>
        protected NetworkClient client;

        /// <summary>
        /// List of all the local players connected to this client. Normally this will only contain
        /// the human local player for multiplayer games, but in the case of single-player games it
        /// will also contain the AI-controlled player.
        /// </summary>
        protected List<Player> localPlayers = new List<Player>();

        /// <summary>
        /// Unity's OnStartClient.
        /// </summary>
        public override void OnStartClient()
        {
            client = NetworkManager.singleton.client;
            RegisterNetworkHandlers();
        }

        /// <summary>
        /// Unity's OnDestroy.
        /// </summary>
        protected virtual void OnDestroy()
        {
            UnregisterNetworkHandlers();
        }

        /// <summary>
        /// Addds a new local player to this client.
        /// </summary>
        /// <param name="player">The local player to add to this client.</param>
        public void AddLocalPlayer(Player player)
        {
            localPlayers.Add(player);
        }

        /// <summary>
        /// Registers the network handlers for the network messages we are interested in handling.
        /// </summary>
        protected virtual void RegisterNetworkHandlers()
        {
            client.RegisterHandler(NetworkProtocol.StartGame, OnStartGame);
            client.RegisterHandler(NetworkProtocol.EndGame, OnEndGame);
            client.RegisterHandler(NetworkProtocol.StartTurn, OnStartTurn);
            client.RegisterHandler(NetworkProtocol.EndTurn, OnEndTurn);
            client.RegisterHandler(NetworkProtocol.UpdatePlayerAttributes, OnUpdatePlayerAttributes);
            client.RegisterHandler(NetworkProtocol.UpdateOpponentAttributes, OnUpdateOpponentAttributes);
            client.RegisterHandler(NetworkProtocol.SelectTargetPlayer, OnSelectTargetPlayer);
            client.RegisterHandler(NetworkProtocol.SelectTargetCard, OnSelectTargetCard);
            client.RegisterHandler(NetworkProtocol.DrawCards, OnDrawCards);
            client.RegisterHandler(NetworkProtocol.DiscardCards, OnDiscardCards);
            client.RegisterHandler(NetworkProtocol.CardsAutoDiscarded, OnCardsAutoDiscarded);
            client.RegisterHandler(NetworkProtocol.BroadcastChatTextMessage, OnReceiveChatText);
            client.RegisterHandler(NetworkProtocol.KilledCard, OnKilledCard);
        }

        /// <summary>
        /// Unregisters the network handlers for the network messages we are interested in handling.
        /// </summary>
        protected virtual void UnregisterNetworkHandlers()
        {
            if (client != null)
            {
                client.UnregisterHandler(NetworkProtocol.KilledCard);
                client.UnregisterHandler(NetworkProtocol.BroadcastChatTextMessage);
                client.UnregisterHandler(NetworkProtocol.CardsAutoDiscarded);
                client.UnregisterHandler(NetworkProtocol.DiscardCards);
                client.UnregisterHandler(NetworkProtocol.DrawCards);
                client.UnregisterHandler(NetworkProtocol.SelectTargetCard);
                client.UnregisterHandler(NetworkProtocol.SelectTargetPlayer);
                client.UnregisterHandler(NetworkProtocol.UpdateOpponentAttributes);
                client.UnregisterHandler(NetworkProtocol.UpdatePlayerAttributes);
                client.UnregisterHandler(NetworkProtocol.EndTurn);
                client.UnregisterHandler(NetworkProtocol.StartTurn);
                client.UnregisterHandler(NetworkProtocol.EndGame);
                client.UnregisterHandler(NetworkProtocol.StartGame);
            }
        }

        /// <summary>
        /// Called when the game starts.
        /// </summary>
        /// <param name="netMsg">Start game message.</param>
        protected void OnStartGame(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<StartGameMessage>();
            Assert.IsTrue(msg != null);
            var player = localPlayers.Find(x => x.netId == msg.RecipientNetId);
            if (player != null)
                player.OnStartGame(msg);
        }

        /// <summary>
        /// Called when the game ends.
        /// </summary>
        /// <param name="netMsg">End game message.</param>
        protected void OnEndGame(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<EndGameMessage>();
            Assert.IsTrue(msg != null);
            foreach (var player in localPlayers)
                player.OnEndGame(msg);
        }

        /// <summary>
        /// Called when a new turn starts.
        /// </summary>
        /// <param name="netMsg">Start turn message.</param>
        protected void OnStartTurn(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<StartTurnMessage>();
            Assert.IsTrue(msg != null);
            var player = localPlayers.Find(x => x.netId == msg.RecipientNetId);
            if (player != null)
                player.OnStartTurn(msg);
        }

        /// <summary>
        /// Called when a new turn ends.
        /// </summary>
        /// <param name="netMsg">End turn message.</param>
        protected void OnEndTurn(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<EndTurnMessage>();
            Assert.IsTrue(msg != null);
            var player = localPlayers.Find(x => x.netId == msg.RecipientNetId);
            if (player != null)
                player.OnEndTurn(msg);
        }

        /// <summary>
        /// Called when the active player attributes are updated.
        /// </summary>
        /// <param name="netMsg">Update player attributes message.</param>
        protected void OnUpdatePlayerAttributes(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<UpdatePlayerAttributesMessage>();
            Assert.IsTrue(msg != null);
            var player = localPlayers.Find(x => x.netId == msg.RecipientNetId);
            if (player != null)
                player.OnUpdatePlayerAttributes(msg);
        }

        /// <summary>
        /// Called when the current opponent attributes are updated.
        /// </summary>
        /// <param name="netMsg">Update opponent attributes message.</param>
        protected void OnUpdateOpponentAttributes(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<UpdateOpponentAttributesMessage>();
            Assert.IsTrue(msg != null);
            var player = localPlayers.Find(x => x.netId == msg.RecipientNetId);
            if (player != null)
                player.OnUpdateOpponentAttributes(msg);
        }

        /// <summary>
        /// Called when the active player needs to select a target player.
        /// </summary>
        /// <param name="netMsg">Select target player message.</param>
        protected void OnSelectTargetPlayer(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<SelectTargetPlayerMessage>();
            Assert.IsTrue(msg != null);
            var player = localPlayers.Find(x => x.netId == msg.RecipientNetId);
            if (player != null)
                player.OnSelectTargetPlayer(msg);
        }

        /// <summary>
        /// Called when the active player needs to select a target card.
        /// </summary>
        /// <param name="netMsg">Select target card message.</param>
        protected void OnSelectTargetCard(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<SelectTargetCardMessage>();
            Assert.IsTrue(msg != null);
            var player = localPlayers.Find(x => x.netId == msg.RecipientNetId);
            if (player != null)
                player.OnSelectTargetCard(msg);
        }

        /// <summary>
        /// Called when the active player needs to draw cards.
        /// </summary>
        /// <param name="netMsg">Draw cards message.</param>
        protected void OnDrawCards(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<DrawCardsMessage>();
            Assert.IsTrue(msg != null);
            var player = localPlayers.Find(x => x.netId == msg.RecipientNetId);
            if (player != null)
                player.OnDrawCards(msg);
        }

        /// <summary>
        /// Called when the active player needs to discard cards.
        /// </summary>
        /// <param name="netMsg">Discard cards message.</param>
        protected void OnDiscardCards(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<DiscardCardsMessage>();
            Assert.IsTrue(msg != null);
            var player = localPlayers.Find(x => x.netId == msg.RecipientNetId);
            if (player != null)
                player.OnDiscardCards(msg);
        }

        /// <summary>
        /// Called when the active player has had some cards automatically discarded
        /// by the server.
        /// </summary>
        /// <param name="netMsg">Cards automatically discarded message.</param>
        protected void OnCardsAutoDiscarded(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<CardsAutoDiscardedMessage>();
            Assert.IsTrue(msg != null);
            var player = localPlayers.Find(x => x.netId == msg.RecipientNetId);
            if (player != null)
                player.OnCardsAutoDiscarded(msg);
        }

        /// <summary>
        /// Called when the player receives a chat text message.
        /// </summary>
        /// <param name="netMsg">Chat text message.</param>
        protected void OnReceiveChatText(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<BroadcastChatTextMessage>();
            Assert.IsTrue(msg != null);
            foreach (var player in localPlayers)
                player.OnReceiveChatText(msg.Text);
        }

        /// <summary>
        /// Called when a card was killed.
        /// </summary>
        /// <param name="netMsg">Card killed message.</param>
        protected void OnKilledCard(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<KilledCardMessage>();
            Assert.IsTrue(msg != null);
            foreach (var player in localPlayers)
                player.OnKilledCard(msg);
        }
    }
}
