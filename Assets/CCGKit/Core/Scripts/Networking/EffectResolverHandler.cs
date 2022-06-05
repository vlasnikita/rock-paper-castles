// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Networking;

namespace CCGKit
{
    /// <summary>
    /// This server handler is responsible for managing the application of a card's effect into its
    /// intended target, which can be a player or another card.
    /// </summary>
    public class EffectResolverHandler : ServerHandler
    {
        /// <summary>
        /// True if the server is currently waiting for the player to select an effect's player target; false
        /// otherwise.
        /// </summary>
        protected bool waitingForPlayerTargetSelection;

        /// <summary>
        /// True if the server is currently waiting for the player to select an effect's card target; false
        /// otherwise.
        /// </summary>
        protected bool waitingForCardTargetSelection;

        /// <summary>
        /// The effect currently waiting to be resolved (because the player needs to select a target for it), if any.
        /// </summary>
        protected Effect pendingEffect;

        /// <summary>
        /// The network identifiers of the effects pending to be resolved.
        /// </summary>
        protected Queue<NetworkInstanceId> effectQueue = new Queue<NetworkInstanceId>();

        /// <summary>
        /// The network identifier of the last effect that was queued.
        /// </summary>
        protected NetworkInstanceId lastEffectQueued;

        public EffectResolverHandler(Server server) : base(server)
        {
        }

        public override void RegisterNetworkHandlers()
        {
            base.RegisterNetworkHandlers();
            NetworkServer.RegisterHandler(NetworkProtocol.TargetPlayerSelected, OnTargetPlayerSelected);
            NetworkServer.RegisterHandler(NetworkProtocol.TargetCardSelected, OnTargetCardSelected);
        }

        public override void UnregisterNetworkHandlers()
        {
            base.UnregisterNetworkHandlers();
            NetworkServer.UnregisterHandler(NetworkProtocol.TargetCardSelected);
            NetworkServer.UnregisterHandler(NetworkProtocol.TargetPlayerSelected);
        }

        public override void OnStartTurn()
        {
            base.OnStartTurn();

            // Clear the internal state when starting a new turn. This effectively cancels any effect
            // currently in progress that did not finish in time.
            waitingForPlayerTargetSelection = false;
            waitingForCardTargetSelection = false;
            pendingEffect = null;
            effectQueue.Clear();
        }

        public override void OnEndTurn()
        {
            base.OnEndTurn();

            // Automatically destroy the pending effect (if any) when the turn ends.
            while (effectQueue.Count > 0)
            {
                var effect = effectQueue.Dequeue();
                server.DestroyCard(effect);
            }
        }

        public virtual void TriggerEffect(EffectTriggerType trigger, NetworkInstanceId netId)
        {
            var effectCard = NetworkingUtils.GetNetworkObject(netId);
            var card = GameManager.Instance.GetCard(effectCard.GetComponent<NetworkCard>().CardId);
            var cardDefinition = GameManager.Instance.Config.CardDefinitions.Find(x => x.Name == card.Definition);
            var effects = new List<Effect>(card.Effects);
            effects.AddRange(cardDefinition.Effects);
            foreach (var effect in effects)
            {
                if (effect.Trigger.Type == trigger)
                {
                    lastEffectQueued = netId;
                    if (!effectQueue.Contains(netId))
                        effectQueue.Enqueue(netId);
                    if (effect is PlayerEffect)
                        ResolvePlayerTargetedEffect(effect as PlayerEffect, effectCard.GetComponent<NetworkCard>());
                    else if (effect is CardEffect)
                        ResolveCardTargetedEffect(effect as CardEffect);
                    else if (effect is GeneralEffect)
                        ResolveGeneralEffect(effect as GeneralEffect, effectCard.GetComponent<NetworkCard>());
                }
            }
        }

        protected virtual void ResolvePlayerTargetedEffect(PlayerEffect effect, NetworkCard card)
        {
            // Effects that target a specific group of players can be resolved automatically by the server, but
            // effects that target a player to be chosen by the current player need to wait until that target is
            // selected before being resolved.
            var effectTarget = effect.Target;
            switch (effectTarget)
            {
                case PlayerEffectTargetType.TargetPlayer:
                    waitingForPlayerTargetSelection = true;
                    pendingEffect = effect;
                    var msg = new SelectTargetPlayerMessage();
                    msg.RecipientNetId = server.CurrentPlayer.NetId;
                    server.SafeSendToClient(server.CurrentPlayer, NetworkProtocol.SelectTargetPlayer, msg);
                    break;

                default:
                    var targetPlayers = GetEffectTargetPlayers(effectTarget, lastEffectQueued);
                    for (var i = 0; i < targetPlayers.Count; i++)
                    {
                        EffectResolver.ResolvePlayerTargetedEffect(effect, targetPlayers[i], server, card);
                        server.SendPlayerStateToAllClients(targetPlayers[i]);
                    }
                    RunPostEffectLogic();
                    break;
            }
        }

