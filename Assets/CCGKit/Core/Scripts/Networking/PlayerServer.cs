// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Networking;

namespace CCGKit
{
    [Serializable]
    public struct LoginPlayerResponse
    {
        public string status;
        public string message;
        public string token;
        public string username;
        public int numUnopenedCardPacks;
        public int currency;
    }

    [Serializable]
    public struct RegisterPlayerResponse
    {
        public string status;
        public string message;
        public string token;
    }

    [Serializable]
    public struct RankingRow
    {
        public string name;
        public int numGamesWon;
    }

    [Serializable]
    public struct RankingResponse
    {
        public string status;
        public List<RankingRow> users;
    }

    [Serializable]
    public struct GetCurrencyResponse
    {
        public string status;
        public int currency;
    }

    [Serializable]
    public struct BuyCardPackResponse
    {
        public string status;
        public int numPacksBought;
        public int numUnopenedPacks;
        public int currency;
    }

    [Serializable]
    public struct OpenCardPackResponse
    {
        public string status;
        public List<int> cards;
        public int numUnopenedPacks;
    }

    [Serializable]
    public struct CardCollectionRow
    {
        public int id;
        public int numCopies;
    }

    [Serializable]
    public struct GetCardCollectionResponse
    {
        public string status;
        public List<CardCollectionRow> cards;
    }

    /// <summary>
    /// This class is used to access the player server that is used for player registration and login,
    /// rankings and card pack purchase.
    /// </summary>
    public class PlayerServer
    {
        /// <summary>
        /// Specifies the URL of the player server.
        /// </summary>
        private static readonly string baseURL = "http://localhost:8080/api/";

        /// <summary>
        /// Logs the player into the server with the specified data.
        /// </summary>
        /// <param name="email">Player's email.</param>
        /// <param name="password">Player's password.</param>
        /// <param name="onSuccess">Action to invoke when the web request is successful.</param>
        /// <param name="onError">Action to invoke when the web request fails.</param>
        /// <returns>The coroutine used for the login.</returns>
        public static IEnumerator LoginPlayer(string email, string password, Action<LoginPlayerResponse> onSuccess, Action<string> onError)
        {
            var form = new WWWForm();
            form.AddField("email", email);
            form.AddField("password", password);

            var www = UnityWebRequest.Post(baseURL + "login", form);
            yield return www.Send();

            if (www.isNetworkError)
            {
                onError(www.error);
            }
            else
            {
                var response = JsonUtility.FromJson<LoginPlayerResponse>(www.downloadHandler.text);
                onSuccess(response);
            }
        }

        /// <summary>
        /// Registers the player into the server with the specified data.
        /// </summary>
        /// <param name="email">Player's email.</param>
        /// <param name="name">Player's name.</param>
        /// <param name="password">Player's password.</param>
        /// <param name="onSuccess">Action to invoke when the web request is successful.</param>
        /// <param name="onError">Action to invoke when the web request fails.</param>
        /// <returns>The coroutine used for the register.</returns>
        public static IEnumerator RegisterPlayer(string email, string name, string password, Action<RegisterPlayerResponse> onSuccess, Action<string> onError)
        {
            var form = new WWWForm();
            form.AddField("email", email);
            form.AddField("name", name);
            form.AddField("password", password);

            var www = UnityWebRequest.Post(baseURL + "register", form);
            yield return www.Send();

            if (www.isNetworkError)
            {
                onError(www.error);
            }
            else
            {
                var response = JsonUtility.FromJson<RegisterPlayerResponse>(www.downloadHandler.text);
                onSuccess(response);
            }
        }

        /// <summary>
        /// Gets the remote card collection of the player with the specified authentication data.
        /// </summary>
        /// <param name="authToken">Player's authentication token.</param>
        /// <param name="onSuccess">Action to invoke when the web request is successful.</param>
        /// <param name="onError">Action to invoke when the web request fails.</param>
        /// <returns>The coroutine used for retrieving the card collection.</returns>
        public static IEnumerator GetCardCollection(string authToken, Action<GetCardCollectionResponse> onSuccess, Action<string> onError)
        {
            var www = UnityWebRequest.Get(baseURL + "getCardCollection");
            www.SetRequestHeader("Authorization", "Bearer " + authToken);
            yield return www.Send();

            if (www.isNetworkError)
            {
                onError(www.error);
            }
            else
            {
                var response = JsonUtility.FromJson<GetCardCollectionResponse>(www.downloadHandler.text);
                onSuccess(response);
            }
        }

