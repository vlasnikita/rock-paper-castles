// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;

namespace CCGKit
{
    /// <summary>
    /// Base class for all windows in the game.
    /// </summary>
    public class Window : MonoBehaviour
    {
        protected string windowName;

        protected bool isShuttingDown;

        protected virtual void OnApplicationQuit()
        {
            isShuttingDown = true;
        }

        protected virtual void OnDestroy()
        {
            if (!isShuttingDown)
                WindowManager.Instance.CloseWindow(windowName, true);
        }

        /// <summary>
        /// Closes this loading dialog.
        /// </summary>
        public void Close()
        {
            WindowManager.Instance.CloseWindow(windowName);
        }
    }
}
