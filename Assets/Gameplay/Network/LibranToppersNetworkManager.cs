using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using System.Collections.Generic;
using Unity.Services.Relay.Models;
using Utp;
using Unity.Services.Core;
using Unity.Services.Authentication;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/components/network-manager
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkManager.html
*/

public class LibranToppersNetworkManager : NetworkManager
{
    // Overrides the base singleton so we don't
    // have to cast to this type everywhere.
    public static new LibranToppersNetworkManager singleton => (LibranToppersNetworkManager)NetworkManager.singleton;

    private UtpTransport utpTransport;

    /// <summary>
    /// Server's join code if using Relay.
    /// </summary>
    public string relayJoinCode = "";

    [Header("Prefabs")]
    [SerializeField] private AIPlayer _aiPlayer;
    
    public override void Awake()
    {
        base.Awake();

        utpTransport = GetComponent<UtpTransport>();

        string[] args = System.Environment.GetCommandLineArgs();
        for (int key = 0; key < args.Length; key++)
        {
            if (args[key] == "-port")
            {
                if (key + 1 < args.Length)
                {
                    string value = args[key + 1];

                    try
                    {
                        utpTransport.Port = ushort.Parse(value);
                    }
                    catch
                    {
                        UtpLog.Warning($"Unable to parse {value} into transport Port");
                    }
                }
            }
        }

        InitializeRelay();
    }

