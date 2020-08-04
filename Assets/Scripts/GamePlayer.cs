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

    public bool IsAIMakingChoice;

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

    public void SetAsCurrentPlayer(bool isFirstCardOfGame)
    {
        if(IsAIMakingChoice == true)
        {
            // Debug.Log($"PlayerName {PlayerName} isAIMakingChoice == true");
            // Debug.Log($"IsAI {IsAI}");
            // Debug.Log($"IsSittingOut {IsSittingOut}");
            return;
        }

        IsAIMakingChoice = IsAI || IsSittingOut;

        this.EnemyPlayerGO.GetComponent<Enemy>().IsThinking = true;

        List<PlayingCard> playableCards = getPlayableCards(isFirstCardOfGame);

        if(IsAIMakingChoice == false)
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

    private List<PlayingCard> getPlayableCards(bool isFirstCardOfGame)
    {
        List<PlayingCard> playableCards = new List<PlayingCard>();

        if(isFirstCardOfGame == true && HasSevenOfDiamonds())
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

    public void ToggleMidGameSittingOutStatus()
    {
        this.EnemyPlayerGO.GetComponent<Enemy>().IsSittingOut = IsSittingOut;

        if(IsSittingOut == true)
        {
            this.NetworkPlayerGO.GetComponent<NetworkPlayer>().RpcResetUI("Sitting Out");
        }
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

    public void ShowPlayerType()
    {
        this.EnemyPlayerGO.GetComponent<Enemy>().IsTableHost = IsTableHost;
        this.EnemyPlayerGO.GetComponent<Enemy>().IsSittingOut = IsSittingOut;
        this.EnemyPlayerGO.GetComponent<Enemy>().IsAI = IsAI;
    }

}