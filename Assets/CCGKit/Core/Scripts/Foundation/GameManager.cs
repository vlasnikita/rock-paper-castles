// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;

namespace CCGKit
{
    /// <summary>
    /// This class is the in-game entry point to the game configuration asset managed from within
    /// the CCG Kit menu option in Unity.
    /// </summary>
    public sealed class GameManager
    {
        public GameConfiguration Config = new GameConfiguration();

        public bool PlayerLoggedIn;
        public string PlayerName;
        public string AuthToken;
        public int NumUnopenedCardPacks;
        public int Currency;

        private static readonly GameManager instance = new GameManager();

        private GameManager()
        {
            Config.LoadGameConfigurationAtRuntime();
        }

        public static GameManager Instance
        {
            get { return instance; }
        }

        public Card GetCard(int id)
        {
            foreach (var set in Config.CardCollection)
            {
                var card = set.Cards.Find(x => x.Id == id);
                if (card != null)
                    return card;
            }
            return null;
        }

        public Card GetCard(string name)
        {
            foreach (var set in Config.CardCollection)
            {
                var card = set.Cards.Find(x => x.Name == name);
                if (card != null)
                    return card;
            }
            return null;
        }
    }
}
