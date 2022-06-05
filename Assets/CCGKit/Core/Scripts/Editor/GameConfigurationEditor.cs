// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CCGKit
{
    public class GameConfigurationEditor
    {
        private GameConfiguration gameConfig;

        private ReorderableList gameStartActionList;
        private GameAction currentGameStartAction;

        private ReorderableList turnStartActionList;
        private GameAction currentTurnStartAction;

        private ReorderableList turnEndActionList;
        private GameAction currentTurnEndAction;

        private List<string> playerAttributeNames = new List<string>();
        private List<string> gameZoneNames = new List<string>();

        private int currentPlayerAttribute;
        private int currentGameZone;

        public GameConfigurationEditor(GameConfiguration config)
        {
            gameConfig = config;
        }

        public void Init()
        {
            foreach (var attribute in gameConfig.PlayerDefinition.Attributes)
            {
                playerAttributeNames.Add(attribute.Name);
            }

            foreach (var zone in gameConfig.Zones)
            {
                gameZoneNames.Add(zone.Name);
            }

            gameStartActionList = EditorUtils.SetupReorderableList("Game start actions", gameConfig.Properties.GameStartActions, ref currentGameStartAction, (rect, x) =>
            {
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight), x.Name);
            },
            (x) =>
            {
                currentGameStartAction = x;
                currentPlayerAttribute = 0;
                currentGameZone = 0;
            },
            () =>
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Set player attribute"), false, CreateGameStartActionCallback, 0);
                menu.AddItem(new GUIContent("Increase player attribute"), false, CreateGameStartActionCallback, 1);
                menu.AddItem(new GUIContent("Shuffle cards"), false, CreateGameStartActionCallback, 2);
                menu.AddItem(new GUIContent("Move cards"), false, CreateGameStartActionCallback, 3);
                menu.ShowAsContext();
            },
            (x) =>
            {
                currentGameStartAction = null;
                currentPlayerAttribute = 0;
                currentGameZone = 0;
            });

            turnStartActionList = EditorUtils.SetupReorderableList("Turn start actions", gameConfig.Properties.TurnStartActions, ref currentTurnStartAction, (rect, x) =>
            {
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight), x.Name);
            },
            (x) =>
            {
                currentTurnStartAction = x;
                currentPlayerAttribute = 0;
                currentGameZone = 0;
            },
            () =>
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Set player attribute"), false, CreateTurnStartActionCallback, 0);
                menu.AddItem(new GUIContent("Increase player attribute"), false, CreateTurnStartActionCallback, 1);
                menu.AddItem(new GUIContent("Shuffle cards"), false, CreateTurnStartActionCallback, 2);
                menu.AddItem(new GUIContent("Move cards"), false, CreateTurnStartActionCallback, 3);
                menu.ShowAsContext();
            },
            (x) =>
            {
                currentTurnStartAction = null;
                currentPlayerAttribute = 0;
                currentGameZone = 0;
            });

            turnEndActionList = EditorUtils.SetupReorderableList("Turn end actions", gameConfig.Properties.TurnEndActions, ref currentTurnEndAction, (rect, x) =>
            {
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight), x.Name);
            },
            (x) =>
            {
                currentTurnEndAction = x;
                currentPlayerAttribute = 0;
                currentGameZone = 0;
            },
            () =>
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Set player attribute"), false, CreateTurnEndActionCallback, 0);
                menu.AddItem(new GUIContent("Increase player attribute"), false, CreateTurnEndActionCallback, 1);
                menu.AddItem(new GUIContent("Shuffle cards"), false, CreateTurnEndActionCallback, 2);
                menu.AddItem(new GUIContent("Move cards"), false, CreateTurnEndActionCallback, 3);
                menu.ShowAsContext();
            },
            (x) =>
            {
                currentTurnEndAction = null;
                currentPlayerAttribute = 0;
                currentGameZone = 0;
            });
        }

        private void CreateGameStartActionCallback(object obj)
        {
            var action = CreateGameAction((int)obj);
            gameConfig.Properties.GameStartActions.Add(action);
        }

        private void CreateTurnStartActionCallback(object obj)
        {
            var action = CreateGameAction((int)obj);
            gameConfig.Properties.TurnStartActions.Add(action);
        }

        private void CreateTurnEndActionCallback(object obj)
        {
            var action = CreateGameAction((int)obj);
            gameConfig.Properties.TurnEndActions.Add(action);
        }

        private GameAction CreateGameAction(int id)
        {
            GameAction action = null;
            switch (id)
            {
                case 0:
                    action = new SetPlayerAttributeAction();
                    break;

                case 1:
                    action = new IncreasePlayerAttributeAction();
                    break;

                case 2:
                    action = new ShuffleCardsAction();
                    break;

                case 3:
                    action = new MoveCardsAction();
                    break;
            }
            return action;
        }

        public void Draw()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Number of players");
            gameConfig.Properties.NumPlayers = EditorGUILayout.IntField(gameConfig.Properties.NumPlayers, GUILayout.MaxWidth(30));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Turn duration");
            gameConfig.Properties.TurnDuration = EditorGUILayout.IntField(gameConfig.Properties.TurnDuration, GUILayout.MaxWidth(30));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Minimum deck size");
            gameConfig.Properties.MinDeckSize = EditorGUILayout.IntField(gameConfig.Properties.MinDeckSize, GUILayout.MaxWidth(30));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Maximum deck size");
            gameConfig.Properties.MaxDeckSize = EditorGUILayout.IntField(gameConfig.Properties.MaxDeckSize, GUILayout.MaxWidth(30));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Maximum hand size");
            gameConfig.Properties.MaxHandSize = EditorGUILayout.IntField(gameConfig.Properties.MaxHandSize, GUILayout.MaxWidth(30));
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.MaxWidth(250));
            if (gameStartActionList != null)
            {
                gameStartActionList.DoLayoutList();
            }
            GUILayout.EndVertical();

            if (currentGameStartAction != null)
            {
                DrawGameAction(currentGameStartAction);
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.MaxWidth(250));
            if (turnStartActionList != null)
            {
                turnStartActionList.DoLayoutList();
            }
            GUILayout.EndVertical();

            if (currentTurnStartAction != null)
            {
                DrawGameAction(currentTurnStartAction);
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.MaxWidth(250));
            if (turnEndActionList != null)
            {
                turnEndActionList.DoLayoutList();
            }
            GUILayout.EndVertical();

            if (currentTurnEndAction != null)
            {
                DrawGameAction(currentTurnEndAction);
            }

            GUILayout.EndHorizontal();
        }

        private void DrawGameAction(GameAction action)
        {
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = EditorSettings.LargeLabelWidth;

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Target");
            action.Target = (GameActionTarget)EditorGUILayout.EnumPopup(action.Target, GUILayout.MaxWidth(100));
            GUILayout.EndHorizontal();

            if (action is SetPlayerAttributeAction)
            {
                var setAttributeAction = action as SetPlayerAttributeAction;

                GUILayout.BeginVertical();

                GUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Attribute");
                if (setAttributeAction.Attribute != null)
                {
                    currentPlayerAttribute = playerAttributeNames.FindIndex(x => x == setAttributeAction.Attribute);
                }
                if (currentPlayerAttribute == -1)
                {
                    currentPlayerAttribute = 0;
                }
                currentPlayerAttribute = EditorGUILayout.Popup(currentPlayerAttribute, playerAttributeNames.ToArray(), GUILayout.MaxWidth(100));
                setAttributeAction.Attribute = playerAttributeNames[currentPlayerAttribute];
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Value");
                setAttributeAction.Value = EditorGUILayout.IntField(setAttributeAction.Value, GUILayout.MaxWidth(30));
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
            }
            else if (action is IncreasePlayerAttributeAction)
            {
                var increaseAttributeAction = action as IncreasePlayerAttributeAction;

                GUILayout.BeginVertical();

                GUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Attribute");
                if (increaseAttributeAction.Attribute != null)
                {
                    currentPlayerAttribute = playerAttributeNames.FindIndex(x => x == increaseAttributeAction.Attribute);
                }
                if (currentPlayerAttribute == -1)
                {
                    currentPlayerAttribute = 0;
                }
                currentPlayerAttribute = EditorGUILayout.Popup(currentPlayerAttribute, playerAttributeNames.ToArray(), GUILayout.MaxWidth(100));
                increaseAttributeAction.Attribute = playerAttributeNames[currentPlayerAttribute];
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Value");
                increaseAttributeAction.Value = EditorGUILayout.IntField(increaseAttributeAction.Value, GUILayout.MaxWidth(30));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Maximum");
                increaseAttributeAction.Max = EditorGUILayout.IntField(increaseAttributeAction.Max, GUILayout.MaxWidth(30));
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
            }
            else if (action is ShuffleCardsAction)
            {
                var shuffleCardsAction = action as ShuffleCardsAction;

                GUILayout.BeginVertical();

                GUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Zone");
                if (shuffleCardsAction.Zone != null)
                {
                    currentGameZone = gameZoneNames.FindIndex(x => x == shuffleCardsAction.Zone);
                }
                if (currentGameZone == -1)
                {
                    currentGameZone = 0;
                }
                currentGameZone = EditorGUILayout.Popup(currentGameZone, gameZoneNames.ToArray(), GUILayout.MaxWidth(100));
                shuffleCardsAction.Zone = gameZoneNames[currentGameZone];
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
            }
            else if (action is MoveCardsAction)
            {
                var moveCardsAction = action as MoveCardsAction;

                GUILayout.BeginVertical();

                GUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Origin zone");
                if (moveCardsAction.OriginZone != null)
                {
                    currentGameZone = gameZoneNames.FindIndex(x => x == moveCardsAction.OriginZone);
                }
                if (currentGameZone == -1)
                {
                    currentGameZone = 0;
                }
                currentGameZone = EditorGUILayout.Popup(currentGameZone, gameZoneNames.ToArray(), GUILayout.MaxWidth(100));
                moveCardsAction.OriginZone = gameZoneNames[currentGameZone];
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Destination zone");
                if (moveCardsAction.DestinationZone != null)
                {
                    currentGameZone = gameZoneNames.FindIndex(x => x == moveCardsAction.DestinationZone);
                }
                if (currentGameZone == -1)
                {
                    currentGameZone = 0;
                }
                currentGameZone = EditorGUILayout.Popup(currentGameZone, gameZoneNames.ToArray(), GUILayout.MaxWidth(100));
                moveCardsAction.DestinationZone = gameZoneNames[currentGameZone];
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Number of cards");
                moveCardsAction.NumCards = EditorGUILayout.IntField(moveCardsAction.NumCards, GUILayout.MaxWidth(30));
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
            }

            GUILayout.EndVertical();

            EditorGUIUtility.labelWidth = oldLabelWidth;
        }
    }
}
