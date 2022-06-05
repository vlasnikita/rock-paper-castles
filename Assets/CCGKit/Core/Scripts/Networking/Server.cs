// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Networking;

namespace CCGKit
{
    /// <summary>
    /// The authoritative game server, which is responsible for driving the game data and logic in a
    /// multiplayer game. It provides the fundamental functionality needed in an online collectible card
    /// game, namely management of the turn sequence and application of card effects. The entire project
    /// structure revolves around the fact that the server is authoritative; in order to prevent hacking,
    /// clients are fundamentally limited to sending the player input to the server and updating the visual
    /// state of the game on screen while all the critical game logic is performed on the server side.
    ///
    /// The goal is to provide useful default behavior that can be applied to a wide spectrum of games while
    /// also allowing further specialization via subclassing.
    /// </summary>
    public class Server : NetworkBehaviour
    {
        /// <summary>
        /// The number of players in a game.
        /// </summary>
        public int MaxPlayers { get; private set; }

        /// <summary>
        /// The duration of a turn in a game (in seconds).
        /// </summary>
        public int TurnDuration { get; private set; }

        /// <summary>
        /// The maximum number of cards that can be in a hand.
        /// </summary>
        public int MaxHandSize { get; private set; }

        /// <summary>
        /// List of players connected to the game.
        /// </summary>
        public List<PlayerState> Players = new List<PlayerState>();

        /// <summary>
        /// Index of the current player in the list of players.
        /// </summary>
        protected int currentPlayerIndex;

        /// <summary>
        /// Current player (i.e., the one that is on his turn).
        /// </summary>
        public PlayerState CurrentPlayer { get; private set; }

        /// <summary>
        /// List of current opponents (i.e., the players that are not on their turn).
        /// </summary>
        public List<PlayerState> CurrentOpponents = new List<PlayerState>();

        /// <summary>
        /// List of server handler classes.
        /// </summary>
        protected List<ServerHandler> handlers = new List<ServerHandler>();

        /// <summary>
        /// Current turn.
        /// </summary>
        protected int currentTurn;

        /// <summary>
        /// True if the game has finished; false otherwise.
        /// </summary>
        protected bool gameFinished;

        /// <summary>
        /// Cached reference to the currently-executing turn coroutine.
        /// </summary>
        protected Coroutine turnCoroutine;

        /// <summary>
        /// We do not want to destroy network cards immediately because we usually want them to
        /// have some kind of destruction visual effect on the clients, so we schedule their
        /// actual destruction for a number of turns via this helper class.
        /// </summary>
        protected class CardDestructionCountdown
        {
            public NetworkInstanceId NetId;
            public int TurnsLeft;
        }

        /// <summary>
        /// List of cards queued to be destroyed.
        /// </summary>
        protected List<CardDestructionCountdown> cardsToDestroy = new List<CardDestructionCountdown>();

        /// <summary>
        /// Called when the server starts listening.
        /// </summary>
        public override void OnStartServer()
        {
            base.OnStartServer();

            LoadGameConfiguration();
            AddServerHandlers();
            RegisterServerHandlers();
        }

        /// <summary>
        /// Loads the game configuration.
        /// </summary>
        protected virtual void LoadGameConfiguration()
        {
            var gameConfig = GameManager.Instance.Config;
            MaxPlayers = gameConfig.Properties.NumPlayers;
            TurnDuration = gameConfig.Properties.TurnDuration;
            MaxHandSize = gameConfig.Properties.MaxHandSize;
        }

        /// <summary>
        /// Adds the server handlers that are actually responsible of implementing the server's logic.
        /// </summary>
        protected virtual void AddServerHandlers()
        {
            handlers.Add(new PlayerRegistrationHandler(this));
            handlers.Add(new TurnSequenceHandler(this));
            handlers.Add(new EffectResolverHandler(this));
            handlers.Add(new CardsToDiscardHandler(this));
            handlers.Add(new MoveCardsHandler(this));
            handlers.Add(new ChatHandler(this));
        }

        /// <summary>
        /// Registers the network handlers for the messages the server is interested in listening to.
        /// </summary>
        protected virtual void RegisterServerHandlers()
        {
            for (var i = 0; i < handlers.Count; i++)
                handlers[i].RegisterNetworkHandlers();
        }

        /// <summary>
        /// Unregisters the network handlers for the messages the server is interested in listening to.
        /// </summary>
        protected virtual void UnregisterServerHandlers()
        {
            for (var i = 0; i < handlers.Count; i++)
                handlers[i].UnregisterNetworkHandlers();
            handlers.Clear();
        }

        /// <summary>
        /// This function is called when the NetworkBehaviour will be destroyed.
        /// </summary>
        protected virtual void OnDestroy()
        {
            UnregisterServerHandlers();
        }

