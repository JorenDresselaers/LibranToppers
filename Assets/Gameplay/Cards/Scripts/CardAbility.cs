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
