using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable, CreateAssetMenu(menuName = "Abilities/SharpshooterConditionalConditional", fileName = "SharpshooterConditionalAbility")]
public class SharpshooterConditionalAbility : CardAbility
{
    [SerializeField] private bool _useCardDamage = false;
    [SerializeField] private int _damage = 0;
    [SerializeField] private List<string> _requiredAlliedCards = new List<string>();

    public override void Activate(Card caster, Card target)
    {
        if (CanTargetCard(caster, target)) target.Assault(_useCardDamage ? caster.Damage : _damage);
    }

    public override bool BoardsContainsValidTarget(Card caster, Player player)
    {
        bool canActivate = false;
        foreach(string currentAlly in _requiredAlliedCards)
        {
            if (player.Board.ContainsCard(currentAlly)) canActivate = true;
        }

        if (canActivate)
            return base.BoardsContainsValidTarget(caster, player);
        else
            return false;
    }
}
