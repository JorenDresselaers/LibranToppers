using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField] private Deck _deck;
    public Deck Deck => _deck;
    
    [SerializeField] private Hand _hand;
    public Hand Hand => _hand;
    
    [SerializeField] private Board _board;
    public Board Board => _board;

    [SerializeField] private Player _opponent;
    public Player Opponent => _opponent;

    private void Awake()
    {
        Deck._player = this;
        Hand._player = this;
        Board._player = this;
        if(_opponent && _opponent._opponent != this) _opponent._opponent = this;
    }
}
