// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Networking;

namespace CCGKit
{
    /// <summary>
    /// This class provides general networking utilities.
    /// </summary>
    public static class NetworkingUtils
    {
        /// <summary>
        /// Returns the local player for this client.
        /// </summary>
        /// <returns>The local player for this client.</returns>
        public static Player GetLocalPlayer()
        {
            Player localPlayer = null;
            foreach (var pc in NetworkManager.singleton.client.connection.playerControllers)
            {
                var player = pc.gameObject.GetComponent<Player>();
                if (player.isLocalPlayer)
                {
                    localPlayer = player;
                    break;
                }
            }
            
            return localPlayer;
        }

        /// <summary>
        /// Returns the human local player for this client.
        /// </summary>
        /// <returns>The human local player for this client.</returns>
        public static Player GetHumanLocalPlayer()
        {
            Player localPlayer = null;
            foreach (var pc in NetworkManager.singleton.client.connection.playerControllers)
            {
                var player = pc.gameObject.GetComponent<Player>();
                if (player.isLocalPlayer && player.IsHuman)
                {
                    localPlayer = player;
                    break;
                }
            }
            return localPlayer;
        }

        /// <summary>
        /// Returns the active local player for this client.
        /// </summary>
        /// <returns>The active local player for this client.</returns>
        public static Player GetActiveLocalPlayer()
        {
            Player localPlayer = null;
            foreach (var pc in NetworkManager.singleton.client.connection.playerControllers)
            {
                var player = pc.gameObject.GetComponent<Player>();
                if (player.isLocalPlayer && player.IsActivePlayer)
                {
                    localPlayer = player;
                    break;
                }
            }
            return localPlayer;
        }

        /// <summary>
        /// Returns the network object with the specified network identifier.
        /// </summary>
        /// <param name="netId">Network identifier of the network object we want to retrieve.</param>
        /// <returns>The network object with the specified network identifier.</returns>
        public static GameObject GetNetworkObject(NetworkInstanceId netId)
        {
            foreach (var pair in NetworkServer.objects)
            {
                var obj = pair.Value.gameObject.GetComponent<NetworkBehaviour>();
                if (obj != null && obj.netId == netId)
                    return obj.gameObject;
            }
            return null;
        }
    }
}
