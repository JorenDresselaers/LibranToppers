using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable, CreateAssetMenu(menuName = "Abilities/BuffSelected", fileName = "BuffSelectedAbility")]
public class BuffSelectedAbility : CardAbility
{
    [SerializeField] private int _vitality = 0;
    [SerializeField] private int _assault = 0;
    [SerializeField] private CardData.Faction _targetFaction;

    private void Awake()
    {
        _abilityTrigger = Trigger.PLAYED;
    }

    public override void Activate(Card caster, Card target)
    {
        if (CanTargetCard(caster, target)) target.ModifyStats(_assault, _vitality);
    }

    protected override bool CanTargetCard(Card caster, Card target)
    {
        return target != caster && target.Faction == _targetFaction;
    }
}
