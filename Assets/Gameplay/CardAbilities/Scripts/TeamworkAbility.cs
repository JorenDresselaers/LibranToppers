using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable, CreateAssetMenu(menuName = "Abilities/Teamwork", fileName = "TeamworkAbility")]
public class TeamworkAbility : CardAbility
{
    [SerializeField] private List<string> _teamworkNames = new();
    [SerializeField] private int _assaultBuff = 0;
    [SerializeField] private int _vitalityBuff = 0;
    [SerializeField] private bool _buffSelf = true;
    [SerializeField] private bool _canStack = true;

    public override void Activate(Card caster, List<Card> targets)
    {
        throw new System.NotImplementedException();
    }

    public override void Activate(Card caster, Card target)
    {
        throw new System.NotImplementedException();
    }

    public override void Activate(Card caster)
    {
        foreach(Card card in caster._player.Board.Cards)
        {
            if (_teamworkNames.Contains(card.CardName))
            {
                if (_buffSelf)
                {
                    caster.ModifyStats(_assaultBuff, _vitalityBuff);
                    Debug.Log($"Stats of {caster.CardName} changed by +{_vitalityBuff}/+{_assaultBuff}");
                }
                else
                {
                    card.ModifyStats(_assaultBuff, _vitalityBuff);
                    Debug.Log($"Stats of {card.CardName} changed by +{_vitalityBuff}/+{_assaultBuff}");

                }

                if(!_canStack) return;
            }
        }
    }

    protected override bool CanTargetCard(Card caster, Card target)
    {
        return target != caster;
    }
}
