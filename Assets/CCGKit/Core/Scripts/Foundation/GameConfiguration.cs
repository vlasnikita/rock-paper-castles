// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.Assertions;

using FullSerializer;

namespace CCGKit
{
    /// <summary>
    /// Contains the entire game configuration details, which are comprised of general match settings,
    /// player/card/effect definitions and the card database.
    /// </summary>
    public class GameConfiguration
    {
        public GameProperties Properties = new GameProperties();

        public List<GameZone> Zones = new List<GameZone>();

        public PlayerDefinition PlayerDefinition = new PlayerDefinition();

        public List<CardDefinition> CardDefinitions = new List<CardDefinition>();

        public List<EffectDefinition> EffectDefinitions = new List<EffectDefinition>();

        public List<CardRarity> CardRarities = new List<CardRarity>();

        public List<CardSet> CardCollection = new List<CardSet>();

        private fsSerializer serializer = new fsSerializer();

        public void LoadGameConfiguration(string path)
        {
            var gamePropertiesPath = path + "/game_properties.json";
            var gameProperties = LoadJSONFile<GameProperties>(gamePropertiesPath);
            if (gameProperties != null)
                Properties = gameProperties;

            var gameZonesPath = path + "/game_zones.json";
            var gameZones = LoadJSONFile<List<GameZone>>(gameZonesPath);
            if (gameZones != null)
                Zones = gameZones;

            var playerDefinitionPath = path + "/player_definition.json";
            var playerDefinition = LoadJSONFile<PlayerDefinition>(playerDefinitionPath);
            if (playerDefinition != null)
                PlayerDefinition = playerDefinition;

            var cardDefinitionsPath = path + "/card_definitions.json";
            var cardDefinitions = LoadJSONFile<List<CardDefinition>>(cardDefinitionsPath);
            if (cardDefinitions != null)
                CardDefinitions = cardDefinitions;

            var effectDefinitionsPath = path + "/effect_definitions.json";
            var effectDefinitions = LoadJSONFile<List<EffectDefinition>>(effectDefinitionsPath);
            if (effectDefinitions != null)
                EffectDefinitions = effectDefinitions;

            var cardRaritiesPath = path + "/card_rarities.json";
            var cardRarities = LoadJSONFile<List<CardRarity>>(cardRaritiesPath);
            if (cardRarities != null)
                CardRarities = cardRarities;

            var cardCollectionPath = path + "/card_collection.json";
            var cardCollection = LoadJSONFile<List<CardSet>>(cardCollectionPath);
            if (cardCollection != null)
                CardCollection = cardCollection;
        }

        public void LoadGameConfigurationAtRuntime()
        {
            var gamePropertiesJSON = Resources.Load<TextAsset>("game_properties");
            Assert.IsTrue(gamePropertiesJSON != null);
            var gameProperties = LoadJSONString<GameProperties>(gamePropertiesJSON.text);
            if (gameProperties != null)
                Properties = gameProperties;

            var gameZonesJSON = Resources.Load<TextAsset>("game_zones");
            Assert.IsTrue(gameZonesJSON != null);
            var gameZones = LoadJSONString<List<GameZone>>(gameZonesJSON.text);
            if (gameZones != null)
                Zones = gameZones;

            var playerDefinitionJSON = Resources.Load<TextAsset>("player_definition");
            Assert.IsTrue(playerDefinitionJSON != null);
            var playerDefinition = LoadJSONString<PlayerDefinition>(playerDefinitionJSON.text);
            if (playerDefinition != null)
                PlayerDefinition = playerDefinition;

            var cardDefinitionsJSON = Resources.Load<TextAsset>("card_definitions");
            Assert.IsTrue(cardDefinitionsJSON != null);
            var cardDefinitions = LoadJSONString<List<CardDefinition>>(cardDefinitionsJSON.text);
            if (cardDefinitions != null)
                CardDefinitions = cardDefinitions;

            var effectDefinitionsJSON = Resources.Load<TextAsset>("effect_definitions");
            Assert.IsTrue(effectDefinitionsJSON != null);
            var effectDefinitions = LoadJSONString<List<EffectDefinition>>(effectDefinitionsJSON.text);
            if (effectDefinitions != null)
                EffectDefinitions = effectDefinitions;

            var cardRaritiesJSON = Resources.Load<TextAsset>("card_rarities");
            Assert.IsTrue(cardRaritiesJSON != null);
            var cardRarities = LoadJSONString<List<CardRarity>>(cardRaritiesJSON.text);
            if (cardRarities != null)
                CardRarities = cardRarities;

            var cardCollectionJSON = Resources.Load<TextAsset>("card_collection");
            Assert.IsTrue(cardCollectionJSON != null);
            var cardCollection = LoadJSONString<List<CardSet>>(cardCollectionJSON.text);
            if (cardCollection != null)
                CardCollection = cardCollection;
        }

        private T LoadJSONFile<T>(string path) where T : class
        {
            if (File.Exists(path))
            {
                var file = new StreamReader(path);
                var fileContents = file.ReadToEnd();
                var data = fsJsonParser.Parse(fileContents);
                object deserialized = null;
                serializer.TryDeserialize(data, typeof(T), ref deserialized).AssertSuccessWithoutWarnings();
                file.Close();
                return deserialized as T;
            }
            return null;
        }

        private T LoadJSONString<T>(string json) where T : class
        {
            var data = fsJsonParser.Parse(json);
            object deserialized = null;
            serializer.TryDeserialize(data, typeof(T), ref deserialized).AssertSuccessWithoutWarnings();
            return deserialized as T;
        }

#if UNITY_EDITOR
        public void SaveGameConfiguration(string path)
        {
            SaveJSONFile(path + "/game_properties.json", Properties);
            SaveJSONFile(path + "/game_zones.json", Zones);
            SaveJSONFile(path + "/player_definition.json", PlayerDefinition);
            SaveJSONFile(path + "/card_definitions.json", CardDefinitions);
            SaveJSONFile(path + "/effect_definitions.json", EffectDefinitions);
            SaveJSONFile(path + "/card_rarities.json", CardRarities);
            SaveJSONFile(path + "/card_collection.json", CardCollection);
            AssetDatabase.Refresh();
        }

        public void SaveGameConfigurationAs()
        {
            var path = EditorUtility.OpenFolderPanel("Select game configuration folder", "", "");
            if (!string.IsNullOrEmpty(path))
            {
                EditorPrefs.SetString("GameConfigurationPath", path);
                SaveGameConfiguration(path);
            }
        }
#endif

        private void SaveJSONFile<T>(string path, T data) where T : class
        {
            fsData serializedData;
            serializer.TrySerialize(data, out serializedData).AssertSuccessWithoutWarnings();
            var file = new StreamWriter(path);
            var json = fsJsonPrinter.PrettyJson(serializedData);
            file.WriteLine(json);
            file.Close();
        }
    }
}
