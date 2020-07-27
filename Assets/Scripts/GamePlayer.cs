using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class GamePlayer
{
    public string PlayerName;
    public int SeatNumber;
    //public bool IsAI;

    public GameObject GameObject;
    public GameObject NetworkPlayerGO;

    public List<PlayingCard> Cards = new List<PlayingCard>();

    public void Reset()
    {
        this.GameObject.GetComponent<Enemy>().Reset();
        this.NetworkPlayerGO.GetComponent<NetworkPlayer>().RpcResetUI();
    }

    public void ShowIsDealer()
    {
        //TODO:implement
    }

    public void ShowCardsInUI()
    {
        this.GameObject.GetComponent<Enemy>().CardCount = Cards.Count;
        this.NetworkPlayerGO.GetComponent<NetworkPlayer>().RpcShowCards(Cards.ToArray());
    }

    public bool HasSevenOfDiamonds()
    {
        return Cards.Any(c => c.Suit == "Diamond" && c.Number == 7);
    } 
}