        /// <summary>
        /// Starts the multiplayer game. This is automatically called when the appropriate number of players
        /// have joined a room.
        /// </summary>
        public virtual void StartGame()
        {
            // Start with turn 1.
            currentTurn = 1;

            // Create an array with all the player names.
            var playerNames = new string[Players.Count];
            for (var i = 0; i < playerNames.Length; i++){
                playerNames[i] = Players[i].Name;
            }


            // Set the current player and opponents.
            CurrentPlayer = Players[currentPlayerIndex];
            CurrentOpponents.Clear();
            for (var i = 0; i < Players.Count; i++)
            {
                if (i != currentPlayerIndex)
                    CurrentOpponents.Add(Players[i]);
            }

            // Execute the game start actions.
            foreach (var action in GameManager.Instance.Config.Properties.GameStartActions)
            {
                ExecuteGameAction(action);
            }

            // Send a StartGame message to all the connected players.
            for (var i = 0; i < Players.Count; i++)
            {
                var player = Players[i];
                var msg = new StartGameMessage();
                msg.RecipientNetId = player.NetId;
                msg.PlayerIndex = i;
                msg.PlayerNames = playerNames;
                msg.TurnDuration = TurnDuration;
                msg.StaticGameZones = GetNetStaticGameZones(player, false);
                msg.DynamicGameZones = GetNetDynamicGameZones(player, false);
                SafeSendToClient(player, NetworkProtocol.StartGame, msg);

                SendPlayerStateToAllClients(player);
            }

            // Start running the turn sequence coroutine.
            turnCoroutine = StartCoroutine(RunTurn());
        }

        /// <summary>
        /// Deals the specified number of cards to the specified player.
        /// </summary>
        /// <param name="player">Player to deal the cards to.</param>
        /// <param name="numCards">Number of cards to deal to the player.</param>
        /// <returns></returns>
        protected virtual List<int> DealCardsToPlayer(PlayerState player, int numCards)
        {
            var deck = player.GetStaticGameZone("Deck").Cards;
            var hand = player.GetStaticGameZone("Hand").Cards;

            // Avoid trying to deal more cards than those available in the deck.
            if (numCards > deck.Count)
                numCards = deck.Count;

            var dealtCards = new List<int>(numCards);
            for (var i = 0; i < numCards; i++)
            {
                // Add the dealt cards to the player's hand.
                hand.Add(deck[i]);
                dealtCards.Add(deck[i]);
            }
            // Remove the dealt cards from the player's deck.
            deck.RemoveRange(0, numCards);

            return dealtCards;
        }

        /// <summary>
        /// Sends the most up-to-date state of the specified player to all the connected players.
        /// </summary>
        /// <param name="player">Player state to send.</param>
        public virtual void SendPlayerStateToAllClients(PlayerState player)
        {
            // Send the complete information to the owner client of the specified player.
            var playerMsg = new UpdatePlayerAttributesMessage();
            playerMsg.RecipientNetId = player.NetId;
            playerMsg.NetId = player.NetId;
            playerMsg.Names = new string[player.Attributes.Count];
            playerMsg.Values = new int[player.Attributes.Count];
            for (var i = 0; i < player.Attributes.Count; i++)
            {
                var attribute = player.Attributes[i];
                playerMsg.Names[i] = attribute.Name;
                playerMsg.Values[i] = attribute.Value;
            }
            playerMsg.StaticGameZones = GetNetStaticGameZones(player, false);
            playerMsg.DynamicGameZones = GetNetDynamicGameZones(player, false);
            SafeSendToClient(player, NetworkProtocol.UpdatePlayerAttributes, playerMsg);

            // Send the minimum needed information to the opponent clients (to avoid hacking).
            var opponentMsg = new UpdateOpponentAttributesMessage();
            opponentMsg.NetId = player.NetId;
            opponentMsg.Names = new string[player.Attributes.Count];
            opponentMsg.Values = new int[player.Attributes.Count];
            for (var i = 0; i < player.Attributes.Count; i++)
            {
                var attribute = player.Attributes[i];
                opponentMsg.Names[i] = attribute.Name;
                opponentMsg.Values[i] = attribute.Value;
            }
            opponentMsg.StaticGameZones = GetNetStaticGameZones(player, true);
            opponentMsg.DynamicGameZones = GetNetDynamicGameZones(player, true);
            for (var i = 0; i < Players.Count; i++)
            {
                opponentMsg.RecipientNetId = Players[i].NetId;
                if (Players[i] != player)
                    SafeSendToClient(Players[i], NetworkProtocol.UpdateOpponentAttributes, opponentMsg);
            }
        }

        /// <summary>
        /// Ends the current game.
        /// </summary>
        public virtual void EndGame()
        {
            gameFinished = true;
        }

