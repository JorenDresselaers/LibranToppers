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
        ENTEREDBOARD
    }
    public Trigger _abilityTrigger;

    //Add a struct that detects targets and whether or not they're valid
    public struct AbilityTarget
    { }

    public virtual void Activate(Card caster, List<Card> targets)
    { }
    public virtual void Activate(Card caster, Card target)
    { }
    
    public virtual void Activate(Card caster)
    { }

    protected virtual bool CanTargetCard(Card caster, Card target)
    { return true; }
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