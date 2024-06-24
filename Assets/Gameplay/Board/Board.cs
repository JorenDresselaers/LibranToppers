using Mirror;
using System.Collections.Generic;
using UnityEngine;
using static APIManager;

public class Board : NetworkBehaviour
{
    public Player _player;
    [SerializeField] private int _maxCards = 5; // Maximum number of cards the board can hold
    //[SerializeField] private float _spacing = 1.5f;  // Space between cards
    [SerializeField] private Vector3 _startPosition = new Vector3(-3f, 0, 0); // Starting position of the first card
    [SerializeField] private BoxCollider _collider; // Collider for board size determination
    [SerializeField] private Hand _hand; 

    private List<Card> _cards = new List<Card>(); // List to hold current cards on the board
    public List<Card> Cards => _cards;
    public bool IsFull => _cards.Count >= _maxCards;

    private void Awake()
    {
        if (!_collider)
        {
            _collider = GetComponent<BoxCollider>();
        }

        _startPosition.x = -_collider.size.x / 2;
    }

    public bool AddCard(Card card)
    {
        if (IsFull)
        {
            Debug.Log("Board is full");
            return false;
        }

        card.transform.SetParent(transform);
        card.transform.localRotation = Quaternion.identity;
        card.transform.localScale = Vector3.one;
        if (isServer) RpcReparentCard(card);
        PlaceCard(card);
        _cards.Add(card);
        if(_hand) _hand.RemoveCard(card);

        List<Card> cardsOnAllBoards = new();
        cardsOnAllBoards.AddRange(_cards);
        if (_player.Opponent != null) cardsOnAllBoards.AddRange(_player.Opponent.Board.Cards);
        foreach (Card boardCard in cardsOnAllBoards)
        {
            boardCard.OnAuraCheck();
        }

        UpdateCardPositions();
        return true;
    }

    [ClientRpc]
    private void RpcReparentCard(Card card)
    {
        card.transform.SetParent(transform);
        card.transform.localRotation = Quaternion.identity;
        card.transform.localScale = Vector3.one;
    }

    //This should have an intermediary step that checks for client or server, and calls the appropriate function
    [Command(requiresAuthority = false)]
    public void CmdRemoveCard(Card card, bool checkForEndOffensive)
    {
        if (card != null)
        {
            _cards.Remove(card);
            RpcRemoveCard(card);
            Destroy(card.gameObject);

            List<Card> cardsOnAllBoards = new();
            cardsOnAllBoards.AddRange(_cards);
            if (_player.Opponent != null) cardsOnAllBoards.AddRange(_player.Opponent.Board.Cards);
            foreach (Card boardCard in cardsOnAllBoards)
            {
                boardCard.OnAuraCheck();
            }

            RpcUpdateCardPositions();
            if (checkForEndOffensive) _player.CheckIfOffensiveOver();
        }
    }

    [ClientRpc]
    private void RpcRemoveCard(Card card)
    {
        _cards.Remove(card);
    }

    private void PlaceCard(Card card)
    {
        float boardWidth = _collider.size.x;
        float cardWidth = boardWidth / _maxCards;
        float startX = (_collider.bounds.min.x + _collider.bounds.max.x) / 2 - boardWidth / 2 + cardWidth / 2; // centering cards

        float xPosition = startX + (_cards.Count * (cardWidth));
        card.transform.localPosition = new Vector3(xPosition, _startPosition.y, _startPosition.z);
    }

    [ClientRpc]
    public void RpcUpdateCardPositions() 
    {
        UpdateCardPositions();
    }

    private void UpdateCardPositions()
    {
        float boardWidth = _collider.size.x;
        float cardWidth = boardWidth / _maxCards;
        float startX = (_collider.bounds.min.x + _collider.bounds.max.x) / 2 - boardWidth / 2 + cardWidth / 2;

        for (int i = 0; i < _cards.Count; i++)
        {
            float xPosition = startX + (i * (cardWidth));
            _cards[i].transform.localPosition = new Vector3(xPosition, _startPosition.y, _startPosition.z);
        }
    }
}
