using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using UnityEngine.UI;

public class NetworkPlayer : NetworkBehaviour
{
    [SyncVar]
    public string PlayerName;

    [SyncVar]
    public int SeatNumber;

    public GameObject PlayerPrefab;

    public GameObject CardPrefab;

    private GameObject playerUI;


    // This fires on server when this player object is network-ready
    public override void OnStartServer()
    {
        base.OnStartServer();

        //playerNo = connectionToClient.connectionId;
        
    }

    // This only fires on the local client when this player object is network-ready
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        var name = PlayerPrefs.GetString("username").Trim();

        if(name.Length == 0)
        {
            name = "Mystery Unicorn";
        }
        if(name.Length > 15)
        {
            name = name.Substring(0, 15);
        }

        CmdSetPlayerName(name);
        setupUI();
    }

    [Command]
    void CmdSetPlayerName(string name)
    {
        //Debug.Log($"CmdSetPlayerName {name}");
        PlayerName = name;
        GameObject.Find("SeatManager").GetComponent<SeatManager>().RequestSeat(gameObject);
    }

    [Client]
    private void setupUI()
    {
        if(isLocalPlayer)
        {
            var canvas = GameObject.Find("Canvas");

            playerUI = Instantiate(PlayerPrefab, Vector2.zero, Quaternion.identity) as GameObject;
            playerUI.transform.SetParent(canvas.transform, false);
            playerUI.GetComponent<Player>().SetStatus($"Waiting for next game");
        }
    }

    [ClientRpc]
    public void RpcResetUI()
    {
        if(isLocalPlayer)
        {
            //TODO: get rid of this debugging if
            if(playerUI == null)
            {
                Debug.Log($"RpcResetUI called for {PlayerName} but playerUI was null???");
                return;
            }

            playerUI.GetComponent<Player>().SetStatus(string.Empty);

            clearHand();
        }
    }

    [Client]
    private void clearHand()
    {
        foreach(Transform child in playerUI.GetComponentInChildren<Hand>().transform)
        {
            Destroy(child.gameObject);
        }

        playerUI.GetComponentInChildren<Hand>().transform.DetachChildren();
    }

    [ClientRpc]
    public void RpcSetIsDealer()
    {
        var commonPlayerUI = playerUI.GetComponent<CommonPlayerUI>();
        commonPlayerUI.ShowIsDealer();
    }

    [ClientRpc]
    public void RpcShowCards(PlayingCard[] cards)
    {
        if(isLocalPlayer)
        {
            foreach(var card in cards)
            {
                addCard(card);
            }
            playerUI.GetComponentInChildren<Hand>().SortCards();
        }
    }

    [Client]
    private void addCard(PlayingCard card)
    {
        if(isLocalPlayer)
        {
            GameObject newCard = Instantiate(CardPrefab, this.transform.position, this.transform.rotation);
            newCard.name = card.CardName;
            newCard.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Sprites/{card.CardName}");
            newCard.GetComponent<Card>().SortOrder = card.SortOrder;
            newCard.GetComponent<Card>().Suit = card.Suit;
            newCard.GetComponent<Card>().Number = card.Number;

            newCard.transform.SetParent(playerUI.GetComponentInChildren<Hand>().transform, false);
        }
    }

    [ClientRpc]
    public void RpcTakeTurn(PlayingCard[] playableCards)
    {
        Debug.Log("RpcTakeTurn");
        Debug.Log($"isLocalPlayer {isLocalPlayer}");
        Debug.Log(playerUI == null);

        if(isLocalPlayer)
        {
            var commonPlayerUI = playerUI.GetComponent<CommonPlayerUI>();
            commonPlayerUI.ShowIsThinking();

            displayOptions(playableCards);
        }
    }

    [Client]
    private void displayOptions(PlayingCard[] playableCards)
    {
        GameObject optionsPanel = GameObject.Find("OptionsPanel");
        GameObject knockButton = GameObject.Find("KnockButton");

        if(playableCards.Count() == 0)
        {
            //could add an auto knock option?
            knockButton.GetComponent<Knock>().SetActivePlayer(gameObject);
            knockButton.GetComponent<Animator>().SetBool("isHidden", false);

            return;
        }

        optionsPanel.SetActive(true);

        foreach(var card in playableCards)
        {
            foreach(Transform child in playerUI.GetComponentInChildren<Hand>().transform)
            {
                if (child.tag == "Card" 
                    && child.GetComponent<Card>().Number == card.Number
                    && child.GetComponent<Card>().Suit == card.Suit)
                {
                    child.transform.SetParent(optionsPanel.transform, false);
                    child.GetComponent<Card>().IsClickable = true;
                    continue; //to prevent duplicates being moved with multipacks
                }
            }  
        }

        //TODO: if no unplayable cards left disable panel so we can click the whole card
    }

    [Client]
    public void Knock()
    {
        var commonPlayerUI = playerUI.GetComponent<CommonPlayerUI>();
        commonPlayerUI.ShowKnock();
        commonPlayerUI.ClearIsThinking();

        CmdKnock();
    }

    [Command]
    private void CmdKnock()
    {
        GameObject.Find("SeatManager").GetComponent<RoundManager>().Knock();
    }

}