        /// <summary>
        /// Runs the coroutine that authoritatively drives the turn sequence.
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator RunTurn()
        {
            while (!gameFinished)
            {
                StartTurn();
                yield return new WaitForSeconds(TurnDuration);
                EndTurn();
            }
        }

        /// <summary>
        /// Starts a new game turn.
        /// </summary>
        protected virtual void StartTurn()
        {
            // Update the current player and opponents.
            CurrentPlayer = Players[currentPlayerIndex];
            CurrentOpponents.Clear();
            for (var i = 0; i < Players.Count; i++)
            {
                if (i != currentPlayerIndex)
                    CurrentOpponents.Add(Players[i]);
            }

            // try{
            //     var opponentBoardCards = CurrentOpponents[0].GetDynamicGameZone("Board").Cards;
            //     var canAttack = true;
            //     if(opponentBoardCards.Count > 0){
            //         for (var i = 0; i < opponentBoardCards.Count; i++){
            //             var netCard = NetworkingUtils.GetNetworkObject(opponentBoardCards[i]).GetComponent<NetworkCard>();
            //             var card = GameManager.Instance.GetCard(netCard.CardId);
            //             Debug.Log(card.Name);
            //             for(var j = 0; j < card.Effects.Count; j++){
            //                 // var cardEffectDefinition = GameManager.Instance.Config.EffectDefinitions.Find(x => x.Name == card.Effects[j].Definition);
            //                 if(card.Effects[j].Definition == "Taunt") canAttack = false;
            //             }
            //         } 
            //     }   
            //     Debug.LogWarning("CAN ATTACK? " + canAttack.ToString()); 
            // } 
            // catch (Exception e) {
            //     Debug.Log(e);
            // }

            // Execute the turn start actions.
            foreach (var action in GameManager.Instance.Config.Properties.TurnStartActions)
            {
                ExecuteGameAction(action);
            }

            // Run any code that needs to be executed at turn start time.
            PerformTurnStartStateInitialization();

            // Let the server handlers know the turn has started.
            for (var i = 0; i < handlers.Count; i++)
                handlers[i].OnStartTurn();

            // Let the networked cards know the turn has started.
            var keys = new List<NetworkInstanceId>(NetworkServer.objects.Keys);
            foreach (var key in keys)
            {
                var netCard = NetworkServer.objects[key].gameObject.GetComponent<NetworkCard>();
                if (netCard != null && netCard.IsAlive)
                    netCard.OnStartTurn(Players[currentPlayerIndex].NetId);
            }

            // Send the StartTurn message to all players.
            foreach (var player in Players)
            {
                var msg = new StartTurnMessage();
                msg.RecipientNetId = player.NetId;
                msg.IsRecipientTheActivePlayer = player == CurrentPlayer;
                msg.Turn = currentTurn;
                msg.StaticGameZones = GetNetStaticGameZones(player, false);
                msg.DynamicGameZones = GetNetDynamicGameZones(player, false);

                // Include the most up-to-date player state.
                msg.AttributeNames = new string[player.Attributes.Count];
                msg.AttributeValues = new int[player.Attributes.Count];
                for (var i = 0; i < player.Attributes.Count; i++)
                {
                    var attribute = player.Attributes[i];
                    msg.AttributeNames[i] = attribute.Name;
                    msg.AttributeValues[i] = attribute.Value;
                }
                SafeSendToClient(player, NetworkProtocol.StartTurn, msg);
                SendPlayerStateToAllClients(player);
            }
        }

        /// <summary>
        /// This method can be used by subclasses to perform turn-start-specific initialization logic.
        /// </summary>
        protected virtual void PerformTurnStartStateInitialization()
        {
        }

        /// <summary>
        /// Ends the current game turn.
        /// </summary>
        protected virtual void EndTurn()
        {
            Debug.Log("End turn for player " + currentPlayerIndex + ".");
            Logger.Log("End turn for player " + currentPlayerIndex + ".");

            // Perform actual destruction of cards queued to be destroyed.
            var removalHelper = new List<CardDestructionCountdown>();
            foreach (var cardInfo in cardsToDestroy)
            {
                cardInfo.TurnsLeft -= 1;
                if (cardInfo.TurnsLeft <= 0)
                {
                    var go = NetworkingUtils.GetNetworkObject(cardInfo.NetId);
                    if (go != null)
                        NetworkServer.Destroy(go);
                    removalHelper.Add(cardInfo);
                }
            }
            foreach (var cardInfo in removalHelper)
                cardsToDestroy.Remove(cardInfo);

            // Check if the current player needs to discard any card from his hand before the end of the turn.
            var hand = CurrentPlayer.GetStaticGameZone("Hand").Cards;
            if (hand.Count > MaxHandSize)
                DiscardCards(CurrentPlayer, hand.Count - MaxHandSize, true);

            CurrentPlayer.LastCardPlayedId = -1;

            // Let the server handlers know the turn has ended.
            for (var i = 0; i < handlers.Count; i++)
                handlers[i].OnEndTurn();

            // Let the networked cards know the turn has ended.
            var keys = new List<NetworkInstanceId>(NetworkServer.objects.Keys);
            foreach (var key in keys)
            {
                var netCard = NetworkServer.objects[key].gameObject.GetComponent<NetworkCard>();
                if (netCard != null && netCard.IsAlive)
                    netCard.OnEndTurn(Players[currentPlayerIndex].NetId);
            }

            // Send the EndTurn message to all players.
            foreach (var player in Players)
            {
                var msg = new EndTurnMessage();
                msg.RecipientNetId = player.NetId;
                msg.IsRecipientTheActivePlayer = player == CurrentPlayer;
                SafeSendToClient(player, NetworkProtocol.EndTurn, msg);
            }

            // Switch to next player.
            currentPlayerIndex += 1;
            if (currentPlayerIndex == Players.Count)
            {
                currentPlayerIndex = 0;
                // Increase turn count.
                currentTurn += 1;
            }
        }

