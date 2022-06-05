// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine.Assertions;
using UnityEngine.UI;

using CCGKit;

/// <summary>
/// Holds information about a player register dialog.
/// </summary>
public class PlayerRegisterDialog : Window
{
    public InputField EmailInputField;
    public InputField NameInputField;
    public InputField PasswordInputField;

    private void Awake()
    {
        Assert.IsTrue(EmailInputField != null);
        Assert.IsTrue(NameInputField != null);
        Assert.IsTrue(PasswordInputField != null);
        windowName = "PlayerRegisterDialog";
    }

    /// <summary>
    /// Accept button callback.
    /// </summary>
    public void OnAcceptButtonPressed()
    {
        StartCoroutine(PlayerServer.RegisterPlayer(EmailInputField.text, NameInputField.text, PasswordInputField.text,
            (response) =>
            {
                if (response.status == "success")
                {
                    WindowUtils.OpenAlertDialog("Welcome!");
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

    /// <summary>
    /// Close button callback.
    /// </summary>
    public void OnCloseButtonPressed()
    {
        Close();
    }
}
