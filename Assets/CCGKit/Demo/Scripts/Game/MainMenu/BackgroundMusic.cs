// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;

/// <summary>
/// Looping background music for the demo game.
/// </summary>
public class BackgroundMusic : MonoBehaviour
{
    private static bool firstLaunch = true;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (firstLaunch)
        {
            GetComponent<AudioSource>().Play();
            firstLaunch = false;
        }
    }
}
