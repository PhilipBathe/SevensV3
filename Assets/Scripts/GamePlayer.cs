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
    public int WineLevel;
    public bool IsSittingOut;
    public bool IsTableHost;

    public GameObject EnemyPlayerGO;
    public GameObject NetworkPlayerGO;

    public List<PlayingCard> Cards = new List<PlayingCard>();

    public void Reset(string status)
    {
        this.EnemyPlayerGO.GetComponent<Enemy>().Reset(status);
        if(IsAI == false)
        {
            this.NetworkPlayerGO.GetComponent<NetworkPlayer>().RpcResetUI(status);
        }
    }

    public void ClearUI()
    {
        if(IsAI == false)
        {
            this.NetworkPlayerGO.GetComponent<NetworkPlayer>().RpcResetUI(string.Empty);
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

        if(IsAI == false && IsSittingOut == false)
        {
            this.NetworkPlayerGO.GetComponent<NetworkPlayer>().RpcTakeTurn(playableCards.ToArray());
            return;
        }

        var wineLevel = WineLevel;

        if(IsSittingOut)
        {
            wineLevel = 1; //always be smart for humans!
        }

        this.EnemyPlayerGO.GetComponent<AIPlayer>().MakeChoice(playableCards, Cards, wineLevel);
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
        if(IsAI == false && IsSittingOut == false)
        {
            this.NetworkPlayerGO.GetComponent<NetworkPlayer>().RpcSetPlaced(position);
        }
    }

    public void SortEnemies()
    {
        if(IsAI == false)
        {
            this.NetworkPlayerGO.GetComponent<NetworkPlayer>().RpcSortEnemies();
        }
    }

    public void ToggleBetweenGameSittingOutStatus()
    {
        this.EnemyPlayerGO.GetComponent<Enemy>().IsSittingOut = IsSittingOut;

        if(IsSittingOut == true)
        { 
            this.EnemyPlayerGO.GetComponent<Enemy>().StatusText = "Sitting Out";
            this.NetworkPlayerGO.GetComponent<NetworkPlayer>().StatusText = "Sitting Out";
        }
        else
        {
            this.EnemyPlayerGO.GetComponent<Enemy>().StatusText = "Waiting for next game";
            this.NetworkPlayerGO.GetComponent<NetworkPlayer>().StatusText = "Waiting for next game";
        }
    }

    public void SetMidGameSittingOutStatus()
    {
        this.EnemyPlayerGO.GetComponent<Enemy>().IsSittingOut = IsSittingOut;
        //enemy still needs to show cards as we are AI (ish)
        this.NetworkPlayerGO.GetComponent<NetworkPlayer>().StatusText = "Sitting Out";
    }

    public void ShowIsTableHost()
    {
        this.EnemyPlayerGO.GetComponent<Enemy>().IsTableHost = true;
        this.NetworkPlayerGO.GetComponent<NetworkPlayer>().IsTableHost = true;
    }

    public void ScrunchCardHolders(int numberOfPlayers, int numberOfCardPacks)
    {
        //TODO: enemies
        if(IsAI == false)
        {
            this.NetworkPlayerGO.GetComponent<NetworkPlayer>().RpcScrunchCardHolders(numberOfPlayers, numberOfCardPacks);
        }
        
    }
}