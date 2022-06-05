// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking.Match;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using CCGKit;

/// <summary>
/// This scene allows the player to create or join a new multiplayer game.
/// </summary>
public class LobbyScene : MonoBehaviour
{
    public GameObject MatchListContent;
    public GameObject MatchButtonPrefab;

    public Button PlayNowButton;
    public Button CreateMatchButton;
    public Button FindMatchesButton;

    public InputField RoomNameInputField;
    public Toggle PasswordProtectedToggle;

    private void Awake()
    {
        Assert.IsTrue(MatchListContent != null);
        Assert.IsTrue(MatchButtonPrefab != null);
        Assert.IsTrue(PlayNowButton != null);
        Assert.IsTrue(CreateMatchButton != null);
        Assert.IsTrue(FindMatchesButton != null);
        Assert.IsTrue(RoomNameInputField != null);
        Assert.IsTrue(PasswordProtectedToggle != null);
    }

    private void Start()
    {
        GameNetworkManager.Instance.StartMatchMaker();

        var playerName = GameManager.Instance.PlayerName;
        if (!string.IsNullOrEmpty(playerName))
            RoomNameInputField.text = playerName + "'s game";
    }

    /// <summary>
    /// Play now button callback.
    /// </summary>
    public void OnPlayNowButtonPressed()
    {
        GameNetworkManager.Instance.IsSinglePlayer = false;
        PlayNow();
    }

    /// <summary>
    /// Create match button callback.
    /// </summary>
    public void OnCreateMatchButtonPressed()
    {
        GameNetworkManager.Instance.IsSinglePlayer = false;
        CreateMatch();
    }

    /// <summary>
    /// Find matches button callback.
    /// </summary>
    public void OnFindMatchesButtonPressed()
    {
        GameNetworkManager.Instance.IsSinglePlayer = false;
        FindMatches();
    }

    /// <summary>
    /// Create LAN match button callback.
    /// </summary>
    public void OnCreateLANMatchButtonPressed()
    {
        GameNetworkManager.Instance.IsSinglePlayer = false;
        GameNetworkManager.Instance.StartHost();
    }

    /// <summary>
    /// Join LAN match button callback.
    /// </summary>
    public void OnJoinLANMatchButtonPressed()
    {
        GameNetworkManager.Instance.IsSinglePlayer = false;
        GameNetworkManager.Instance.StartClient();
    }

    /// <summary>
    /// Create single-player match button callback.
    /// </summary>
    public void OnCreateSinglePlayerMatchButtonPressed(string deck)
    {
        GameNetworkManager.Instance.ActiveDeck = deck;
        GameNetworkManager.Instance.IsSinglePlayer = true;
        GameNetworkManager.Instance.StartHost();
    }

