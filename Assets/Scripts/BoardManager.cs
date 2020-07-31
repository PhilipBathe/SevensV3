using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class BoardManager : NetworkBehaviour
{
    public GameObject CardPrefab;

    private Dictionary<string, GameObject> suitHolders = new Dictionary<string, GameObject>();

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
    public void RpcClearBoard()
    {
        foreach(var suit in suitHolders)
        {
            foreach(Transform child in suit.Value.transform)
            {
                if (child.tag == "Card")
                {
                    Destroy(child.gameObject);
                }
            }

            suit.Value.transform.DetachChildren();
        }
    }

    [ClientRpc]
    public void RpcPlayCard(PlayingCard card)
    {
        Debug.Log($"playing card {card.CardName}");
        GameObject suitSlot = suitHolders[card.Suit];

        
        GameObject playerCard = Instantiate(CardPrefab, Vector2.zero, Quaternion.identity, suitSlot.transform);
        playerCard.GetComponentInChildren<Image>().sprite = Resources.Load<Sprite>($"Sprites/{card.CardName}");
        playerCard.GetComponentInChildren<Image>().rectTransform.sizeDelta = new Vector2(96, 96);

        float y = 0f;

        if(card.Number < 7)
        {
            y = (card.Number - 7) * 6f - 44f;
        }

        if(card.Number > 7)
        {
            y = (card.Number - 7) * 6f + 44f;
        }

        playerCard.transform.localPosition = new Vector2(0f, y);
    }
}
