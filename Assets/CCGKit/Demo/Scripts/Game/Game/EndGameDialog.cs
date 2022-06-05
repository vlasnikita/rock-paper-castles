// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

using CCGKit;

/// <summary>
/// Holds information about an end-game dialog.
/// </summary>
public class EndGameDialog : Window
{
    /// <summary>
    /// Title text UI component.
    /// </summary>
    public Text TitleTextUI;

    /// <summary>
    /// Body text UI component;
    /// </summary>
    public Text BodyTextUI;

    /// <summary>
    /// Title to display inside the dialog window.
    /// </summary>
    [HideInInspector]
    private string titleText;

    public string TitleText
    {
        get
        {
            return titleText;
        }
        set
        {
            titleText = value;
            TitleTextUI.text = titleText;
        }
    }

    /// <summary>
    /// Text to display inside the dialog window.
    /// </summary>
    [HideInInspector]
    private string bodyText;

    public string BodyText
    {
        get
        {
            return bodyText;
        }
        set
        {
            bodyText = value;
            BodyTextUI.text = bodyText;
        }
    }

    /// <summary>
    /// Callback to execute when the dialog is accepted.
    /// </summary>
    public Action OnAccept;

    private void Awake()
    {
        Assert.IsTrue(TitleTextUI != null);
        Assert.IsTrue(BodyTextUI != null);
        windowName = "EndGameDialog";
    }

    /// <summary>
    /// Accept button callback.
    /// </summary>
    public void OnAcceptButtonPressed()
    {
        if (OnAccept != null)
            OnAccept();
        Close();
    }
}