        /// <summary>
        /// Stops the current turn.
        /// </summary>
        public virtual void StopTurn()
        {
            // Check if the current player needs to discard any card from his hand before the end of the turn.
            var hand = CurrentPlayer.GetStaticGameZone("Hand").Cards;
            if (hand.Count > MaxHandSize)
            {
                DiscardCards(CurrentPlayer, hand.Count - MaxHandSize, true);
            }
            else
            {
                if (turnCoroutine != null)
                    StopCoroutine(turnCoroutine);
                EndTurn();
                turnCoroutine = StartCoroutine(RunTurn());
            }
        }

        /// <summary>
        /// Creates a new networked card. This method is intended to be overriden by child classes.
        /// </summary>
        /// <param name="card">The card to create.</param>
        /// <returns>A new networked card.</returns>
        public virtual GameObject CreateNetworkCard(Card card)
        {
            return null;
        }

        /// <summary>
        /// Plays the card with the specified unique identifier.
        /// </summary>
        /// <param name="cardId">Unique identifier of the card to play.</param>
        /// <param name="ownerNetId">Network identifier of the player that owns the card to play.</param>
        public virtual void PlayCard(int cardId, NetworkInstanceId ownerNetId)
        {
            var handler = handlers.Find(x => x is CardPlayingHandler);
            if (handler != null)
                (handler as CardPlayingHandler).PlayCard(cardId, ownerNetId);
        }

        /// <summary>
        /// Destroys the networked card with the specified network identifier.
        /// </summary>
        /// <param name="netId">The network identifier of the card to destroy.</param>
        public void DestroyCard(NetworkInstanceId netId)
        {
            var card = NetworkingUtils.GetNetworkObject(netId);

            // Prevent the card from being destroyed multiple times.
            if (cardsToDestroy.Find(x => x.NetId == netId) != null)
                return;

            card.GetComponent<NetworkCard>().Kill();

            var handler = handlers.Find(x => x is CardPlayingHandler);
            if (handler != null)
                (handler as CardPlayingHandler).KillCard(netId);

            var destructionInfo = new CardDestructionCountdown();
            destructionInfo.NetId = netId;
            destructionInfo.TurnsLeft = 3;
            cardsToDestroy.Add(destructionInfo);

            var msg = new KilledCardMessage();
            msg.NetId = netId;
            NetworkServer.SendToAll(NetworkProtocol.KilledCard, msg);

            // Trigger any 'on exit board' effects for the card.
            // Make sure to only do that if the card is not marked as to be destroyed, in order to avoid
            // entering an infinite loop.
            if (cardsToDestroy.Find(x => x.NetId == netId) == null)
                OnCardLeftZone(netId, "Board");
        }

        /// <summary>
        /// Destroys the networked card with the specified network identifier after the specified number
        /// of seconds has elapsed.
        /// </summary>
        /// <param name="netId">The network identifier of the card to destroy.</param>
        /// <param name="delay">The delay in seconds after which to perform the destruction.</param>
        public void DestroyCardWithDelay(NetworkInstanceId netId, float delay)
        {
            StartCoroutine(DestroyCard(netId, delay));
        }

        /// <summary>
        /// Internal method to perform the delayed destruction of a networked card.
        /// </summary>
        /// <param name="netId">The network identifier of the card to destroy.</param>
        /// <param name="delay">The delay in seconds after which to perform the destruction.</param>
        /// <returns>The coroutine for destroying the card.</returns>
        private IEnumerator DestroyCard(NetworkInstanceId netId, float delay)
        {
            yield return new WaitForSeconds(delay);
            DestroyCard(netId);
        }

