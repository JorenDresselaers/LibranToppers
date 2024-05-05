using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class DeckData : ScriptableObject
{
    public string deckName;
    public List<CardData> cards = new();
}
