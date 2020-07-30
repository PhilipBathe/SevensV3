using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NetworkPlayer : NetworkBehaviour
{
    [SyncVar]
    public string PlayerName;

    [SyncVar]
    public int SeatNumber;

    public GameObject PlayerPrefab;

    public GameObject CardPrefab;

    private GameObject playerUI;

    private GameObject newGamePanel;

    private GameObject waitingGO;

    private GameObject countdownGO;

    private UnityEngine.UI.Slider aiNumberSlider;


    // This fires on server when this player object is network-ready
    public override void OnStartServer()
    {
        base.OnStartServer();

        //playerNo = connectionToClient.connectionId;
        
    }

    // This only fires on the local client when this player object is network-ready
    public override void OnStartLocalPlayer()
    {
        gameObject.name = "LocalNetworkPlayer";

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

        GameObject.Find("AIManager").GetComponent<AIManager>().NumberChangedEvent.AddListener(CmdChangeNumberOfAIPlayers);
        GameObject.Find("AIManager").GetComponent<AIManager>().WineLevelChangedEvent.AddListener(CmdChangeWineLevel);
    }

    [Command]
    private void CmdChangeNumberOfAIPlayers(int number)
    {
        GameObject.Find("SeatManager").GetComponent<SeatManager>().ChangeNumberOfAIPlayers(number);
        GameObject.Find("AIManager").GetComponent<AIManager>().NumberOfAIPlayers = number;
    }

    [Command]
    private void CmdChangeWineLevel(int number)
    {
        GameObject.Find("SeatManager").GetComponent<SeatManager>().ChangeWineLevel(number);
        GameObject.Find("AIManager").GetComponent<AIManager>().WineLevel = number;
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

            var commonPlayerUI = playerUI.GetComponent<CommonPlayerUI>();
            commonPlayerUI.SetStatusText($"Waiting for next game");

            newGamePanel = GameObject.Find("NextGame");
            waitingGO = GameObject.Find("WaitingForPlayers");
            countdownGO = GameObject.Find("StartCountdown");
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

            playerUI.GetComponent<CommonPlayerUI>().ClearAll();

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
        if(isLocalPlayer)
        {
            playerUI.GetComponent<CommonPlayerUI>().ShowIsDealer();
        }
    }

     [ClientRpc]
    public void RpcSetPlaced(int placed)
    {
        if(isLocalPlayer)
        {
            playerUI.GetComponent<CommonPlayerUI>().ShowPlaced(placed);
        }
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
            newCard.GetComponent<Card>().PlayingCard = card;

            //TODO: get rid of these other properties now we are attaching the PlayingCard
            newCard.GetComponent<Card>().SortOrder = card.SortOrder;
            newCard.GetComponent<Card>().Suit = card.Suit;
            newCard.GetComponent<Card>().Number = card.Number;

            newCard.transform.SetParent(playerUI.GetComponentInChildren<Hand>().transform, false);
        }
    }

    [ClientRpc]
    public void RpcTakeTurn(PlayingCard[] playableCards)
    {
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

        optionsPanel.GetComponent<OptionsPanel>().SortCards();

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

    [Client]
    public void PlayCard(PlayingCard card)
    {
        GameObject optionsPanel = GameObject.Find("OptionsPanel");
        List<GameObject> buggersToKill = new List<GameObject>();
        List<GameObject> buggersToMove = new List<GameObject>();

        foreach(Transform child in optionsPanel.transform)
        {
            //Debug.Log($"card found {child.GetComponent<Card>().PlayingCard.CardName}");

            if (child.GetComponent<Card>().Number == card.Number && child.GetComponent<Card>().Suit == card.Suit)
            {
                buggersToKill.Add(child.gameObject);
            }
            else
            {
                buggersToMove.Add(child.gameObject);
            }
        }

        foreach(var child in buggersToKill)
        {
            child.transform.SetParent(null);
            Destroy(child);
        }

        foreach(var child in buggersToMove)
        {
            child.transform.SetParent(playerUI.GetComponentInChildren<Hand>().transform, false);
            child.GetComponent<Card>().IsClickable = false;
        }

        playerUI.GetComponentInChildren<Hand>().SortCards();

        var commonPlayerUI = playerUI.GetComponent<CommonPlayerUI>();
        commonPlayerUI.ShowLastGo(card);
        commonPlayerUI.ClearIsThinking();

        CmdPlayCard(card);
    }

    [Command]
    private void CmdPlayCard(PlayingCard card)
    {
        GameObject.Find("SeatManager").GetComponent<RoundManager>().PlayCard(card);
    }


    [ClientRpc]
    public void RpcHideNextGamePanel()
    {
        if(isLocalPlayer)
        {
            newGamePanel.GetComponent<Animator>().SetBool("isHidden", true);
        }
    }

    private Coroutine countdownCoroutine;

    [ClientRpc]
    public void RpcStartCountdown(int countdownSeconds)
    {
        if(isLocalPlayer)
        {
            showNextGamePanel();

            setNextGameState(false);

            if(countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
            }
            countdownCoroutine = StartCoroutine(countdown(countdownSeconds));
        }
    }

    [ClientRpc]
    public void RpcShowWaitingForPlayers()
    {
        if(isLocalPlayer)
        {
            showNextGamePanel();

            setNextGameState(true);

            if(countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
            }
        }
    }

    [Client]
    private void showNextGamePanel()
    {
        if(isLocalPlayer)
        {
            newGamePanel.transform.SetAsLastSibling();
            newGamePanel.GetComponent<Animator>().SetBool("isHidden", false);
        }
    }

    [Client]
    private IEnumerator countdown(int countdownSeconds)
    {
        var text = GameObject.Find("CountdownText").GetComponent<Text>();
        for(int i = countdownSeconds; i >= 0; i--)
        {
            text.text = i.ToString();
            yield return new WaitForSeconds(1);
        }
    }

    [Client]
    private void setNextGameState(bool isWaitingForPlayers)
    {
        if(isLocalPlayer)
        {
            waitingGO.SetActive(isWaitingForPlayers);
            countdownGO.SetActive(!isWaitingForPlayers);
        }
    }
}
