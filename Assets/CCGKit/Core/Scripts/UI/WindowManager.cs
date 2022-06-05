// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace CCGKit
{
    /// <summary>
    /// Manages the opening and closing of windows inside the game. Windows are scenes that are
    /// loaded additively into the current scene via the scene management facilities provided
    /// by Unity starting on versions 5.3 and higher.
    /// </summary>
    public class WindowManager : MonoBehaviour
    {
        /// <summary>
        /// Singleton instance. The singleton is lazily instantiated for convenience (i.e., avoid
        /// the need to create an object in every scene of the game).
        /// </summary>
        private static WindowManager instance;

        public static WindowManager Instance
        {
            get
            {
                if (instance == null)
                {
                    var manager = new GameObject("WindowManager");
                    instance = manager.AddComponent<WindowManager>();
                    DontDestroyOnLoad(manager);
                }
                return instance;
            }
        }

        /// <summary>
        /// Stores the currently open windows.
        /// </summary>
        private readonly List<WindowDescriptor> windows = new List<WindowDescriptor>();

        /// <summary>
        /// Opens a new window with the specified id. This id needs to be the same as the
        /// name of the scene where the window is stored.
        /// </summary>
        /// <param name="id">Id of the window to open.</param>
        /// <param name="onOpen">Optional callback for when the window is opened.</param>
        /// <param name="onClose">Optional callback for when the window is closed.</param>
        public void OpenWindow(string id, Action onOpen = null, Action onClose = null)
        {
            // Avoid opening the same window more than once.
            var window = windows.Find(x => x.Id == id);
            if (window != null)
                return;

            var newWindow = new WindowDescriptor(id, onOpen, onClose);
            windows.Add(newWindow);
            StartCoroutine(LoadScene(id));
        }

        /// <summary>
        /// Closes the window with the specified id.
        /// </summary>
        /// <param name="id">Id of the window to close.</param>
        public void CloseWindow(string id, bool preventUnload = false)
        {
            var window = windows.Find(x => x.Id == id);
            if (window != null)
            {
                window.OnClose();
                if (!preventUnload)
                    UnloadScene(window.Id);
                windows.Remove(window);
            }
        }

        /// <summary>
        /// Loads the scene with the specified id.
        /// </summary>
        /// <param name="id">Id of the scene to load.</param>
        /// <returns>Async operation for the loaded scene.</returns>
        private IEnumerator LoadScene(string id)
        {
            var async = SceneManager.LoadSceneAsync(id, LoadSceneMode.Additive);
            yield return async;
            var window = windows.Find(x => x.Id == id);
            window.OnOpen();
        }

        /// <summary>
        /// Unloads the scene with the specified id.
        /// </summary>
        /// <param name="id">Id of the scene to unload.</param>
        private void UnloadScene(string id)
        {
            SceneManager.UnloadScene(id);
        }
    }
}
