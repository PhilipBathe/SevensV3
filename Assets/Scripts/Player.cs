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

    public GameObject Board;


    private bool isTakingTurn = false;

    private List<GameObject> suitSlots = new List<GameObject>();

    public bool HasSevenOfDiamonds()
    {
        //TODO: find a less clunky way to find this?
        var sevenOfDiamonds = transform.Find("Hand/Diamond07(Clone)");
        return sevenOfDiamonds != null;
    }

    void Start() {
        foreach(Transform child in Board.transform)
        {
            if (child.tag == "SuitSlot")
            {
                suitSlots.Add(child.gameObject);
            }
        }
    }

    void Update()
    {
        //FIXME: probably a better way to work out if should be taking turn, than checking active flag on every frame!
        Text activeText = transform.Find("Active").GetComponent<Text>();
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
             activeText.text = string.Empty;
        }
    }

    private void startTurn()
    {
        Text activeText = transform.Find("Active").GetComponent<Text>();

        activeText.text = "Active";
        isTakingTurn = true;

        List<GameObject> playableCards = getPlayableCards();

        if(IsAI)
        {
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
        else
        {
            displayOptions();
        }


    }

    private List<GameObject> getPlayableCards()
    {
        List<GameObject> playableCards = new List<GameObject>();

        if(this.HasSevenOfDiamonds())
        {
            var sevenOfDiamonds = transform.Find("Hand/Diamond07(Clone)");
            playableCards.Add(sevenOfDiamonds.gameObject);
        }

        //TODO: work out what playable cards we have

        return playableCards;

    }

    private void knock()
    {
        //TODO: show "knock"
        //TODO: tell gamemanager to move to next player
    }

    private void playCard(GameObject card)
    {
        Debug.Log($"Playing {card.name}");
        //TODO: move card to board
        card.transform.SetParent(suitSlots.First().transform, false);
        //TODO: tell gamemanager to move to next player
    }

    private void displayOptions()
    {
        Debug.Log("Need to display options");
        isTakingTurn = false;
    }
}
