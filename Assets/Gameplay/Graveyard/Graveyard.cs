using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graveyard : NetworkBehaviour
{
    public Player _player;
    List<CardData> _cardsData = new();
    public List<CardData> CardsData => _cardsData;

    [Command(requiresAuthority = false)]
    public void CmdAddCard(CardData cardData)
    {
        _cardsData.Add(cardData);
    }

    [Command(requiresAuthority = false)]
    public void CmdRemoveCard(CardData cardData)
    {
        _cardsData.Remove(cardData);
    }

    [ClientRpc]
    private void RpcAddCard(CardData cardData)
    {
        print($"{cardData.cardName} added to {_player}'s graveyard");
    }
}
