// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using CCGKit;

/// <summary>
/// This scene shows the game ranking, with players being sorted according to their number of wins.
/// </summary>
public class RankingScene : MonoBehaviour
{
    public List<Text> PlayerNames;
    public List<Text> PlayerScores;

    private void Start()
    {
        foreach (var name in PlayerNames)
            name.text = "";

        foreach (var score in PlayerScores)
            score.text = "";

        StartCoroutine(PlayerServer.Ranking(GameManager.Instance.AuthToken, 10,
            (response) =>
            {
                if (response.status == "success")
                {
                    for (var i = 0; i < response.users.Count; i++)
                    {
                        var user = response.users[i];
                        PlayerNames[i].text = user.name;
                        PlayerScores[i].text = user.numGamesWon.ToString();
                    }
                }
                else if (response.status == "error")
                {
                    WindowUtils.OpenAlertDialog("There was an error retrieving the ranking.");
                }
            },
            (error) =>
            {
                WindowUtils.OpenAlertDialog("Connection error. Please check your network.");
            }));
    }

    /// <summary>
    /// Back button callback.
    /// </summary>
    public void OnBackButtonPressed()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
