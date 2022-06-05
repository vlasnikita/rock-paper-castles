// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CCGKit
{
    partial class CCGKitEditor
    {
        private ReorderableList gameZonesList;
        private GameZone currentGameZone;

        private void InitGameZonesEditor()
        {
            currentPlayerAttribute = null;

            gameZonesList = EditorUtils.SetupReorderableList("Game zones", gameConfig.Zones, ref currentGameZone, (rect, x) =>
            {
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight), x.Name);
            },
            (x) =>
            {
                currentGameZone = x;
            },
            () =>
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Static"), false, CreateGameZoneCallback, "Static");
                menu.AddItem(new GUIContent("Dynamic"), false, CreateGameZoneCallback, "Dynamic");
                menu.ShowAsContext();
            },
            (x) =>
            {
                currentGameZone = null;
            });
        }

        private void CreateGameZoneCallback(object obj)
        {
            GameZone zone = null;
            switch ((string)obj)
            {
                case "Static":
                    zone = new StaticGameZone();
                    break;

                case "Dynamic":
                    zone = new DynamicGameZone();
                    break;
            }
            gameConfig.Zones.Add(zone);
        }

        public void DrawGameZonesEditor()
        {
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.MaxWidth(250));
            if (gameZonesList != null)
                gameZonesList.DoLayoutList();
            GUILayout.EndVertical();

            if (currentGameZone != null)
                DrawGameZone(currentGameZone);

            GUILayout.EndHorizontal();
        }

        private void DrawGameZone(GameZone zone)
        {
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = EditorSettings.RegularLabelWidth;

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            if (zone is StaticGameZone)
                GUILayout.Label("Static", EditorStyles.boldLabel);
            else if (zone is DynamicGameZone)
                GUILayout.Label("Dynamic", EditorStyles.boldLabel);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Private");
            zone.Private = EditorGUILayout.Toggle(zone.Private, GUILayout.MaxWidth(EditorSettings.RegularTextFieldWidth));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Name");
            zone.Name = EditorGUILayout.TextField(zone.Name, GUILayout.MaxWidth(EditorSettings.RegularTextFieldWidth));
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            EditorGUIUtility.labelWidth = oldLabelWidth;
        }
    }
}
