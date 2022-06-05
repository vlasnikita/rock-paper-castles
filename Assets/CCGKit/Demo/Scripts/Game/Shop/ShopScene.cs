// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using CCGKit;

/// <summary>
/// This scene is where players buy and open new card packs to evolve their card collections.
/// </summary>
public class ShopScene : MonoBehaviour
{
    public Text CurrencyText;
    public Text OpenCardPackText;
    public Button OpenCardPackButton;

    private static readonly int cardPackPrice = 50;

    private void Awake()
    {
        Assert.IsTrue(CurrencyText != null);
        Assert.IsTrue(OpenCardPackText != null);
        Assert.IsTrue(OpenCardPackButton != null);

        CurrencyText.text = "";
    }

    private void Start()
    {
        StartCoroutine(PlayerServer.GetCurrency(GameManager.Instance.AuthToken,
            (response) =>
            {
                if (response.status == "success")
                {
                    GameManager.Instance.Currency = response.currency;
                    UpdateCurrencyText();
                }
                else if (response.status == "error")
                {
                    WindowUtils.OpenAlertDialog("There was an error.");
                }
            },
            (error) =>
            {
                WindowUtils.OpenAlertDialog("Connection error. Please check your network.");
            }));
        UpdateOpenCardPackUI();
    }

    private void UpdateCurrencyText()
    {
        CurrencyText.text = "You have: " + GameManager.Instance.Currency + " currency";
    }

    private void UpdateOpenCardPackUI()
    {
        OpenCardPackText.text = "Unopened card packs: " + GameManager.Instance.NumUnopenedCardPacks.ToString();
        OpenCardPackButton.interactable = GameManager.Instance.NumUnopenedCardPacks > 0;
    }

    /// <summary>
    /// Buy card pack button callback.
    /// </summary>
    public void OnBuyCardPackButtonPressed()
    {
        if (GameManager.Instance.Currency < cardPackPrice)
        {
            WindowUtils.OpenAlertDialog("Insufficient funds.");
        }
        else
        {
            WindowUtils.OpenConfirmationDialog("Do you really want to buy a card pack?",
                () =>
                {
                    BuyCardPack();
                });
        }
    }

    /// <summary>
    /// Helper function that actually buys a card pack.
    /// </summary>
    private void BuyCardPack()
    {
        StartCoroutine(PlayerServer.BuyCardPack(GameManager.Instance.AuthToken, 1,
            (response) =>
            {
                if (response.status == "success")
                {
                    GameManager.Instance.NumUnopenedCardPacks = response.numUnopenedPacks;
                    GameManager.Instance.Currency = response.currency;
                    UpdateCurrencyText();
                    UpdateOpenCardPackUI();
                    WindowUtils.OpenAlertDialog("Good luck!");
                }
                else if (response.status == "error")
                {
                    WindowUtils.OpenAlertDialog("There was an error buying the card pack.");
                }
            },
            (error) =>
            {
                WindowUtils.OpenAlertDialog("Connection error. Please check your network.");
            }));
    }

    /// <summary>
    /// Open card pack button callback.
    /// </summary>
    public void OnOpenCardPackButtonPressed()
    {
        WindowUtils.OpenConfirmationDialog("Do you really want to open a card pack?",
            () =>
            {
                OpenCardPack();
            });
    }

    /// <summary>
    /// Helper function that actually opens a card pack.
    /// </summary>
    private void OpenCardPack()
    {
        StartCoroutine(PlayerServer.OpenCardPack(GameManager.Instance.AuthToken,
            (response) =>
            {
                if (response.status == "success")
                {
                    GameManager.Instance.NumUnopenedCardPacks = response.numUnopenedPacks;
                    UpdateOpenCardPackUI();
                    GameWindowUtils.OpenCardPackDialog(response.cards);
                }
                else if (response.status == "error")
                {
                    WindowUtils.OpenAlertDialog("There was an error opening the card pack.");
                }
            },
            (error) =>
            {
                WindowUtils.OpenAlertDialog("Connection error. Please check your network.");
            }));
    }

    /// <summary>
    /// Back button callback.
    /// </summary>
    public void OnBackButtonPressed()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
