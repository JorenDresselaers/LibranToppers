using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnManager : NetworkBehaviour
{
    private static TurnManager _instance;
    public static TurnManager Instance => _instance;

    [SyncVar]
    public List<Player> _players = new(); // List of players in the game
    [SyncVar]
    private int _currentPlayerIndex = 0; // Index of the current player

    public EndTurnOnClick _endTurnButton;

    public void AddPlayer(Player player)
    {
        _players.Add(player);
    }

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        StartCoroutine(StartOffensiveWhenReady());
    }

    [ClientRpc]
    private void RpcColourButton(Player player)
    {
        _endTurnButton?.SetText(player.gameObject == NetworkClient.localPlayer.gameObject ? "End\nTurn" : "Opponent's\nTurn");
    }

    private IEnumerator StartTurnWhenReady()
    {
        print("Waiting for players");
        yield return new WaitUntil(() => _players.Count > 1);

        foreach (Player player in _players)
        {
            player.RpcSyncNames();
        }

        StartTurn();
    }
    
    private IEnumerator StartOffensiveWhenReady()
    {
        print("Waiting for players");
        yield return new WaitUntil(() => _players.Count > 1);

        foreach (Player player in _players)
        {
            player.RpcSyncNames();
        }

        StartOffensive();
    }

    private void StartOffensive()
    {
        foreach (Player player in _players)
        {
            player.ServerStartOffensive();
        }

        print($"Turn started for player {_currentPlayerIndex}");

        // Get the current player
        Player currentPlayer = _players[_currentPlayerIndex];
        currentPlayer.ServerStartTurn();

        foreach (Player player in _players)
        {
            if (player != currentPlayer)
            {
                player.ServerEndTurn();
            }
        }

        RpcColourButton(currentPlayer);
    }

    private void StartTurn()
    {
        print($"Turn started for player {_currentPlayerIndex}");

        // Get the current player
        Player currentPlayer = _players[_currentPlayerIndex];
        currentPlayer.ServerStartTurn();

        foreach(Player player in _players)
        {
            if(player != currentPlayer)
            {
                player.ServerEndTurn();
            }
        }

        //RpcStartTurn(currentPlayer);

        RpcColourButton(currentPlayer);
    }

    public void StartNextTurn()
    {
        if (isServer)
        {
            _currentPlayerIndex = (_currentPlayerIndex + 1) % _players.Count;
            StartTurn();
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdEndTurn(Player player)
    {
        if (player == _players[_currentPlayerIndex])
        {
            player.ServerEndTurn();
            RpcEndTurn(player);

            RpcColourButton(player);
        }
    }

    [ClientRpc]
    public void RpcEndTurn(Player player)
    {
        //player.EndTurn();
        StartNextTurn();
    }
}
