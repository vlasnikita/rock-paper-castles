// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Collections.Generic;

/// <summary>
/// Stores the per-card information in a single player deck, which is a pair consisting of the
/// card's unique identifier and the number of occurrences of said card within the deck.
/// </summary>
[Serializable]
public class DeckCardInfo
{
    /// <summary>
    /// Unique identifier of this card.
    /// </summary>
    public int Id;

    /// <summary>
    /// Number of occurrences of this card within the deck.
    /// </summary>
    public int Count;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="id">Unique identifier of this card.</param>
    /// <param name="count">Number of occurrences of this card within the deck.</param>
    public DeckCardInfo(int id, int count)
    {
        Id = id;
        Count = count;
    }
}

/// <summary>
/// Holds information about a player's single deck.
/// </summary>
[Serializable]
public class Deck
{
    /// <summary>
    /// Name of this deck.
    /// </summary>
    public string Name = "Unnamed deck";

    /// <summary>
    /// Stores the cards of this deck.
    /// </summary>
    public List<DeckCardInfo> Cards = new List<DeckCardInfo>();

    /// <summary>
    /// The size (in number of cards) of this deck.
    /// </summary>
    public int Size
    {
        get
        {
            var size = 0;
            foreach (var card in Cards)
                size += card.Count;
            return size;
        }
    }

    /// <summary>
    /// Adds a new card to this deck.
    /// </summary>
    /// <param name="id">Unique identifier of the card to add to the deck.</param>
    public void AddCard(int id)
    {
        var info = Cards.Find(x => x.Id == id);
        if (info == null)
            Cards.Add(new DeckCardInfo(id, 1));
        else
            info.Count += 1;
    }

    /// <summary>
    /// Removes an existing card from this deck.
    /// </summary>
    /// <param name="id">Unique identifier of the card to remove from the deck.</param>
    public void RemoveCard(int id)
    {
        var info = Cards.Find(x => x.Id == id);
        if (info != null)
        {
            info.Count -= 1;
            if (info.Count <= 0)
                Cards.Remove(info);
        }
    }

    /// <summary>
    /// Override the number of ocurrences of the card with the specified id in this deck.
    /// </summary>
    /// <param name="id">Unique identifier of the card.</param>
    /// <param name="count">New number of occurrences of the card within the deck.</param>
    public void SetCardCount(int id, int count)
    {
        var info = Cards.Find(x => x.Id == id);
        if (info == null)
            Cards.Add(new DeckCardInfo(id, count));
        else
            info.Count = count;
    }
}

/// <summary>
/// This class manages the decks of the player, which are persisted to a disk location in
/// a JSON file using the built-in serialization facilities provided by Unity.
///
/// A player may edit his deck collection via the DeckEditor scene.
/// </summary>
[Serializable]
public class DeckCollection
{
    /// <summary>
    /// Stores the decks that form this collection.
    /// </summary>
    public List<Deck> Decks = new List<Deck>();

    /// <summary>
    /// Index of the deck that is currently set as the default deck for the player.
    /// </summary>
    public int DefaultDeck;
}
