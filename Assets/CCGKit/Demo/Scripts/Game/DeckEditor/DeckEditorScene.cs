// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using CCGKit;

/// <summary>
/// This scene allows the player to customize his collection of decks. Decks are stored in
/// a JSON file located in the application's persistent data path.
/// </summary>
public class DeckEditorScene : MonoBehaviour
{
    /// <summary>
    /// Prefab to use for the deck scroll view content.
    /// </summary>
    public GameObject DeckWidgetPrefab;

    /// <summary>
    /// Prefab to use for the deck detail scroll view content.
    /// </summary>
    public GameObject CardInfoWidgetPrefab;

    /// <summary>
    /// Prefab to use for the card library scroll view content.
    /// </summary>
    public GameObject LibraryCardWidgetPrefab;

    /// <summary>
    /// Scroll view that shows the available decks.
    /// </summary>
    public GameObject DeckScrollView;

    /// <summary>
    /// Scroll view that shows the cards in the selected deck.
    /// </summary>
    public GameObject DeckDetailScrollView;

    /// <summary>
    /// Scroll view that shows the available cards in the library.
    /// </summary>
    public GameObject CardLibraryScrollView;

    /// <summary>
    /// Card preview showing the in-game appearance of the selected card.
    /// </summary>
    public GameObject CardPreview;

    /// <summary>
    /// Input field to specify the current deck's name.
    /// </summary>
    public InputField DeckNameInputField;

    /// <summary>
    /// Text that specifies the current deck's size.
    /// </summary>
    public Text DeckSizeText;

    /// <summary>
    /// Text that specifies the number of spells in the current deck.
    /// </summary>
    public Text SpellCountText;

    /// <summary>
    /// Text that specifies the number of creatures in the current deck.
    /// </summary>
    public Text CreatureCountText;

    /// <summary>
    /// Button to set the current deck as the default deck for the player.
    /// </summary>
    public Button SetDefaultDeckButton;

    /// <summary>
    /// Path that will store the JSON file containing the player decks.
    /// </summary>
    private string decksDirPath;

    /// <summary>
    /// Player's deck collection.
    /// </summary>
    private DeckCollection deckCollection;

    /// <summary>
    /// Currently-selected deck.
    /// </summary>
    private Deck currentDeck;

    /// <summary>
    /// Currently-selected deck button.
    /// </summary>
    private Button currentDeckButton;

    /// <summary>
    /// The player's card collection downloaded from the server.
    /// </summary>
    private List<CardCollectionRow> playerCardCollection;

    private void Awake()
    {
        Assert.IsTrue(DeckWidgetPrefab != null);
        Assert.IsTrue(CardInfoWidgetPrefab != null);
        Assert.IsTrue(LibraryCardWidgetPrefab != null);
        Assert.IsTrue(DeckScrollView != null);
        Assert.IsTrue(DeckDetailScrollView != null);
        Assert.IsTrue(CardLibraryScrollView != null);
        Assert.IsTrue(CardPreview != null);
        Assert.IsTrue(DeckNameInputField != null);
        Assert.IsTrue(DeckSizeText != null);
        Assert.IsTrue(SpellCountText != null);
        Assert.IsTrue(CreatureCountText != null);
        Assert.IsTrue(SetDefaultDeckButton != null);
    }

    private void Start()
    {
        decksDirPath = Application.persistentDataPath + "/Decks";

        CardPreview.SetActive(false);

        WindowManager.Instance.OpenWindow("LoadingDialog",
            () =>
            {
                StartCoroutine(PlayerServer.GetCardCollection(GameManager.Instance.AuthToken,
                    (response) =>
                    {
                        WindowManager.Instance.CloseWindow("LoadingDialog");
                        playerCardCollection = response.cards;
                        LoadCardLibrary();
                        LoadPlayerDecks();
                    },
                    (error) =>
                    {
                        WindowManager.Instance.CloseWindow("LoadingDialog");
                        WindowUtils.OpenAlertDialog("The card collection could not be loaded.");
                    }));
            });
    }

