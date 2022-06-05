// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Networking;

using CCGKit;

/// <summary>
/// This class holds information about a player avatar from the game scene, which can be clicked
/// to select a target player for an effect or during combat (this will send the appropriate
/// information to the server).
/// </summary>
public class PlayerAvatar : MonoBehaviour
{
    public bool IsBottom;

    public void OnClicked()
    {
        var client = NetworkManager.singleton.client;
        var localPlayer = NetworkingUtils.GetActiveLocalPlayer();
        var targetPlayer = GetTargetPlayer();
        if (localPlayer.IsWaitingForEffectPlayerTargetSelection())
        {
            var msg = new TargetPlayerSelectedMessage();
            msg.NetId = targetPlayer.netId;
            client.Send(NetworkProtocol.TargetPlayerSelected, msg);
            localPlayer.SetWaitingForEffectPlayerTargetSelection(false);
        }
        else if (localPlayer.IsWaitingForAttackTargetSelection())
        {
            var msg = new AttackedPlayerSelectedMessage();
            msg.NetId = targetPlayer.netId;
            client.Send(NetworkProtocol.AttackedPlayerSelected, msg);
            localPlayer.SetWaitingForAttackTargetSelection(false);
        }
    }

    private Player GetTargetPlayer()
    {
        var players = FindObjectsOfType<Player>();
        if (IsBottom)
        {
            foreach (var player in players)
            {
                if (player.isLocalPlayer && player.IsHuman)
                    return player;
            }
        }
        else
        {
            foreach (var player in players)
            {
                if (!player.isLocalPlayer || (player.isLocalPlayer && !player.IsHuman))
                    return player;
            }
        }
        return null;
    }
}
