// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;

namespace CCGKit
{
    /// <summary>
    /// Holds information about an in-game window.
    /// </summary>
    public class WindowDescriptor
    {
        /// <summary>
        /// Unique identifier of this window. This id is the same as the name of the scene that contains
        /// the respective window.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Callback to execute when the window is opened.
        /// </summary>
        private Action onOpen;

        /// <summary>
        /// Callback to execute when the window is closed.
        /// </summary>
        private Action onClose;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="id">Unique identifier of this window.</param>
        /// <param name="onOpen">Optional callback for when the window is opened.</param>
        /// <param name="onClose">Optional callback for when the window is closed.</param>
        public WindowDescriptor(string id, Action onOpen, Action onClose)
        {
            Id = id;
            this.onOpen = onOpen;
            this.onClose = onClose;
        }

        /// <summary>
        /// Called from the WindowManager when this window is opened.
        /// </summary>
        public void OnOpen()
        {
            if (onOpen != null)
                onOpen();
        }

        /// <summary>
        /// Called from the WindowManager when this window is closed.
        /// </summary>
        public void OnClose()
        {
            if (onClose != null)
                onClose();
        }
    }
}
