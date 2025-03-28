using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable, CreateAssetMenu(menuName = "Abilities/Teamwork", fileName = "TeamworkAbility")]
public class TeamworkAbility : CardAbility
{
    [SerializeField] private Mode _mode = Mode.NAME;
    [SerializeField] private List<string> _teamworkNames = new();
    [SerializeField] private CardData.Faction _teamworkFaction = CardData.Faction.None;
    [SerializeField] private int _assaultBuff = 0;
    [SerializeField] private int _vitalityBuff = 0;
    [SerializeField] private bool _buffSelf = true;
    [SerializeField] private bool _canStack = true;

    enum Mode
    { 
        FACTION,
        NAME
    }

    public override void Activate(Card caster, List<Card> targets)
    {
        Activate(caster);
    }

    public override void Activate(Card caster, Card target)
    {
        Activate(caster);
    }

    public override void Activate(Card caster)
    {
        foreach(Card card in caster._player.Board.Cards)
        {
            if (_mode == Mode.NAME)
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

                    if (!_canStack) return;
                }
            }
            else if(_mode == Mode.FACTION)
            {
                if (card.Faction == _teamworkFaction)
                {
                    if (_buffSelf)
                    {
                        caster.ModifyStats(_assaultBuff, _vitalityBuff);
                        Debug.Log($"Stats of {caster.CardName} changed by +{_vitalityBuff}/+{_assaultBuff}");
                    }
                    else
                    {
                        if (card != caster)
                        {
                            card.ModifyStats(_assaultBuff, _vitalityBuff);
                            Debug.Log($"Stats of {card.CardName} changed by +{_vitalityBuff}/+{_assaultBuff}");
                        }
                    }

                    if (!_canStack) return;
                }
            }
        }
    }

    protected override bool CanTargetCard(Card caster, Card target)
    {
        return target != caster;
    }
}
