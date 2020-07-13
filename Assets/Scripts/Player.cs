using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Player : MonoBehaviour
{
    // Start is called before the first frame update
    public bool IsActive = false;

    public bool IsAI = false;



    private bool isTakingTurn = false;

    private Transform hand;
    private TableManager tableManager;
    private BoardManager boardManager;


    public bool HasSevenOfDiamonds()
    {
        //TODO: find a less clunky way to find this?
        var sevenOfDiamonds = transform.Find("Hand/Diamond07(Clone)");
        return sevenOfDiamonds != null;
    }

    void Start() {
        boardManager = GameObject.Find("Board").GetComponent<BoardManager>();
        tableManager = GameObject.Find("TableManager").GetComponent<TableManager>();
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
        //Text activeText = transform.Find("Active").GetComponent<Text>();
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
        else
        {
             //activeText.text = string.Empty;
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
            displayOptions();
        }


    }

    IEnumerator AiThinkingCoroutine(List<GameObject> playableCards)
    {
        yield return new WaitForSeconds(1);

        if(playableCards.Count == 0)
        {
            knock();
        }
        else
        {
            //TODO: cleverer AI than this!
            playCard(playableCards.First());
        }
        isTakingTurn = false;
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
        boardManager.PlayCard(card);
        if(this.cardCount() == 0)
        {
            var position = tableManager.OutOfCards();
        }
        tableManager.NextPlayer();
        isTakingTurn = false;
    }

    private void displayOptions()
    {
        Debug.Log("Need to display options");
        //isTakingTurn = false;
    }
}
