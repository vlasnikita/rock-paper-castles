// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking.Match;
using UnityEngine.UI;

using CCGKit;

/// <summary>
/// This button is used in the lobby scene's match list and it is used to join a multiplayer game.
/// </summary>
public class JoinMatchButton : MonoBehaviour
{
    public Image PrivateMatchImage;

    /// <summary>
    /// The underlying multiplayer match information.
    /// </summary>
    public MatchInfoSnapshot MatchInfo;

    private void Awake()
    {
        Assert.IsTrue(PrivateMatchImage != null);
    }

    /// <summary>
    /// Button callback.
    /// </summary>
    public void OnPressed()
    {
        if (PrivateMatchImage.gameObject.activeSelf)
            GameWindowUtils.OpenPasswordEntryDialog(password => { JoinMatch(password); });
        else
            JoinMatch();
    }

    private void JoinMatch(string password = "")
    {
        WindowUtils.OpenLoadingDialog("Joining game");
        GameNetworkManager.Instance.matchMaker.JoinMatch(MatchInfo.networkId, password, string.Empty, string.Empty, 0, 0, OnMatchJoined);
    }

    /// <summary>
    /// Callback called when a UNET match is joined.
    /// </summary>
    /// <param name="success">Indicates if the request succeeded.</param>
    /// <param name="extendedInfo">If success is false, this will contain a text string indicating the reason.</param>
    /// <param name="responseData">The generic passed in containing data required by the callback. This typically contains data returned from a call to the service backend.</param>
    public void OnMatchJoined(bool success, string extendedInfo, MatchInfo responseData)
    {
        if (success)
        {
            GameNetworkManager.Instance.OnMatchJoined(success, extendedInfo, responseData);
        }
        else
        {
            WindowManager.Instance.CloseWindow("PasswordEntryDialog");
            WindowManager.Instance.CloseWindow("LoadingDialog");
            WindowUtils.OpenAlertDialog("Could not join match.");
        }
    }
}
