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
        //TODO:implement
        this.GameObject.GetComponent<Enemy>().Reset();
        //this.NetworkPlayerGO.GetComponent<Player>().Reset();
    }

    public void ShowIsDealer()
    {
        //TODO:implement
    }

    public void ShowCardsInUI()
    {
        this.GameObject.GetComponent<Enemy>().CardCount = Cards.Count;
    }

    public bool HasSevenOfDiamonds()
    {
        return Cards.Any(c => c.Suit == "Diamond" && c.Number == 7);
    } 
}