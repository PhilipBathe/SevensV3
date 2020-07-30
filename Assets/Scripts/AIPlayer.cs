using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Mirror;

public class AIPlayer : NetworkBehaviour
{
    public float WaitTimeForAI = 0.5f;


    [Server]
    public void MakeChoice(List<PlayingCard> playableCards, List<PlayingCard> allCards, int wineLevel)
    {
        StartCoroutine(doAiThinkingCoroutine(playableCards, allCards, wineLevel));
    }

    [Server]
    IEnumerator doAiThinkingCoroutine(List<PlayingCard> playableCards, List<PlayingCard> allCards, int wineLevel)
    {
        //Thinking time!
        yield return new WaitForSeconds(WaitTimeForAI);

        if(playableCards.Count == 0)
        {
            GameObject.Find("SeatManager").GetComponent<RoundManager>().Knock();
        }
        else
        {
            var cardToPlay = playableCards.First();

            if(wineLevel == 3) //one bottle of wine so just pick a card at random
            {
                var randomIndex = UnityEngine.Random.Range(0, playableCards.Count);
                cardToPlay = playableCards[randomIndex];
            }
            else
            {
                if(playableCards.Count > 1)
                {
                    cardToPlay = chooseCard(playableCards, allCards, wineLevel);      
                }
            }

            GameObject.Find("SeatManager").GetComponent<RoundManager>().PlayCard(cardToPlay);
        }
    }

     [Server]
    private PlayingCard chooseCard(List<PlayingCard> playableCards, List<PlayingCard> allCards, int wineLevel)
    {
        //Debug.Log($"Choice to make with wineLevel {wineLevel}");

        float randomness = 0.09f;
        if(wineLevel == 2) //two glasses of wine so try to make a good decision but with a wider randomness range
        {
            randomness = 10f;
        }

        var cardToPlay = playableCards.First();
        float bestCardWorth = -100f;
        float worstCardWorth = 100f;

        foreach(var playableCard in playableCards)
        {
            int distUp = 0;
            int distDown = 0;
            int suitCardCount = 1;

            //we need clever maths - gap to card furthest away =1 (in both directions if a 7) - number of cards in suit
            foreach(PlayingCard card in allCards)
            {
                if(card.Suit != playableCard.Suit || card.Number == playableCard.Number)
                {
                    continue;
                }

                if(card.Number > 7 && playableCard.Number >= 7 && distUp < card.Number - playableCard.Number)
                {
                    distUp = card.Number - playableCard.Number; 
                    suitCardCount++;
                }
                
                if(card.Number < 7 && playableCard.Number <= 7 && distDown < playableCard.Number - card.Number)
                {
                    distDown = playableCard.Number - card.Number; 
                    suitCardCount++;
                }
            }

            //make playing a card that has a neighbour more valuable than if no neighbour exists
            if(distUp > 0)
            {
                distUp++;
            }
            if(distDown > 0)
            {
                distDown++;
            }

            float currentWorth = distUp + distDown - suitCardCount;

            //in the event of a tie favour cards nearer to the ends
            currentWorth += Math.Abs(7 - playableCard.Number) / 10f;

            //Debug.Log($"Card {playableCardGO.name} had worth of {currentWorth}");

            //if still a tie then don't always pick the first suit ("add some flavour")
            currentWorth += UnityEngine.Random.Range(0.01f, randomness);

            //Debug.Log($"Card {playableCardGO.name} gets wine worth of {currentWorth}");

            //one glass or two glasses of wine so still trying to win
            if(wineLevel < 4 && currentWorth > bestCardWorth)
            {
                bestCardWorth = currentWorth;
                cardToPlay = playableCard;
            }

            // Debug.Log($"card {playableCard.CardName}");
            // Debug.Log($"currentWorth {currentWorth}");
            // Debug.Log($"worstCardWorth {worstCardWorth}");

            //two bottles of wine - so blotto we are trying to lose
            if(wineLevel >= 4 && currentWorth < worstCardWorth)
            {
                worstCardWorth = currentWorth;
                cardToPlay = playableCard;
                //Debug.Log($"worst card {playableCard.CardName}");
            }
        }

        return cardToPlay;
    }
}
