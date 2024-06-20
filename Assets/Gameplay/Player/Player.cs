using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [Header("Scene Objects")]
    [SerializeField] private Deck _deck;
    public Deck Deck => _deck;

    [SerializeField] private Hand _hand;
    public Hand Hand => _hand;

    [SerializeField] private Board _board;
    public Board Board => _board;

    [SerializeField] private Graveyard _graveYard;
    public Graveyard Graveyard => _graveYard;

    [SerializeField] private Player _opponent;
    public Player Opponent => _opponent;

    public EndTurnOnClick _endTurnButton;

    [Header("Settings")]
    [SerializeField] private bool _drawCardsAutomatically = true;
    [SerializeField] private int _cardsDrawnPerTurn = 5;
    [SerializeField] private int _cardsPlayedPerTurn = 1;
    private int _cardsPlayedThisTurn = 0;

    [Header("UI")]
    [SerializeField] private TMP_Text _name;

    private void Awake()
    {
        Deck._player = this;
        Hand._player = this;
        Board._player = this;
        Graveyard._player = this;
        if (_opponent && _opponent._opponent != this) _opponent._opponent = this;

        Deck.ClickToDrawCards = !_drawCardsAutomatically;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        TurnManager.Instance.AddPlayer(this);
    }

    public override void OnStartClient()
    {
        //StartCoroutine(SetNameAfterSeconds(1f));
    }

    private IEnumerator SetNameAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (APIManager.Instance.IsLoggedIn) CmdSetName(APIManager.Instance.UserData.username);
    }

    [Server]
    public void ServerStartOffensive()
    {
        // Draw cards if the setting is enabled
        if (_drawCardsAutomatically)
        {
            StartCoroutine(DrawCards());
        }
        else
        {
            RpcToggleClickableObjects(true, true, true, true); //This will do weird shit
        }
    }

    [Server]
    public void ServerStartTurn()
    {
        List<Card> cardsToUpdate = new List<Card>();
        cardsToUpdate.AddRange(_hand.Cards);
        cardsToUpdate.AddRange(_board.Cards);

        foreach (Card card in cardsToUpdate)
        {
            card.OnStartOfTurn();
        }

        _cardsPlayedThisTurn = 0;
        RpcToggleClickableObjects(true, true, true, true);
    }

    [ClientRpc]
    private void RpcToggleClickableObjects(bool isClickable, bool includeDeck, bool includeHand, bool includeBoard)
    {
        bool isThisPlayer = NetworkClient.localPlayer == this;

        if(includeDeck) _deck._isClickable = isClickable;

        List<Card> cardsToToggle = new List<Card>();
        if(includeHand) cardsToToggle.AddRange(_hand.Cards);
        if(includeBoard) cardsToToggle.AddRange(_board.Cards);

        foreach (Card card in cardsToToggle)
        {
            card._isClickable = isClickable;
        }
    }

    [Server]
    private IEnumerator DrawCards()
    {
        for (int i = 0; i < _cardsDrawnPerTurn; i++)
        {
            if (!Hand.IsFull)
            {
                Deck.CmdCreateCard();
                yield return new WaitForSeconds(0.1f); // Allow time for cards to be created and synced
            }
            else break;
        }
    }

    [Server]
    public void ServerEndTurn()
    {
        List<Card> cardsToUpdate = new List<Card>();
        cardsToUpdate.AddRange(_hand.Cards);
        cardsToUpdate.AddRange(_board.Cards);

        foreach (Card card in cardsToUpdate)
        {
            card.OnEndOfTurn();
        }

        RpcToggleClickableObjects(false, true, true, true);
    }

    [Command]
    public void CmdSetName(string name)
    {
        print($"Setting name of {name} on server");
        RpcSetName(name);
    }

    [ClientRpc]
    private void RpcSetName(string name)
    {
        print("Setting name of " + name + " on client");
        if (_name) _name.text = name;
    }

    [ClientRpc]
    public void RpcSyncNames()
    {
        if (APIManager.Instance.IsLoggedIn) CmdSetName(APIManager.Instance.UserData.username);
    }

    [Command]
    public void OnCardPlayed()
    {
        _cardsPlayedThisTurn++;
        if (_cardsPlayedThisTurn >= _cardsPlayedPerTurn)
        {
            RpcToggleClickableObjects(false, false, true, false);
        }
    }
}
