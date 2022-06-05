// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

using CCGKit;

/// <summary>
/// Holds information about a player login dialog.
/// </summary>
public class PlayerLoginDialog : Window
{
    public InputField EmailInputField;
    public InputField PasswordInputField;

    private void Awake()
    {
        Assert.IsTrue(EmailInputField != null);
        Assert.IsTrue(PasswordInputField != null);
        windowName = "PlayerLoginDialog";
    }

    /// <summary>
    /// Offline button callback.
    /// </summary>
    public void OnOfflineButtonPressed()
    {
        var mainMenuScene = GameObject.Find("MainMenuScene");
        if (mainMenuScene != null)
            mainMenuScene.GetComponent<MainMenuScene>().SetPlayerInfoText("Guest mode");
        Close();
    }

    /// <summary>
    /// Register button callback.
    /// </summary>
    public void OnRegisterButtonPressed()
    {
        WindowManager.Instance.OpenWindow("PlayerRegisterDialog");
    }

    /// <summary>
    /// Accept button callback.
    /// </summary>
    public void OnAcceptButtonPressed()
    {
        StartCoroutine(PlayerServer.LoginPlayer(EmailInputField.text, PasswordInputField.text,
            (response) =>
            {
                if (response.status == "success")
                {
                    GameManager.Instance.PlayerLoggedIn = true;
                    GameManager.Instance.PlayerName = response.username;
                    GameManager.Instance.AuthToken = response.token;
                    GameManager.Instance.NumUnopenedCardPacks = response.numUnopenedCardPacks;
                    GameManager.Instance.Currency = response.currency;
                    var mainMenuScene = GameObject.Find("MainMenuScene");
                    if (mainMenuScene != null)
                        mainMenuScene.GetComponent<MainMenuScene>().SetPlayerInfoText("Logged in as: " + response.username);
                    Close();
                }
                else if (response.status == "error")
                {
                    if (!string.IsNullOrEmpty(response.message))
                        WindowUtils.OpenAlertDialog("There was an error: " + response.message);
                    else
                        WindowUtils.OpenAlertDialog("There was an error.");
                }
            },
            (error) =>
            {
                WindowUtils.OpenAlertDialog("Connection error. Please check your network.");
            }));
    }
}
