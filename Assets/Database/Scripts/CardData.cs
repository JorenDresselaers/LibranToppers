using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CardData : ScriptableObject
{
    [Serializable]
    public enum Faction
    { 
        None,
        DusksOfDawn,
    }
    
    [Serializable]
    public enum Alignment
    { 
        None,
        Gothurian,
        Lucarian,
        Interitian,
    }

    [Serializable]
    public enum Rarity
    {
        Standard,
        Silver,
        Gold,
        Platinunm,
        Legendary
    }

    public string cardName;
    public int vitality;
    public int damage;
    [TextArea]
    public string description;
    public Sprite image;
    public Faction faction;
    public Alignment alignment;
    public Rarity rarity;
    public bool isGolden;

    public List<CardAbility> abilities = new();
}
