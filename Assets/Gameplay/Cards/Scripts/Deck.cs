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

    private void Awake()
    {
        _cardsData = _deckdata.cards.ToList();
        ShuffleCards();
        UpdateText();
    }

    public GameObject CreateCard()
    {
        if (_cardsData.Count > 0)
        {
            CardData data = _cardsData[0];
            if (data != null)
            {
                GameObject card = Instantiate(_cardPrefab, transform.position, Quaternion.identity);
                card.GetComponent<Card>().Initialize(data);
                card.GetComponent<Card>()._player = _player;

                NetworkServer.Spawn(card);
                _cardsData.RemoveAt(0);

                UpdateText();

                return card;
            }
            _cardsData.RemoveAt(0);
        }
        return null;
    }
    
    public GameObject CreateCard(CardData cardToCreate)
    {
        foreach(CardData data in _cardsData)
        {
            if(data == cardToCreate)
            {
                GameObject card = Instantiate(_cardPrefab, transform.position, Quaternion.identity);
                card.GetComponent<Card>().Initialize(data);
                card.GetComponent<Card>()._player = _player;
                _cardsData.RemoveAt(0);

                UpdateText();

                _cardsData.Remove(data);
                ShuffleCards();

                return card;
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
        GameObject card = CreateCard();
        if (card != null)
        {
            if(_moveCardToHand)
            {
                if(!_hand.AddCard(card.GetComponent<Card>()))
                {
                    Destroy(card);
                }
            }
            else
            {
                card.GetComponent<Card>().BeginDrag();
            }
        }
    }

    private void UpdateText()
    {
        if (!_cardsRemainingText) return;
        _cardsRemainingText.text = _cardsData.Count.ToString();
    }
}
