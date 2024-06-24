using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class TurnManager : NetworkBehaviour
{
    private static TurnManager _instance;
    public static TurnManager Instance => _instance;

    [SerializeField] private TextMeshProUGUI _endScreenText;

    [SyncVar]
    public List<Player> _players = new(); // List of players in the game
    [SyncVar]
    private int _currentPlayerIndex = 0; // Index of the current player

    public EndTurnOnClick _endTurnButton;

    private Coroutine _endOffensiveCoroutine;

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
        _endScreenText.gameObject.SetActive(false);
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

        SetOpponents();

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

        SetOpponents();

        StartOffensive();
    }

    private void SetOpponents()
    {
        for (int currentPlayer = 0; currentPlayer < _players.Count; currentPlayer++)
        {
            int opponentIndex = currentPlayer + 1;
            if (opponentIndex >= _players.Count) opponentIndex = 0;
            _players[currentPlayer].SetOpponent(_players[opponentIndex]);
        }
    }

    private void StartOffensive()
    {
        foreach (Player player in _players)
        {
            player.ServerStartOffensive();
        }

        print($"Offensive started");

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

    public void EndOffensive()
    {
        int playersWithCardsLeft = 0;
        foreach (Player player in _players)
        {
            //player.ServerEndTurn();
            player.ServerEndOffensive();
            if (player.TotalCardsRemaining > 0) playersWithCardsLeft++;
        }

        if(playersWithCardsLeft > 1)
        {
            print("Players still have cards, starting new offensive");
            StartOffensive();
        }
        else
        {
            Player winner = null;
            foreach (Player player in _players)
            {
                if (player.TotalCardsRemaining > 0) winner = player;
            }
            EndGame(winner);
        }
    }

    [ClientRpc]
    private void RpcSetEndText(Player winner)
    {
        _endScreenText.gameObject.SetActive(true);
        string endText = "This should not be here";
        endText = winner.isLocalPlayer ? "Victory!" : "Defeat!";

        _endScreenText.text = endText;
    }

    public void EndOffensiveAfterSeconds(float seconds)
    {
        if (_endOffensiveCoroutine == null)
        {
            print("Ending offensive");
            _endOffensiveCoroutine = StartCoroutine(EndOffensiveAfterSecondsCoroutine(seconds));
        }
    }

    private IEnumerator EndOffensiveAfterSecondsCoroutine(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        EndOffensive();
        _endOffensiveCoroutine = null;
        print("Offensive ended");
    }

    private void EndGame(Player winner)
    {
        foreach(Player player in _players)
        {
            player.ServerEndGame();
        }
        print($"The game is now over!");
        RpcSetEndText(winner);
    }

    private void StartTurn()
    {
        print($"Turn started for player {_currentPlayerIndex}");

        // Get the current player
        Player currentPlayer = _players[_currentPlayerIndex];
        currentPlayer.ServerStartTurn();

        foreach (Player player in _players)
        {
            if(player != currentPlayer)
            {
                player.ServerEndTurn();
                print($"{player.Username}'s turn started");
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
