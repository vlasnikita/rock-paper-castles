// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

/// <summary>
/// This widget is used in the deck editor scene to select a certain deck from the player's
/// collection of decks.
/// </summary>
public class DeckWidget : MonoBehaviour
{
    /// <summary>
    /// Button that allows to select this deck.
    /// </summary>
    public Button Button;

    /// <summary>
    /// The underlying deck.
    /// </summary>
    [HideInInspector]
    public Deck Deck;

    private void Awake()
    {
        Assert.IsTrue(Button != null);
    }

    /// <summary>
    /// Button callback.
    /// </summary>
    public void OnButtonPressed()
    {
        var scene = GameObject.Find("DeckEditorScene").GetComponent<DeckEditorScene>();
        scene.OnDeckButtonPressed(this);
    }
}
