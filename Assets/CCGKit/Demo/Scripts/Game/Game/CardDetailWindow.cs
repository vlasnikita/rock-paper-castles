// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;

using CCGKit;

/// <summary>
/// Holds information about the card detail window that is displayed when right-clicking over
/// a card in the demo game.
/// </summary>
public class CardDetailWindow : Window
{
    public CardPreview CardPreview;

    private void Awake()
    {
        Assert.IsTrue(CardPreview != null);
        windowName = "CardDetailWindow";
    }

    public void SetCardData(Card card)
    {
        CardPreview.SetCardData(card);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            Close();
    }
}
