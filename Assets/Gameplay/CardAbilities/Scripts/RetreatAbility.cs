using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable, CreateAssetMenu(menuName = "Abilities/Retreat", fileName = "RetreatAbility")]
public class RetreatAbility : CardAbility
{
    public override void Activate(Card caster)
    {
        CardData data = caster.Data;
        caster.Board.CmdRemoveCard(caster);
        caster._player.Deck.CmdAddCard(data);
    }

    public override void Activate(Card caster, Card target)
    {
        Activate(caster);
    }
}
