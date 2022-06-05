// Copyright (C) 2016 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Networking;

using CCGKit;

/// <summary>
/// Create a NetworkManager subclass with an automatically-managed lifetime. Having a subclass will also
/// come in handy if we need to extend the capabilities of the vanilla NetworkManager in the future.
/// </summary>
public class GameNetworkManager : NetworkManager
{
    private static GameNetworkManager instance;

    public static GameNetworkManager Instance
    {
        get
        {
            return instance ?? new GameObject("GameNetworkManager").AddComponent<GameNetworkManager>();
        }
    }

    /// <summary>
    /// Indicates if the current match is single-player against the AI or multiplayer between humans.
    /// </summary>
    public bool IsSinglePlayer;

    public string ActiveDeck;

    /// <summary>
    /// Prefab for the AI-controlled player.
    /// </summary>
    public GameObject AIPlayerPrefab;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        // UNET currently crashes on iOS if the runInBackground property is set to true.
        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.tvOS)
            runInBackground = false;
    }

    private void OnEnable()
    {
        if (instance == null)
            instance = this;
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        GameObject player = null;
        if (IsSinglePlayer && playerControllerId == 1)
        {
            player = Instantiate(AIPlayerPrefab);
            player.name = "AIPlayer";
        }
        else
        {
            player = Instantiate(playerPrefab);
            player.name = "HumanPlayer";
        }
        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect(conn);

        var server = GameObject.Find("Server");
        if (server != null)
            server.GetComponent<Server>().OnPlayerConnected(conn.connectionId);
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        base.OnServerDisconnect(conn);

        var server = GameObject.Find("Server");
        if (server != null)
            server.GetComponent<Server>().OnPlayerDisconnected(conn.connectionId);
    }
}
