using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable, CreateAssetMenu(menuName = "Abilities/Sharpshooter", fileName = "SharpshooterAbility")]
public class SharpshooterAbility : CardAbility
{
    [SerializeField] private bool _useCardDamage = false;
    [SerializeField] private int _damage = 0;

    private void Awake()
    {
        _abilityTrigger = Trigger.PLAYED;
    }

    public override void Activate(Card caster, Card target)
    {
        if (CanTargetCard(caster, target)) target.Assault(_useCardDamage ? caster.Damage : _damage);
    }
}
