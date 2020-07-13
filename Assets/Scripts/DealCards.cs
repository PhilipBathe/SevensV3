using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class DealCards : MonoBehaviour
{
    public GameObject DealerPuck;

    private int dealerIndex = -1;
    private TableManager tableManager;
    private BoardManager boardManager;
    private GameObject pack;
    private List<GameObject> cards = new List<GameObject>();
    private List<GameObject> players;
    private List<GameObject> playerHands = new List<GameObject>();


    void Start()
    {
        boardManager = GameObject.Find("Board").GetComponent<BoardManager>();
        tableManager = GameObject.Find("TableManager").GetComponent<TableManager>();
        pack = GameObject.Find("Pack");

        players = tableManager.GetPlayers();

        foreach(Transform child in pack.transform)
        {
            //Debug.Log("child");
            if (child.tag == "Card")
            {
                cards.Add(child.gameObject);
            }
        }

        foreach(var player in players)
        {
            foreach(Transform child in player.transform)
            {
                if (child.tag == "Hand")
                {
                    playerHands.Add(child.gameObject);
                }
            }
        }
    }

    public void OnClick()
    {
        changeDealer();
        collectCards();
        deal();
        sortCards();
        findFirstPlayer();
    }

    private void changeDealer()
    {
        dealerIndex++;
        if(dealerIndex >= players.Count)
        {
            dealerIndex = 0;
        }

        DealerPuck.transform.SetParent(players[dealerIndex].transform, false);
    }

    private void collectCards()
    {
        foreach(var hand in playerHands)
        {
            foreach(Transform child in hand.transform)
            {
                if (child.tag == "Card")
                {
                    Destroy(child.gameObject);
                }
            }
            hand.transform.DetachChildren();
        }

        boardManager.ClearBoard();
    }

    private void deal()
    {
        List<GameObject> shuffledCards = cards.OrderBy(a => Guid.NewGuid()).ToList();

        for(var i = 0; i < shuffledCards.Count; i++)
        {
            GameObject playerCard = Instantiate(shuffledCards[i], new Vector3(0, 0, 0), Quaternion.identity);

            var playerIndex = (i + dealerIndex + 1) % players.Count;

            playerCard.transform.SetParent(playerHands[playerIndex].transform, false);
        }
    }

    private void sortCards()
    {
        foreach(var hand in playerHands)
        {
            hand.GetComponent<SortCards>().Run();
        }
    }

    private void findFirstPlayer()
    {
        tableManager.NextPlayer(true);
    }
}
