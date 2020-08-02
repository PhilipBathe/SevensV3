using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using System;

public static class Extensions
{
    public static List<T> Clone<T>(this List<T> listToClone) where T: ICloneable
    {
        return listToClone.Select(item => (T)item.Clone()).ToList();
    }
}

public class RoundManager : NetworkBehaviour
{
    private List<GamePlayer> players;

    private int dealerIndex = -1;

    private int numberOfCardPacks = 1;

    private List<PlayingCard> singlePack;
    private List<PlayingCard> cards;
    private List<PlayingCard> playedCards = new List<PlayingCard>();
    private List<int> finishedPlayers = new List<int>();

    void Start() {
        singlePack = Pack.GetCards(); 
    }

    public void StartNewGame(List<GamePlayer> gamePlayers, int numberOfPacks)
    {
        players = gamePlayers.Clone();
        numberOfCardPacks = numberOfPacks;
        loadCards();
        playedCards = new List<PlayingCard>();
        finishedPlayers = new List<int>();
        StartCoroutine(NewGameCoroutine());
    }

    private void loadCards()
    {
        //Debug.Log(numberOfPacks);
        cards = new List<PlayingCard>();
        for(int i = 0; i < numberOfCardPacks; i++)
        {
            //Debug.Log("Adding a pack");
            cards.AddRange(singlePack);
        }
    }

    IEnumerator NewGameCoroutine()
    {
        resetPlayers();
        prepareBoard();
        yield return new WaitForSeconds(1);
        changeDealer();
        deal();
        showHands();
        findFirstPlayer();
    }

    private void resetPlayers()
    {
        foreach(var player in players)
        {
            player.Reset(string.Empty);
            player.ScrunchCardHolders(players.Count, numberOfCardPacks);
        }
    }

    private void prepareBoard()
    {
        GameObject.Find("Board").GetComponent<BoardManager>().RpcPrepareBoard(numberOfCardPacks);
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
        //Debug.Log("deal");
        //Debug.Log(cards.Count);
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

    public void PlayCard(PlayingCard card)
    {
        //TODO: check card is playable and owned by the current player

        playedCards.Add(card);

        players[currentPlayerIndex].Cards.Remove(card);
        players[currentPlayerIndex].ShowPlayedCard(card);
        
        GameObject.Find("Board").GetComponent<BoardManager>().RpcPlayCard(card);

        checkPlayerOutOfCards();

        nextPlayer();
    }

    private void checkPlayerOutOfCards()
    {
        if(players[currentPlayerIndex].Cards.Count == 0)
        {
            finishedPlayers.Add(currentPlayerIndex);
            players[currentPlayerIndex].SetFinalPosition(finishedPlayers.Count);
        }
    }

    private void nextPlayer()
    {
        if(players.Count <= finishedPlayers.Count)  
        {
            //Debug.Log("All done!");
            GameObject.Find("SeatManager").GetComponent<SeatManager>().GameFinished();
            return;
        }

        currentPlayerIndex++;
        if(currentPlayerIndex >= players.Count)
        {
            currentPlayerIndex = 0;
        }

        while(finishedPlayers.Contains(currentPlayerIndex))
        {
            currentPlayerIndex++;
            if(currentPlayerIndex >= players.Count)
            {
                currentPlayerIndex = 0;
            }
        }

        players[currentPlayerIndex].SetAsCurrentPlayer();
    }

    public void NetworkPlayerNowAI(int seatNumber)
    {
        var lostPlayer = players.FirstOrDefault(p => p.SeatNumber == seatNumber);
        if(lostPlayer != null)
        {
            lostPlayer.IsAI = true;
            lostPlayer.NetworkPlayerGO = null;   
            
            if(players[currentPlayerIndex].SeatNumber == seatNumber)
            {
                players[currentPlayerIndex].SetAsCurrentPlayer();
            }
        }
    }

    public void NetworkPlayerToggleSittingOut(int seatNumber, bool isSittingOut, out bool isPlayerInCurrentGame)
    {
        isPlayerInCurrentGame = false;
        var player = players.FirstOrDefault(p => p.SeatNumber == seatNumber);
        if(player != null)
        {
            player.IsSittingOut = isSittingOut; 

            player.ClearUI();

            StartCoroutine(rebuildUICoroutine(player));
            isPlayerInCurrentGame = true;
        }
    }

    private IEnumerator rebuildUICoroutine(GamePlayer player)
    {
        yield return new WaitForSeconds(1);
        if(player.IsSittingOut == false)
        {
            player.ShowCardsInUI();
        }
        else
        {
            player.SetMidGameSittingOutStatus();
        }

        if(players[currentPlayerIndex]== player)
        {
            players[currentPlayerIndex].SetAsCurrentPlayer();
        }
    }

    public bool IsCardPlayable(PlayingCard card)
    {
        if(card.Number == 7)
        {
            return true;
        }

        int numberOfSameCardsPlayed = playedCards.Count(c => c.Suit == card.Suit && c.Number == card.Number);
        int numberOfParentsPlayed = playedCards.Count(c => c.Suit == card.Suit && c.Number == card.Number + 1);

        if(card.Number > 7)
        {
            numberOfParentsPlayed = playedCards.Count(c => c.Suit == card.Suit && c.Number == card.Number - 1);
        }

        return numberOfParentsPlayed > numberOfSameCardsPlayed;
    }

}
