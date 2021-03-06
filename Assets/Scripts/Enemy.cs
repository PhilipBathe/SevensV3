﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class Enemy : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnPlayerNameChanged))]
    public string PlayerName;

    [SyncVar(hook = nameof(OnPlayerColorChanged))]
    public string PlayerColor;

    [SyncVar]
    public int SeatNumber;


    [SyncVar(hook = nameof(OnParentChanged))]
    public GameObject Parent;


    [SyncVar(hook = nameof(OnCardCountChanged))]
    public int CardCount;

    [SyncVar(hook = nameof(OnIsThinkingChanged))]
    public bool IsThinking;

    [SyncVar(hook = nameof(OnIsDealerChanged))]
    public bool IsDealer;

    [SyncVar(hook = nameof(OnPlacedChanged))]
    public int Placed;

    [SyncVar(hook = nameof(OnStatusTextChanged))]
    public string StatusText;

    [SyncVar(hook = nameof(OnIsTableHostChanged))]
    public bool IsTableHost;



    public GameObject EnemyCardPrefab;

    public GameObject Hand;

    [SyncVar(hook = nameof(OnIsAIChanged))]
    public bool IsAI;

    [SyncVar(hook = nameof(OnIsSittingOutChanged))]
    public bool IsSittingOut;

    void OnIsTableHostChanged(bool oldIsTableHost, bool newIsTableHost)
    {
        setPlayerType();
    }

    void OnIsSittingOutChanged(bool oldIsSittingOut, bool newIsSittingOut)
    {
        setPlayerType();
    }

    void OnIsAIChanged(bool oldIsAI, bool newIsAI)
    {
        setPlayerType();
    }

    private void setPlayerType()
    {
        var commonPlayerUI = gameObject.GetComponent<CommonPlayerUI>();

        //TODO: add special graphic for host sitting out???

        if(IsAI == true)
        {
            commonPlayerUI.ShowIsAI();
            return;
        }

        if(IsSittingOut == true)
        {
            commonPlayerUI.ShowIsSittingOut();
            return;
        }

        if(IsTableHost == true)
        {
            commonPlayerUI.ShowIsTableHost();
            return;
        }

        

        commonPlayerUI.ClearPlayerType();
    }

    void OnStatusTextChanged(string oldStatusText, string newStatusText)
    {
        gameObject.GetComponent<CommonPlayerUI>().SetStatusText(newStatusText);
    }

    void OnPlacedChanged(int oldPlaced, int newPlaced)
    {
        //Debug.Log($"newIsDealer {newIsDealer}");
        var commonPlayerUI = gameObject.GetComponent<CommonPlayerUI>();
        if(newPlaced == 0)
        {
            commonPlayerUI.ClearPlaced();
        }
        else
        {
            commonPlayerUI.ShowPlaced(newPlaced);
        }
    }

    void OnIsDealerChanged(bool oldIsDealer, bool newIsDealer)
    {
        //Debug.Log($"newIsDealer {newIsDealer}");
        var commonPlayerUI = gameObject.GetComponent<CommonPlayerUI>();
        if(newIsDealer)
        {
            commonPlayerUI.ShowIsDealer();
        }
        else
        {
            commonPlayerUI.ClearIsDealer();
        }
    }

    void OnIsThinkingChanged(bool oldIsThinking, bool newIsThinking)
    {
        //Debug.Log($"newIsThinking {newIsThinking}");
        var commonPlayerUI = gameObject.GetComponent<CommonPlayerUI>();
        if(newIsThinking)
        {
            commonPlayerUI.ShowIsThinking();
        }
        else
        {
            commonPlayerUI.ClearIsThinking();
        }
    }

    [ClientRpc]
    public void RpcKnock()
    {
        gameObject.GetComponent<CommonPlayerUI>().ShowKnock();
    }

    [ClientRpc]
    public void RpcPlayedCard(PlayingCard card)
    {
        gameObject.GetComponent<CommonPlayerUI>().ShowLastGo(card);
    }
    

    void OnPlayerNameChanged(string oldPlayerName, string newPlayerName)
    {
        foreach (Text text in gameObject.GetComponentsInChildren<Text> ())
        {
            if(text.name == "PlayerNameText")
            {
                text.text = newPlayerName;
            }
        }
    }

    void OnPlayerColorChanged(string oldPlayerColor, string newPlayerColor)
    {

        Transform background = null;
        Sprite sprite = Resources.Load<Sprite>($"Sprites/enemy_background_{newPlayerColor}");

        if(sprite == null)
        {
            sprite = Resources.Load<Sprite>($"Sprites/enemy_background_black");
            Debug.Log($"Couldn't find background resource for color '{newPlayerColor}'");
        }

        foreach(Transform child in this.transform)
        {
            if(child.name == "Background")
            {
                background = child;
            }
        }

        if(background == null)
        {
            return;
        }

        background.GetComponent<Image>().sprite = sprite;
    }

    void OnParentChanged(GameObject oldParent, GameObject newParent)
    {
        this.transform.SetParent(newParent.transform, false);
        this.transform.localScale = new Vector2(1, 1);
    }

    [Server]
    public void Reset(string status) 
    {
        CardCount = 0;
        Placed = 0;
        IsDealer = false;
        IsThinking = false;
        RpcClearLastGo();
        StatusText = status;
    }

    [ClientRpc]
    private void RpcClearLastGo()
    {
        gameObject.GetComponent<CommonPlayerUI>().ClearLastGo();
    }

    void OnCardCountChanged(int oldCardCount, int newCardCount)
    {
        if(newCardCount == 0)
        {
            clearHand();
            return;
        }

        setSpacing(newCardCount);

        if(newCardCount == oldCardCount-1)
        {
            removeOneCard();
            return;
        }

        //if we are here then we should be creating a new hand at the start of a new game

        clearHand();

        for(int i = 0; i < newCardCount; i++)
        {
            GameObject playerCard = Instantiate(EnemyCardPrefab, Vector2.zero, Quaternion.identity, Hand.transform);
        }
    }

    private void setSpacing(float cardCount)
    {
        float newSpacing = -33f;
        if(cardCount > 21f) //let hands shrink after we are down to 21 or less
        {
            newSpacing = (180 - (cardCount * 40)) / (cardCount - 1);
        }

        Hand.GetComponent<HorizontalLayoutGroup>().spacing = newSpacing;
    }

    private void clearHand()
    {
        foreach(Transform child in Hand.transform)
        {
            Destroy(child.gameObject);
        }

        Hand.transform.DetachChildren();
    }

    private void removeOneCard()
    {
        foreach(Transform child in Hand.transform)
        {
            Destroy(child.gameObject);
            break;
        }
    }

    [ClientRpc]
    public void RpcDie()
    {
        this.transform.SetParent(null);
        Destroy(gameObject);
    }

}