        /// <summary>
        /// Gets the ranking of the game, sorted by number of wins.
        /// </summary>
        /// <param name="authToken">Player's authentication token.</param>
        /// <param name="numEntries">Number of entries of the ranking to return.</param>
        /// <param name="onSuccess">Action to invoke when the web request is successful.</param>
        /// <param name="onError">Action to invoke when the web request fails.</param>
        /// <returns>The coroutine used for retrieving the ranking.</returns>
        public static IEnumerator Ranking(string authToken, int numEntries, Action<RankingResponse> onSuccess, Action<string> onError)
        {
            var www = UnityWebRequest.Get(baseURL + "ranking?numEntries=" + numEntries);
            www.SetRequestHeader("Authorization", "Bearer " + authToken);
            yield return www.Send();

            if (www.isNetworkError)
            {
                onError(www.error);
            }
            else
            {
                var response = JsonUtility.FromJson<RankingResponse>(www.downloadHandler.text);
                onSuccess(response);
            }
        }

        /// <summary>
        /// Adds a new win to the specified player's profile.
        /// </summary>
        /// <param name="username">Player's name.</param>
        /// <param name="onSuccess">Action to invoke when the web request is successful.</param>
        /// <param name="onError">Action to invoke when the web request fails.</param>
        /// <returns>The coroutine used for reporting a new win.</returns>
        public static IEnumerator ReportGameWin(string username, Action<string> onSuccess, Action<string> onError)
        {
            var form = new WWWForm();
            form.AddField("name", username);

            var www = UnityWebRequest.Post(baseURL + "reportGameWin", form);
            yield return www.Send();

            if (www.isNetworkError)
                onError(www.error);
            else
                onSuccess(www.downloadHandler.text);
        }

        /// <summary>
        /// Adds a new loss to the specified player's profile.
        /// </summary>
        /// <param name="username">Player's name.</param>
        /// <param name="onSuccess">Action to invoke when the web request is successful.</param>
        /// <param name="onError">Action to invoke when the web request fails.</param>
        /// <returns>The coroutine used for reporting a new loss.</returns>
        public static IEnumerator ReportGameLoss(string username, Action<string> onSuccess, Action<string> onError)
        {
            var form = new WWWForm();
            form.AddField("name", username);

            var www = UnityWebRequest.Post(baseURL + "reportGameLoss", form);
            yield return www.Send();

            if (www.isNetworkError)
                onError(www.error);
            else
                onSuccess(www.downloadHandler.text);
        }

        /// <summary>
        /// Gets the currency of the player with the specified authentication data.
        /// </summary>
        /// <param name="authToken">Player's authentication token.</param>
        /// <param name="onSuccess">Action to invoke when the web request is successful.</param>
        /// <param name="onError">Action to invoke when the web request fails.</param>
        /// <returns>The coroutine used for retrieving the player's currency.</returns>
        public static IEnumerator GetCurrency(string authToken, Action<GetCurrencyResponse> onSuccess, Action<string> onError)
        {
            var www = UnityWebRequest.Get(baseURL + "getCurrency");
            www.SetRequestHeader("Authorization", "Bearer " + authToken);
            yield return www.Send();

            if (www.isNetworkError)
            {
                onError(www.error);
            }
            else
            {
                var response = JsonUtility.FromJson<GetCurrencyResponse>(www.downloadHandler.text);
                onSuccess(response);
            }
        }

        /// <summary>
        /// Buys a new card pack for the player with the specified authentication data.
        /// </summary>
        /// <param name="authToken">Player's authentication token.</param>
        /// <param name="numPacks">Number of card packs to buy.</param>
        /// <param name="onSuccess">Action to invoke when the web request is successful.</param>
        /// <param name="onError">Action to invoke when the web request fails.</param>
        /// <returns>The coroutine used for buying a new card pack.</returns>
        public static IEnumerator BuyCardPack(string authToken, int numPacks, Action<BuyCardPackResponse> onSuccess, Action<string> onError)
        {
            var form = new WWWForm();
            form.AddField("numPacks", numPacks);

            var www = UnityWebRequest.Post(baseURL + "buyCardPacks", form);
            www.SetRequestHeader("Authorization", "Bearer " + authToken);
            yield return www.Send();

            if (www.isNetworkError)
            {
                onError(www.error);
            }
            else
            {
                var response = JsonUtility.FromJson<BuyCardPackResponse>(www.downloadHandler.text);
                onSuccess(response);
            }
        }

        /// <summary>
        /// Opens a new card pack for the player with the specified authentication data.
        /// </summary>
        /// <param name="authToken">Player's authentication token.</param>
        /// <param name="onSuccess">Action to invoke when the web request is successful.</param>
        /// <param name="onError">Action to invoke when the web request fails.</param>
        /// <returns>The coroutine used for opening a card pack.</returns>
        public static IEnumerator OpenCardPack(string authToken, Action<OpenCardPackResponse> onSuccess, Action<string> onError)
        {
            var www = UnityWebRequest.Get(baseURL + "openCardPack");
            www.SetRequestHeader("Authorization", "Bearer " + authToken);
            yield return www.Send();

            if (www.isNetworkError)
            {
                onError(www.error);
            }
            else
            {
                var response = JsonUtility.FromJson<OpenCardPackResponse>(www.downloadHandler.text);
                onSuccess(response);
            }
        }
    }
}
