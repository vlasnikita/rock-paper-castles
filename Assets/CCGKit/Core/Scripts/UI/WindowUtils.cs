// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;

using UnityEngine;

namespace CCGKit
{
    /// <summary>
    /// Provides high-level utilities for managing special types of windows.
    /// </summary>
    public static class WindowUtils
    {
        /// <summary>
        /// Opens a new alert dialog with the specified text.
        /// </summary>
        /// <param name="text">Text to display inside the alert dialog.</param>
        /// <param name="onAccept">Optional callback for when the dialog is accepted.</param>
        public static void OpenAlertDialog(string text, Action onAccept = null)
        {
            WindowManager.Instance.OpenWindow("AlertDialog",
                () =>
                {
                    var alertDialog = GameObject.Find("AlertDialog").GetComponent<AlertDialog>();
                    alertDialog.Text = text;
                    alertDialog.OnAccept = onAccept;
                });
        }

        /// <summary>
        /// Opens a new confirmation dialog with the specified text.
        /// </summary>
        /// <param name="text">Text to display inside the confirmation dialog.</param>
        /// <param name="onAccept">Optional callback for when the dialog is accepted.</param>
        /// <param name="onCancel">Optional callback for when the dialog is canceled.</param>
        public static void OpenConfirmationDialog(string text, Action onAccept = null, Action onCancel = null)
        {
            WindowManager.Instance.OpenWindow("ConfirmationDialog",
                () =>
                {
                    var confirmationDialog = GameObject.Find("ConfirmationDialog").GetComponent<ConfirmationDialog>();
                    confirmationDialog.Text = text;
                    confirmationDialog.OnAccept = onAccept;
                    confirmationDialog.OnCancel = onCancel;
                });
        }

        /// <summary>
        /// Opens a new loading dialog with the specified text.
        /// </summary>
        /// <param name="text">Text to display inside the loading dialog.</param>
        public static void OpenLoadingDialog(string text)
        {
            WindowManager.Instance.OpenWindow("LoadingDialog",
                () =>
                {
                    var loadingDialog = GameObject.Find("LoadingDialog").GetComponent<LoadingDialog>();
                    loadingDialog.Text = text;
                });
        }
    }
}
