using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable, CreateAssetMenu(menuName = "Abilities/Consume", fileName = "ConsumeAbility")]
public class ConsumeAbility : CardAbility
{
    [SerializeField] private CardData.Faction _cannotTargetFaction;

    public override void Activate(Card caster, Card target)
    {
        if (CanTargetCard(caster, target))
        {
            caster.ModifyStats(target.Damage, 0);
            target.DestroyCard();
        }
    }

    protected override bool CanTargetCard(Card caster, Card target)
    {
        return target != caster && target.Faction != _cannotTargetFaction;
    }
}
