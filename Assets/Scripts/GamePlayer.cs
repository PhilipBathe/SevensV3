using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class GamePlayer
{
    public string PlayerName;
    public int SeatNumber;
    //public bool IsAI;

    public GameObject EnemyPlayerGO;
    public GameObject NetworkPlayerGO;

    public List<PlayingCard> Cards = new List<PlayingCard>();

    public void Reset()
    {
        this.EnemyPlayerGO.GetComponent<Enemy>().Reset();
        this.NetworkPlayerGO.GetComponent<NetworkPlayer>().RpcResetUI();
    }

    public void ShowIsDealer()
    {
        this.EnemyPlayerGO.GetComponent<Enemy>().IsDealer = true;
        this.NetworkPlayerGO.GetComponent<NetworkPlayer>().RpcSetIsDealer();
    }

    public void ShowCardsInUI()
    {
        this.EnemyPlayerGO.GetComponent<Enemy>().CardCount = Cards.Count;
        this.NetworkPlayerGO.GetComponent<NetworkPlayer>().RpcShowCards(Cards.ToArray());
    }

    public bool HasSevenOfDiamonds()
    {
        return Cards.Any(c => c.Suit == "Diamond" && c.Number == 7);
    }

    public void SetAsCurrentPlayer()
    {
        this.EnemyPlayerGO.GetComponent<Enemy>().IsThinking = true;

        List<PlayingCard> playableCards = getPlayableCards();

        this.NetworkPlayerGO.GetComponent<NetworkPlayer>().RpcTakeTurn(playableCards.ToArray());
    }

    private List<PlayingCard> getPlayableCards()
    {
        List<PlayingCard> playableCards = new List<PlayingCard>();

        // if(HasSevenOfDiamonds())
        // {
        //     playableCards.Add(Cards.First(c => c.Suit == "Diamond" && c.Number == 7));
        //     return playableCards;
        // }

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
}