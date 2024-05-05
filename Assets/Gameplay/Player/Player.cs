using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private Deck _deck;
    public Deck Deck => _deck;
    
    [SerializeField] private Hand _hand;
    public Hand Hand => _hand;
    
    [SerializeField] private Board _board;
    public Board Board => _board;


    private void Awake()
    {
        Deck._player = this;
        Hand._player = this;
        Board._player = this;
    }
}
