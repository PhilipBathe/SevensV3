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

    private void findFirstPlayer()
    {
        //TODO: this
        Debug.Log("findFirstPlayer");
    }

}
