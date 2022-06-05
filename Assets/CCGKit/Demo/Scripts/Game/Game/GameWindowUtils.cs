// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Collections.Generic;

using UnityEngine;

using CCGKit;

/// <summary>
/// Provides high-level utilities for managing game-specific types of windows.
/// </summary>
public static class GameWindowUtils
{
    /// <summary>
    /// Opens a new password entry dialog.
    /// </summary>
    /// <param name="onAccept">Callback to invoke when the accept button on the dialog is pressed.</param>
    public static void OpenPasswordEntryDialog(Action<string> onAccept)
    {
        WindowManager.Instance.OpenWindow("PasswordEntryDialog",
            () =>
            {
                var endGameDialog = GameObject.Find("PasswordEntryDialog").GetComponent<PasswordEntryDialog>();
                endGameDialog.OnAccept = onAccept;
            });
    }

    /// <summary>
    /// Opens a new dialog that displays the cards opened from a card pack.
    /// </summary>
    /// <param name="cardIds">Unique identifiers of the cards opened in the card pack.</param>
    public static void OpenCardPackDialog(List<int> cardIds)
    {
        WindowManager.Instance.OpenWindow("OpenCardPackDialog",
            () =>
            {
                var openCardPackDialog = GameObject.Find("OpenCardPackDialog").GetComponent<OpenCardPackDialog>();
                openCardPackDialog.SetCardData(cardIds);
            });
    }

    /// <summary>
    /// Opens a new loading dialog for when the game is about to get started.
    /// </summary>
    public static void OpenWaitGameStartDialog()
    {
        WindowManager.Instance.OpenWindow("WaitGameStartDialog",
            () =>
            {
                var waitingDialog = GameObject.Find("WaitGameStartDialog").GetComponent<WaitGameStartDialog>();
                waitingDialog.Text = "Waiting for game to start";
            });
    }

    /// <summary>
    /// Opens a new card detail window that shows a zoomed view of the specified card.
    /// </summary>
    public static void OpenCardDetailWindow(Card card)
    {
        WindowManager.Instance.OpenWindow("CardDetailWindow",
            () =>
            {
                var cardDetailWindow = GameObject.Find("CardDetailWindow").GetComponent<CardDetailWindow>();
                cardDetailWindow.SetCardData(card);
            });
    }

    /// <summary>
    /// Opens a new end game dialog.
    /// </summary>
    /// <param name="win">True if the dialog must display a winning message; false otherwise.</param>
    public static void OpenEndGameDialog(bool win)
    {
        WindowManager.Instance.OpenWindow("EndGameDialog",
            () =>
            {
                var endGameDialog = GameObject.Find("EndGameDialog").GetComponent<EndGameDialog>();
                if (win)
                {
                    endGameDialog.TitleText = "You win!";
                    endGameDialog.BodyText = "You are a powerful wizard.";
                }
                else
                {
                    endGameDialog.TitleText = "You lose!";
                    endGameDialog.BodyText = "You need to improve your wizardry...";
                }
                endGameDialog.OnAccept = () =>
                {
                    if (NetworkingUtils.GetLocalPlayer().isServer)
                        GameNetworkManager.Instance.StopHost();
                    else
                        GameNetworkManager.Instance.StopClient();
                };
            });
    }
}