        /// <summary>
        /// Transforms the card with the specified network identifier into the card with the specified
        /// card name.
        /// </summary>
        /// <param name="netId">Network identifier of the card to transform.</param>
        /// <param name="cardName">Name of the card to which transform the specified card.</param>
        public void TransformCard(NetworkInstanceId netId, string cardName)
        {
            var handler = handlers.Find(x => x is CardPlayingHandler);
            if (handler != null)
                (handler as CardPlayingHandler).TransformCard(netId, cardName);
        }

        /// <summary>
        /// Draws the specified number of cards for the specified player.
        /// </summary>
        /// <param name="player">Player for which to draw new cards.</param>
        /// <param name="numCards">Number of cards to draw.</param>
        public void DrawCards(PlayerState player, int numCards)
        {
            // A player cannot draw more cards than those available in his deck.
            var deck = player.GetStaticGameZone("Deck").Cards;
            if (numCards > deck.Count)
                numCards = deck.Count;

            var cards = DealCardsToPlayer(player, numCards);
            var msg = new DrawCardsMessage();
            msg.RecipientNetId = player.NetId;
            msg.Cards = cards.ToArray();
            SafeSendToClient(player, NetworkProtocol.DrawCards, msg);
        }

        /// <summary>
        /// Discards the specified number of cards for the specified player.
        /// </summary>
        /// <param name="player">Player for which to discard cards.</param>
        /// <param name="numCards">Number of cards to discard.</param>
        /// <param name="isEOTDiscard">True if this is an end of turn discard; false otherwise.</param>
        public void DiscardCards(PlayerState player, int numCards, bool isEOTDiscard)
        {
            // A player cannot discard more cards than those available in his hand.
            var hand = player.GetStaticGameZone("Hand").Cards;
            if (numCards > hand.Count)
                numCards = hand.Count;

            var handler = handlers.Find(x => x is CardsToDiscardHandler);
            if (handler != null)
                (handler as CardsToDiscardHandler).SetWaitingForCardsToDiscardSelection(player, numCards);

            var msg = new DiscardCardsMessage();
            msg.RecipientNetId = player.NetId;
            msg.NumCards = numCards;
            msg.IsEOTDiscard = isEOTDiscard;
            SafeSendToClient(player, NetworkProtocol.DiscardCards, msg);
        }

        /// <summary>
        /// Moves the specified amount of cards from the specified origin zone to the specified
        /// destination zone.
        /// </summary>
        /// <param name="player">Player for which to move the cards.</param>
        /// <param name="fromZone">Origin zone.</param>
        /// <param name="toZone">Destination zone.</param>
        /// <param name="numCards">Number of cards to move.</param>
        public void MoveCards(PlayerState player, string fromZone, string toZone, int numCards)
        {
            var handler = handlers.Find(x => x is MoveCardsHandler);
            if (handler != null)
                (handler as MoveCardsHandler).MoveCards(player, fromZone, toZone, numCards);
        }

        /// <summary>
        /// Called when a player with the specified connection identifier connects to the server.
        /// </summary>
        /// <param name="connectionId">The player's connection identifier.</param>
        public virtual void OnPlayerConnected(int connectionId)
        {
            Debug.Log("Player with id " + connectionId + " connected to server.");
            Logger.Log("Player with id " + connectionId + " connected to server.");
            var player = Players.Find(x => x.ConnectionId == connectionId);
            if (player != null)
                player.IsConnected = true;
        }

        /// <summary>
        /// Called when a player with the specified connection identifier disconnects from the server.
        /// </summary>
        /// <param name="connectionId">The player's connection identifier.</param>
        public virtual void OnPlayerDisconnected(int connectionId)
        {
            Logger.Log("Player with id " + connectionId + " disconnected from server.");
            var player = Players.Find(x => x.ConnectionId == connectionId);
            if (player != null)
                player.IsConnected = false;
        }

        /// <summary>
        /// Utility method that sends a the specified message to the client connection owning the specified player
        /// only if that client is currently connected to the server. Trying to send a message to a disconnected
        /// client triggers an error on UNET.
        /// </summary>
        /// <param name="player">The destination player.</param>
        /// <param name="msgType">The type of the message to send.</param>
        /// <param name="msg">The actual message to send.</param>
        public virtual void SafeSendToClient(PlayerState player, short msgType, MessageBase msg)
        {
            if (player != null && player.IsConnected)
                NetworkServer.SendToClient(player.ConnectionId, msgType, msg);
        }

        /// <summary>
        /// Called when a player plays a card.
        /// </summary>
        /// <param name="netId">Network identifier of the card with the triggered effect.</param>
        public virtual void OnPlayerPlayedCard(NetworkInstanceId netId)
        {
            var handler = handlers.Find(x => x is EffectResolverHandler);
            if (handler != null)
                (handler as EffectResolverHandler).TriggerEffect(EffectTriggerType.WhenPlayerPlaysACard, netId);
        }

