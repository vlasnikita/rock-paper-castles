// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;

namespace CCGKit
{
    /// <summary>
    /// Utility class that contains the information of a single runtime attribute for a card.
    /// </summary>
    public struct CardAttribute
    {
        public string Name;
        public int Value;

        public CardAttribute(string name, int value)
        {
            Name = name;
            Value = value;
        }
    }

    /// <summary>
    /// Non-templated class to be used as the type of a card's synchronized list of attributes
    /// (templated classes are not allowed).
    /// </summary>
    public class SyncListCardAttributes : SyncListStruct<CardAttribute>
    {
    }

    /// <summary>
    /// The base class for multiplayer-aware card objects in a game.
    /// </summary>
    public class NetworkCard : NetworkBehaviour
    {
        /// <summary>
        /// The network identifier of this card's owner player.
        /// </summary>
        [HideInInspector]
        [SyncVar]
        public NetworkInstanceId OwnerNetId;

        /// <summary>
        /// The underlying card id of this network card.
        /// </summary>
        [HideInInspector]
        [SyncVar]
        public int CardId;

        /// <summary>
        /// True it this card is currently alive; false otherwise.
        /// </summary>
        [SyncVar]
        public bool IsAlive = true;

        /// <summary>
        /// The cached owner player of this card.
        /// </summary>
        protected Player ownerPlayer;

        /// <summary>
        /// List of dynamic attributes of this card.
        /// </summary>
        protected SyncListCardAttributes Attributes = new SyncListCardAttributes();

        /// <summary>
        /// The number of turns this card has been in play.
        /// </summary>
        protected int numTurns;

        /// <summary>
        /// True if the card data has been set on this network card; false otherwise.
        /// </summary>
        protected bool cardDataSet;

        // We provide a virtual, empty Awake() method to avoid running into trouble
        // with the attributes sync list in subclasses (Unity automatically inserts
        // a sync list's initialization code in Awake()).
        protected virtual void Awake()
        {
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            // Cache the owner player of this card for convenience.
            ownerPlayer = ClientScene.FindLocalObject(OwnerNetId).GetComponent<Player>();
            Assert.IsTrue(ownerPlayer != null);

            Attributes.Callback += OnAttributesChanged;
            SetCardData();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            // Cache the owner player of this card for convenience.
            ownerPlayer = NetworkingUtils.GetNetworkObject(OwnerNetId).GetComponent<Player>();
            Assert.IsTrue(ownerPlayer != null);

            SetCardData();
        }

        /// <summary>
        /// At initialization time, we fill the dynamic attributes of a card taking the underlying card
        /// data as a reference. The attributes are different because, as opposed to those of the underlying
        /// card, they may change during a game.
        ///
        /// Think of a creature with 2 points of health. While the underlying card data will always contain 2
        /// as the number of health points, a specific in-game instance of that card may suffer damage and only
        /// contain 1 health point. This is the reason behind keeping a separate list of dynamic attributes for
        /// every networked card. These attributes are stored in a SyncList in order to be properly synchronized
        /// across the network.
        /// </summary>
        protected virtual void SetCardData()
        {
            // Avoid entering twice here in the case of hosts (where both OnStartClient() and OnStartServer()
            // will be called).
            if (cardDataSet)
                return;

            var card = GameManager.Instance.GetCard(CardId);
            for (var i = 0; i < card.Attributes.Count; i++)
            {
                var cardAttribute = card.Attributes[i];
                if (cardAttribute is IntAttribute)
                    Attributes.Add(new CardAttribute(cardAttribute.Name, (cardAttribute as IntAttribute).Value));
            }

            cardDataSet = true;
        }

        /// <summary>
        /// Returns the dynamic attribute with the specified name.
        /// </summary>
        /// <param name="name">The name of the dynamic attribute to return.</param>
        /// <returns>The dynamic attribute with the specified name; null otherwise.</returns>
        public CardAttribute? GetAttribute(string name)
        {
            for (var i = 0; i < Attributes.Count; i++)
                if (Attributes[i].Name == name)
                    return Attributes[i];
            return null;
        }

