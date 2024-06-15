using Mirror;
using System.Collections;
using System.Collections.Generic;
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

    private void Awake()
    {
        Deck._player = this;
        Hand._player = this;
        Board._player = this;
        Graveyard._player = this;
        if(_opponent && _opponent._opponent != this) _opponent._opponent = this;

        Deck.ClickToDrawCards = !_drawCardsAutomatically;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        TurnManager.Instance.AddPlayer(this);
    }

    [Server]
    public void ServerStartTurn()
    {
        List<Card> cardsToUpdate = new List<Card>();
        cardsToUpdate.AddRange(_hand.Cards);
        cardsToUpdate.AddRange(_board.Cards);

        Debug.Log("Starting turn");
        foreach (Card card in cardsToUpdate)
        {
            card.OnStartOfTurn();
        }

        ToggleClickableObjects(true);

        if(_drawCardsAutomatically)
        {
            for (int i = 0; i < _cardsDrawnPerTurn; i++)
            {
                if (!Hand.IsFull)
                {
                    Deck.CmdCreateCard();
                }
                else break;
            }
        }
    }

    [ClientRpc]
    private void ToggleClickableObjects(bool isClickable)
    {
        _deck._isClickable = isClickable;
        List<Card> cardsToDisable = new List<Card>();
        cardsToDisable.AddRange(_hand.Cards);
        cardsToDisable.AddRange(_board.Cards);

        foreach (Card card in cardsToDisable)
        {
            card._isClickable = isClickable;
        }
    }

    [Server]
    public void ServerEndTurn()
    {
        List<Card> cardsToUpdate = new List<Card>();
        cardsToUpdate.AddRange(_hand.Cards);
        cardsToUpdate.AddRange(_board.Cards);

        Debug.Log("Ending turn");
        foreach (Card card in cardsToUpdate)
        {
            card.OnEndOfTurn();
        }

        ToggleClickableObjects(false);
    }
}