        /// <summary>
        /// Called when the attribute of a player is increased.
        /// </summary>
        /// <param name="netId">Network identifier of the card with the triggered effect.</param>
        public virtual void OnPlayerIncreasedAttribute(NetworkInstanceId netId)
        {
            var handler = handlers.Find(x => x is EffectResolverHandler);
            if (handler != null)
                (handler as EffectResolverHandler).TriggerEffect(EffectTriggerType.WhenPlayerAttributeIncreases, netId);
        }

        /// <summary>
        /// Called when the attribute of a player is increased.
        /// </summary>
        /// <param name="netId">Network identifier of the card with the triggered effect.</param>
        public virtual void OnPlayerDecreasedAttribute(NetworkInstanceId netId)
        {
            var handler = handlers.Find(x => x is EffectResolverHandler);
            if (handler != null)
                (handler as EffectResolverHandler).TriggerEffect(EffectTriggerType.WhenPlayerAttributeDecreases, netId);
        }

        /// <summary>
        /// Called when a new card enters the specified zone.
        /// </summary>
        /// <param name="netId">Network identifier of the card that entered the zone.</param>
        public virtual void OnCardEnteredZone(NetworkInstanceId netId, string zone)
        {
            Debug.Log("Card entered zone: " + zone + " and net_id: " + netId);
            var netCard = NetworkingUtils.GetNetworkObject(netId).GetComponent<NetworkCard>();
            var card = GameManager.Instance.GetCard(netCard.CardId);
            var cardDefinition = GameManager.Instance.Config.CardDefinitions.Find(x => x.Name == card.Definition);
            var definitionEffects = cardDefinition.Effects.FindAll(x => x.Trigger.Type == EffectTriggerType.WhenCardEntersZone);
            definitionEffects = definitionEffects.FindAll(x => (x.Trigger as CardEnteredZoneTrigger).Zone == zone);

            var cardEffects = card.Effects.FindAll(x => x.Trigger.Type == EffectTriggerType.WhenCardEntersZone);
            cardEffects = cardEffects.FindAll(x => (x.Trigger as CardEnteredZoneTrigger).Zone == zone);

            definitionEffects.AddRange(cardEffects);
            if (definitionEffects.Count > 0)
            {
                var handler = handlers.Find(x => x is EffectResolverHandler);
                if (handler != null)
                    (handler as EffectResolverHandler).TriggerEffect(EffectTriggerType.WhenCardEntersZone, netId);
            }
        }

        /// <summary>
        /// Called when a new card exits the specified zone.
        /// </summary>
        /// <param name="netId">Network identifier of the card that exited the zone.</param>
        public virtual void OnCardLeftZone(NetworkInstanceId netId, string zone)
        {
            Debug.Log("Card left zone: " + zone + " and net_id: " + netId);
            var netCard = NetworkingUtils.GetNetworkObject(netId).GetComponent<NetworkCard>();
            var card = GameManager.Instance.GetCard(netCard.CardId);
            var cardDefinition = GameManager.Instance.Config.CardDefinitions.Find(x => x.Name == card.Definition);
            var definitionEffects = cardDefinition.Effects.FindAll(x => x.Trigger.Type == EffectTriggerType.WhenCardLeavesZone);
            definitionEffects = definitionEffects.FindAll(x => (x.Trigger as CardLeftZoneTrigger).Zone == zone);

            var cardEffects = card.Effects.FindAll(x => x.Trigger.Type == EffectTriggerType.WhenCardLeavesZone);
            cardEffects = cardEffects.FindAll(x => (x.Trigger as CardLeftZoneTrigger).Zone == zone);

            definitionEffects.AddRange(cardEffects);
            if (definitionEffects.Count > 0)
            {
                var handler = handlers.Find(x => x is EffectResolverHandler);
                if (handler != null)
                    (handler as EffectResolverHandler).TriggerEffect(EffectTriggerType.WhenCardLeavesZone, netId);
            }
        }

        /// <summary>
        /// Called when the attribute of a card is increased.
        /// </summary>
        /// <param name="netId">Network identifier of the card whose attribute was increased.</param>
        public virtual void OnCardIncreasedAttribute(NetworkInstanceId netId)
        {
            var handler = handlers.Find(x => x is EffectResolverHandler);
            if (handler != null)
                (handler as EffectResolverHandler).TriggerEffect(EffectTriggerType.WhenCardAttributeIncreases, netId);
        }

        /// <summary>
        /// Called when the attribute of a card changes and is less than a certain value.
        /// </summary>
        /// <param name="netId">Network identifier of the card whose attribute was changed.</param>
        public virtual void OnCardAttributeIsLessThanValue(NetworkInstanceId netId)
        {
            var handler = handlers.Find(x => x is EffectResolverHandler);
            if (handler != null)
                (handler as EffectResolverHandler).TriggerEffect(EffectTriggerType.WhenCardAttributeIsLessThan, netId);
        }