    private async void InitializeRelay()
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            //If not already logged, log the user in
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        print("Relay initialized");
    }

    #region Relay
    /// <summary>
    /// Get the port the server is listening on.
    /// </summary>
    /// <returns>The port.</returns>
    public ushort GetPort()
    {
        return utpTransport.Port;
    }

    /// <summary>
    /// Get whether Relay is enabled or not.
    /// </summary>
    /// <returns>True if enabled, false otherwise.</returns>
    public bool IsRelayEnabled()
    {
        return utpTransport.useRelay;
    }

    /// <summary>
    /// Ensures Relay is disabled. Starts the server, listening for incoming connections.
    /// </summary>
    public void StartStandardServer()
    {
        utpTransport.useRelay = false;
        StartServer();
    }

    /// <summary>
    /// Ensures Relay is disabled. Starts a network "host" - a server and client in the same application
    /// </summary>
    public void StartStandardHost()
    {
        utpTransport.useRelay = false;
        StartHost();
    }

    /// <summary>
    /// Gets available Relay regions.
    /// </summary>
    /// 
    public void GetRelayRegions(Action<List<Region>> onSuccess, Action onFailure)
    {
        utpTransport.GetRelayRegions(onSuccess, onFailure);
    }

    /// <summary>
    /// Ensures Relay is enabled. Starts a network "host" - a server and client in the same application
    /// </summary>
    public void StartRelayHost(int maxPlayers, string regionId = null)
    {
        utpTransport.useRelay = true;
        utpTransport.AllocateRelayServer(maxPlayers, regionId,
        (string joinCode) =>
        {
            relayJoinCode = joinCode;

            StartHost();
        },
        () =>
        {
            UtpLog.Error($"Failed to start a Relay host.");
        });
    }

    /// <summary>
    /// Ensures Relay is disabled. Starts the client, connects it to the server with networkAddress.
    /// </summary>
    public void JoinStandardServer()
    {
        utpTransport.useRelay = false;
        StartClient();
    }

    /// <summary>
    /// Ensures Relay is enabled. Starts the client, connects to the server with the relayJoinCode.
    /// </summary>
    public void JoinRelayServer()
    {
        utpTransport.useRelay = true;
        utpTransport.ConfigureClientWithJoinCode(relayJoinCode,
        () =>
        {
            StartClient();
        },
        () =>
        {
            UtpLog.Error($"Failed to join Relay server.");
        });
    }
    #endregion

    #region Unity Callbacks

    public override void OnValidate()
    {
        base.OnValidate();
    }

    /// <summary>
    /// Runs on both Server and Client
    /// Networking is NOT initialized when this fires
    /// </summary>
    public override void Start()
    {
        base.Start();
    }

    /// <summary>
    /// Runs on both Server and Client
    /// </summary>
    public override void LateUpdate()
    {
        base.LateUpdate();
    }

    /// <summary>
    /// Runs on both Server and Client
    /// </summary>
    public override void OnDestroy()
    {
        base.OnDestroy();
    }

    #endregion

    #region Start & Stop

    /// <summary>
    /// Set the frame rate for a headless server.
    /// <para>Override if you wish to disable the behavior or set your own tick rate.</para>
    /// </summary>
    public override void ConfigureHeadlessFrameRate()
    {
        base.ConfigureHeadlessFrameRate();
    }

    /// <summary>
    /// called when quitting the application by closing the window / pressing stop in the editor
    /// </summary>
    public override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
    }

    #endregion

    #region Scene Management

    /// <summary>
    /// This causes the server to switch scenes and sets the networkSceneName.
    /// <para>Clients that connect to this server will automatically switch to this scene. This is called automatically if onlineScene or offlineScene are set, but it can be called from user code to switch scenes again while the game is in progress. This automatically sets clients to be not-ready. The clients must call NetworkClient.Ready() again to participate in the new scene.</para>
    /// </summary>
    /// <param name="newSceneName"></param>
    public override void ServerChangeScene(string newSceneName)
    {
        base.ServerChangeScene(newSceneName);
    }

    /// <summary>
    /// Called from ServerChangeScene immediately before SceneManager.LoadSceneAsync is executed
    /// <para>This allows server to do work / cleanup / prep before the scene changes.</para>
    /// </summary>
    /// <param name="newSceneName">Name of the scene that's about to be loaded</param>
    public override void OnServerChangeScene(string newSceneName) { }

    /// <summary>
    /// Called on the server when a scene is completed loaded, when the scene load was initiated by the server with ServerChangeScene().
    /// </summary>
    /// <param name="sceneName">The name of the new scene.</param>
    public override void OnServerSceneChanged(string sceneName) { }

    /// <summary>
    /// Called from ClientChangeScene immediately before SceneManager.LoadSceneAsync is executed
    /// <para>This allows client to do work / cleanup / prep before the scene changes.</para>
    /// </summary>
    /// <param name="newSceneName">Name of the scene that's about to be loaded</param>
    /// <param name="sceneOperation">Scene operation that's about to happen</param>
    /// <param name="customHandling">true to indicate that scene loading will be handled through overrides</param>
    public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling) { }

    /// <summary>
    /// Called on clients when a scene has completed loaded, when the scene load was initiated by the server.
    /// <para>Scene changes can cause player objects to be destroyed. The default implementation of OnClientSceneChanged in the NetworkManager is to add a player object for the connection if no player object exists.</para>
    /// </summary>
    public override void OnClientSceneChanged()
    {
        base.OnClientSceneChanged();
    }

    #endregion

    #region Server System Callbacks

    /// <summary>
    /// Called on the server when a new client connects.
    /// <para>Unity calls this on the Server when a Client connects to the Server. Use an override to tell the NetworkManager what to do when a client connects to the server.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public override void OnServerConnect(NetworkConnectionToClient conn) 
    {
    }

    /// <summary>
    /// Called on the server when a client is ready.
    /// <para>The default implementation of this function calls NetworkServer.SetClientReady() to continue the network setup process.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public override void OnServerReady(NetworkConnectionToClient conn)
    {
        base.OnServerReady(conn);
    }

    /// <summary>
    /// Called on the server when a client adds a new player with ClientScene.AddPlayer.
    /// <para>The default implementation for this function creates a new player object from the playerPrefab.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
    }

    /// <summary>
    /// Called on the server when a client disconnects.
    /// <para>This is called on the Server when a Client disconnects from the Server. Use an override to decide what should happen when a disconnection is detected.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
    }

    /// <summary>
    /// Called on server when transport raises an error.
    /// <para>NetworkConnection may be null.</para>
    /// </summary>
    /// <param name="conn">Connection of the client...may be null</param>
    /// <param name="transportError">TransportError enum</param>
    /// <param name="message">String message of the error.</param>
    public override void OnServerError(NetworkConnectionToClient conn, TransportError transportError, string message) { }

    #endregion

    #region Client System Callbacks

    /// <summary>
    /// Called on the client when connected to a server.
    /// <para>The default implementation of this function sets the client as ready and adds a player. Override the function to dictate what happens when the client connects.</para>
    /// </summary>
    public override void OnClientConnect()
    {
        base.OnClientConnect();
    }

    /// <summary>
    /// Called on clients when disconnected from a server.
    /// <para>This is called on the client when it disconnects from the server. Override this function to decide what happens when the client disconnects.</para>
    /// </summary>
    public override void OnClientDisconnect() { }

    /// <summary>
    /// Called on clients when a servers tells the client it is no longer ready.
    /// <para>This is commonly used when switching scenes.</para>
    /// </summary>
    public override void OnClientNotReady() { }

    /// <summary>
    /// Called on client when transport raises an error.</summary>
    /// </summary>
    /// <param name="transportError">TransportError enum.</param>
    /// <param name="message">String message of the error.</param>
    public override void OnClientError(TransportError transportError, string message) { }

    #endregion

    #region Start & Stop Callbacks

    // Since there are multiple versions of StartServer, StartClient and StartHost, to reliably customize
    // their functionality, users would need override all the versions. Instead these callbacks are invoked
    // from all versions, so users only need to implement this one case.

    /// <summary>
    /// This is invoked when a host is started.
    /// <para>StartHost has multiple signatures, but they all cause this hook to be called.</para>
    /// </summary>
    public override void OnStartHost() 
    {
        APIManager.Instance?.ToggleUI(false);
    }

    /// <summary>
    /// This is invoked when a server is started - including when a host is started.
    /// <para>StartServer has multiple signatures, but they all cause this hook to be called.</para>
    /// </summary>
    public override void OnStartServer() { }

    /// <summary>
    /// This is invoked when the client is started.
    /// </summary>
    public override void OnStartClient() { }

    /// <summary>
    /// This is called when a host is stopped.
    /// </summary>
    public override void OnStopHost() { }

    /// <summary>
    /// This is called when a server is stopped - including when a host is stopped.
    /// </summary>
    public override void OnStopServer() { }

    /// <summary>
    /// This is called when a client is stopped.
    /// </summary>
    public override void OnStopClient() { }

    #endregion
}
