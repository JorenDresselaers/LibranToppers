using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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

        //Debug.Log($"Starting turn for player {name}");
        foreach (Card card in cardsToUpdate)
        {
            card.OnStartOfTurn();
        }

        //Done here beforehand to set cards clickable correctly
        _deck._isClickable = true;
        if (_drawCardsAutomatically)
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
        ToggleClickableObjects(true);
    }

    [ClientRpc]
    private void ToggleClickableObjects(bool isClickable)
    {
        _deck._isClickable = isClickable;
        print("Deck is clickable: " + isClickable);
        List<Card> cardsToToggle = new List<Card>();
        cardsToToggle.AddRange(_hand.Cards);
        cardsToToggle.AddRange(_board.Cards);

        foreach (Card card in cardsToToggle)
        {
            //print($"{card.CardName} clickable: {isClickable}");
            card._isClickable = isClickable;
        }
    }

    [Server]
    public void ServerEndTurn()
    {
        List<Card> cardsToUpdate = new List<Card>();
        cardsToUpdate.AddRange(_hand.Cards);
        cardsToUpdate.AddRange(_board.Cards);

        //Debug.Log($"Ending turn for player {name}");
        foreach (Card card in cardsToUpdate)
        {
            card.OnEndOfTurn();
        }

        ToggleClickableObjects(false);
    }
}
