using UnityEngine;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AIPlayer : Player
{
    [Header("AI Settings")]
    [SerializeField] private float _minActionDelay = 0.5f;
    [SerializeField] private float _maxActionDelay = 2f;
    [SerializeField] private bool _debugMode = false;

    private bool _isThinking = false;
    private Coroutine _thinkingCoroutine;

    public override void OnStartServer()
    {
        base.OnStartServer();
        CmdSetName("AI Player");
    }

    [Server]
    public override void ServerStartTurn()
    {
        base.ServerStartTurn();
        if (_thinkingCoroutine != null)
        {
            StopCoroutine(_thinkingCoroutine);
        }
        _thinkingCoroutine = StartCoroutine(ThinkAndPlay());
    }

    private IEnumerator ThinkAndPlay()
    {
        if (!isServer) yield break;

        _isThinking = true;
        
        // Wait a bit to simulate thinking
        yield return new WaitForSeconds(Random.Range(_minActionDelay, _maxActionDelay));

        // Play cards while we can
        while (CanPlayCards && Hand.Cards.Count > 0)
        {
            // Find best card to play
            Card bestCard = ChooseBestCardToPlay();
            if (bestCard != null)
            {
                if (_debugMode) Debug.Log($"AI playing card: {bestCard.CardName}");
                
                // Play the card
                if (Board.AddCard(bestCard))
                {
                    CmdOnCardPlayed();
                    yield return new WaitForSeconds(Random.Range(_minActionDelay, _maxActionDelay));
                }
            }
            else
            {
                break;
            }
        }

        // Handle card interactions
        yield return StartCoroutine(HandleCardInteractions());

        // End turn
        if (_debugMode) Debug.Log("AI ending turn");
        _isThinking = false;
        _thinkingCoroutine = null;
    }

    private Card ChooseBestCardToPlay()
    {
        if (Hand.Cards.Count == 0) return null;

        // Simple strategy: Play highest damage card first
        return Hand.Cards.OrderByDescending(card => card.Damage).FirstOrDefault();
    }

    private IEnumerator HandleCardInteractions()
    {
        if (!isServer) yield break;

        var myBoardCards = Board.Cards.ToList();
        var opponentBoardCards = Opponent.Board.Cards.ToList();

        foreach (var myCard in myBoardCards)
        {
            if (!myCard.CanInteract) continue;

            // Find best target for this card
            Card bestTarget = ChooseBestTarget(myCard, opponentBoardCards);
            if (bestTarget != null)
            {
                if (_debugMode) Debug.Log($"AI attacking {bestTarget.CardName} with {myCard.CardName}");
                
                myCard.CmdInteract(bestTarget);
                yield return new WaitForSeconds(Random.Range(_minActionDelay, _maxActionDelay));
            }
        }
    }

    private Card ChooseBestTarget(Card attacker, List<Card> possibleTargets)
    {
        if (possibleTargets.Count == 0) return null;

        // Simple strategy: Attack the card we can kill, or the one with lowest health
        return possibleTargets
            .Where(target => target.Vitality <= attacker.Damage) // Cards we can kill
            .OrderByDescending(target => target.Damage) // Prioritize high damage threats
            .FirstOrDefault()
            ?? possibleTargets
                .OrderBy(target => target.Vitality) // If we can't kill any, attack the weakest
                .FirstOrDefault();
    }
} 