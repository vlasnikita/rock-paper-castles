// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;
using UnityEngine.UI;

using CCGKit;

/// <summary>
/// This classes manages the game scene.
/// </summary>
public class GameScene : MonoBehaviour
{
    public Text CurrentPlayerNameText;
    public Text OpponentNameText;

    public Button EndTurnButton;

    public GameObject ChatDialogPrefab;

    private ChatDialog chatDialog;

    private void Awake()
    {
        Assert.IsTrue(CurrentPlayerNameText != null);
        Assert.IsTrue(OpponentNameText != null);
        Assert.IsTrue(EndTurnButton != null);
        Assert.IsTrue(ChatDialogPrefab != null);

        // Null the player name labels; they will be filled with the appropriate information
        // as players enter the game.
        CurrentPlayerNameText.text = "";
        OpponentNameText.text = "";
    }

    private void Start()
    {
        GameWindowUtils.OpenWaitGameStartDialog();
        if (GameNetworkManager.Instance.IsSinglePlayer)
            Invoke("AddBot", 1.5f);

        chatDialog = Instantiate(ChatDialogPrefab).GetComponent<ChatDialog>();
        chatDialog.name = "ChatDialog";
        chatDialog.Hide();
    }

    private void AddBot()
    {
        ClientScene.AddPlayer(1);
    }

    public void CloseWaitingWindow()
    {
        WindowManager.Instance.CloseWindow("WaitGameStartDialog");
    }

    /// <summary>
    /// Callback for when the end turn button is pressed.
    /// </summary>
    public void OnEndTurnButtonPressed()
    {
        var localPlayer = NetworkingUtils.GetLocalPlayer() as DemoHumanPlayer;
        if (localPlayer != null)
        {
            var maxHandSize = GameManager.Instance.Config.Properties.MaxHandSize;
            if (localPlayer.HandSize > maxHandSize)
            {
                var diff = localPlayer.HandSize - maxHandSize;
                if (diff == 1)
                    WindowUtils.OpenAlertDialog("You need to discard " + diff + " card from your hand.");
                else
                    WindowUtils.OpenAlertDialog("You need to discard " + diff + " cards from your hand.");
            }
            localPlayer.StopTurn();
        }
    }

    /// <summary>
    /// Callback for when the exit game button is pressed.
    /// </summary>
    public void OnExitGameButtonPressed()
    {
        WindowUtils.OpenConfirmationDialog("Do you really want to leave this game?", () =>
        {
            if (NetworkingUtils.GetLocalPlayer().isServer)
                GameNetworkManager.Instance.StopHost();
            else
                GameNetworkManager.Instance.StopClient();
        });
    }

    /// <summary>
    /// Callback for when the chat button is pressed.
    /// </summary>
    public void OnChatButtonPressed()
    {
        chatDialog.Show();
        var canvas = GameObject.Find("Canvas");
        if (canvas != null)
            chatDialog.gameObject.transform.SetParent(canvas.transform, false);
    }
}
