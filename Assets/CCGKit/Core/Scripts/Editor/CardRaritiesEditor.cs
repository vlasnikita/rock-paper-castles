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
        private ReorderableList cardRaritiesList;

        private CardRarity currentCardRarity;

        private void InitCardRaritiesEditor()
        {
            cardRaritiesList = EditorUtils.SetupReorderableList("Card rarities", gameConfig.CardRarities, ref currentCardRarity, (rect, x) =>
            {
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight), x.Name);
            },
            (x) =>
            {
                currentCardRarity = x;
            },
            () =>
            {
                var newCardRarity = new CardRarity();
                gameConfig.CardRarities.Add(newCardRarity);
            },
            (x) =>
            {
                currentCardRarity = null;
            });
        }

        private void DrawCardRaritiesEditor()
        {
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.MaxWidth(250));
            if (cardRaritiesList != null)
                cardRaritiesList.DoLayoutList();
            GUILayout.EndVertical();

            if (currentCardRarity != null)
                DrawCardRarity(currentCardRarity);

            GUILayout.EndHorizontal();
        }

        private void DrawCardRarity(CardRarity cardRarity)
        {
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = EditorSettings.RegularLabelWidth;

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Name");
            cardRarity.Name = EditorGUILayout.TextField(cardRarity.Name, GUILayout.MaxWidth(EditorSettings.RegularTextFieldWidth));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Value");
            cardRarity.Chance = EditorGUILayout.IntField(cardRarity.Chance, GUILayout.MaxWidth(30));
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            EditorGUIUtility.labelWidth = oldLabelWidth;
        }
    }
}
