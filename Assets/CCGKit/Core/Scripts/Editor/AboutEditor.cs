// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;

namespace CCGKit
{
    partial class CCGKitEditor
    {
        private void DrawAboutInformation()
        {
            GUILayout.Space(20);

            GUILayout.BeginVertical();
            GUILayout.Label("Current version: " + CCGKitInfo.Version);
            GUILayout.Label(CCGKitInfo.Copyright);
            GUILayout.EndVertical();

            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Documentation", GUILayout.MaxWidth(100), GUILayout.MaxHeight(50)))
                Application.OpenURL("http://www.spelltwinegames.com/ccgkit/documentation");
            if (GUILayout.Button("Public roadmap", GUILayout.MaxWidth(100), GUILayout.MaxHeight(50)))
                Application.OpenURL("https://trello.com/b/qXRx5Y0R/ccg-kit");
            if (GUILayout.Button("Support", GUILayout.MaxWidth(100), GUILayout.MaxHeight(50)))
                Application.OpenURL("mailto:support@spelltwinegames.com");
            GUILayout.EndHorizontal();
        }
    }
}
