// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Collections.Generic;

using UnityEngine.Networking;

namespace CCGKit
{
    public class CardsToDiscardHandler : ServerHandler
    {
        protected bool waitingForCardsToDiscardSelection;
        protected int numCardsToDiscard;
        protected PlayerState targetPlayer;
        protected Random rng = new Random();

        public CardsToDiscardHandler(Server server) : base(server)
        {
        }

        public override void RegisterNetworkHandlers()
        {
            base.RegisterNetworkHandlers();
            NetworkServer.RegisterHandler(NetworkProtocol.CardsToDiscardSelected, OnCardsToDiscardSelected);
        }

        public override void UnregisterNetworkHandlers()
        {
            base.UnregisterNetworkHandlers();
            NetworkServer.UnregisterHandler(NetworkProtocol.CardsToDiscardSelected);
        }

        public override void OnEndTurn()
        {
            base.OnEndTurn();

            if (waitingForCardsToDiscardSelection)
            {
                // Remove the appropriate number of cards from the player with an excess.
                var removedCards = new List<int>();
                var hand = targetPlayer.GetStaticGameZone("Hand").Cards;
                for (var i = 0; i < numCardsToDiscard; i++)
                {
                    var randomIndex = rng.Next(0, hand.Count - 1);
                    removedCards.Add(hand[randomIndex]);
                    hand.RemoveAt(randomIndex);
                }
                // Notify the player of which cards have been automatically discarded from his
                // hand so that he can update the UI appropriately.
                var msg = new CardsAutoDiscardedMessage();
                msg.RecipientNetId = targetPlayer.NetId;
                msg.Cards = removedCards.ToArray();
                server.SafeSendToClient(targetPlayer, NetworkProtocol.CardsAutoDiscarded, msg);
            }

            waitingForCardsToDiscardSelection = false;
            numCardsToDiscard = 0;
            targetPlayer = null;
        }

        public virtual void OnCardsToDiscardSelected(NetworkMessage netMsg)
        {
            if (!waitingForCardsToDiscardSelection)
                return;

            var msg = netMsg.ReadMessage<CardsToDiscardSelectedMessage>();
            var player = server.Players.Find(x => x.NetId == msg.SenderNetId);
            if (player != null)
            {
                var hand = player.GetStaticGameZone("Hand").Cards;
                for (var i = 0; i < msg.Cards.Length; i++)
                {
                    hand.Remove(msg.Cards[i]);
                }
                server.SendPlayerStateToAllClients(player);
            }
            waitingForCardsToDiscardSelection = false;
            numCardsToDiscard = 0;
            targetPlayer = null;
        }

        public void SetWaitingForCardsToDiscardSelection(PlayerState player, int numCards)
        {
            waitingForCardsToDiscardSelection = true;
            targetPlayer = player;
            numCardsToDiscard = numCards;
        }
    }
}