        /// <summary>
        /// Called when the attribute of a card changes and is less than or equal to a certain value.
        /// </summary>
        /// <param name="netId">Network identifier of the card whose attribute was changed.</param>
        public virtual void OnCardAttributeIsLessThanOrEqualToValue(NetworkInstanceId netId)
        {
            var handler = handlers.Find(x => x is EffectResolverHandler);
            if (handler != null)
                (handler as EffectResolverHandler).TriggerEffect(EffectTriggerType.WhenCardAttributeIsLessThanOrEqualTo, netId);
        }

        /// <summary>
        /// Called when the attribute of a card changes and is equal to a certain value.
        /// </summary>
        /// <param name="netId">Network identifier of the card whose attribute was changed.</param>
        public virtual void OnCardAttributeIsEqualToValue(NetworkInstanceId netId)
        {
            var handler = handlers.Find(x => x is EffectResolverHandler);
            if (handler != null)
                (handler as EffectResolverHandler).TriggerEffect(EffectTriggerType.WhenCardAttributeIsEqualTo, netId);
        }

        /// <summary>
        /// Called when the attribute of a card changes and is greater than or equal to a certain value.
        /// </summary>
        /// <param name="netId">Network identifier of the card whose attribute was changed.</param>
        public virtual void OnCardAttributeIsGreaterThanOrEqualToValue(NetworkInstanceId netId)
        {
            var handler = handlers.Find(x => x is EffectResolverHandler);
            if (handler != null)
                (handler as EffectResolverHandler).TriggerEffect(EffectTriggerType.WhenCardAttributeIsGreaterThanOrEqualTo, netId);
        }

        /// <summary>
        /// Called when the attribute of a card changes and is greater than a certain value.
        /// </summary>
        /// <param name="netId">Network identifier of the card whose attribute was changed.</param>
        public virtual void OnCardAttributeIsGreaterThanValue(NetworkInstanceId netId)
        {
            var handler = handlers.Find(x => x is EffectResolverHandler);
            if (handler != null)
                (handler as EffectResolverHandler).TriggerEffect(EffectTriggerType.WhenCardAttributeIsGreaterThan, netId);
        }

        /// <summary>
        /// Called when the attribute of a card is decreased.
        /// </summary>
        /// <param name="netId">Network identifier of the card whose attribute was decreased.</param>
        public virtual void OnCardDecreasedAttribute(NetworkInstanceId netId)
        {
            var handler = handlers.Find(x => x is EffectResolverHandler);
            if (handler != null)
                (handler as EffectResolverHandler).TriggerEffect(EffectTriggerType.WhenCardAttributeDecreases, netId);
        }

        /// <summary>
        /// Called when specified card attacks.
        /// </summary>
        /// <param name="netId">Network identifier of the card that attacked.</param>
        public virtual void OnCardAttacked(NetworkInstanceId netId)
        {
            var handler = handlers.Find(x => x is EffectResolverHandler);
            if (handler != null)
                (handler as EffectResolverHandler).TriggerEffect(EffectTriggerType.WhenCardAttacks, netId);
        }

        /// <summary>
        /// Called when the active player turn starts.
        /// </summary>
        /// <param name="netId">Network identifier of the card with the triggered effect.</param>
        public virtual void OnPlayerTurnStarted(NetworkInstanceId netId)
        {
            var handler = handlers.Find(x => x is EffectResolverHandler);
            if (handler != null)
                (handler as EffectResolverHandler).TriggerEffect(EffectTriggerType.WhenPlayerTurnStarts, netId);
        }

        /// <summary>
        /// Called when the active player turn ends.
        /// </summary>
        /// <param name="netId">Network identifier of the card with the triggered effect.</param>
        public virtual void OnPlayerTurnEnded(NetworkInstanceId netId)
        {
            var handler = handlers.Find(x => x is EffectResolverHandler);
            if (handler != null)
                (handler as EffectResolverHandler).TriggerEffect(EffectTriggerType.WhenPlayerTurnEnds, netId);
        }

        /// <summary>
        /// Called after a number of turns have passed.
        /// </summary>
        /// <param name="netId">Network identifier of the card with the triggered effect.</param>
        public virtual void OnAfterNumberOfTurns(NetworkInstanceId netId)
        {
            var handler = handlers.Find(x => x is EffectResolverHandler);
            if (handler != null)
                (handler as EffectResolverHandler).TriggerEffect(EffectTriggerType.AfterNumberOfTurns, netId);
        }

        /// <summary>
        /// Called every number of turns.
        /// </summary>
        /// <param name="netId">Network identifier of the card with the triggered effect.</param>
        public virtual void OnEveryNumberOfTurns(NetworkInstanceId netId)
        {
            var handler = handlers.Find(x => x is EffectResolverHandler);
            if (handler != null)
                (handler as EffectResolverHandler).TriggerEffect(EffectTriggerType.EveryNumberOfTurns, netId);
        }

