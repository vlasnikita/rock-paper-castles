// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

using UnityEngine.Networking;

namespace CCGKit
{
    /// <summary>
    /// This server handler is responsible for managing client requests for playing new cards.
    /// </summary>
    public class CardPlayingHandler : ServerHandler
    {
        public CardPlayingHandler(Server server) : base(server)
        {
        }

        public override void RegisterNetworkHandlers()
        {
            base.RegisterNetworkHandlers();
            NetworkServer.RegisterHandler(NetworkProtocol.PlayCard, OnPlayCard);
        }

        public override void UnregisterNetworkHandlers()
        {
            base.UnregisterNetworkHandlers();
            NetworkServer.UnregisterHandler(NetworkProtocol.PlayCard);
        }

        protected virtual void OnPlayCard(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<PlayCardMessage>();

            // Ignore this request if the requesting player is not the current player.
            if (msg.NetId != server.CurrentPlayer.NetId)
                return;

            // Ignore this request if the cost of the card to play cannot be fulfilled by
            // the current player.
            var card = GameManager.Instance.GetCard(msg.CardId);
            var cardCost = card.GetIntegerAttribute("Cost");
            var availableMana = server.CurrentPlayer.GetAttribute("Mana").Value;
            if (cardCost > availableMana.Value)
                return;

            server.CurrentPlayer.SetAttribute("Mana", availableMana.Value - cardCost);

            // Remove the card from the player's hand.
            server.CurrentPlayer.GetStaticGameZone("Hand").Cards.Remove(card.Id);

            PlayCard(card.Id, msg.NetId);

            // Send the updated player state to all connected clients.
            server.SendPlayerStateToAllClients(server.CurrentPlayer);
        }

        public virtual void PlayCard(int cardId, NetworkInstanceId ownerNetId)
        {
            var card = GameManager.Instance.GetCard(cardId);

            // Spawn a new networked card with the appropriate information.
            var go = server.CreateNetworkCard(card);
            var networkCard = go.GetComponent<NetworkCard>();
            networkCard.OwnerNetId = ownerNetId;
            networkCard.CardId = card.Id;
            NetworkServer.Spawn(go);

            // Add the card to the player's board.
            server.CurrentPlayer.GetDynamicGameZone("Board").Cards.Add(networkCard.netId);
            server.CurrentPlayer.LastCardPlayedId = card.Id;

            // Trigger any 'on enter board' effects for the new card.
            server.OnCardEnteredZone(networkCard.netId, "Board");

            // Trigger any 'when this player plays a card' effects.
            var keys = new List<NetworkInstanceId>(NetworkServer.objects.Keys);
            foreach (var key in keys)
            {
                var netCard = NetworkServer.objects[key].gameObject.GetComponent<NetworkCard>();
                if (netCard != null && netCard.IsAlive)
                    server.OnPlayerPlayedCard(netCard.netId);
            }
        }

        public virtual void TransformCard(NetworkInstanceId netId, string cardName)
        {
            var cardOwnerNetId = NetworkingUtils.GetNetworkObject(netId).GetComponent<NetworkCard>().OwnerNetId;

            server.DestroyCard(netId);

            var card = GameManager.Instance.GetCard(cardName);
            // Spawn a new networked card with the appropriate information.
            var go = server.CreateNetworkCard(card);
            var networkCard = go.GetComponent<NetworkCard>();
            networkCard.OwnerNetId = cardOwnerNetId;
            networkCard.CardId = card.Id;
            NetworkServer.Spawn(go);
        }

        public virtual void KillCard(NetworkInstanceId netId)
        {
            // Trigger any 'on exit board' effects for the card.
            server.OnCardLeftZone(netId, "Board");

            var go = NetworkingUtils.GetNetworkObject(netId);
            if (go != null)
            {
                var card = go.GetComponent<NetworkCard>();
                if (card != null)
                {
                    var ownerPlayer = server.Players.Find(x => x.NetId == card.OwnerNetId);
                    ownerPlayer.GetDynamicGameZone("Board").Cards.Remove(card.netId);
                    ownerPlayer.GetStaticGameZone("Cemetery").Cards.Add(card.CardId);
                    server.SendPlayerStateToAllClients(ownerPlayer);
                }
            }
        }
    }
}
