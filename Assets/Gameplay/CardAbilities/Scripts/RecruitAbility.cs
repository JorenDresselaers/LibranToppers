using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable, CreateAssetMenu(menuName = "Abilities/Recruit", fileName = "RecruitAbility")]
public class RecruitAbility : CardAbility
{
    [SerializeField] private List<string> _recruitNames = new();
    [SerializeField] private int _recruitCount = 1;

    private void Awake()
    {
        _abilityTrigger = Trigger.PLAYED;
    }

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
        Deck deck = caster._player.Deck;
        int recruitCount = 0;

        foreach (CardData cardData in deck.CardsData)
        {
            if (_recruitNames.Contains(cardData.cardName))
            {
                Card recruitedCard = deck.CreateCard(cardData).GetComponent<Card>();
                caster._player.Board.AddCard(recruitedCard);
                Debug.Log($"{caster.CardName} recruited {recruitedCard.CardName}");

                recruitCount++;

                if(recruitCount >= _recruitCount)
                {
                    return;
                }
            }
        }
    }

    protected override bool CanTargetCard(Card caster, Card target)
    {
        return target != caster;
    }
}