        /// <summary>
        /// Updates the value of the dynamic attribute with the specified name.
        /// </summary>
        /// <param name="name">The name of the dynamic attribute to update.</param>
        /// <param name="value">The new value of the specified dynamic attribute.</param>
        public void SetAttribute(string name, int value)
        {
            var server = GameObject.Find("Server").GetComponent<Server>();

            var card = GameManager.Instance.GetCard(CardId);
            var cardDefinition = GameManager.Instance.Config.CardDefinitions.Find(x => x.Name == card.Definition);
            for (var i = 0; i < Attributes.Count; i++)
            {
                if (Attributes[i].Name == name)
                {
                    // Check for triggered effects from changing the attribute's value.
                    var oldValue = Attributes[i].Value;
                    if (value > oldValue)
                    {
                        if (isServer)
                        {
                            var definitionEffects = cardDefinition.Effects.FindAll(x => x.Trigger.Type == EffectTriggerType.WhenCardAttributeIncreases);
                            definitionEffects = definitionEffects.FindAll(x => (x.Trigger as CardAttributeIncreasedTrigger).Attribute == Attributes[i].Name);

                            var cardEffects = card.Effects.FindAll(x => x.Trigger.Type == EffectTriggerType.WhenCardAttributeIncreases);
                            cardEffects = cardEffects.FindAll(x => (x.Trigger as CardAttributeIncreasedTrigger).Attribute == Attributes[i].Name);

                            definitionEffects.AddRange(cardEffects);
                            if (definitionEffects.Count > 0)
                                server.OnCardIncreasedAttribute(netId);
                        }
                    }
                    else if (value < oldValue)
                    {
                        if (isServer)
                        {
                            var definitionEffects = cardDefinition.Effects.FindAll(x => x.Trigger.Type == EffectTriggerType.WhenCardAttributeDecreases);
                            definitionEffects = definitionEffects.FindAll(x => (x.Trigger as CardAttributeDecreasedTrigger).Attribute == Attributes[i].Name);

                            var cardEffects = card.Effects.FindAll(x => x.Trigger.Type == EffectTriggerType.WhenCardAttributeDecreases);
                            cardEffects = cardEffects.FindAll(x => (x.Trigger as CardAttributeDecreasedTrigger).Attribute == Attributes[i].Name);

                            definitionEffects.AddRange(cardEffects);
                            if (definitionEffects.Count > 0)
                                server.OnCardDecreasedAttribute(netId);
                        }
                    }

                    if (isServer)
                    {
                        {
                            var definitionEffects = cardDefinition.Effects.FindAll(x => x.Trigger.Type == EffectTriggerType.WhenCardAttributeIsLessThan);
                            definitionEffects = definitionEffects.FindAll(x => (x.Trigger as CardAttributeLessThanTrigger).Attribute == Attributes[i].Name && value < (x.Trigger as CardAttributeLessThanTrigger).Value);

                            var cardEffects = card.Effects.FindAll(x => x.Trigger.Type == EffectTriggerType.WhenCardAttributeIsLessThan);
                            cardEffects = cardEffects.FindAll(x => (x.Trigger as CardAttributeLessThanTrigger).Attribute == Attributes[i].Name && value < (x.Trigger as CardAttributeLessThanTrigger).Value);

                            definitionEffects.AddRange(cardEffects);
                            if (definitionEffects.Count > 0)
                                server.OnCardAttributeIsLessThanValue(netId);
                        }

                        {
                            var definitionEffects = cardDefinition.Effects.FindAll(x => x.Trigger.Type == EffectTriggerType.WhenCardAttributeIsLessThanOrEqualTo);
                            definitionEffects = definitionEffects.FindAll(x => (x.Trigger as CardAttributeLessThanOrEqualToTrigger).Attribute == Attributes[i].Name && value <= (x.Trigger as CardAttributeLessThanOrEqualToTrigger).Value);

                            var cardEffects = card.Effects.FindAll(x => x.Trigger.Type == EffectTriggerType.WhenCardAttributeIsLessThanOrEqualTo);
                            cardEffects = cardEffects.FindAll(x => (x.Trigger as CardAttributeLessThanOrEqualToTrigger).Attribute == Attributes[i].Name && value <= (x.Trigger as CardAttributeLessThanOrEqualToTrigger).Value);

                            definitionEffects.AddRange(cardEffects);
                            if (definitionEffects.Count > 0)
                                server.OnCardAttributeIsLessThanOrEqualToValue(netId);
                        }

                        {
                            var definitionEffects = cardDefinition.Effects.FindAll(x => x.Trigger.Type == EffectTriggerType.WhenCardAttributeIsEqualTo);
                            definitionEffects = definitionEffects.FindAll(x => (x.Trigger as CardAttributeEqualToTrigger).Attribute == Attributes[i].Name && value == (x.Trigger as CardAttributeEqualToTrigger).Value);

                            var cardEffects = card.Effects.FindAll(x => x.Trigger.Type == EffectTriggerType.WhenCardAttributeIsEqualTo);
                            cardEffects = cardEffects.FindAll(x => (x.Trigger as CardAttributeEqualToTrigger).Attribute == Attributes[i].Name && value == (x.Trigger as CardAttributeEqualToTrigger).Value);

                            definitionEffects.AddRange(cardEffects);
                            if (definitionEffects.Count > 0)
                                server.OnCardAttributeIsEqualToValue(netId);
                        }

                        {
                            var definitionEffects = cardDefinition.Effects.FindAll(x => x.Trigger.Type == EffectTriggerType.WhenCardAttributeIsGreaterThanOrEqualTo);
                            definitionEffects = definitionEffects.FindAll(x => (x.Trigger as CardAttributeGreaterThanOrEqualToTrigger).Attribute == Attributes[i].Name && value >= (x.Trigger as CardAttributeGreaterThanOrEqualToTrigger).Value);

                            var cardEffects = card.Effects.FindAll(x => x.Trigger.Type == EffectTriggerType.WhenCardAttributeIsGreaterThanOrEqualTo);
                            cardEffects = cardEffects.FindAll(x => (x.Trigger as CardAttributeGreaterThanOrEqualToTrigger).Attribute == Attributes[i].Name && value >= (x.Trigger as CardAttributeGreaterThanOrEqualToTrigger).Value);

                            definitionEffects.AddRange(cardEffects);
                            if (definitionEffects.Count > 0)
                                server.OnCardAttributeIsGreaterThanOrEqualToValue(netId);
                        }

                        {
                            var definitionEffects = cardDefinition.Effects.FindAll(x => x.Trigger.Type == EffectTriggerType.WhenCardAttributeIsGreaterThan);
                            definitionEffects = definitionEffects.FindAll(x => (x.Trigger as CardAttributeGreaterThanTrigger).Attribute == Attributes[i].Name && value > (x.Trigger as CardAttributeGreaterThanTrigger).Value);

                            var cardEffects = card.Effects.FindAll(x => x.Trigger.Type == EffectTriggerType.WhenCardAttributeIsGreaterThan);
                            cardEffects = cardEffects.FindAll(x => (x.Trigger as CardAttributeGreaterThanTrigger).Attribute == Attributes[i].Name && value > (x.Trigger as CardAttributeGreaterThanTrigger).Value);

                            definitionEffects.AddRange(cardEffects);
                            if (definitionEffects.Count > 0)
                                server.OnCardAttributeIsGreaterThanValue(netId);
                        }
                    }

                    Attributes[i] = new CardAttribute(name, value);
                    Attributes.Dirty(i);
                    return;
                }
            }
            Attributes.Add(new CardAttribute(name, value));
            Attributes.Dirty(Attributes.Count - 1);
        }

