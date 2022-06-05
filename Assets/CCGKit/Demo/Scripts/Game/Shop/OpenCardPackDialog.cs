// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

using CCGKit;

/// <summary>
/// Holds information about the dialog that is displayed when a player opens one of his card
/// packs.
/// </summary>
public class OpenCardPackDialog : Window
{
    public List<CardPreview> CardPreviews;

    private void Awake()
    {
        windowName = "OpenCardPackDialog";
    }

    public void SetCardData(List<int> cardIds)
    {
        for (var i = 0; i < cardIds.Count; i++)
        {
            var card = GameManager.Instance.GetCard(cardIds[i]);
            CardPreviews[i].SetCardData(card);
        }
    }
}
