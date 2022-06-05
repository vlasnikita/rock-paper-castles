// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine.Networking;

namespace CCGKit
{
    /// <summary>
    /// This server handler is responsible for moving cards between game zones.
    /// </summary>
    public class MoveCardsHandler : ServerHandler
    {
        public MoveCardsHandler(Server server) : base(server)
        {
        }

        public override void RegisterNetworkHandlers()
        {
            base.RegisterNetworkHandlers();
            NetworkServer.RegisterHandler(NetworkProtocol.MoveCards, OnMoveCards);
        }

        public override void UnregisterNetworkHandlers()
        {
            base.UnregisterNetworkHandlers();
            NetworkServer.UnregisterHandler(NetworkProtocol.MoveCards);
        }

        public virtual void OnMoveCards(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<MoveCardsMessage>();

            var player = server.Players.Find(x => x.NetId == msg.RecipientNetId);
            MoveCards(player, msg.OriginZone, msg.DestinationZone, msg.NumCards);
        }

        public virtual void MoveCards(PlayerState player, string fromZoneName, string toZoneName, int numCards)
        {
            var fromZone = player.GetGameZone(fromZoneName);
            var toZone = player.GetGameZone(toZoneName);
            if (fromZone is StaticGameZone && toZone is StaticGameZone)
            {
                MoveCards(player, fromZone as StaticGameZone, toZone as StaticGameZone, numCards);
            }
            else if (fromZone is DynamicGameZone && toZone is DynamicGameZone)
            {
                MoveCards(player, fromZone as DynamicGameZone, toZone as DynamicGameZone, numCards);
            }
            else if (fromZone is StaticGameZone && toZone is DynamicGameZone)
            {
                MoveCards(player, fromZone as StaticGameZone, toZone as DynamicGameZone, numCards);
            }
            else if (fromZone is DynamicGameZone && toZone is StaticGameZone)
            {
                MoveCards(player, fromZone as DynamicGameZone, toZone as StaticGameZone, numCards);
            }
            server.SendPlayerStateToAllClients(player);
        }

        protected virtual void MoveCards(PlayerState player, StaticGameZone fromZone, StaticGameZone toZone, int numCards)
        {
            // Do not move more cards than those available in the origin zone.
            if (numCards > fromZone.Cards.Count)
                numCards = fromZone.Cards.Count;

            for (var i = 0; i < numCards; i++)
            {
                toZone.Cards.Add(fromZone.Cards[i]);
            }
            fromZone.Cards.RemoveRange(0, numCards);
        }

        protected virtual void MoveCards(PlayerState player, DynamicGameZone fromZone, DynamicGameZone toZone, int numCards)
        {
            // Do not move more cards than those available in the origin zone.
            if (numCards > fromZone.Cards.Count)
                numCards = fromZone.Cards.Count;

            for (var i = 0; i < numCards; i++)
            {
                toZone.Cards.Add(fromZone.Cards[i]);
            }
            fromZone.Cards.RemoveRange(0, numCards);
        }

        protected virtual void MoveCards(PlayerState player, StaticGameZone fromZone, DynamicGameZone toZone, int numCards)
        {
            // Do not move more cards than those available in the origin zone.
            if (numCards > fromZone.Cards.Count)
                numCards = fromZone.Cards.Count;

            for (var i = 0; i < numCards; i++)
            {
                MoveCard(player, fromZone, toZone, fromZone.Cards[i]);
            }
        }

        protected virtual void MoveCards(PlayerState player, DynamicGameZone fromZone, StaticGameZone toZone, int numCards)
        {
            // Do not move more cards than those available in the origin zone.
            if (numCards > fromZone.Cards.Count)
                numCards = fromZone.Cards.Count;

            for (var i = 0; i < numCards; i++)
            {
                MoveCard(player, fromZone, toZone, fromZone.Cards[i]);
            }
        }

        protected virtual void MoveCard(PlayerState player, StaticGameZone fromZone, StaticGameZone toZone, int cardId)
        {
            fromZone.Cards.Remove(cardId);
            toZone.Cards.Add(cardId);
        }

        protected virtual void MoveCard(PlayerState player, DynamicGameZone fromZone, DynamicGameZone toZone, NetworkInstanceId cardNetId)
        {
            fromZone.Cards.Remove(cardNetId);
            toZone.Cards.Add(cardNetId);

            server.OnCardLeftZone(cardNetId, fromZone.Name);
            server.OnCardEnteredZone(cardNetId, toZone.Name);
        }

        protected virtual void MoveCard(PlayerState player, StaticGameZone fromZone, DynamicGameZone toZone, int cardId)
        {
            var card = GameManager.Instance.GetCard(cardId);

            // Spawn a new networked card with the appropriate information.
            var go = server.CreateNetworkCard(card);
            var networkCard = go.GetComponent<NetworkCard>();
            networkCard.OwnerNetId = player.NetId;
            networkCard.CardId = card.Id;
            NetworkServer.Spawn(go);

            // Add the card to the destination zone.
            fromZone.Cards.Remove(cardId);
            toZone.Cards.Add(networkCard.netId);

            // Trigger any 'on enter zone' effects for the card.
            server.OnCardEnteredZone(networkCard.netId, toZone.Name);
        }

        protected virtual void MoveCard(PlayerState player, DynamicGameZone fromZone, StaticGameZone toZone, NetworkInstanceId cardNetId)
        {
            var card = NetworkingUtils.GetNetworkObject(cardNetId);

            // Add the card to the destination zone.
            fromZone.Cards.Remove(cardNetId);
            toZone.Cards.Add(card.GetComponent<NetworkCard>().CardId);

            server.OnCardLeftZone(cardNetId, fromZone.Name);
            NetworkServer.Destroy(card);
        }
    }
}