    /// <summary>
    /// Loads all the cards available in the game that the player can choose from.
    /// </summary>
    private void LoadCardLibrary()
    {
        foreach (var card in GameManager.Instance.Config.CardCollection[0].Cards)
        {
            // Do not load token cards, as they cannot be part of a deck.
            if (!card.Subtypes.Contains("Token"))
                LoadCard(card);
        }
    }

    /// <summary>
    /// Loads all the decks created by the player.
    /// </summary>
    private void LoadPlayerDecks()
    {
        // Create a directory to store the player decks (this will do nothing if the directory
        // already exists).
        Directory.CreateDirectory(decksDirPath);
        var path = decksDirPath + "/decks.json";
        // Load existing decks, if any.
        if (File.Exists(path))
        {
            using (var streamReader = new StreamReader(path))
            {
                var json = streamReader.ReadToEnd();
                deckCollection = JsonUtility.FromJson<DeckCollection>(json);
                CreateDeckButtons();

                // Load default deck.
                var defaultDeck = deckCollection.Decks[deckCollection.DefaultDeck];
                LoadDeck(defaultDeck);
            }
        }
        else
        {
            // No custom decks exist; create a default one.
            var defaultDeckTextAsset = Resources.Load<TextAsset>("DefaultDeck");
            if (defaultDeckTextAsset != null)
            {
                var defaultDeck = JsonUtility.FromJson<Deck>(defaultDeckTextAsset.text);
                deckCollection = new DeckCollection();
                deckCollection.Decks.Add(defaultDeck);
                CreateDeckButtons();
                OnSaveButtonPressed();
                LoadDeck(defaultDeck);
            }
        }
    }

    private void CreateDeckButtons()
    {
        foreach (var deck in deckCollection.Decks)
        {
            var go = Instantiate(DeckWidgetPrefab) as GameObject;
            go.transform.SetParent(DeckScrollView.transform, false);
            var widget = go.GetComponent<DeckWidget>();
            currentDeckButton = widget.Button;
            currentDeckButton.transform.Find("Text").GetComponent<Text>().text = deck.Name;

            currentDeck = deck;
            widget.Deck = deck;

            DeckNameInputField.text = deck.Name;
            UpdateDeckSizeText();
        }
    }

    /// <summary>
    /// Loads the specified card into the card library scrollview.
    /// </summary>
    /// <param name="card">Card to load.</param>
    private void LoadCard(Card card)
    {
        var go = Instantiate(LibraryCardWidgetPrefab) as GameObject;
        go.transform.SetParent(CardLibraryScrollView.transform, false);
        var numCopies = GetNumCopiesOfCardInPlayerCollection(card.Id);
        go.transform.Find("Toggle/Text").GetComponent<Text>().text = card.Name + " (" + numCopies + " copies)";
        var widget = go.GetComponent<LibraryCardWidget>();
        widget.Toggle.group = CardLibraryScrollView.GetComponent<ToggleGroup>();
        widget.Card = card;
        widget.NumCopies = numCopies;
    }

    /// <summary>
    /// Create deck button callback.
    /// </summary>
    public void OnCreateDeckButtonPressed()
    {
        var go = Instantiate(DeckWidgetPrefab) as GameObject;
        go.transform.SetParent(DeckScrollView.transform, false);
        var widget = go.GetComponent<DeckWidget>();
        currentDeckButton = widget.Button;

        var deck = new Deck();
        deckCollection.Decks.Add(deck);
        currentDeck = deck;

        widget.Deck = deck;

        DeckNameInputField.text = deck.Name;
        UpdateDeckSizeText();

        foreach (Transform child in DeckDetailScrollView.transform)
            Destroy(child.gameObject);
    }

