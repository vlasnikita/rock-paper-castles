// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

using CCGKit;

/// <summary>
/// This widget is used in the deck editor scene to select a certain card from the game's
/// card library.
/// </summary>
public class LibraryCardWidget : MonoBehaviour
{
    /// <summary>
    /// Toggle that allows to select this card.
    /// </summary>
    public Toggle Toggle;

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

    private void Awake()
    {
        Assert.IsTrue(Toggle != null);
    }

    /// <summary>
    /// Toggle callback.
    /// </summary>
    public void OnTogglePressed()
    {
        if (Toggle.isOn)
        {
            var scene = GameObject.Find("DeckEditorScene").GetComponent<DeckEditorScene>();
            scene.UpdateCardPreview(Card);
        }
    }
}
