using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable, CreateAssetMenu(menuName = "Abilities/Aftershock", fileName = "AftershockAbility")]
public class AftershockAbility : CardAbility
{
    [SerializeField] private DamageSource source = DamageSource.CUSTOM;
    [SerializeField] private int _damage = 0;
    [SerializeField] private int _turns = 0;

    enum DamageSource
    { 
        CUSTOM,
        CARDASSAULT,
        CARDVITALITY
    }

    private void Awake()
    {
        _abilityTrigger = Trigger.ASSAULT;
    }

    public override void Activate(Card caster, Card target)
    {
        int damage = source == DamageSource.CUSTOM ? _damage : source == DamageSource.CARDASSAULT ? caster.Damage : caster.Vitality;
        target.AddEndOfTurnEffect(() => { target.Assault(damage); }, _turns);
    }
}
