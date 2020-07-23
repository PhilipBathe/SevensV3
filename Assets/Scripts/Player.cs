using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class Player : MonoBehaviour
{
    // Start is called before the first frame update
    public bool IsActive = false;

    public bool IsAI = false;



    private bool isTakingTurn = false;

    private Transform hand;
    private TableManager tableManager;
    private BoardManager boardManager;
    private GameObject optionsPanel;

    public bool HasSevenOfDiamonds()
    {
        //TODO: find a less clunky way to find this?
        var sevenOfDiamonds = transform.Find("Hand/Diamond07(Clone)");
        return sevenOfDiamonds != null;
    }

    void Start() {
        boardManager = GameObject.Find("Board").GetComponent<BoardManager>();
        tableManager = GameObject.Find("TableManager").GetComponent<TableManager>();

        if(IsAI == false)
        {
            optionsPanel = GameObject.Find("OptionsPanel");
            optionsPanel.SetActive(false);
        }

        foreach(Transform child in this.transform)
        {
            if (child.tag == "Hand")
            {
                this.hand = child;
            }
        }
    }

    void Update()
    {
        //FIXME: probably a better way to work out if should be taking turn, than checking active flag on every frame!
        if(IsActive)
        {
            
            if(isTakingTurn)
            {
                return;
            }
            else
            {
                startTurn();
            }
        }
    }

    private void startTurn()
    {

        isTakingTurn = true;
        GameObject.Find("ActivePointer").transform.SetParent(this.transform, false);

        List<GameObject> playableCards = getPlayableCards();

        if(IsAI)
        {
            StartCoroutine(AiThinkingCoroutine(playableCards));
        }
        else
        {
            displayOptions(playableCards);
        }


    }

    IEnumerator AiThinkingCoroutine(List<GameObject> playableCards)
    {
        //Thinking time!
        yield return new WaitForSeconds(1);

        if(playableCards.Count == 0)
        {
            knock();
        }
        else
        {
            if(playableCards.Count == 1)
            {
                playCard(playableCards.First());
            }
            else
            {
                var cardToPlay = chooseCard(playableCards);

                playCard(cardToPlay);
            }
        }
        isTakingTurn = false;
    }

    private GameObject chooseCard(List<GameObject> playableCards)
    {
        Debug.Log("Choice to make");

        var cardToPlay = playableCards.First();
        float bestCardWorth = -100f;

        foreach(var playableCardGO in playableCards)
        {
            var playableCard = playableCardGO.GetComponent<Card>();
            int distUp = 0;
            int distDown = 0;
            int suitCardCount = 1;

            //we need clever maths - gap to card furthest away =1 (in both directions if a 7) - number of cards in suit
            foreach(Transform cardChild in this.hand)
            {
                if (cardChild.tag != "Card")
                {
                    continue;
                }

                var card = cardChild.GetComponent<Card>();
                if(card.Suit != playableCard.Suit || card.Number == playableCard.Number)
                {
                    continue;
                }

                if(card.Number > 7 && playableCard.Number >= 7 && distUp < card.Number - playableCard.Number)
                {
                    distUp = card.Number - playableCard.Number; 
                    suitCardCount++;
                }
                
                if(card.Number < 7 && playableCard.Number <= 7 && distDown < playableCard.Number - card.Number)
                {
                    distDown = playableCard.Number - card.Number; 
                    suitCardCount++;
                }
            }

            //make playing a card that has a neighbour more valuable than if no neighbour exists
            if(distUp > 0)
            {
                distUp++;
            }
            if(distDown > 0)
            {
                distDown++;
            }

            float currentWorth = distUp + distDown - suitCardCount;

            //in the event of a tie favour cards nearer to the ends
            currentWorth += Math.Abs(7 - playableCard.Number) / 10f;

            //if still a tie then don't always pick the first suit ("add some flavour")
            currentWorth += UnityEngine.Random.Range(0.01f, 0.09f);

            //TODO: maybe add some stupid setting ("Wine level")?

            Debug.Log($"Card {playableCardGO.name} gets worth of {currentWorth}");

            if(currentWorth > bestCardWorth)
            {
                bestCardWorth = currentWorth;
                cardToPlay = playableCardGO;
            }
        }

        return cardToPlay;
    }

    private List<GameObject> getPlayableCards()
    {
        List<GameObject> playableCards = new List<GameObject>();

        if(this.HasSevenOfDiamonds())
        {
            var sevenOfDiamonds = transform.Find("Hand/Diamond07(Clone)");
            playableCards.Add(sevenOfDiamonds.gameObject);
        }
        else{
            foreach(Transform cardChild in this.hand)
            {
                if (cardChild.tag == "Card")
                {
                    if(boardManager.IsCardPlayable(cardChild.gameObject))
                    {
                        playableCards.Add(cardChild.gameObject);
                    }
                }
            }
        }

        return playableCards;
    }

    private int cardCount()
    {
        int count = 0;

        foreach(Transform cardChild in this.hand)
        {
            if (cardChild.tag == "Card")
            {
                count++;
            }
        }

        return count;
    }

    private void knock()
    {
        Debug.Log("Knock!");
        //TODO: show "knock"
        tableManager.NextPlayer();
        isTakingTurn = false;
    }

    private void playCard(GameObject card)
    {
        //Debug.Log($"Playing {card.name}");
        //TODO: show playing card
        boardManager.PlayCard(card);
        if(this.cardCount() == 0)
        {
            var position = tableManager.OutOfCards();
            //TODO: show final position in game
        }
        tableManager.NextPlayer();
        isTakingTurn = false;
    }


    private void displayOptions(List<GameObject> playableCards)
    {
        //Debug.Log("Need to display options");

        if(playableCards.Count == 0)
        {
            //show knock button
            //TODO: auto knock option
            knock();

            return;
        }

        optionsPanel.SetActive(true);

        Dictionary<string, int> suitPositions = new Dictionary<string, int>{
            { "Heart", 140 },
            { "Spade", 50 },
            { "Diamond", -40 },
            { "Club", -130 }
        };

        foreach(var card in playableCards)
        {
            var dupCard = Instantiate(card, Vector2.zero, Quaternion.identity);

            dupCard.transform.SetParent(optionsPanel.transform, false);
            dupCard.GetComponent<Image>().rectTransform.sizeDelta = new Vector2(80, 80);

            var cardProperties = card.GetComponent<Card>();
            int x = cardProperties.Number <= 7 ? 50 : 150;
            int y = suitPositions[cardProperties.Suit];
            dupCard.transform.localPosition = new Vector2(x, y);
        }
    }

    public void SelectCard(GameObject card)
    {
        Debug.Log("Selecting Card");
        var cardToPlay = getPlayableCards().First(c => c.GetComponent<Card>().Suit == card.GetComponent<Card>().Suit && c.GetComponent<Card>().Number == card.GetComponent<Card>().Number);

        foreach(Transform child in optionsPanel.transform)
        {
            if (child.tag == "Card")
            {
                Destroy(child.gameObject);
            }
        }
        optionsPanel.transform.DetachChildren();
        optionsPanel.SetActive(false);        
        
        playCard(cardToPlay);
    }
}
