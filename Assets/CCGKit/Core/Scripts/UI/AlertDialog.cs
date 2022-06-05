// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace CCGKit
{
    /// <summary>
    /// Holds information about an in-game alert dialog.
    /// </summary>
    public class AlertDialog : Window
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

        /// <summary>
        /// Callback to execute when the dialog is accepted.
        /// </summary>
        public Action OnAccept;

        private void Awake()
        {
            Assert.IsTrue(TextUI != null);
            windowName = "AlertDialog";
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
}
