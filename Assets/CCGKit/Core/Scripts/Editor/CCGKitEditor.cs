// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEditor;
using UnityEngine;

namespace CCGKit
{
    /// <summary>
    /// CCG Kit editor accessible from the Unity menu. This editor provides an intuitive way to define
    /// the fundamental properties of a collectible card game.
    /// </summary>
    public partial class CCGKitEditor : EditorWindow
    {
        private GameConfiguration gameConfig;

        private string gameConfigPath;

        private int selectedTabIndex = -1;

        private GameConfigurationEditor gameConfigEditor;

        [MenuItem("Window/CCG Kit Editor")]
        private static void Init()
        {
            var window = GetWindow(typeof(CCGKitEditor));
            window.titleContent = new GUIContent("CCG Kit Editor");
        }

        private void OnEnable()
        {
            if (EditorPrefs.HasKey("GameConfigurationPath"))
            {
                gameConfigPath = EditorPrefs.GetString("GameConfigurationPath");
                gameConfig = new GameConfiguration();
                gameConfig.LoadGameConfiguration(gameConfigPath);
                InitPlayerEditor();
                InitCardEditor();
            }

            gameConfigEditor = new GameConfigurationEditor(gameConfig);
        }

        private void ResetGameConfiguration()
        {
            gameConfig = new GameConfiguration();
            InitPlayerEditor();
            InitCardEditor();
            selectedTabIndex = 0;
        }

        private void OpenGameConfiguration()
        {
            var path = EditorUtility.OpenFolderPanel("Select game configuration folder", "", "");
            if (!string.IsNullOrEmpty(path))
            {
                gameConfigPath = path;
                gameConfig = new GameConfiguration();
                gameConfig.LoadGameConfiguration(gameConfigPath);
                EditorPrefs.SetString("GameConfigurationPath", gameConfigPath);
                InitPlayerEditor();
                InitCardEditor();
                selectedTabIndex = 0;
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("New", GUILayout.MaxWidth(60)))
                ResetGameConfiguration();
            if (GUILayout.Button("Open", GUILayout.MaxWidth(60)))
                OpenGameConfiguration();
            if (GUILayout.Button("Save", GUILayout.MaxWidth(60)))
                gameConfig.SaveGameConfiguration(gameConfigPath);
            if (GUILayout.Button("Save as", GUILayout.MaxWidth(60)))
            {
                gameConfig.SaveGameConfigurationAs();
                gameConfigPath = EditorPrefs.GetString("GameConfigurationPath");
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Current path: ", GUILayout.MaxWidth(90));
            GUILayout.Label(gameConfigPath);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            if (gameConfig == null)
                return;

            var prevSelectedTabIndex = selectedTabIndex;
            selectedTabIndex = GUILayout.Toolbar(selectedTabIndex, new string[] { "Game configuration", "Game zones", "Player definition", "Card definitions", "Effect definitions", "Card rarities", "Card collection", "About CCG Kit" });
            switch (selectedTabIndex)
            {
                case 0:
                    if (selectedTabIndex != prevSelectedTabIndex)
                        gameConfigEditor.Init();
                    gameConfigEditor.Draw();
                    break;

                case 1:
                    if (selectedTabIndex != prevSelectedTabIndex)
                        InitGameZonesEditor();
                    DrawGameZonesEditor();
                    break;

                case 2:
                    DrawPlayerEditor();
                    break;

                case 3:
                    DrawCardEditor();
                    break;

                case 4:
                    if (selectedTabIndex != prevSelectedTabIndex)
                        InitEffectEditor();
                    DrawEffectEditor();
                    break;

                case 5:
                    if (selectedTabIndex != prevSelectedTabIndex)
                        InitCardRaritiesEditor();
                    DrawCardRaritiesEditor();
                    break;

                case 6:
                    if (selectedTabIndex != prevSelectedTabIndex)
                        InitCardCollectionEditor();
                    DrawCardCollectionEditor();
                    break;

                case 7:
                    DrawAboutInformation();
                    break;
            }
        }
    }
}
