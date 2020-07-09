using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class DealCards : MonoBehaviour
{
    public List<GameObject> Players;
    public GameObject Pack;
    public GameObject DealerPuck;
    public GameObject Board;

    private List<GameObject> cards = new List<GameObject>();
    private int dealerIndex = -1;
    private List<GameObject> playerHands = new List<GameObject>();
    private List<GameObject> suitSlots = new List<GameObject>();


    // Start is called before the first frame update
    void Start()
    {
        foreach(Transform child in Pack.transform)
        {
            //Debug.Log("child");
            if (child.tag == "Card")
            {
                cards.Add(child.gameObject);
            }
        }

        foreach(var player in Players)
        {
            foreach(Transform child in player.transform)
            {
                if (child.tag == "Hand")
                {
                    playerHands.Add(child.gameObject);
                }
            }
        }

        foreach(Transform child in Board.transform)
        {
            if (child.tag == "SuitSlot")
            {
                suitSlots.Add(child.gameObject);
            }
        }
    }

    // Update is called once per frame
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
        if(dealerIndex >= Players.Count)
        {
            dealerIndex = 0;
        }

        DealerPuck.transform.SetParent(Players[dealerIndex].transform, false);
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

        foreach(var slot in suitSlots)
        {
            slot.transform.DetachChildren();
        }
    }

    private void deal()
    {
        List<GameObject> shuffledCards = cards.OrderBy(a => Guid.NewGuid()).ToList();

        for(var i = 0; i < shuffledCards.Count; i++)
        {
            GameObject playerCard = Instantiate(shuffledCards[i], new Vector3(0, 0, 0), Quaternion.identity);

            var playerIndex = (i + dealerIndex + 1) % Players.Count;

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
        foreach(var playerGO in Players)
        {
            var player = playerGO.GetComponent<Player>();
            player.IsActive = false;
            if(player.HasSevenOfDiamonds()) 
            {
                //Debug.Log("Found 7 of diamonds");
                player.IsActive = true;
            }
        }
    }
}
