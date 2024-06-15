using Mirror;
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
        Mortal
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

    [HideInInspector] public List<CardAbility> abilities = new();
}

public static class CardDataSerializer
{
    public static void WriteCardData(this NetworkWriter writer, CardData data)
    {
        if(data) writer.WriteString(data.name);
    }

    public static CardData ReadCardData(this NetworkReader reader)
    {
        string resourceName = reader.ReadString();
        resourceName = "ScriptableObjects/CardData/" + resourceName;
        if (string.IsNullOrEmpty(resourceName))
        {
            Debug.LogWarning("Invalid or missing resource name during deserialization.");
            return null; // or return a default CardData instance
        }

        CardData data = Resources.Load<CardData>(resourceName);
        if (data == null)
        {
            Debug.LogWarning("Failed to load CardData resource: " + resourceName);
        }

        return data;
    }
}
