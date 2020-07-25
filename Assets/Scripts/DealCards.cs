using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.UI;

public class DealCards : MonoBehaviour
{
    private int dealerIndex = -1;
    private TableManager tableManager;
    private BoardManager boardManager;
    private GameObject pack;
    private List<GameObject> cards = new List<GameObject>();
    private List<GameObject> players;
    private List<GameObject> hands = new List<GameObject>();

    public Animator newGamePanel;


    void Start()
    {
        boardManager = GameObject.Find("Board").GetComponent<BoardManager>();
        tableManager = GameObject.Find("TableManager").GetComponent<TableManager>();
        pack = GameObject.Find("Pack");

        //TODO: move this to the pack to organise itself?
        foreach(Transform child in pack.transform)
        {
            if (child.tag == "Card")
            {
                cards.Add(child.gameObject);
            }
        }
    }

    public void OnClick()
    {
        hideNextGamePanel();
        refreshPlayerLists();
        collectCards();
        changeDealer();
        deal();
        sortCards();
        findFirstPlayer();
    }

    private void hideNextGamePanel()
    {
        newGamePanel.SetBool("isHidden", true);
    }

    public void ShowNextGamePanel()
    {
        newGamePanel.SetBool("isHidden", false);
    }

    private void refreshPlayerLists()
    {
        //refetch players to allow people to join and leave between games
        players = tableManager.GetPlayers();

        hands = getPlayerHands();
    }

    private List<GameObject> getPlayerHands()
    {
        var playerHands = new List<GameObject>();

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

        return playerHands;
    }

    private void changeDealer()
    {
        //this may need to change when we start allowing players to join and leave the table
        dealerIndex++;
        if(dealerIndex >= players.Count)
        {
            dealerIndex = 0;
        }

        players[dealerIndex].GetComponent<Player>().ShowIsDealer();
    }

    private void collectCards()
    {
        //This is just wiping player hands - we should move this to the hand and just call it from here (same as sort cards)
        //this would let players leave after a game has been played
        foreach(var hand in hands)
        {
            hand.GetComponent<Hand>().ClearHand();
        }

        foreach(var player in players)
        {
            player.GetComponent<Player>().Reset();
        }

        boardManager.ClearBoard();
    }

    private void deal()
    {
        //Debug.Log("deal");
        //Debug.Log(cards.Count);
        List<GameObject> shuffledCards = cards.OrderBy(a => Guid.NewGuid()).ToList();

        for(var i = 0; i < shuffledCards.Count; i++)
        {
            GameObject playerCard = Instantiate(shuffledCards[i], Vector2.zero, Quaternion.identity);

            var playerIndex = (i + dealerIndex + 1) % players.Count;

            playerCard.transform.SetParent(hands[playerIndex].transform, false);
            if(playerIndex > 0)
            {
                playerCard.GetComponent<Card>().Flip();
                playerCard.GetComponent<Image>().rectTransform.sizeDelta = new Vector2(50, 50);
            }
        }
    }

    private void sortCards()
    {
        foreach(var hand in hands)
        {
            hand.GetComponent<Hand>().SortCards();
        }
    }

    private void findFirstPlayer()
    {
        //TODO: have tablemanager expose a method called FindFirstPlayer and let this then call NextPlayer internally 
        //so we don't need to know what this boolean is for
        tableManager.NextPlayer(true);
    }
}
