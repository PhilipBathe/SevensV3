using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System;

public class GamePlayer : ICloneable
{
    public object Clone() {
        return this.MemberwiseClone();
    }
    
    public string PlayerName;
    public int SeatNumber;
    public bool IsAI;

    public GameObject EnemyPlayerGO;
    public GameObject NetworkPlayerGO;

    public List<PlayingCard> Cards = new List<PlayingCard>();

    public void Reset()
    {
        this.EnemyPlayerGO.GetComponent<Enemy>().Reset();
        if(IsAI == false)
        {
            this.NetworkPlayerGO.GetComponent<NetworkPlayer>().RpcResetUI();
        }
    }

    public void ShowIsDealer()
    {
        this.EnemyPlayerGO.GetComponent<Enemy>().IsDealer = true;
        if(IsAI == false)
        {
            this.NetworkPlayerGO.GetComponent<NetworkPlayer>().RpcSetIsDealer();
        }
    }

    public void ShowCardsInUI()
    {
        this.EnemyPlayerGO.GetComponent<Enemy>().CardCount = Cards.Count;
        if(IsAI == false)
        {
            this.NetworkPlayerGO.GetComponent<NetworkPlayer>().RpcShowCards(Cards.ToArray());
        }
    }

    public bool HasSevenOfDiamonds()
    {
        return Cards.Any(c => c.Suit == "Diamond" && c.Number == 7);
    }

    public void SetAsCurrentPlayer()
    {
        this.EnemyPlayerGO.GetComponent<Enemy>().IsThinking = true;

        List<PlayingCard> playableCards = getPlayableCards();

        if(IsAI == false)
        {
            this.NetworkPlayerGO.GetComponent<NetworkPlayer>().RpcTakeTurn(playableCards.ToArray());
        }
        else
        {
            this.EnemyPlayerGO.GetComponent<AIPlayer>().MakeChoice(playableCards, Cards);
        }
    }

    private List<PlayingCard> getPlayableCards()
    {
        List<PlayingCard> playableCards = new List<PlayingCard>();

        if(HasSevenOfDiamonds())
        {
            playableCards.Add(Cards.First(c => c.Suit == "Diamond" && c.Number == 7));
            return playableCards;
        }

        foreach(var card in Cards)
        {
            if(GameObject.Find("SeatManager").GetComponent<RoundManager>().IsCardPlayable(card))
            {
                playableCards.Add(card);
            }
        }

        return playableCards;
    }

    public void ShowKnock()
    {
        this.EnemyPlayerGO.GetComponent<Enemy>().RpcKnock();

        endTurn();
    }

    private void endTurn()
    {
        this.EnemyPlayerGO.GetComponent<Enemy>().IsThinking = false;
    }

    public void ShowPlayedCard(PlayingCard card)
    {
        this.EnemyPlayerGO.GetComponent<Enemy>().CardCount = Cards.Count;
        this.EnemyPlayerGO.GetComponent<Enemy>().RpcPlayedCard(card);

        endTurn();
    }

    public void SetFinalPosition(int position)
    {
        this.EnemyPlayerGO.GetComponent<Enemy>().Placed = position;
        if(IsAI == false)
        {
            this.NetworkPlayerGO.GetComponent<NetworkPlayer>().RpcSetPlaced(position);
        }
    }
}