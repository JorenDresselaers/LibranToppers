using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class Player : NetworkBehaviour
{
    [Header("Scene Objects")]
    [SerializeField] protected GameObject _gameElements;

    [SerializeField] protected Deck _deck;
    public Deck Deck => _deck;

    [SerializeField] protected Hand _hand;
    public Hand Hand => _hand;

    [SerializeField] protected Board _board;
    public Board Board => _board;

    [SerializeField] protected Graveyard _graveYard;
    public Graveyard Graveyard => _graveYard;

    [SerializeField] protected Player _opponent;
    public Player Opponent => _opponent;

    public EndTurnOnClick _endTurnButton;
    protected TurnManager _turnManager;

    [Header("Settings")]
    [SerializeField] protected bool _drawCardsAutomatically = true;
    [SerializeField] protected int _cardsDrawnPerTurn = 5;
    [SerializeField] protected int _cardsPlayedPerTurn = 1;
    protected int _cardsPlayedThisTurn = 0;
    public bool CanPlayCards => _cardsPlayedThisTurn < _cardsPlayedPerTurn;

    [SyncVar] protected string _username;
    public string Username => _username;

    [Header("UI")]
    [SerializeField] protected TMP_Text _name;

    public int TotalCardsRemaining => _hand.Cards.Count + _board.Cards.Count + _deck.CardsData.Count;
    public int CardsLeftInOffensive => _hand.Cards.Count + _board.Cards.Count;

    protected Coroutine _drawCardsCoroutine;

    protected void Awake()
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
        _turnManager = TurnManager.Instance;
        _endTurnButton = _turnManager._endTurnButton;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        _turnManager = TurnManager.Instance;
        _endTurnButton = _turnManager._endTurnButton;
    }

    public void ToggleVisuals(bool enabled)
    {
        if (isServer) RpcToggleVisuals(enabled);
        else CmdToggleVisuals(enabled);
    }
    
    [ClientRpc]
    protected void RpcToggleVisuals(bool enabled)
    {
        _gameElements.SetActive(enabled);
    }

    [Command]
    protected void CmdToggleVisuals(bool enabled)
    {
        RpcToggleVisuals(enabled);
    }

    [ClientRpc]
    public void RpcSetOpponent(Player opponent)
    {
        _opponent = opponent;
    }

    [Command]
    public void CmdSetOpponent(Player opponent)
    {
        RpcSetOpponent(opponent);
    }

    public void SetOpponent(Player opponent)
    {
        if (isServer) RpcSetOpponent(opponent);
        else CmdSetOpponent(opponent);
    }

    protected IEnumerator SetNameAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (APIManager.Instance.IsLoggedIn) CmdSetName(APIManager.Instance.UserData.username);
    }

    [Server]
    public void ServerStartOffensive()
    {
        ToggleVisuals(true);
        _deck._isClickable = true;

        // Draw cards if the setting is enabled
        if (_drawCardsAutomatically)
        {
            if(_drawCardsCoroutine == null) _drawCardsCoroutine = StartCoroutine(DrawCards());
        }
        else
        {
            RpcToggleClickableObjects(true, true, true, true); //This will do weird shit
        }

        //RpcUpdateCardInteractionIndicators();
    }
    
    [Server]
    public void ServerEndOffensive()
    {
        List<Card> cardsToUpdate = new List<Card>();
        cardsToUpdate.AddRange(_hand.Cards);
        cardsToUpdate.AddRange(_board.Cards);

        foreach (Card card in cardsToUpdate)
        {
            if (card != null)
            {
                CardData data = card.Data;
                if(card.Board != null) card.Board.CmdRemoveCard(card, false);
                if(card.Hand != null) card.Hand.RemoveCard(card);
                card._player.Deck.CmdAddCard(data);
                Destroy(card.gameObject);
            }
        }

        //RpcToggleCardInteractionIndicators(false, true, true);
    }

    [Server]
    public void ServerEndGame()
    {
        ToggleVisuals(false);
    }

    [Server]
    virtual public void ServerStartTurn()
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
        RpcUpdateCardInteractionIndicators();
        //print($"{_username}'s turn started");
    }

    [ClientRpc]
    protected void RpcToggleClickableObjects(bool isClickable, bool includeDeck, bool includeHand, bool includeBoard)
    {
        bool isThisPlayer = NetworkClient.localPlayer == this;

        if(includeDeck) _deck._isClickable = isClickable;

        List<Card> cardsToToggle = new List<Card>();
        if(includeHand) cardsToToggle.AddRange(_hand.Cards);
        if(includeBoard) cardsToToggle.AddRange(_board.Cards);

        foreach (Card card in cardsToToggle)
        {
            card._isClickable = isClickable;
            //card.ToggleInteractionIndicator(isClickable);
        }
    }

    [ClientRpc]
    protected void RpcUpdateCardInteractionIndicators()
    {
        List<Card> cardsToToggle = new List<Card>();
        cardsToToggle.AddRange(_hand.Cards);
        cardsToToggle.AddRange(_board.Cards);

        foreach (Card card in cardsToToggle)
        {
            card.UpdateInteractionIndicator();
        }
    }

    [ClientRpc]
    protected void RpcToggleCardInteractionIndicators(bool toggle, bool includeHand, bool includeBoard)
    {
        List<Card> cardsToToggle = new List<Card>();
        if (includeHand) cardsToToggle.AddRange(_hand.Cards);
        if (includeBoard) cardsToToggle.AddRange(_board.Cards);

        foreach (Card card in cardsToToggle)
        {
            card.ToggleInteractionIndicator(toggle);
        }
    }

    [ClientRpc]
    protected void RpcToggleEndTurnInteractionIndicator(bool toggle)
    {
        _endTurnButton.ToggleInteractionIndicator(toggle);
    }

    [Server]
    protected IEnumerator DrawCards()
    {
        print("Drawing cards for " + _username);
        for (int i = 0; i < _cardsDrawnPerTurn; i++)
        {
            if (!Hand.IsFull)
            {
                Deck.CmdCreateCard();
                yield return new WaitForSeconds(0.2f); // Allow time for cards to be created and synced
            }
            else break;
        }
        
        print("Updating cards, cards in hand: " + _hand.Cards.Count);

        if(_turnManager.IsThisPlayersTurn(this)) RpcUpdateCardInteractionIndicators();
        else RpcToggleCardInteractionIndicators(false, true, true);
        _drawCardsCoroutine = null;
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
        RpcToggleCardInteractionIndicators(false, true, true);
        RpcToggleEndTurnInteractionIndicator(false);
        //print($"{_username}'s turn ended");
    }

    [Command]
    public void CmdSetName(string name)
    {
        print($"Setting name of {name} on server");
        _username = name;
        RpcSetName(name);
    }

    [ClientRpc]
    protected void RpcSetName(string name)
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
    public void CmdOnCardPlayed()
    {
        _cardsPlayedThisTurn++;
        if (_cardsPlayedThisTurn >= _cardsPlayedPerTurn)
        {
            RpcToggleClickableObjects(false, false, true, false);
            RpcToggleCardInteractionIndicators(false, true, false);
        }
    }

    public void CmdOnCardInteracted(Card caster, Card target)
    {
        if (!_board.CanCardsInteract) _endTurnButton.ToggleInteractionIndicator(true);
    }

    [Server]
    public bool CheckIfOffensiveOver()
    {
        if(CardsLeftInOffensive <= 0 || _opponent.CardsLeftInOffensive <= 0)
        {
            print("Ending offensive");
            _turnManager.EndOffensiveAfterSeconds(1f);
            return true;
        }
        return false;
    }
}
