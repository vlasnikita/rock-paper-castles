// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;

using UnityEngine.Assertions;
using UnityEngine.UI;

namespace CCGKit
{
    /// <summary>
    /// Holds information about the dialog displayed when prompting the player for a multiplayer
    /// game room password.
    /// </summary>
    public class PasswordEntryDialog : Window
    {
        public InputField PasswordInputField;

        private void Awake()
        {
            Assert.IsTrue(PasswordInputField != null);
            windowName = "PasswordEntryDialog";
        }

        /// <summary>
        /// Callback to execute when the dialog is accepted.
        /// </summary>
        public Action<string> OnAccept;

        /// <summary>
        /// Accept button callback.
        /// </summary>
        public void OnAcceptButtonPressed()
        {
            if (OnAccept != null)
                OnAccept(PasswordInputField.text);
            Close();
        }
    }
}