        protected List<PlayerState> GetEffectTargetPlayers(PlayerEffectTargetType targetType, NetworkInstanceId netId)
        {
            var obj = NetworkingUtils.GetNetworkObject(netId);
            var netCard = obj.GetComponent<NetworkCard>();
            if (targetType == PlayerEffectTargetType.CurrentPlayer)
            {
                return server.Players.FindAll(x => x.NetId == netCard.OwnerNetId);
            }
            else if (targetType == PlayerEffectTargetType.CurrentOpponent)
            {
                return server.Players.FindAll(x => x.NetId != netCard.OwnerNetId);
            }
            else if (targetType == PlayerEffectTargetType.AllPlayers)
            {
                return server.Players;
            }
            else if (targetType == PlayerEffectTargetType.RandomPlayer)
            {
                var players = new List<PlayerState>();
                var index = Random.Range(0, server.Players.Count);
                players.Add(server.Players[index]);
                return players;
            }
            return null;
        }

        protected virtual void ResolveCardTargetedEffect(CardEffect effect)
        {
            // Effects that target a specific group of cards can be resolved automatically by the server, but
            // effects that target a card to be choosed by the current player need to wait until that target is
            // selected before being resolved.
            var effectDefinition = GameManager.Instance.Config.EffectDefinitions.Find(x => x.Name == effect.Definition) as CardEffectDefinition;
            var effectTarget = effect.Target;
            switch (effectTarget)
            {
                case CardEffectTargetType.TargetCard:
                    {
                        waitingForCardTargetSelection = true;
                        pendingEffect = effect;
                        var msg = new SelectTargetCardMessage();
                        msg.RecipientNetId = server.CurrentPlayer.NetId;
                        server.SafeSendToClient(server.CurrentPlayer, NetworkProtocol.SelectTargetCard, msg);
                    }
                    break;

                case CardEffectTargetType.CurrentPlayerCard:
                    {
                        waitingForCardTargetSelection = true;
                        pendingEffect = effect;
                        var msg = new SelectTargetCardMessage();
                        msg.RecipientNetId = server.CurrentPlayer.NetId;
                        server.SafeSendToClient(server.CurrentPlayer, NetworkProtocol.SelectTargetCard, msg);
                    }
                    break;

                case CardEffectTargetType.CurrentOpponentCard:
                    {
                        waitingForCardTargetSelection = true;
                        pendingEffect = effect;
                        var msg = new SelectTargetCardMessage();
                        msg.RecipientNetId = server.CurrentPlayer.NetId;
                        server.SafeSendToClient(server.CurrentPlayer, NetworkProtocol.SelectTargetCard, msg);
                    }
                    break;

                case CardEffectTargetType.ThisCard:
                    {
                        var obj = NetworkingUtils.GetNetworkObject(lastEffectQueued);
                        var netCard = obj.GetComponent<NetworkCard>();
                        EffectResolver.ResolveCardTargetedEffect(effect, netCard, server);
                        RunPostEffectLogic();
                    }
                    break;

                default:
                    {
                        var targetCards = GetEffectTargetCards(effectTarget, effectDefinition, lastEffectQueued);
                        for (var i = 0; i < targetCards.Count; i++)
                            EffectResolver.ResolveCardTargetedEffect(effect, targetCards[i], server);
                        RunPostEffectLogic();
                    }
                    break;
            }
        }

        protected List<NetworkCard> GetEffectTargetCards(CardEffectTargetType targetType, CardEffectDefinition effectDefinition, NetworkInstanceId netId)
        {
            var obj = NetworkingUtils.GetNetworkObject(netId);
            var netCard = obj.GetComponent<NetworkCard>();

            if (targetType == CardEffectTargetType.RandomCard)
            {
                var index = Random.Range(0, server.Players.Count);
                var randomPlayer = server.Players[index];
                var playerCards = new List<NetworkCard>();
                playerCards.AddRange(GetPlayerCards(effectDefinition.Card, randomPlayer.NetId));
                index = Random.Range(0, playerCards.Count);
                var randomCard = playerCards[index];
                var cards = new List<NetworkCard>();
                cards.Add(randomCard);
                return cards;
            }
            else
            {
                var targetPlayers = new List<PlayerState>();
                if (targetType == CardEffectTargetType.AllCurrentPlayerCards)
                    targetPlayers = server.Players.FindAll(x => x.NetId == netCard.OwnerNetId);
                else if (targetType == CardEffectTargetType.AllCurrentOpponentCards)
                    targetPlayers = server.Players.FindAll(x => x.NetId != netCard.OwnerNetId);
                else if (targetType == CardEffectTargetType.AllCards)
                    targetPlayers = server.Players;

                var cards = new List<NetworkCard>();
                for (var i = 0; i < targetPlayers.Count; i++)
                    cards.AddRange(GetPlayerCards(effectDefinition.Card, targetPlayers[i].NetId));
                return cards;
            }
        }

        protected virtual void ResolveGeneralEffect(GeneralEffect effect, NetworkCard card)
        {
            EffectResolver.ResolveGeneralEffect(effect, card, server);
            RunPostEffectLogic();
        }

