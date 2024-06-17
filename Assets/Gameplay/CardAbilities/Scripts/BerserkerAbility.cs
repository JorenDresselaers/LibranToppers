using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable, CreateAssetMenu(menuName = "Abilities/Berserker", fileName = "BerserkerAbility")]
public class BerserkerAbility : CardAbility
{
    public int _multiplier = 1;

    public override void Activate(Card caster)
    {
        caster.ResetStats(true, false, false);
        caster.ModifyStats(caster.MissingVitality * 1, 0, false);
    }
}
