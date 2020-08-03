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
    private bool isFirstCardOfGame;

    void Start() {
        singlePack = Pack.GetCards(); 
    }

    public void StartNewGame(List<GamePlayer> gamePlayers, int numberOfPacks)
    {
        isFirstCardOfGame = true;
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
                 players[playerIndex].SetAsCurrentPlayer(isFirstCardOfGame);
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
        isFirstCardOfGame = false;

        if(players.Count <= finishedPlayers.Count)  
        {
            //Debug.Log("All done!");
            GameObject.Find("SeatManager").GetComponent<SeatManager>().GameFinished();
            return;
        }

        //could wait for AI to finish here (using a callback in doAiThinkingCoroutine) - just forcing it for now instead
        players[currentPlayerIndex].IsAIMakingChoice = false;

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

        players[currentPlayerIndex].SetAsCurrentPlayer(isFirstCardOfGame);
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
                players[currentPlayerIndex].SetAsCurrentPlayer(isFirstCardOfGame);
            }
        }
    }

    public void NetworkPlayerToggleSittingOut(int seatNumber, bool isSittingOut, out bool isPlayerInCurrentGame)
    {
        var player = players.FirstOrDefault(p => p.SeatNumber == seatNumber);
        if(player != null)
        {
            player.IsSittingOut = isSittingOut; 

            isPlayerInCurrentGame = true;

            StartCoroutine(rebuildUICoroutine(player));     
        }
        else
        {
            isPlayerInCurrentGame = false;
        }
    }

    private IEnumerator rebuildUICoroutine(GamePlayer player)
    {
        if(player.IsSittingOut == false)
        {
            player.ClearUI();
            yield return new WaitForSeconds(1);
            if(player.Cards.Any())
            {
                player.ShowCardsInUI();
            }
            //TODO: show where placed instead?
        }
        else
        {
            player.SetMidGameSittingOutStatus();
        }

        if(player.IsAI == false && players[currentPlayerIndex].SeatNumber == player.SeatNumber && player.Cards.Any())
        {
            players[currentPlayerIndex].SetAsCurrentPlayer(isFirstCardOfGame);
        }
    }

    public bool IsCardPlayable(PlayingCard card)
    {
        if(card.Number == 7)
        {
            return true;
        }

        int numberOfSameCardsPlayed = playedCards.Count(c => c.Suit == card.Suit && c.Number == card.Number);
        int numberOfParentsPlayed = 0;

        if(card.Number > 7)
        {
            numberOfParentsPlayed = playedCards.Count(c => c.Suit == card.Suit && c.Number == card.Number - 1);
        }
        else
        {
            numberOfParentsPlayed = playedCards.Count(c => c.Suit == card.Suit && c.Number == card.Number + 1);
        }

        return numberOfParentsPlayed > numberOfSameCardsPlayed;
    }

}
