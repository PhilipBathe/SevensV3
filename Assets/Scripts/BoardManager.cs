using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Linq;

public class BoardManager : NetworkBehaviour
{
    public GameObject CardPrefab;
    public GameObject CardHolderPrefab;

    private Dictionary<string, GameObject> suitHolders = new Dictionary<string, GameObject>();
    private int numberOfCardPacks;
    private List<PlayingCard> playedCards = new List<PlayingCard>();

    void Start()
    {
        foreach(Transform child in this.transform)
        {
            if(child.tag == "SuitSlot")
            {
                suitHolders.Add(child.transform.name, child.gameObject);
            }
        }
    }

    [ClientRpc]
    public void RpcPrepareBoard(int numberOfPacks)
    {
        numberOfCardPacks = numberOfPacks;
        playedCards = new List<PlayingCard>();
        resetCardHolders();
    }

    [Client]
    private void resetCardHolders()
    {
        foreach(var suit in suitHolders)
        {
            foreach(Transform child in suit.Value.transform)
            {
                Destroy(child.gameObject);
            }

            suit.Value.transform.DetachChildren();

            for(int i = 0; i < numberOfCardPacks; i++)
            {
                var holder = Instantiate(CardHolderPrefab, Vector2.zero, Quaternion.identity, suit.Value.transform);
                holder.transform.localPosition = Vector2.zero;
            }
            
        }
    }

    [ClientRpc]
    public void RpcPlayCard(PlayingCard card)
    {
        //Debug.Log($"playing card {card.CardName}");

        GameObject suitSlot = suitHolders[card.Suit];
        Transform cardHolderTransform = suitSlot.transform.GetChild(playedCards.Count(p => p.SortOrder == card.SortOrder));

        GameObject playerCard = Instantiate(CardPrefab, Vector2.zero, Quaternion.identity, cardHolderTransform);
        playerCard.GetComponentInChildren<Image>().sprite = Resources.Load<Sprite>($"Sprites/{card.CardName}");
        playerCard.GetComponent<RectTransform>().sizeDelta = new Vector2(getCardSize(), getCardSize());
        playerCard.GetComponent<RectMask2D>().padding = Vector2.zero;

        float y = calculateY(card);
        
        playerCard.transform.localPosition = new Vector2(0f, y);

        playedCards.Add(card);
    }

    private float calculateY(PlayingCard card)
    {
        float y = 0f;

        if(card.Number < 7)
        {
            y = (card.Number - 7) * 15f - 44f;
        }

        if(card.Number > 7)
        {
            y = (card.Number - 7) * 15f + 44f;
        }

        return y;

    }

    [Client]
    private int getCardSize()
    {
        switch(numberOfCardPacks)
        {
            case 1:
                return 96;
            default:
                return 80;
        }
    }
}