    /// <summary>
    /// Back button callback.
    /// </summary>
    public void OnBackButtonPressed()
    {
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Joins an existing game or creates a new one if none was found.
    /// </summary>
    private void PlayNow()
    {
        WindowUtils.OpenLoadingDialog("Preparing to play...");
        PlayNowButton.interactable = false;
        GameNetworkManager.Instance.matchMaker.ListMatches(0, 10, string.Empty, false, 0, 0, OnPlayNowMatchList);
    }

    /// <summary>
    /// Creates a new UNET match.
    /// </summary>
    private void CreateMatch()
    {
        if (PasswordProtectedToggle.isOn)
            GameWindowUtils.OpenPasswordEntryDialog(password => ActuallyCreateMatch(password));
        else
            ActuallyCreateMatch();
    }

    private void ActuallyCreateMatch(string password = "")
    {
        WindowUtils.OpenLoadingDialog("Creating new game");
        CreateMatchButton.interactable = false;
        var roomName = RoomNameInputField.text;
        if (string.IsNullOrEmpty(roomName))
            roomName = "Game room";
        GameNetworkManager.Instance.matchMaker.CreateMatch(roomName, 2, true, password, string.Empty, string.Empty, 0, 0, OnMatchCreate);
    }

    /// <summary>
    /// Finds all available UNET matches.
    /// </summary>
    private void FindMatches()
    {
        WindowUtils.OpenLoadingDialog("Finding games");
        FindMatchesButton.interactable = false;
        GameNetworkManager.Instance.matchMaker.ListMatches(0, 10, string.Empty, false, 0, 0, OnMatchList);
    }

    /// <summary>
    /// Callback called when a new UNET match is created.
    /// </summary>
    /// <param name="success">Indicates if the request succeeded.</param>
    /// <param name="extendedInfo">If success is false, this will contain a text string indicating the reason.</param>
    /// <param name="responseData">The generic passed in containing data required by the callback. This typically contains data returned from a call to the service backend.</param>
    private void OnMatchCreate(bool success, string extendedInfo, MatchInfo responseData)
    {
        CreateMatchButton.interactable = true;
        if (success)
            GameNetworkManager.Instance.OnMatchCreate(success, extendedInfo, responseData);
        else
            WindowUtils.OpenAlertDialog("Could not create match.");
    }

    /// <summary>
    /// Callback called when a new list of matches is received.
    /// </summary>
    /// <param name="success">Indicates if the request succeeded.</param>
    /// <param name="extendedInfo">If success is false, this will contain a text string indicating the reason.</param>
    /// <param name="responseData">The generic passed in containing data required by the callback. This typically contains data returned from a call to the service backend.</param>
    private void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> responseData)
    {
        WindowManager.Instance.CloseWindow("LoadingDialog");

        GameNetworkManager.Instance.OnMatchList(success, extendedInfo, responseData);

        foreach (Transform child in MatchListContent.transform)
            Destroy(child.gameObject);

        foreach (var match in responseData)
        {
            if (match.currentSize > 0)
            {
                var go = Instantiate(MatchButtonPrefab) as GameObject;
                go.transform.SetParent(MatchListContent.transform, false);
                go.transform.Find("Text").GetComponent<Text>().text = "Join '" + match.name + "'";
                var matchButton = go.GetComponent<JoinMatchButton>();
                matchButton.PrivateMatchImage.gameObject.SetActive(match.isPrivate);
                matchButton.MatchInfo = match;
            }
        }

        FindMatchesButton.interactable = true;
    }

    /// <summary>
    /// Callback called when a new list of matches is received.
    /// </summary>
    /// <param name="success">Indicates if the request succeeded.</param>
    /// <param name="extendedInfo">If success is false, this will contain a text string indicating the reason.</param>
    /// <param name="responseData">The generic passed in containing data required by the callback. This typically contains data returned from a call to the service backend.</param>
    private void OnPlayNowMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> responseData)
    {
        GameNetworkManager.Instance.OnMatchList(success, extendedInfo, responseData);

        var foundExistingGame = false;
        foreach (var match in responseData)
        {
            if (match.currentSize > 0)
            {
                foundExistingGame = true;
                GameNetworkManager.Instance.matchMaker.JoinMatch(match.networkId, "", string.Empty, string.Empty, 0, 0, OnPlayNowMatchJoined);
                break;
            }
        }

        if (!foundExistingGame)
            GameNetworkManager.Instance.matchMaker.CreateMatch("Game room", 2, true, "", string.Empty, string.Empty, 0, 0, OnPlayNowMatchCreate);
    }

    /// <summary>
    /// Callback called when a UNET match is joined.
    /// </summary>
    /// <param name="success">Indicates if the request succeeded.</param>
    /// <param name="extendedInfo">If success is false, this will contain a text string indicating the reason.</param>
    /// <param name="responseData">The generic passed in containing data required by the callback. This typically contains data returned from a call to the service backend.</param>
    public void OnPlayNowMatchJoined(bool success, string extendedInfo, MatchInfo responseData)
    {
        PlayNowButton.interactable = true;
        if (success)
        {
            GameNetworkManager.Instance.OnMatchJoined(success, extendedInfo, responseData);
        }
        else
        {
            WindowManager.Instance.CloseWindow("LoadingDialog");
            WindowUtils.OpenAlertDialog("Could not join match.");
        }
    }

    /// <summary>
    /// Callback called when a new UNET match is created.
    /// </summary>
    /// <param name="success">Indicates if the request succeeded.</param>
    /// <param name="extendedInfo">If success is false, this will contain a text string indicating the reason.</param>
    /// <param name="responseData">The generic passed in containing data required by the callback. This typically contains data returned from a call to the service backend.</param>
    private void OnPlayNowMatchCreate(bool success, string extendedInfo, MatchInfo responseData)
    {
        PlayNowButton.interactable = true;
        if (success)
        {
            GameNetworkManager.Instance.OnMatchCreate(success, extendedInfo, responseData);
        }
        else
        {
            WindowManager.Instance.CloseWindow("LoadingDialog");
            WindowUtils.OpenAlertDialog("Could not create match.");
        }
    }
}
