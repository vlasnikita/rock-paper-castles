// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using CCGKit;

/// <summary>
/// For every scene in the demo, we create a specific game object to handle the user-interface logic
/// belonging to that scene. The main menu scene just contains the button delegates that trigger
/// transitions to new scenes or exit the game.
/// </summary>
public class MainMenuScene : MonoBehaviour
{
    public Text PlayerInfoText;

    private static bool isFirstLaunch = true;

    private void Awake()
    {
        Assert.IsTrue(PlayerInfoText != null);

        if (!string.IsNullOrEmpty(GameManager.Instance.PlayerName))
            PlayerInfoText.text = "Logged in as: " + GameManager.Instance.PlayerName;
    }

    private void Start()
    {
        // Create a default deck if none is found.
        var defaultDeckJson = PlayerPrefs.GetString("CCGKitv07_DefaultDeck");
        var defaultDeck = JsonUtility.FromJson<Deck>(defaultDeckJson);
        if (defaultDeck == null)
        {
            var defaultDeckTextAsset = Resources.Load<TextAsset>("DefaultDeck");
            if (defaultDeckTextAsset != null)
                PlayerPrefs.SetString("CCGKitv07_DefaultDeck", defaultDeckTextAsset.text);
        }

        if (isFirstLaunch)
        {
            isFirstLaunch = false;
            if (!GameManager.Instance.PlayerLoggedIn)
                WindowManager.Instance.OpenWindow("PlayerLoginDialog");
        }
    }

    /// <summary>
    /// Play button callback.
    /// </summary>
    public void OnPlayButtonPressed()
    {
        SceneManager.LoadScene("Lobby");
    }

    /// <summary>
    /// Deck editor button callback.
    /// </summary>
    public void OnDecksButtonPressed()
    {
        if (GameManager.Instance.PlayerLoggedIn)
            SceneManager.LoadScene("DeckEditor");
        else
            WindowUtils.OpenAlertDialog("You need to be logged in to be able to access the deck editor.");
    }

    /// <summary>
    /// Ranking button callback.
    /// </summary>
    public void OnRankingButtonPressed()
    {
        if (GameManager.Instance.PlayerLoggedIn)
            SceneManager.LoadScene("Ranking");
        else
            WindowUtils.OpenAlertDialog("You need to be logged in to be able to access the ranking.");
    }

    /// <summary>
    /// Shop button callback.
    /// </summary>
    public void OnShopButtonPressed()
    {
        if (GameManager.Instance.PlayerLoggedIn)
            SceneManager.LoadScene("Shop");
        else
            WindowUtils.OpenAlertDialog("You need to be logged in to be able to access the shop.");
    }

    /// <summary>
    /// Exit game button callback.
    /// </summary>
    public void OnExitGameButtonPressed()
    {
        WindowUtils.OpenConfirmationDialog("Do you really want to exit the game?",
            () => { Application.Quit(); });
    }

    public void SetPlayerInfoText(string text)
    {
        PlayerInfoText.text = text;
    }
}
