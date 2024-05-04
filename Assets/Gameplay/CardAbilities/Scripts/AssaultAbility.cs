using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable, CreateAssetMenu(menuName = "Abilities/Assault", fileName = "AssaultAbility")]
public class AssaultAbility : CardAbility
{
    public override void Activate(Card caster, List<Card> targets)
    {
        throw new System.NotImplementedException();
    }

    public override void Activate(Card caster, Card target)
    {
        if(CanTargetCard(caster, target)) target.Assault(caster.Damage);
    }

    protected override bool CanTargetCard(Card caster, Card target)
    {
        return target != caster;
    }
}
