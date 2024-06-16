using Mirror;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Deck : NetworkBehaviour
{
    public Player _player;
    [SerializeField] private TextMeshProUGUI _cardsRemainingText;
    [SerializeField] private GameObject _cardPrefab;
    [SerializeField] private DeckData _deckdata;
    private List<CardData> _cardsData = new List<CardData>();
    public List<CardData> CardsData => _cardsData;
    [SerializeField] private Hand _hand;
    [SerializeField] private bool _moveCardToHand = true;

    public bool _isClickable = false;
    public bool ClickToDrawCards { get; set; } = true;

    private void Awake()
    {
        _cardsData = _deckdata.cards.ToList();
        ShuffleCards();
        UpdateText(_cardsData.Count);
    }

    public GameObject CreateCard()
    {
        if (_cardsData.Count > 0)
        {
            CardData data = _cardsData[0];
            if (data != null)
            {
                GameObject cardObject = Instantiate(_cardPrefab, transform.position, Quaternion.identity);
                Card card = cardObject.GetComponent<Card>();
                card.Initialize(data);
                card._player = _player;
                card._isClickable = _isClickable;
                print("Card created with clickable = " + _isClickable);

                NetworkServer.Spawn(cardObject, _player.gameObject);
                _cardsData.RemoveAt(0);

                return cardObject;
            }
            _cardsData.RemoveAt(0);
        }
        return null;
    }

    [Command(requiresAuthority = false)]
    public void CmdAddCard(CardData cardData)
    {
        _cardsData.Add(cardData);
        ShuffleCards();
        RpcUpdateText(_cardsData.Count);
    }

    [Command(requiresAuthority = false)]
    public void CmdCreateCard()
    {
        GameObject card = CreateCard();
        if (card != null)
        {
            if (_moveCardToHand)
            {
                card.GetComponent<Card>().SetFlipped(true);
                if (!_hand.AddCard(card.GetComponent<Card>()))
                {
                    Destroy(card);
                }
            }
            else
            {
                card.GetComponent<Card>().BeginDrag();
            }
        }
        RpcUpdateText(_cardsData.Count);
    }

    [Server]
    public void ServerCreateCard()
    {
        GameObject card = CreateCard();
        if (card != null)
        {
            if (_moveCardToHand)
            {
                card.GetComponent<Card>().SetFlipped(true);
                if (!_hand.AddCard(card.GetComponent<Card>()))
                {
                    Destroy(card);
                }
            }
            else
            {
                card.GetComponent<Card>().BeginDrag();
            }
        }
        RpcUpdateText(_cardsData.Count);
    }

    [ClientRpc]
    private void RpcUpdateText(int cardsLeft)
    {
        UpdateText(cardsLeft);
    }

    public GameObject CreateCard(CardData cardToCreate)
    {
        foreach(CardData data in _cardsData)
        {
            if(data == cardToCreate)
            {
                GameObject cardObject = Instantiate(_cardPrefab, transform.position, Quaternion.identity);
                Card card = cardObject.GetComponent<Card>();
                card.Initialize(data);
                card._player = _player;
                card._isClickable = _isClickable;

                if(isServer) NetworkServer.Spawn(cardObject, _player.gameObject);
                _cardsData.Remove(data);
                ShuffleCards();

                return cardObject;
            }
        }
        return null;
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space)) ShuffleCards();
    }

    public void ShuffleCards()
    {
        for (int i = _cardsData.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            CardData temp = _cardsData[i];
            _cardsData[i] = _cardsData[randomIndex];
            _cardsData[randomIndex] = temp;
        }
    }

    private void OnMouseDown()
    {
        if (!isLocalPlayer) return;
        if (!_isClickable) return;
        if (ClickToDrawCards)
        {
            CmdCreateCard();
        }
    }

    private void UpdateText(int cardsRemaining)
    {
        if (!_cardsRemainingText) return;
        _cardsRemainingText.text = cardsRemaining.ToString();
    }
}
