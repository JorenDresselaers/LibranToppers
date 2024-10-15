using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable, CreateAssetMenu(menuName = "Abilities/Crumbling", fileName = "CrumblingAbility")]
public class CrumblingAbility : CardAbility
{
    public int _damage = 1;

    public override void Activate(Card caster)
    {
        caster.ModifyStats(0, -_damage, false);
    }
}
