// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

using CCGKit;

/// <summary>
/// Holds information about the loading dialog displayed when a game is about to get started.
/// </summary>
public class WaitGameStartDialog : Window
{
    /// <summary>
    /// Text UI component.
    /// </summary>
    public Text TextUI;

    /// <summary>
    /// Text to display inside the dialog window.
    /// </summary>
    [HideInInspector]
    private string text;

    public string Text
    {
        get
        {
            return text;
        }
        set
        {
            text = value;
            TextUI.text = text;
        }
    }

    private void Awake()
    {
        Assert.IsTrue(TextUI != null);
        windowName = "WaitGameStartDialog";
    }

    private void Start()
    {
        StartCoroutine(UpdateLoadingText());
    }

    private IEnumerator UpdateLoadingText()
    {
        while (true)
        {
            TextUI.text = text + "...";
            yield return new WaitForSeconds(1.0f);
            TextUI.text = text + "....";
            yield return new WaitForSeconds(1.0f);
            TextUI.text = text + ".....";
            yield return new WaitForSeconds(1.0f);
        }
    }

    /// <summary>
    /// Accept button callback.
    /// </summary>
    public void OnAcceptButtonPressed()
    {
        if (NetworkingUtils.GetLocalPlayer().isServer)
            GameNetworkManager.Instance.StopHost();
        else
            GameNetworkManager.Instance.StopClient();
    }
}
