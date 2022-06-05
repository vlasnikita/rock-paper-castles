// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;

using CCGKit;

/// <summary>
/// The demo server is a subclass of the core Server type which adds demo-specific functionality,
/// like automatically increasing the players' mana pool every turn and determining the game's win
/// condition.
/// </summary>
public class DemoServer : Server
{
    public GameObject NetworkCardPrefab;

    protected void Awake()
    {
        Assert.IsTrue(NetworkCardPrefab != null);
    }

    protected override void AddServerHandlers()
    {
        base.AddServerHandlers();
        handlers.Add(new CardPlayingHandler(this));
        handlers.Add(new CombatHandler(this));
    }

    public override void OnPlayerConnected(int connectionId){
        base.OnPlayerConnected(connectionId);
        Debug.Log("Player connected: " + connectionId);
    }

    protected override void PerformTurnStartStateInitialization()
    {
        base.PerformTurnStartStateInitialization();

        // Automatically increase the mana available to a player every turn.
        if (currentTurn > 1)
        {
            var manaIndex = CurrentPlayer.Attributes.FindIndex(x => x.Name == "Mana");
            if (manaIndex >= 0)
            {
                var newMana = currentTurn;
                // Cap maximum available mana to 10.
                if (newMana >= 10)
                    newMana = 10;
                CurrentPlayer.Attributes[manaIndex] = new PlayerAttribute("Mana", newMana);
            }
        }
    }

    public override GameObject CreateNetworkCard(Card card)
    {
        return Instantiate(NetworkCardPrefab) as GameObject;
    }

    public override void SendPlayerStateToAllClients(PlayerState player)
    {
        base.SendPlayerStateToAllClients(player);

        // Check if a player has 0 or less lives and, therefore, the game should end.
        if (player.GetAttribute("Life").Value.Value <= 0)
        {
            var msg = new EndGameMessage();
            msg.WinnerPlayerIndex = Players.FindIndex(x => x != player);
            NetworkServer.SendToAll(NetworkProtocol.EndGame, msg);

            // Report winners and losers for ranking purposes.
            StartCoroutine(PlayerServer.ReportGameLoss(player.Name, (success) => { }, (error) => { }));
            foreach (var loser in Players.FindAll(x => x != player))
                StartCoroutine(PlayerServer.ReportGameWin(loser.Name, (success) => { }, (error) => { }));
        }
    }
}
