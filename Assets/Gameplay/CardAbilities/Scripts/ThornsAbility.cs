using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable, CreateAssetMenu(menuName = "Abilities/Thorns", fileName = "ThornsAbility")]
public class ThornsAbility : CardAbility
{
    public override void Activate(Card caster, Card target)
    {
        target.ModifyStats(0, -caster.Damage);
    }
}
