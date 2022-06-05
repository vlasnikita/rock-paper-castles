// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;
using UnityEngine.UI;

using CCGKit;

/// <summary>
/// The demo player is a subclass of the core HumanPlayer type which extends it with demo-specific
/// functionality. Most of which is straightforward updating of the user interface when receiving
/// new state from the server.
/// </summary>
public class DemoHumanPlayer : HumanPlayer
{
    public GameObject ProxyCardPrefab;

    public GameObject HealthIncreaseTextPrefab;
    public GameObject HealthDecreaseTextPrefab;

    public AudioClip CardPlacedSound;
    public AudioClip HealthIncreasedSound;
    public AudioClip HealthDecreasedSound;

    protected List<DemoProxyCard> handCards = new List<DemoProxyCard>();

    public int HandSize
    {
        get { return handCards.Count; }
    }

    protected DemoProxyCard currentProxyCard;

    protected List<NetworkCard> bottomBoardCards = new List<NetworkCard>();
    protected List<NetworkCard> topBoardCards = new List<NetworkCard>();

    protected GameUI gameUI;

    protected float accTime;
    protected float secsAccTime;

    protected Queue<GameObject> healthIncreaseTexts = new Queue<GameObject>();
    protected Queue<GameObject> healthDecreaseTexts = new Queue<GameObject>();

    protected AudioSource audioSource;

    protected override void Awake()
    {
        base.Awake();

        Assert.IsTrue(ProxyCardPrefab != null);
        Assert.IsTrue(HealthIncreaseTextPrefab != null);
        Assert.IsTrue(HealthDecreaseTextPrefab != null);

        audioSource = GetComponent<AudioSource>();
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        gameUI = GameObject.Find("Canvas").GetComponent<GameUI>();
        Assert.IsTrue(gameUI != null);
    }