        /// <summary>
        /// This callback is automatically invoked when an item of the synchronized list of attributes
        /// is changed. It may be particularly useful for subclasses (e.g., to update the visual
        /// representation of the networked card).
        /// </summary>
        /// <param name="op">Operation that occurred within the list.</param>
        /// <param name="index">Index of the element that was effected by the operation.</param>
        protected virtual void OnAttributesChanged(SyncListCardAttributes.Operation op, int index)
        {
        }

        /// <summary>
        /// Callback called when the card is selected (e.g., clicked over).
        /// </summary>
        public virtual void OnCardSelected()
        {
        }

        /// <summary>
        /// Destroys this card.
        /// </summary>
        public virtual void Kill()
        {
            if (!IsAlive)
                return;

            IsAlive = false;
        }

        /// <summary>
        /// Callback called when the player that owns this card starts a new turn.
        /// </summary>
        /// <param name="activePlayerNetId">Network identifier of the active player.</param>
        public virtual void OnStartTurn(NetworkInstanceId activePlayerNetId)
        {
            numTurns += 1;

            if (isServer)
            {
                var server = GameObject.Find("Server").GetComponent<Server>();

                // Check for triggered effects of type 'on turn start'.
                var card = GameManager.Instance.GetCard(CardId);
                var cardDefinition = GameManager.Instance.Config.CardDefinitions.Find(x => x.Name == card.Definition);
                if (ownerPlayer.netId == activePlayerNetId)
                {
                    var definitionEffects = cardDefinition.Effects.FindAll(x => x.Trigger.Type == EffectTriggerType.WhenPlayerTurnStarts);
                    definitionEffects = definitionEffects.FindAll(x => ((x.Trigger as PlayerTurnStartedTrigger).Source == EffectPlayerTriggerSource.OwnerPlayer) ||
                        ((x.Trigger as PlayerTurnStartedTrigger).Source == EffectPlayerTriggerSource.AnyPlayer));

                    var cardEffects = card.Effects.FindAll(x => x.Trigger.Type == EffectTriggerType.WhenPlayerTurnStarts);
                    cardEffects = cardEffects.FindAll(x => ((x.Trigger as PlayerTurnStartedTrigger).Source == EffectPlayerTriggerSource.OwnerPlayer) ||
                        ((x.Trigger as PlayerTurnStartedTrigger).Source == EffectPlayerTriggerSource.AnyPlayer));

                    definitionEffects.AddRange(cardEffects);
                    if (definitionEffects.Count > 0)
                        server.OnPlayerTurnStarted(netId);
                }
                else
                {
                    var definitionEffects = cardDefinition.Effects.FindAll(x => x.Trigger.Type == EffectTriggerType.WhenPlayerTurnStarts);
                    definitionEffects = definitionEffects.FindAll(x => ((x.Trigger as PlayerTurnStartedTrigger).Source == EffectPlayerTriggerSource.AnyOpponent) ||
                        ((x.Trigger as PlayerTurnStartedTrigger).Source == EffectPlayerTriggerSource.AnyPlayer));

                    var cardEffects = card.Effects.FindAll(x => x.Trigger.Type == EffectTriggerType.WhenPlayerTurnStarts);
                    cardEffects = cardEffects.FindAll(x => ((x.Trigger as PlayerTurnStartedTrigger).Source == EffectPlayerTriggerSource.AnyOpponent) ||
                        ((x.Trigger as PlayerTurnStartedTrigger).Source == EffectPlayerTriggerSource.AnyPlayer));

                    definitionEffects.AddRange(cardEffects);
                    if (definitionEffects.Count > 0)
                        server.OnPlayerTurnStarted(netId);
                }

                // Check for triggered effects of type 'after X turns'.
                {
                    var definitionEffects = cardDefinition.Effects.FindAll(x => x.Trigger.Type == EffectTriggerType.AfterNumberOfTurns);
                    definitionEffects = definitionEffects.FindAll(x => numTurns == (x.Trigger as AfterNumberOfTurnsTrigger).NumTurns + 1);

                    var cardEffects = card.Effects.FindAll(x => x.Trigger.Type == EffectTriggerType.AfterNumberOfTurns);
                    cardEffects = cardEffects.FindAll(x => numTurns == (x.Trigger as AfterNumberOfTurnsTrigger).NumTurns + 1);

                    definitionEffects.AddRange(cardEffects);
                    if (definitionEffects.Count > 0)
                        server.OnAfterNumberOfTurns(netId);
                }

                // Check for triggered effects of type 'every X turns'.
                {
                    var definitionEffects = cardDefinition.Effects.FindAll(x => x.Trigger.Type == EffectTriggerType.EveryNumberOfTurns);
                    definitionEffects = definitionEffects.FindAll(x => (numTurns % (x.Trigger as EveryNumberOfTurnsTrigger).NumTurns) == 0);

                    var cardEffects = card.Effects.FindAll(x => x.Trigger.Type == EffectTriggerType.EveryNumberOfTurns);
                    cardEffects = cardEffects.FindAll(x => (numTurns % (x.Trigger as EveryNumberOfTurnsTrigger).NumTurns) == 0);

                    definitionEffects.AddRange(cardEffects);
                    if (definitionEffects.Count > 0)
                        server.OnEveryNumberOfTurns(netId);
                }
            }
        }

