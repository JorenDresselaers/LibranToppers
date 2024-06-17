using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable, CreateAssetMenu(menuName = "Abilities/Loyalty", fileName = "LoyaltyAbility")]
public class LoyaltyAbility : CardAbility
{
    [SerializeField] private List<string> _loyaltyNames = new();
    [SerializeField] private int _assaultBuff = 0;
    [SerializeField] private int _vitalityBuff = 0;
    [SerializeField] private bool _canStack = false;
    private static string Tag = "HasLoyaltyBuff";

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
        bool isTargetPresent = false;
        if(_canStack)
        {
            ResetStats(caster, true);
        }
        bool isActive = caster.HasTag(Tag);

        foreach (Card card in caster._player.Board.Cards)
        {
            if (_loyaltyNames.Contains(card.CardName))
            {
                Debug.Log($"Board contains {card.CardName}");
                isTargetPresent = true;
                if (!isActive)
                {
                    caster.ModifyStats(_assaultBuff, _vitalityBuff);
                    Debug.Log($"Loyalty: Stats of {caster.CardName} changed by +{_vitalityBuff}/+{_assaultBuff}");
                    caster.AddTag(Tag);
                    if(!_canStack) return;
                }
            }
        }

        if(!isTargetPresent && isActive) 
        {
            ResetStats(caster, true);
            Debug.Log($"Loyalty: Stats of {caster.CardName} were reset");
        }
    }

    private void ResetStats(Card caster, bool removeTag)
    {
        if(caster.HasTag(Tag)) caster.ResetStats(true, true, false);
        if(removeTag) caster.RemoveTag(Tag);
    }

    protected override bool CanTargetCard(Card caster, Card target)
    {
        return target != caster;
    }
}