    protected override void RegisterWithServer()
        {
            // Load player's default deck from PlayerPrefs.
            var defaultDeckJson = GetActiveDeck();
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

    public override void OnStartGame(StartGameMessage msg)
    {
        base.OnStartGame(msg);

        // Add the new cards dealt to the player's hand.
        var hand = Array.Find(msg.StaticGameZones, x => x.Name == "Hand");
        for (var i = 0; i < hand.Cards.Length; i++)
            AddCardToHand(hand.Cards[i]);

        for (var i = 0; i < msg.PlayerNames.Length; i++)
        {
            if (i == msg.PlayerIndex)
                gameUI.SetPlayerNameBottom(msg.PlayerNames[i]);
            else
                gameUI.SetPlayerNameTop(msg.PlayerNames[i]);
        }

        // Update UI as appropriate.
        if (msg.PlayerIndex != 0)
        {
            gameUI.PlayerBoxBottom.SetAvatarGlowEnabled(false);
            gameUI.PlayerBoxTop.SetAvatarGlowEnabled(true);
        }

        var gameScene = GameObject.Find("GameScene");
        if (gameScene != null)
            gameScene.GetComponent<GameScene>().CloseWaitingWindow();
    }

    public override void OnStartTurn(StartTurnMessage msg)
    {
        base.OnStartTurn(msg);

        if (msg.IsRecipientTheActivePlayer)
        {
            // Update hand.
            for (var i = 0; i < handCards.Count; i++)
                Destroy(handCards[i].gameObject);
            handCards.Clear();

            var hand = Array.Find(msg.StaticGameZones, x => x.Name == "Hand");
            for (var i = 0; i < hand.Cards.Length; i++)
                AddCardToHand(hand.Cards[i]);

            // Update UI as appropriate.
            gameUI.PlayerBoxBottom.SetAvatarGlowEnabled(true);
            gameUI.PlayerBoxTop.SetAvatarGlowEnabled(false);
            gameUI.SetLivesBottom(GetAttribute("Life").Value);
            gameUI.UpdateManaText(GetAttribute("Mana").Value);
            gameUI.SetEndTurnButtonEnabled(true);

            // Highlight the playable cards from the player's hand.
            HighlightPlayableCardsFromHand(true);
            ResetTurnTimer();

            RearrangeBoardCards();
        }
    }

    public override void OnEndTurn(EndTurnMessage msg)
    {
        base.OnEndTurn(msg);

        if (msg.IsRecipientTheActivePlayer)
        {
            // Update UI as appropriate.
            gameUI.PlayerBoxBottom.SetAvatarGlowEnabled(false);
            gameUI.PlayerBoxTop.SetAvatarGlowEnabled(true);
            gameUI.SetEndTurnButtonEnabled(false);
            // Stop highlighting the playable cards from the player's hand as it is no longer his turn.
            HighlightPlayableCardsFromHand(false);
            ResetTurnTimer();

            RearrangeBoardCards();
        }
    }

    public override void StopTurn()
    {
        var maxHandSize = GameManager.Instance.Config.Properties.MaxHandSize;
        if (handCards.Count < maxHandSize)
        {
            base.StopTurn();

            // Update UI as appropriate.
            gameUI.PlayerBoxBottom.SetAvatarGlowEnabled(false);
            gameUI.PlayerBoxTop.SetAvatarGlowEnabled(true);
            gameUI.SetEndTurnButtonEnabled(false);
            // Stop highlighting the playable cards from the player's hand as it is no longer his turn.
            HighlightPlayableCardsFromHand(false);
            ResetTurnTimer();

            if (currentProxyCard != null)
            {
                Destroy(currentProxyCard.gameObject);
                currentProxyCard = null;
            }
        }
        else
        {
            var msg = new StopTurnMessage();
            client.Send(NetworkProtocol.StopTurn, msg);
        }
    }

    protected virtual void ResetTurnTimer()
    {
        accTime = 0;
        secsAccTime = 0;
        gameUI.UpdateTurnTimerTop(0);
        gameUI.UpdateTurnTimerBottom(0);
    }

    protected virtual void Update()
    {
        if (!isLocalPlayer)
            return;

        if (!gameStarted)
            return;

        accTime += Time.deltaTime;
        secsAccTime += Time.deltaTime;
        if (secsAccTime >= 1)
        {
            secsAccTime = 0;
            if (IsActivePlayer) gameUI.UpdateTurnTimerBottom((int)(turnDuration - accTime));
            else gameUI.UpdateTurnTimerTop((int)(turnDuration - accTime));
        }
        if (accTime >= turnDuration)
            accTime = 0;
    }

    public override void OnUpdatePlayerAttributes(UpdatePlayerAttributesMessage msg)
    {
        base.OnUpdatePlayerAttributes(msg);

        var oldLife = int.Parse(gameUI.PlayerBoxBottom.LivesText.text);
        var newLife = GetAttribute("Life").Value;
        if (newLife > oldLife)
            ShowHealthIncreaseText(newLife - oldLife, gameUI.PlayerBoxBottom.LivesText.gameObject);
        else if (oldLife > newLife)
            ShowHealthDecreaseText(oldLife - newLife, gameUI.PlayerBoxBottom.LivesText.gameObject);

        gameUI.SetLivesBottom(newLife);
        gameUI.UpdateManaText(GetAttribute("Mana").Value);

        var deck = Array.Find(msg.StaticGameZones, x => x.Name == "Deck");
        var hand = Array.Find(msg.StaticGameZones, x => x.Name == "Hand");
        var cemetery = Array.Find(msg.StaticGameZones, x => x.Name == "Cemetery");
        gameUI.SetDeckCardsBottom(deck.NumCards);
        gameUI.SetHandCardsBottom(hand.NumCards);
        gameUI.SetDeadCardsBottom(cemetery.NumCards);

        if (hand.NumCards != handCards.Count)
        {
            for (var i = 0; i < handCards.Count; i++)
                Destroy(handCards[i].gameObject);
            handCards.Clear();

            for (var i = 0; i < hand.Cards.Length; i++)
                AddCardToHand(hand.Cards[i]);

            if (IsActivePlayer)
                HighlightPlayableCardsFromHand(true);
        }
    }

    public override void OnUpdateOpponentAttributes(UpdateOpponentAttributesMessage msg)
    {
        base.OnUpdateOpponentAttributes(msg);

        var oldLife = int.Parse(gameUI.PlayerBoxTop.LivesText.text);
        var newLife = GetAttributeValueFromOpponentMessage("Life", msg);
        if (newLife > oldLife)
            ShowHealthIncreaseText(newLife - oldLife, gameUI.PlayerBoxTop.LivesText.gameObject);
        else if (oldLife > newLife)
            ShowHealthDecreaseText(oldLife - newLife, gameUI.PlayerBoxTop.LivesText.gameObject);

        gameUI.SetLivesTop(newLife);

        var deck = Array.Find(msg.StaticGameZones, x => x.Name == "Deck");
        var hand = Array.Find(msg.StaticGameZones, x => x.Name == "Hand");
        var cemetery = Array.Find(msg.StaticGameZones, x => x.Name == "Cemetery");
        gameUI.SetDeckCardsTop(deck.NumCards);
        gameUI.SetHandCardsTop(hand.NumCards);
        gameUI.SetDeadCardsTop(cemetery.NumCards);
    }

    protected virtual int GetAttributeValueFromOpponentMessage(string name, UpdateOpponentAttributesMessage msg)
    {
        for (var i = 0; i < msg.Names.Length; i++)
        {
            if (msg.Names[i] == name)
                return msg.Values[i];
        }
        return 0;
    }

    protected virtual void AddCardToHand(int cardId)
    {
        var handZone = GameObject.Find("Canvas/HandZone");

        var go = Instantiate(ProxyCardPrefab) as GameObject;
        var card = go.GetComponent<DemoProxyCard>();
        card.OwnerNetId = netId;
        card.SetCardData(cardId);
        card.transform.SetParent(handZone.transform, false);
        card.GetComponent<RectTransform>().anchoredPosition = new Vector2(210.0f * handCards.Count, 0);

        handCards.Add(card);
    }

    protected virtual void HighlightPlayableCardsFromHand(bool highlight)
    {
        if (highlight)
        {
            for (var i = 0; i < handCards.Count; i++)
            {
                var card = handCards[i];
                var cardCost = card.Card.GetIntegerAttribute("Cost");
                var availableMana = GetAttribute("Mana").Value;
                card.GlowImage.gameObject.SetActive(cardCost <= availableMana);
            }
        }
        else
        {
            for (var i = 0; i < handCards.Count; i++)
            {
                var card = handCards[i];
                card.GlowImage.gameObject.SetActive(false);
            }
        }
    }

    public void PlayCard(DemoProxyCard card)
    {
        if (!isLocalPlayer)
            return;

        if (!IsActivePlayer)
            return;

        var cardCost = card.Card.GetIntegerAttribute("Cost");
        var availableMana = GetAttribute("Mana");
        if (cardCost > availableMana.Value)
            return;

        SetAttribute("Mana", availableMana.Value - cardCost);

        // Place proxy card.
        currentProxyCard = card;
        card.GlowImage.gameObject.SetActive(false);
        RemoveCardFromHand(card);
        PlaceProxyCardOnBoard(card);
        HighlightPlayableCardsFromHand(true);

        // Play 'card placed' sound effect.
        AudioManager.Instance.PlaySound(CardPlacedSound);

        // Actually play the card.
        PlayCard(card.Card.Id);
    }

    public void PlayCard(Card card)
    {
        if (!isLocalPlayer)
            return;

        if (!IsActivePlayer)
            return;

        var cardCost = card.GetIntegerAttribute("Cost");
        var availableMana = GetAttribute("Mana");
        if (cardCost > availableMana.Value)
            return;

        SetAttribute("Mana", availableMana.Value - cardCost);

        // Play 'card placed' sound effect.
        AudioManager.Instance.PlaySound(CardPlacedSound);

        // Actually play the card.
        PlayCard(card.Id);
    }

    protected void PlaceProxyCardOnBoard(DemoProxyCard card)
    {
        var boardZone = GameObject.Find("Canvas/BoardBottom/CardZone");
        card.transform.SetParent(boardZone.transform, false);
        card.GetComponent<RectTransform>().anchoredPosition = new Vector2(210.0f * bottomBoardCards.Count, 0);
    }

    public void AddCardToBottomBoard(DemoNetworkCard card)
    {
        if (currentProxyCard != null)
        {
            Destroy(currentProxyCard.gameObject);
            currentProxyCard = null;
        }
        bottomBoardCards.Add(card);
    }

    public void AddCardToTopBoard(DemoNetworkCard card)
    {
        topBoardCards.Add(card);
    }

    public void RemoveCardFromHand(DemoProxyCard card)
    {
        handCards.Remove(card);
        for (int i = 0; i < handCards.Count; i++)
        {
            var c = handCards[i];
            c.GetComponent<RectTransform>().anchoredPosition = new Vector2(210.0f * i, 0);
        }
    }

    public void RemoveCardFromBoard(DemoNetworkCard card)
    {
        topBoardCards.Remove(card);
        bottomBoardCards.Remove(card);
    }

    public int GetNumberOfCardsInTopBoard()
    {
        return topBoardCards.Count;
    }

    public int GetNumberOfCardsInBottomBoard()
    {
        return bottomBoardCards.Count;
    }

    private void RearrangeBoardCards()
    {
        RearrangeBoardCards(topBoardCards);
        RearrangeBoardCards(bottomBoardCards);
    }

    private void RearrangeBoardCards(List<NetworkCard> cards)
    {
        var cardsToRemove = new List<NetworkCard>(cards.Count);
        foreach (var card in cards)
            if (card == null)
                cardsToRemove.Add(card);
        foreach (var card in cardsToRemove)
            cards.Remove(card);
        for (int i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            card.GetComponent<RectTransform>().anchoredPosition = new Vector2(210.0f * i, 0);
        }
    }

    public override void OnEndGame(EndGameMessage msg)
    {
        base.OnEndGame(msg);

        GameWindowUtils.OpenEndGameDialog(playerIndex == msg.WinnerPlayerIndex);
    }

    protected void ShowHealthIncreaseText(int value, GameObject reference)
    {
        var healthIncreaseText = Instantiate(HealthIncreaseTextPrefab);
        healthIncreaseText.GetComponent<Text>().text = "+" + value.ToString();
        var canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        healthIncreaseText.transform.SetParent(canvas.transform, false);
        healthIncreaseText.transform.position = reference.transform.position;
        healthIncreaseTexts.Enqueue(healthIncreaseText);
        Invoke("DestroyHealthIncreaseText", 1.0f);

        // Play 'health increased' sound effect.
        AudioManager.Instance.PlaySound(HealthIncreasedSound);
    }

    private void DestroyHealthIncreaseText()
    {
        if (healthIncreaseTexts.Count > 0)
            Destroy(healthIncreaseTexts.Dequeue());
    }

    protected void ShowHealthDecreaseText(int value, GameObject reference)
    {
        var healthDecreaseText = Instantiate(HealthDecreaseTextPrefab);
        healthDecreaseText.GetComponent<Text>().text = "-" + value.ToString();
        var canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        healthDecreaseText.transform.SetParent(canvas.transform, false);
        healthDecreaseText.transform.position = reference.transform.position;
        healthDecreaseTexts.Enqueue(healthDecreaseText);
        Invoke("DestroyHealthDecreaseText", 1.0f);

        // Play 'health decreased' sound effect.
        AudioManager.Instance.PlaySound(HealthDecreasedSound);
    }

    private void DestroyHealthDecreaseText()
    {
        if (healthDecreaseTexts.Count > 0)
            Destroy(healthDecreaseTexts.Dequeue());
    }

    public override void OnDrawCards(DrawCardsMessage msg)
    {
        base.OnDrawCards(msg);

        for (var i = 0; i < msg.Cards.Length; i++)
            AddCardToHand(msg.Cards[i]);

        if (IsActivePlayer)
            HighlightPlayableCardsFromHand(true);
    }

    public override void OnCardsAutoDiscarded(CardsAutoDiscardedMessage msg)
    {
        base.OnCardsAutoDiscarded(msg);

        foreach (var cardId in msg.Cards)
        {
            var handCard = handCards.Find(x => x.Card.Id == cardId);
            if (handCard != null)
            {
                RemoveCardFromHand(handCard);
                Destroy(handCard.gameObject);
            }
        }
    }

    public override void OnReceiveChatText(string text)
    {
        base.OnReceiveChatText(text);

        var chatDialog = GameObject.Find("ChatDialog");
        if (chatDialog != null)
            chatDialog.GetComponent<ChatDialog>().AddTextEntry(text);
    }

    public override void OnKilledCard(KilledCardMessage msg)
    {
        base.OnKilledCard(msg);
        var card = ClientScene.FindLocalObject(msg.NetId);
        if (card != null)
            RemoveCardFromBoard(card.GetComponent<DemoNetworkCard>());
    }
}