    /// <summary>
    /// Deck button callback.
    /// </summary>
    /// <param name="widget"></param>
    public void OnDeckButtonPressed(DeckWidget widget)
    {
        currentDeckButton = widget.Button;
        LoadDeck(widget.Deck);
    }

    /// <summary>
    /// Loads the specified deck information.
    /// </summary>
    /// <param name="deck">Deck to load.</param>
    private void LoadDeck(Deck deck)
    {
        currentDeck = deck;
        DeckNameInputField.text = currentDeck.Name;
        UpdateDeckSizeText();
        SetDefaultDeckButton.interactable = deckCollection.Decks.FindIndex(x => x == deck) != deckCollection.DefaultDeck;

        foreach (Transform child in DeckDetailScrollView.transform)
            Destroy(child.gameObject);

        foreach (var card in currentDeck.Cards)
        {
            var go = Instantiate(CardInfoWidgetPrefab) as GameObject;
            go.transform.SetParent(DeckDetailScrollView.transform, false);
            var cardInfo = go.GetComponent<CardInfoWidget>();
            cardInfo.Deck = currentDeck;
            cardInfo.Card = GameManager.Instance.GetCard(card.Id);
            cardInfo.NumCopies = GetNumCopiesOfCardInPlayerCollection(card.Id);
            cardInfo.Count = card.Count;
        }
    }

    /// <summary>
    /// Deck name input field callback.
    /// </summary>
    public void OnDeckNameInputFieldValueChanged()
    {
        if (currentDeckButton != null)
            currentDeckButton.transform.Find("Text").GetComponent<Text>().text = DeckNameInputField.text;
        currentDeck.Name = DeckNameInputField.text;
    }

    /// <summary>
    /// Remove deck button callback.
    /// </summary>
    public void OnRemoveDeckButtonPressed()
    {
        if (deckCollection.Decks.Count > 1)
            WindowUtils.OpenConfirmationDialog("Do you really want to remove this deck?",
                () => { RemoveCurrentDeck(); });
        else
            WindowUtils.OpenAlertDialog("You need to have at least one deck in your collection.");
    }

    /// <summary>
    /// Removes the current deck from the player's collection.
    /// </summary>
    private void RemoveCurrentDeck()
    {
        if (currentDeckButton != null)
            Destroy(currentDeckButton.transform.parent.gameObject);

        if (currentDeck != null)
        {
            // Replace default deck if needed.
            if (deckCollection.Decks.FindIndex(x => x == currentDeck) == deckCollection.DefaultDeck)
                deckCollection.DefaultDeck = 0;
            deckCollection.Decks.Remove(currentDeck);
            LoadDeck(deckCollection.Decks[0]);
        }
    }

