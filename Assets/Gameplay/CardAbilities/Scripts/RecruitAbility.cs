using Mirror;
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
        Activate(caster);
    }

    public override void Activate(Card caster, Card target)
    {
        Activate(caster);
    }

    public override void Activate(Card caster)
    {
        //Deck deck = caster._player.Deck;
        //Board board = caster._player.Board;
        //int recruitCount = 0;
        //
        //if (!deck || !board) return;
        //
        //foreach (CardData cardData in deck.CardsData)
        //{
        //    if (_recruitNames.Contains(cardData.cardName))
        //    {
        //        Card recruitedCard = deck.CreateCard(cardData).GetComponent<Card>();
        //        caster._player.Board.AddCard(recruitedCard);
        //        Debug.Log($"{caster.CardName} recruited {recruitedCard.CardName}");
        //
        //        recruitCount++;
        //
        //        if(recruitCount >= _recruitCount)
        //        {
        //            return;
        //        }
        //    }
        //}
        CmdActivate(caster);
    }

    [Command]
    private void CmdActivate(Card caster)
    {
        if (!caster._player.isServer) return;

        Deck deck = caster._player.Deck;
        Board board = caster.Board;
        int recruitCount = 0;

        if (!deck || !board || board.IsFull) return;

        foreach (CardData cardData in deck.CardsData)
        {
            if (_recruitNames.Contains(cardData.cardName))
            {
                Card recruitedCard = deck.CreateCard(cardData).GetComponent<Card>();
                recruitedCard.CmdAddToBoard(caster._player.Board);
                Debug.Log($"{caster.CardName} recruited {recruitedCard.CardName}");

                recruitCount++;

                if (recruitCount >= _recruitCount)
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
