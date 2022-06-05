// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.UI;

using CCGKit;

/// <summary>
/// This widget is used in the deck editor scene to show the information of a specific card in
/// the selected deck. It contains two buttons that can be used to easily modify the number of
/// occurrences of the card in the deck.
/// </summary>
public class CardInfoWidget : MonoBehaviour
{
    /// <summary>
    /// The deck where this card belongs to.
    /// </summary>
    [HideInInspector]
    public Deck Deck;

    /// <summary>
    /// The underlying card data.
    /// </summary>
    [HideInInspector]
    public Card Card;

    /// <summary>
    /// The number of copies of this card the player owns in his collection.
    /// </summary>
    [HideInInspector]
    public int NumCopies;

    /// <summary>
    /// The current number of occurrences of this card within the deck.
    /// </summary>
    private int count;

    public int Count
    {
        get { return count; }
        set
        {
            count = value;
            CountText.text = Count.ToString();
            Deck.SetCardCount(Card.Id, count);

            if (value <= 0)
                Destroy(gameObject);
        }
    }

    /// <summary>
    /// Text to display this card's count.
    /// </summary>
    public Text CountText;

    /// <summary>
    /// Button to display this card's name.
    /// </summary>
    public Button CardButton;

    private void Start()
    {
        CardButton.interactable = false;
        CardButton.transform.Find("Text").GetComponent<Text>().text = Card.Name;
    }

    /// <summary>
    /// Add card button callback.
    /// </summary>
    public void OnAddCardButtonPressed()
    {
        // Limit the maximum number of occurrences of a given card within a deck
        // to the number specified in the editor, for gameplay balancing reasons.
        if (Count + 1 > Card.MaxCopies)
        {
            WindowUtils.OpenAlertDialog("You cannot have more than " + Card.MaxCopies + " copies of this card in your deck.");
            return;
        }

        // Limit the maximum number of occurrences of a given card within a deck
        // to the number of copies the player owns in his collection.
        if (Count + 1 > NumCopies)
        {
            WindowUtils.OpenAlertDialog("You have no more copies of this card in your collection.");
            return;
        }

        ++Count;

        var scene = GameObject.Find("DeckEditorScene").GetComponent<DeckEditorScene>();
        scene.UpdateDeckSizeText();
    }

    /// <summary>
    /// Remove card button callback.
    /// </summary>
    public void OnRemoveCardButtonPressed()
    {
        --Count;

        var scene = GameObject.Find("DeckEditorScene").GetComponent<DeckEditorScene>();
        scene.UpdateDeckSizeText();
    }
}