        /// <summary>
        /// Executes the specified game action.
        /// </summary>
        /// <param name="action">Game action to execute.</param>
        protected void ExecuteGameAction(GameAction action)
        {
            var players = new List<PlayerState>();
            switch (action.Target)
            {
                case GameActionTarget.CurrentPlayer:
                    players.Add(CurrentPlayer);
                    break;

                case GameActionTarget.CurrentOpponents:
                    players.AddRange(CurrentOpponents);
                    break;

                case GameActionTarget.AllPlayers:
                    players = Players;
                    break;
            }

            if (action is SetPlayerAttributeAction)
            {
                var setAttributeAction = action as SetPlayerAttributeAction;
                foreach (var player in players)
                {
                    player.SetAttribute(setAttributeAction.Attribute, setAttributeAction.Value);
                }
            }
            else if (action is IncreasePlayerAttributeAction)
            {
                var increaseAttributeAction = action as IncreasePlayerAttributeAction;
                foreach (var player in players)
                {
                    var currentValue = player.GetAttribute(increaseAttributeAction.Attribute).Value.Value;
                    var newValue = currentValue + increaseAttributeAction.Value;
                    if (newValue > increaseAttributeAction.Max)
                    {
                        newValue = increaseAttributeAction.Max;
                    }
                    player.SetAttribute(increaseAttributeAction.Attribute, newValue);
                }
            }
            else if (action is ShuffleCardsAction)
            {
                var shuffleCardsAction = action as ShuffleCardsAction;
                foreach (var player in players)
                {
                    var zone = player.GetGameZone(shuffleCardsAction.Zone);
                    if (zone is StaticGameZone)
                    {
                        (zone as StaticGameZone).Cards.Shuffle();
                    }
                    else if (zone is DynamicGameZone)
                    {
                        (zone as DynamicGameZone).Cards.Shuffle();
                    }
                }
            }
            else if (action is MoveCardsAction)
            {
                var handler = handlers.Find(x => x is MoveCardsHandler);
                if (handler != null)
                {
                    var moveCardsHandler = handler as MoveCardsHandler;
                    var moveCardsAction = action as MoveCardsAction;
                    foreach (var player in players)
                    {
                        moveCardsHandler.MoveCards(player, moveCardsAction.OriginZone, moveCardsAction.DestinationZone, moveCardsAction.NumCards);
                    }
                }
            }
        }

        /// <summary>
        /// Returns an array of the static game zones for the specified player that can be sent across the
        /// network.
        /// </summary>
        /// <param name="player">Player from which to retrieve the game zones.</param>
        /// <param name="hideCards">True if the cards should not be included as part of the returned array's data;
        /// false otherwise.</param>
        /// <returns>An array containing the static game zones for the specified player that can be sent
        /// across the network.</returns>
        protected virtual NetStaticGameZone[] GetNetStaticGameZones(PlayerState player, bool hideCards)
        {
            var staticGameZones = new List<NetStaticGameZone>();
            foreach (var zone in player.GameZones)
            {
                if (zone is StaticGameZone)
                {
                    var staticZone = new NetStaticGameZone();
                    staticZone.Name = zone.Name;
                    staticZone.NumCards = (zone as StaticGameZone).Cards.Count;
                    if (!zone.Private && !hideCards)
                    {
                        staticZone.Cards = (zone as StaticGameZone).Cards.ToArray();
                    }
                    staticGameZones.Add(staticZone);
                }
            }
            return staticGameZones.ToArray();
        }

        /// <summary>
        /// Returns an array of the dynamic game zones for the specified player that can be sent across the
        /// network.
        /// </summary>
        /// <param name="player">Player from which to retrieve the game zones.</param>
        /// <param name="hideCards">True if the cards should not be included as part of the returned array's data;
        /// false otherwise.</param>
        /// <returns>An array containing the dynamic game zones for the specified player that can be sent
        /// across the network.</returns>
        protected virtual NetDynamicGameZone[] GetNetDynamicGameZones(PlayerState player, bool hideCards)
        {
            var dynamicGameZones = new List<NetDynamicGameZone>();
            foreach (var zone in player.GameZones)
            {
                if (zone is DynamicGameZone)
                {
                    var dynamicZone = new NetDynamicGameZone();
                    dynamicZone.Name = zone.Name;
                    dynamicZone.NumCards = (zone as DynamicGameZone).Cards.Count;
                    if (!zone.Private && !hideCards)
                    {
                        dynamicZone.Cards = (zone as DynamicGameZone).Cards.ToArray();
                    }
                    dynamicGameZones.Add(dynamicZone);
                }
            }
            return dynamicGameZones.ToArray();
        }
    }
}
