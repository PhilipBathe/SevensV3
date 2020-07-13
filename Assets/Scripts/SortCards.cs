using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SortCards : MonoBehaviour
{
    public void Run()
    {
        List<GameObject> cards = new List<GameObject>();

        foreach(Transform child in this.transform)
            {
                if (child.tag == "Card")
                {
                    cards.Add(child.gameObject);
                }
            }

        GameObject[] cardsOrdered = cards.OrderBy(go => go.GetComponent<Card>().SortOrder).ToArray();

       // Debug.Log(cardsOrdered.Length);

        for (int i = 0; i < cardsOrdered.Length; i++)
        {
            cardsOrdered[i].transform.SetSiblingIndex(i);
            cardsOrdered[i].transform.localPosition = new Vector3(
                cardsOrdered[i].transform.localPosition.x + (i * 0.3f), 
                cardsOrdered[i].transform.localPosition.y, 
                cardsOrdered[i].transform.localPosition.z - ((i + 1) * 0.01f));
        }


    }
}
