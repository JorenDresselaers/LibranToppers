using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable, CreateAssetMenu(menuName = "Abilities/BelleNormal", fileName = "BelleNormalAbility")]
public class BelleNormalAbility : CardAbility
{
    public int basicHealing = 2;
    public int factionHealing = 4;
    public CardData.Faction faction = CardData.Faction.DusksOfDawn;

    public override void Activate(Card caster, Card target)
    {
        target.ModifyStats(0, target.Faction == faction ? factionHealing : basicHealing);
    }
}
