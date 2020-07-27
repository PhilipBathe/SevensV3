using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using System;

public class RoundManager : NetworkBehaviour
{
    private List<GamePlayer> players;

    private int dealerIndex = -1;

    private List<PlayingCard> cards;

    void Start() {
        cards = Pack.GetCards(); 
    }

    public void StartNewGame(List<GamePlayer> gamePlayers)
    {
        players = gamePlayers;
        resetPlayers();
        clearBoard();
        changeDealer();
        deal();
        showHands();
        findFirstPlayer();
    }

    private void resetPlayers()
    {
        foreach(var player in players)
        {
            player.Reset();
        }
    }

    private void clearBoard()
    {
        //TODO: clear the board
    }

    private void changeDealer()
    {
        //this may need to change when we start allowing players to join and leave the table
        dealerIndex++;
        if(dealerIndex >= players.Count)
        {
            dealerIndex = 0;
        }

        players[dealerIndex].ShowIsDealer();
    }

    private void deal()
    {
        Debug.Log("deal");
        Debug.Log(cards.Count);
        List<PlayingCard> shuffledCards = cards.OrderBy(a => Guid.NewGuid()).ToList();

        for(var i = 0; i < shuffledCards.Count; i++)
        {
            var playerIndex = (i + dealerIndex + 1) % players.Count;

            players[playerIndex].Cards.Add(shuffledCards[i]);
        }
    }

    private void showHands()
    {
        foreach(var player in players)
        {
            player.ShowCardsInUI();
        }
    }

    private int currentPlayerIndex = -1;

    private void findFirstPlayer()
    {
        //start to the left of the dealer and find the first player with a 7 of diamonds
        //in multipack games we need to find the first to the left as there might be more than one player with a 7d

        for(int i = 0; i < players.Count; i++)
        {
             var playerIndex = (i + dealerIndex) % players.Count;

             if(players[playerIndex].HasSevenOfDiamonds())
             {
                 currentPlayerIndex = playerIndex;
                 players[playerIndex].SetAsCurrentPlayer();
                 break;
             }
        }
    }

    public void Knock()
    {
        //TODO: check player can knock - if not pick a random card for them!

        players[currentPlayerIndex].ShowKnock();

        nextPlayer();
    }

    public void PlayCard(PlayingCard cardPlayed)
    {
        //TODO: check card is playable and owned by the current player

        //TODO: play card!

        //TODO: check player is finished

        nextPlayer();
    }

    private List<int> finishedPlayerIndices = new List<int>();

    private void nextPlayer()
    {
        //flag current player as done

        // var player = players[currentPlayerIndex].GetComponent<Player>();
        //     player.IsActivePlayer = false;

        //check game has ended

            // if(players.Count <= finishedPlayers.Count)
            // {
            //     Debug.Log("All done!");
            //     GameObject.Find("DealButton").GetComponent<DealCards>().ShowNextGamePanel();
            //     return;
            // }

            currentPlayerIndex++;
            if(currentPlayerIndex >= players.Count)
            {
                currentPlayerIndex = 0;
            }

            while(finishedPlayerIndices.Contains(currentPlayerIndex))
            {
                currentPlayerIndex++;
                if(currentPlayerIndex >= players.Count)
                {
                    currentPlayerIndex = 0;
                }
            }

            players[currentPlayerIndex].SetAsCurrentPlayer();
    }



}
