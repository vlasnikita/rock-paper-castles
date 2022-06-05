// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Networking;

namespace CCGKit
{
    /// <summary>
    /// This type represents a game player and, as a multiplayer-aware entity, it is derived from
    /// NetworkBehaviour.
    /// </summary>
    public class Player : NetworkBehaviour
    {
        /// <summary>
        /// True if this player is the current active player in the game; false otherwise. 'Active' meaning
        /// the current game turn is his turn.
        /// </summary>
        public bool IsActivePlayer;

        /// <summary>
        /// True if this player is controlled by a human; false otherwise (AI).
        /// </summary>
        public bool IsHuman;

        /// <summary>
        /// Cached network client.
        /// </summary>
        protected NetworkClient client;

        /// <summary>
        /// List of this attributes this player has.
        /// </summary>
        protected List<PlayerAttribute> attributes = new List<PlayerAttribute>();

        /// <summary>
        /// 
        /// </summary>
        public string ActiveDeck;

        /// <summary>
        /// True if the game has started; false otherwise.
        /// </summary>
        protected bool gameStarted;

        /// <summary>
        /// Index of this player in the game.
        /// </summary>
        protected int playerIndex;

        /// <summary>
        /// This game's turn duration (in seconds).
        /// </summary>
        protected int turnDuration;

        /// <summary>
        /// True if this player is waiting to select an effect's player target; false otherwise.
        /// </summary>
        protected bool waitingForEffectPlayerTargetSelection;

        /// <summary>
        /// True if this player is waiting to select an effect's card target; false otherwise.
        /// </summary>
        protected bool waitingForEffectCardTargetSelection;

        /// <summary>
        /// True if this player is waiting to select an attack's target; false otherwise.
        /// </summary>
        protected bool waitingForAttackTargetSelection;

        /// <summary>
        /// True if this player is waiting to select the cards to discard; false otherwise.
        /// </summary>
        protected bool waitingForDiscardedCardsSelection;

        /// <summary>
        /// Number of cards this player needs to discard from his hand.
        /// </summary>
        protected int numCardsToDiscard;

        /// <summary>
        /// List that contains the unique identifiers of the cards to discard.
        /// </summary>
        protected List<int> cardsToDiscard = new List<int>();

        /// <summary>
        /// List that contains the unique identifiers of the hand cards.
        /// </summary>
        protected List<int> hand;

        protected virtual void Awake()
        {
            client = NetworkManager.singleton.client;
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            RegisterWithServer();
            var gameNetworkClient = GameObject.Find("GameNetworkClient").GetComponent<GameNetworkClient>();
            gameNetworkClient.AddLocalPlayer(this);
        }

        protected string GetActiveDeck()
        {
            var activeDeck = GameNetworkManager.Instance.ActiveDeck;
            switch (activeDeck)
            {
                case "castle": return Resources.Load<TextAsset>("CastleDeck").text;
                case "inferno": return Resources.Load<TextAsset>("InfernoDeck").text;
                case "tower": return Resources.Load<TextAsset>("TowerDeck").text;
                default: return PlayerPrefs.GetString("CCGKitv07_DefaultDeck");
            }
        }

        protected virtual void SetActiveDeck()
        {
            Debug.Log("SET ACTIVE");
            var defaultDeckJson = GetActiveDeck();
            var defaultDeck = JsonUtility.FromJson<Deck>(defaultDeckJson);
            var msgDefaultDeck = new List<int>(defaultDeck.Size);
            for (var i = 0; i < defaultDeck.Cards.Count; i++)
            {
                for (var j = 0; j < defaultDeck.Cards[i].Count; j++)
                    msgDefaultDeck.Add(defaultDeck.Cards[i].Id);
            }
            
            var msg = new RegisterPlayerMessage();
            msg.Name = "Unnamed wizard";
            msg.IsHuman = true;
            msg.Deck = msgDefaultDeck.ToArray();
            client.Send(NetworkProtocol.RegisterPlayer, msg);
        }

        protected virtual void RegisterWithServer()
        {
            // Load player's default deck from PlayerPrefs.
            var defaultDeckJson = PlayerPrefs.GetString("CCGKitv07_DefaultDeck");
            var defaultDeck = JsonUtility.FromJson<Deck>(defaultDeckJson);
            var msgDefaultDeck = new List<int>(defaultDeck.Size);
            for (var i = 0; i < defaultDeck.Cards.Count; i++)
            {
                for (var j = 0; j < defaultDeck.Cards[i].Count; j++)
                    msgDefaultDeck.Add(defaultDeck.Cards[i].Id);
            }

            // Register the player to the game and send the server his information.
            var msg = new RegisterPlayerMessage();
            msg.NetId = netId;
            if (IsHuman)
            {
                var playerName = GameManager.Instance.PlayerName;
                msg.Name = string.IsNullOrEmpty(playerName) ? "Unnamed wizard" : playerName;
            }
            else
            {
                msg.Name = "WizardBot";
            }
            msg.IsHuman = IsHuman;
            msg.Deck = msgDefaultDeck.ToArray();
            client.Send(NetworkProtocol.RegisterPlayer, msg);
        }

        public virtual void OnStartGame(StartGameMessage msg)
        {
            gameStarted = true;
            playerIndex = msg.PlayerIndex;
            turnDuration = msg.TurnDuration;
        }

        public virtual void OnEndGame(EndGameMessage msg)
        {
        }

