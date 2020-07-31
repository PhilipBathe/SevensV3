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
        GameObject.Find("AIManager").GetComponent<AIManager>().LeaveTableEvent.AddListener(leaveTable);
    }

    [Client]
    private void leaveTable()
    {
        if(isLocalPlayer)
        {        
            var networkManager = GameObject.Find("NetworkManager").GetComponent<SevensNetworkManager>();  
            Debug.Log($"isClientOnly, {isClientOnly}");
            if(isClientOnly)
            {
                Debug.Log("Stop client");
                networkManager.StopClient();
            }
            else
            {
                Debug.Log("Stop host");
                networkManager.StopHost();
            } 
        }
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
        // foreach(Transform child in playerUI.GetComponentInChildren<Hand>().transform)
        // {
        //     Destroy(child.gameObject);
        // }

        // playerUI.GetComponentInChildren<Hand>().transform.DetachChildren();
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
            sortCards();
        }
    }

    [Client]
    private void sortCards()
    {
        if(isLocalPlayer)
        {
            foreach(var hand in GameObject.FindGameObjectsWithTag("Hand"))
            {
                Debug.Log($"hand with name {hand.name}");
                hand.GetComponent<Hand>().SortCards();
            }
        }
    }

    [Client]
    private void addCard(PlayingCard card)
    {
        if(isLocalPlayer)
        {
            GameObject newCard = Instantiate(CardPrefab, this.transform.position, this.transform.rotation);
            newCard.name = card.CardName;
            newCard.GetComponentInChildren<Image>().sprite = Resources.Load<Sprite>($"Sprites/{card.CardName}");
            newCard.GetComponent<Card>().PlayingCard = card;

            //TODO: get rid of these other properties now we are attaching the PlayingCard
            newCard.GetComponent<Card>().SortOrder = card.SortOrder;
            newCard.GetComponent<Card>().Suit = card.Suit;
            newCard.GetComponent<Card>().Number = card.Number;

            var parentTransform = GameObject.Find($"{card.Suit}Cards").transform;

            newCard.transform.SetParent(parentTransform, false);
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
            var parentTransform = GameObject.Find($"{card.Suit}Cards").transform;
            foreach(Transform child in parentTransform)
            {
                if (child.tag == "Card" 
                    && child.GetComponent<Card>().Number == card.Number
                    && child.GetComponent<Card>().Suit == card.Suit)
                {
                    var newParentTransform = GameObject.Find($"{card.Suit}Playable").transform;
                    child.transform.SetParent(newParentTransform, false);
                    child.GetComponent<Card>().IsClickable = true;
                    continue; //to prevent duplicates being moved with multipacks
                }
            }  
        }

        sortCards();

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
            foreach(Transform grandChild in child)
            {
                if (grandChild.GetComponent<Card>().Number == card.Number && grandChild.GetComponent<Card>().Suit == card.Suit)
                {
                    buggersToKill.Add(grandChild.gameObject);
                }
                else
                {
                    buggersToMove.Add(grandChild.gameObject);
                }
            }
        }

        foreach(var child in buggersToKill)
        {
            child.transform.SetParent(null);
            Destroy(child);
        }

        foreach(var child in buggersToMove)
        {
            var parentTransform = GameObject.Find($"{child.GetComponent<Card>().Suit}Cards").transform;
            child.transform.SetParent(parentTransform, false);
            child.GetComponent<Card>().IsClickable = false;
        }

        sortCards();

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

    [ClientRpc]
    public void RpcSortEnemies()
    {
        if(isLocalPlayer)
        {
            var enemiesPanel = GameObject.Find("Enemies");

            List<GameObject> enemies = new List<GameObject>();

            GameObject self = null;

            foreach(Transform child in enemiesPanel.transform)
            {
                if(child.gameObject.GetComponent<Enemy>().SeatNumber == SeatNumber)
                {
                    self = child.gameObject;
                }
                else
                {
                    enemies.Add(child.gameObject);
                }
            }

            if(self != null)
            {
                self.transform.SetParent(null);
            }

            List<GameObject> enemiesOrderedLower = enemies
                                                .Where(go => go.GetComponent<Enemy>().SeatNumber < SeatNumber)
                                                .OrderBy(go => go.GetComponent<Enemy>().SeatNumber).ToList();

            List<GameObject> enemiesOrderedUpper = enemies
                                                .Where(go => go.GetComponent<Enemy>().SeatNumber > SeatNumber)
                                                .OrderBy(go => go.GetComponent<Enemy>().SeatNumber).ToList();



            for (int i = 0; i < enemies.Count; i++)
            {
                if(i < enemiesOrderedUpper.Count)
                {
                    enemiesOrderedUpper[i].transform.SetSiblingIndex(i);
                }
                else
                {
                    enemiesOrderedLower[i-enemiesOrderedUpper.Count].transform.SetSiblingIndex(i);
                }
            }
        }
    }
}
