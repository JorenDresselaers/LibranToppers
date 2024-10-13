using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class Hand : NetworkBehaviour
{
    public Player _player;
    [SerializeField] private int _maxCards = 10; // Maximum number of cards the hand can hold
    [SerializeField] private BoxCollider _collider; // Collider for hand size determination
    [SerializeField] private float maxTiltAngle = 15f; // Maximum angle cards will tilt at the edges

    private List<Card> _cards = new(); // List to store cards in hand
    public List<Card> Cards => _cards;

    public bool IsFull => _cards.Count >= _maxCards;

    private void Awake()
    {
        if (!_collider)
        {
            _collider = GetComponent<BoxCollider>();
        }
    }

    // Method to add a card to the hand
    public bool AddCard(Card card)
    {
        if (_cards.Count >= _maxCards)
        {
            Debug.Log("Hand is full");
            return false;
        }

        CmdReparentCard(card);
        return true;
    }

    [Command(requiresAuthority = false)]
    private void CmdReparentCard(Card card)
    {
        RpcReparentCard(card);
    }

    [ClientRpc]
    private void RpcReparentCard(Card card)
    {
        card.transform.SetParent(transform);
        card.transform.localScale = Vector3.one;
        card.transform.localPosition = Vector3.zero;
        card._player = _player;
        _cards.Add(card);
        card.SetHand(this);
        UpdateCardPositions();
    }

    // Method to remove a card from the hand
    public void RemoveCard(Card card)
    {
        if (_cards.Contains(card))
        {
            _cards.Remove(card);
            card.SetHand(null);
            UpdateCardPositions();
        }
    }

    // Update the position of each card in the hand
    public void UpdateCardPositions()
    {
        float handWidth = _collider.size.x;
        float cardWidth = handWidth / Mathf.Clamp(_cards.Count, 1, _maxCards); // Avoid division by zero
        float startX = -handWidth / 2 + cardWidth / 2; // Start positions so that cards are centered

        for (int i = 0; i < _cards.Count; i++)
        {
            float xPosition = startX + i * cardWidth;
            _cards[i].transform.localPosition = new Vector3(xPosition, 0, 0);

            if(_cards.Count > 1)
            {
                float tilt = CalculateTilt(i);
                _cards[i].transform.localRotation = Quaternion.Euler(0, 0, tilt);
            }
        }
    }

    private float CalculateTilt(int index)
    {
        int middleIndex = _cards.Count / 2;
        float tilt = -maxTiltAngle * (index - middleIndex) / middleIndex; // Scale tilt relative to distance from center
        return tilt;
    }

    public void UpdateCardInteractionIndicators()
    {
        for (int i = 0; i < _cards.Count; i++)
        {
            _cards[i].UpdateInteractionIndicator();
        }
    }
}
