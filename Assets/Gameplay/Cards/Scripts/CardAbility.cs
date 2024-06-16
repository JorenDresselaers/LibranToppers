using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CardAbility : ScriptableObject
{
    public enum Trigger
    {
        DRAWN,
        PLAYED,
        ASSAULT,
        DEFEND,
        DEATH,
        STARTOFTURN,
        ENDOFTURN,
        OTHERCARDDEFENDS,
        ENTEREDBOARD,
        AURA,
        STATCHANGE,
    }
    public Trigger _abilityTrigger;
    [SerializeField] protected bool _isTargeted = false;
    public bool IsTargeted => _isTargeted;

    public virtual void Activate(Card caster, List<Card> targets)
    { }
    public virtual void Activate(Card caster, Card target)
    { }
    
    public virtual void Activate(Card caster)
    { }

    protected virtual bool CanTargetCard(Card caster, Card target)
    { return true; }

    public bool BoardsContainsValidTarget(Card caster, Player player)
    {
        if (player == null) return false;
        List<Card> cardsOnBoards = new();
        cardsOnBoards.AddRange(player.Board.Cards);
        if(player.Opponent != null) cardsOnBoards.AddRange(player.Opponent.Board.Cards);

        foreach(Card card in cardsOnBoards)
        {
            if (CanTargetCard(caster, card)) return true;
        }
        return false;
    }
}

public static class AbilityDataSerializer
{
    public static void WriteCardData(this NetworkWriter writer, CardAbility data)
    {
        if (data) writer.WriteString(data.name);
    }

    public static CardAbility ReadCardData(this NetworkReader reader)
    {
        string resourceName = reader.ReadString();
        resourceName = "ScriptableObjects/CardData/Abilities/" + resourceName;
        if (string.IsNullOrEmpty(resourceName))
        {
            Debug.LogWarning("Invalid or missing resource name during deserialization.");
            return null; // or return a default CardData instance
        }

        CardAbility data = Resources.Load<CardAbility>(resourceName);
        if (data == null)
        {
            Debug.LogWarning("Failed to load CardData resource: " + resourceName);
        }

        return data;
    }
}