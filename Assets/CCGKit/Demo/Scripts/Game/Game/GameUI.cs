// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

/// <summary>
/// This class wraps the game scene's user interface and it is mostly updated when the server
/// sends updated information to the client.
/// </summary>
public class GameUI : MonoBehaviour
{
    public PlayerBox PlayerBoxTop;
    public PlayerBox PlayerBoxBottom;

    public Text TurnTimerTop;
    public Text TurnTimerBottom;

    public Text ManaText;

    public Button EndTurnButton;

    private void Awake()
    {
        Assert.IsTrue(PlayerBoxTop != null);
        Assert.IsTrue(PlayerBoxBottom != null);
        Assert.IsTrue(TurnTimerTop != null);
        Assert.IsTrue(TurnTimerBottom != null);
        Assert.IsTrue(ManaText != null);
        Assert.IsTrue(EndTurnButton != null);
    }

    private void Start()
    {
        TurnTimerTop.gameObject.SetActive(false);
        TurnTimerBottom.gameObject.SetActive(false);
    }

    public void SetPlayerNameTop(string text)
    {
        PlayerBoxTop.SetPlayerNameText(text);
    }

    public void SetPlayerNameBottom(string text)
    {
        PlayerBoxBottom.SetPlayerNameText(text);
    }

    public void SetLivesTop(int lives)
    {
        PlayerBoxTop.SetLivesText(lives);
    }

    public void SetLivesBottom(int lives)
    {
        PlayerBoxBottom.SetLivesText(lives);
    }

    public void SetDeckCardsTop(int cards)
    {
        PlayerBoxTop.SetNumCardsInDeckText(cards);
    }

    public void SetHandCardsTop(int cards)
    {
        PlayerBoxTop.SetNumCardsInHandText(cards);
    }

    public void SetDeadCardsTop(int cards)
    {
        PlayerBoxTop.SetNumCardsDeadText(cards);
    }

    public void SetDeckCardsBottom(int cards)
    {
        PlayerBoxBottom.SetNumCardsInDeckText(cards);
    }

    public void SetHandCardsBottom(int cards)
    {
        PlayerBoxBottom.SetNumCardsInHandText(cards);
    }

    public void SetDeadCardsBottom(int cards)
    {
        PlayerBoxBottom.SetNumCardsDeadText(cards);
    }

    public void UpdateTurnTimerTop(int seconds)
    {
        if (seconds <= 0)
        {
            TurnTimerTop.gameObject.SetActive(false);
        }
        else if (seconds <= 5)
        {
            TurnTimerTop.gameObject.SetActive(true);
            TurnTimerTop.text = seconds.ToString();
        }
        TurnTimerTop.text = seconds.ToString();
    }

    public void UpdateTurnTimerBottom(int seconds)
    {
        if (seconds <= 0)
        {
            TurnTimerBottom.gameObject.SetActive(false);
        }
        else if (seconds <= 5)
        {
            TurnTimerBottom.gameObject.SetActive(true);
            TurnTimerBottom.text = seconds.ToString();
        }
        TurnTimerBottom.text = seconds.ToString();
    }

    public void UpdateManaText(int mana)
    {
        ManaText.text = mana.ToString();
    }

    public void SetEndTurnButtonEnabled(bool enabled)
    {
        EndTurnButton.gameObject.SetActive(enabled);
    }
}