        protected virtual void OnTargetPlayerSelected(NetworkMessage netMsg)
        {
            if (!waitingForPlayerTargetSelection)
                return;

            // The player has selected a target player; proceed with resolving the pending effect.
            var msg = netMsg.ReadMessage<TargetPlayerSelectedMessage>();
            var targetPlayer = server.Players.Find(x => x.NetId == msg.NetId);
            if (targetPlayer != null)
            {
                var effectCard = NetworkingUtils.GetNetworkObject(lastEffectQueued);
                EffectResolver.ResolvePlayerTargetedEffect(pendingEffect as PlayerEffect, targetPlayer, server, effectCard.GetComponent<NetworkCard>());
                RunPostEffectLogic();
                server.SendPlayerStateToAllClients(targetPlayer);
                waitingForPlayerTargetSelection = false;
                pendingEffect = null;
            }
        }

        protected virtual void OnTargetCardSelected(NetworkMessage netMsg)
        {
            if (!waitingForCardTargetSelection)
                return;

            // The player has selected a target card; proceed with resolving the pending effect.
            var msg = netMsg.ReadMessage<TargetCardSelectedMessage>();
            var go = NetworkingUtils.GetNetworkObject(msg.NetId);
            if (go != null)
            {
                var card = go.GetComponent<NetworkCard>();
                if (card != null)
                {
                    var pendingCardEffect = pendingEffect as CardEffect;
                    var selectedTargetCard = GameManager.Instance.GetCard(card.CardId);
                    var pendingCardEffectDefinition = GameManager.Instance.Config.EffectDefinitions.Find(x => x.Name == pendingCardEffect.Definition) as CardEffectDefinition;
                    if (selectedTargetCard.Definition != pendingCardEffectDefinition.Card)
                    {
                        var selectTargetMsg = new SelectTargetCardMessage();
                        selectTargetMsg.RecipientNetId = server.CurrentPlayer.NetId;
                        server.SafeSendToClient(server.CurrentPlayer, NetworkProtocol.SelectTargetCard, selectTargetMsg);
                        return;
                    }

                    if ((pendingCardEffect.Target == CardEffectTargetType.CurrentOpponentCard && server.CurrentOpponents.Find(x => x.NetId == card.OwnerNetId) == null) ||
                        (pendingCardEffect.Target == CardEffectTargetType.CurrentPlayerCard && card.OwnerNetId != server.CurrentPlayer.NetId))
                    {
                        var selectTargetMsg = new SelectTargetCardMessage();
                        selectTargetMsg.RecipientNetId = server.CurrentPlayer.NetId;
                        server.SafeSendToClient(server.CurrentPlayer, NetworkProtocol.SelectTargetCard, selectTargetMsg);
                        return;
                    }

                    EffectResolver.ResolveCardTargetedEffect(pendingEffect as CardEffect, card, server);
                    RunPostEffectLogic();
                    waitingForPlayerTargetSelection = false;
                    pendingEffect = null;
                }
            }
        }

        /// <summary>
        /// Returns a list of all the networked cards that are owned by the player with the specified network identifier and
        /// are of the specified type.
        /// </summary>
        /// <param name="cardType">Card type of the cards to return.</param>
        /// <param name="playerNetId">Network identifier of the player that owns the cards to return.</param>
        /// <returns>A list of all the networked cards that are owned by the player with the specified network identifier and
        /// are of the specified type.</returns>
        protected List<NetworkCard> GetPlayerCards(string cardType, NetworkInstanceId playerNetId)
        {
            var playerCards = new List<NetworkCard>();
            var keys = new List<NetworkInstanceId>(NetworkServer.objects.Keys);
            foreach (var key in keys)
            {
                var netCard = NetworkServer.objects[key].gameObject.GetComponent<NetworkCard>();
                if (netCard != null && netCard.IsAlive && netCard.OwnerNetId == playerNetId)
                {
                    var card = GameManager.Instance.GetCard(netCard.CardId);
                    if (card.Definition == cardType)
                        playerCards.Add(netCard);
                }
            }
            return playerCards;
        }

        /// <summary>
        /// Executes any logic that needs to run after an effect is resolved.
        /// </summary>
        protected void RunPostEffectLogic()
        {
            // Destroy the card with the effect that was triggered if needed.
            while (effectQueue.Count > 0)
            {
                var effect = effectQueue.Dequeue();
                var obj = NetworkingUtils.GetNetworkObject(effect);
                if (obj != null)
                {
                    var netCard = obj.GetComponent<NetworkCard>();
                    if (netCard != null)
                    {
                        var card = GameManager.Instance.GetCard(netCard.CardId);
                        var cardDefinition = GameManager.Instance.Config.CardDefinitions.Find(x => x.Name == card.Definition);
                        if (cardDefinition.DestroyAfterTriggeringEffect)
                        {
                            server.DestroyCardWithDelay(effect, 1.0f);
                        }
                    }
                }
            }
        }
    }
}
