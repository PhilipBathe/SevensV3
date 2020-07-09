using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class DealCards : MonoBehaviour
{
    public List<GameObject> Areas;
    public GameObject Pack;
    public GameObject DealerPuck;

    private List<GameObject> cards = new List<GameObject>();
    private int dealerIndex = -1;
    private List<GameObject> areaHands = new List<GameObject>();


    // Start is called before the first frame update
    void Start()
    {
        foreach(Transform child in Pack.transform)
        {
            //Debug.Log("child");
            if (child.tag == "Card")
            {
                cards.Add(child.gameObject);
            }
        }

        foreach(var area in Areas)
        {
            foreach(Transform child in area.transform)
            {
                if (child.tag == "Hand")
                {
                    areaHands.Add(child.gameObject);
                }
            } 
        }
    }

    // Update is called once per frame
    public void OnClick()
    {
        changeDealer();
        collectCards();
        deal();
        sortCards();
    }

    private void changeDealer()
    {
        dealerIndex++;
        if(dealerIndex >= Areas.Count)
        {
            dealerIndex = 0;
        }

        DealerPuck.transform.SetParent(Areas[dealerIndex].transform, false);
    }

    private void collectCards()
    {
        foreach(var area in areaHands)
        {
            foreach(Transform child in area.transform)
            {
                if (child.tag == "Card")
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }

    private void deal()
    {
        List<GameObject> shuffledCards = cards.OrderBy(a => Guid.NewGuid()).ToList();

        for(var i = 0; i < shuffledCards.Count; i++)
        {
            GameObject playerCard = Instantiate(shuffledCards[i], new Vector3(0, 0, 0), Quaternion.identity);

            var areaIndex = (i + dealerIndex + 1) % Areas.Count;

            playerCard.transform.SetParent(areaHands[areaIndex].transform, false);
        }
    }

    private void sortCards()
    {
        foreach(var hand in areaHands)
        {
           hand.GetComponent<SortCards>().Run();
        }
    }
}