        public virtual void OnStartTurn(StartTurnMessage msg)
        {
            if (msg.IsRecipientTheActivePlayer)
            {
                IsActivePlayer = true;
                CleanupTurnLocalState();
                var handZone = Array.Find(msg.StaticGameZones, x => x.Name == "Hand");
                hand = new List<int>(handZone.Cards);
            }

            // At every turn's start, update the player attributes with the most current
            // values from the server (they are also automatically updated on response to
            // certain game events).
            for (var i = 0; i < msg.AttributeNames.Length; i++)
            {
                var name = msg.AttributeNames[i];
                var value = msg.AttributeValues[i];
                var attributeIndex = attributes.FindIndex(x => x.Name == name);
                if (attributeIndex >= 0)
                    attributes[i] = new PlayerAttribute(name, value);
                else
                    attributes.Add(new PlayerAttribute(name, value));
            }
        }

        public virtual void OnEndTurn(EndTurnMessage msg)
        {
            if (msg.IsRecipientTheActivePlayer)
            {
                IsActivePlayer = false;
                CleanupTurnLocalState();
            }
        }

        public virtual void OnUpdatePlayerAttributes(UpdatePlayerAttributesMessage msg)
        {
            for (var i = 0; i < msg.Names.Length; i++)
            {
                var name = msg.Names[i];
                var value = msg.Values[i];
                var attributeIndex = attributes.FindIndex(x => x.Name == name);
                if (attributeIndex >= 0)
                    attributes[i] = new PlayerAttribute(name, value);
                else
                    attributes.Add(new PlayerAttribute(name, value));
            }
        }

        public virtual void OnUpdateOpponentAttributes(UpdateOpponentAttributesMessage msg)
        {
        }

        public PlayerAttribute GetAttribute(string name)
        {
            var attribute = attributes.Find(x => x.Name == name);
            return attribute;
        }

        protected void SetAttribute(string name, int value)
        {
            for (var i = 0; i < attributes.Count; i++)
            {
                if (attributes[i].Name == name)
                {
                    attributes[i] = new PlayerAttribute(name, value);
                    return;
                }
            }
            attributes.Add(new PlayerAttribute(name, value));
        }

        public virtual void StopTurn()
        {
            if (!isLocalPlayer)
                return;

            IsActivePlayer = false;
            var msg = new StopTurnMessage();
            client.Send(NetworkProtocol.StopTurn, msg);
        }

        public virtual void OnSelectTargetPlayer(SelectTargetPlayerMessage msg)
        {
            waitingForEffectPlayerTargetSelection = true;
        }

        public virtual void OnSelectTargetCard(SelectTargetCardMessage msg)
        {
            waitingForEffectCardTargetSelection = true;
        }

        public bool IsWaitingForEffectPlayerTargetSelection()
        {
            return waitingForEffectPlayerTargetSelection;
        }

        public void SetWaitingForEffectPlayerTargetSelection(bool waiting)
        {
            waitingForEffectPlayerTargetSelection = waiting;
        }

        public bool IsWaitingForEffectCardTargetSelection()
        {
            return waitingForEffectCardTargetSelection;
        }

        public void SetWaitingForEffectCardTargetSelection(bool waiting)
        {
            waitingForEffectCardTargetSelection = waiting;
        }

        public bool IsWaitingForDiscardedCardsSelection()
        {
            return waitingForDiscardedCardsSelection;
        }

        public void SetWaitingForDiscardedCardsSelection(bool waiting)
        {
            waitingForDiscardedCardsSelection = waiting;
        }

        protected virtual void CleanupTurnLocalState()
        {
            waitingForEffectPlayerTargetSelection = false;
            waitingForEffectCardTargetSelection = false;
            waitingForAttackTargetSelection = false;
            waitingForDiscardedCardsSelection = false;
            numCardsToDiscard = 0;
            cardsToDiscard.Clear();
        }

        public bool IsWaitingForAttackTargetSelection()
        {
            return waitingForAttackTargetSelection;
        }

        public void SetWaitingForAttackTargetSelection(bool waiting)
        {
            waitingForAttackTargetSelection = waiting;
        }

        public virtual void OnDrawCards(DrawCardsMessage msg)
        {
        }

        public virtual void OnDiscardCards(DiscardCardsMessage msg)
        {
            waitingForDiscardedCardsSelection = true;
            numCardsToDiscard = msg.NumCards;
        }

        public virtual void OnCardsAutoDiscarded(CardsAutoDiscardedMessage msg)
        {
        }

        public void AddCardToDiscard(int cardId)
        {
            cardsToDiscard.Add(cardId);
            if (cardsToDiscard.Count == numCardsToDiscard)
            {
                SetWaitingForDiscardedCardsSelection(false);
                var msg = new CardsToDiscardSelectedMessage();
                msg.SenderNetId = netId;
                msg.Cards = cardsToDiscard.ToArray();
                client.Send(NetworkProtocol.CardsToDiscardSelected, msg);
            }
        }

        public virtual void OnReceiveChatText(string text)
        {
        }

        public virtual void OnKilledCard(KilledCardMessage msg)
        {
        }

        public void PlayCard(int cardId)
        {
            var msg = new PlayCardMessage();
            msg.NetId = netId;
            msg.CardId = cardId;
            client.Send(NetworkProtocol.PlayCard, msg);
        }
    }
}