    /// <summary>
    /// Add card button callback.
    /// </summary>
    public void OnAddCardButtonPressed()
    {
        if (currentDeck == null)
            return;

        foreach (var toggle in CardLibraryScrollView.GetComponent<ToggleGroup>().ActiveToggles())
        {
            var card = toggle.transform.parent.gameObject.GetComponent<LibraryCardWidget>().Card;

            var found = false;
            foreach (Transform child in DeckDetailScrollView.transform)
            {
                var widget = child.GetComponent<CardInfoWidget>();
                if (widget.Card == card)
                {
                    if (widget.Count + 1 > card.MaxCopies)
                    {
                        WindowUtils.OpenAlertDialog("You cannot have more than " + card.MaxCopies + " copies of this card in your deck.");
                        return;
                    }

                    if (widget.Count >= GetNumCopiesOfCardInPlayerCollection(card.Id))
                    {
                        WindowUtils.OpenAlertDialog("You have no more copies of this card in your collection.");
                        return;
                    }

                    widget.Count += 1;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                if (card.MaxCopies <= 0)
                {
                    WindowUtils.OpenAlertDialog("You cannot have more than " + card.MaxCopies + " copies of this card in your deck.");
                    return;
                }

                if (GetNumCopiesOfCardInPlayerCollection(card.Id) <= 0)
                {
                    WindowUtils.OpenAlertDialog("You have no more copies of this card in your collection.");
                    return;
                }

                var go = Instantiate(CardInfoWidgetPrefab) as GameObject;
                go.transform.SetParent(DeckDetailScrollView.transform, false);
                var widget = go.GetComponent<CardInfoWidget>();
                widget.Deck = currentDeck;
                widget.Card = card;
                widget.NumCopies = GetNumCopiesOfCardInPlayerCollection(card.Id);
                widget.Count = 1;
            }
        }

        UpdateDeckSizeText();
    }

    /// <summary>
    /// Returns the number of copies of the specified card in the player's collection.
    /// </summary>
    /// <param name="cardId">Unique identifier of the card.</param>
    /// <returns>The number of copies of the specified card in the player's collection.</returns>
    private int GetNumCopiesOfCardInPlayerCollection(int cardId)
    {
        var numCopies = 0;
        foreach (var collectionCard in playerCardCollection)
        {
            if (collectionCard.id == cardId)
            {
                numCopies = collectionCard.numCopies;
                break;
            }
        }
        return numCopies;
    }

    /// <summary>
    /// Updates the card preview based on the currently selected card.
    /// </summary>
    /// <param name="card"></param>
    public void UpdateCardPreview(Card card)
    {
        CardPreview.SetActive(true);
        CardPreview.GetComponent<CardPreview>().SetCardData(card);
    }

    /// <summary>
    /// Updates the deck size UI text.
    /// </summary>
    public void UpdateDeckSizeText()
    {
        DeckSizeText.text = currentDeck.Size.ToString();

        var numSpells = 0;
        var numCreatures = 0;
        foreach (var cardInfo in currentDeck.Cards)
        {
            var card = GameManager.Instance.GetCard(cardInfo.Id);
            if (card.Definition == "Spell")
                numSpells += cardInfo.Count;
            else if (card.Definition == "Creature")
                numCreatures += cardInfo.Count;
        }
        SpellCountText.text = numSpells.ToString();
        CreatureCountText.text = numCreatures.ToString();
    }

    /// <summary>
    /// Set default deck button callback.
    /// </summary>
    public void OnSetDefaultDeckButtonPressed()
    {
        if (currentDeck == null)
            return;

        var idx = deckCollection.Decks.FindIndex(x => x == currentDeck);
        if (idx != -1)
            deckCollection.DefaultDeck = idx;

        SetDefaultDeckButton.interactable = false;
    }

    /// <summary>
    /// Back button callback.
    /// </summary>
    public void OnBackButtonPressed()
    {
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Save button callback.
    /// </summary>
    public void OnSaveButtonPressed()
    {
        // Check if the deck sizes are legal according to the game configuration.
        var minDeckSize = GameManager.Instance.Config.Properties.MinDeckSize;
        var maxDeckSize = GameManager.Instance.Config.Properties.MaxDeckSize;
        foreach (var deck in deckCollection.Decks)
        {
            if (deck.Size < minDeckSize)
            {
                WindowUtils.OpenAlertDialog("Your deck '" + deck.Name + "' is not valid because it contains less than " + minDeckSize + " cards.");
                return;
            }
            else if (deck.Size > maxDeckSize)
            {
                WindowUtils.OpenAlertDialog("Your deck '" + deck.Name + "' is not valid because it contains more than " + maxDeckSize + " cards.");
                return;
            }
        }

        // Save player decks to a persistent path.
        var json = JsonUtility.ToJson(deckCollection);
        var file = new StreamWriter(decksDirPath + "/decks.json");
        file.WriteLine(json);
        file.Close();

        // Also save the default player deck to PlayerPrefs so it can be easily retrieved later
        // when playing a game.
        var defaultDeck = deckCollection.Decks[deckCollection.DefaultDeck];
        PlayerPrefs.SetString("CCGKitv07_DefaultDeck", JsonUtility.ToJson(defaultDeck));
    }
}
