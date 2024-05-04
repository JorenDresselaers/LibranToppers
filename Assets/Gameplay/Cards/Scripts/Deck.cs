using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Deck : MonoBehaviour
{
    [SerializeField] private GameObject _cardPrefab;
    [SerializeField] private List<CardData> _cardsData = new List<CardData>();

    private void Awake()
    {
        _cardsData = _cardsData.ToList();
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
                _cardsData.RemoveAt(0);
                return card;
            }
            _cardsData.RemoveAt(0);
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
            // Begin dragging the card
            card.GetComponent<Card>().BeginDrag();
        }
    }
}