        /// <summary>
        /// Callback called when the player that owns this card ends his current turn.
        /// </summary>
        /// <param name="activePlayerNetId">Network identifier of the active player.</param>
        public virtual void OnEndTurn(NetworkInstanceId activePlayerNetId)
        {
            if (isServer)
            {
                var server = GameObject.Find("Server").GetComponent<Server>();

                // Check for triggered effects of type 'on turn end'.
                var card = GameManager.Instance.GetCard(CardId);
                var cardDefinition = GameManager.Instance.Config.CardDefinitions.Find(x => x.Name == card.Definition);
                if (ownerPlayer.netId == activePlayerNetId)
                {
                    var definitionEffects = cardDefinition.Effects.FindAll(x => x.Trigger.Type == EffectTriggerType.WhenPlayerTurnEnds);
                    definitionEffects = definitionEffects.FindAll(x => ((x.Trigger as PlayerTurnEndedTrigger).Source == EffectPlayerTriggerSource.OwnerPlayer) ||
                        ((x.Trigger as PlayerTurnEndedTrigger).Source == EffectPlayerTriggerSource.AnyPlayer));

                    var cardEffects = card.Effects.FindAll(x => x.Trigger.Type == EffectTriggerType.WhenPlayerTurnEnds);
                    cardEffects = cardEffects.FindAll(x => ((x.Trigger as PlayerTurnEndedTrigger).Source == EffectPlayerTriggerSource.OwnerPlayer) ||
                        ((x.Trigger as PlayerTurnEndedTrigger).Source == EffectPlayerTriggerSource.AnyPlayer));

                    definitionEffects.AddRange(cardEffects);
                    if (definitionEffects.Count > 0)
                        server.OnPlayerTurnEnded(netId);
                }
                else
                {
                    var definitionEffects = cardDefinition.Effects.FindAll(x => x.Trigger.Type == EffectTriggerType.WhenPlayerTurnEnds);
                    definitionEffects = definitionEffects.FindAll(x => ((x.Trigger as PlayerTurnEndedTrigger).Source == EffectPlayerTriggerSource.AnyOpponent) ||
                        ((x.Trigger as PlayerTurnEndedTrigger).Source == EffectPlayerTriggerSource.AnyPlayer));

                    var cardEffects = card.Effects.FindAll(x => x.Trigger.Type == EffectTriggerType.WhenPlayerTurnEnds);
                    cardEffects = cardEffects.FindAll(x => ((x.Trigger as PlayerTurnEndedTrigger).Source == EffectPlayerTriggerSource.AnyOpponent) ||
                        ((x.Trigger as PlayerTurnEndedTrigger).Source == EffectPlayerTriggerSource.AnyPlayer));

                    definitionEffects.AddRange(cardEffects);
                    if (definitionEffects.Count > 0)
                        server.OnPlayerTurnEnded(netId);
                }
            }
        }

        /// <summary>
        /// Returns true if this card can be targeted by the specified effect.
        /// </summary>
        /// <param name="effect">Effect against which to compare this card.</param>
        /// <returns>True if this card can be targeted by the specified effect; false otherwise.</returns>
        public virtual bool CanBeTargetOfEffect(Effect effect)
        {
            return true;
        }
    }
}
