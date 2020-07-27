using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class OptionsPanel : MonoBehaviour
{
    public void SortCards()
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

        for (int i = 0; i < cardsOrdered.Length; i++)
        {
            cardsOrdered[i].transform.SetSiblingIndex(i);
        }
    }
}
