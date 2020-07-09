using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreatePack : MonoBehaviour
{
    public GameObject CardPrefab;

    private string[] suits = new string[] {"Club", "Diamond", "Heart", "Spade"};

    void Start()
    {
        Debug.Log("Creating Pack");

        string cardName = string.Empty;
        string id = string.Empty;

        foreach(var suit in suits)
        {
            for(int i = 1; i < 14; i++)
            {
                id = $"0{i}";
                cardName = suit + id.Substring(id.Length - 2);
                addCard(cardName);
            }
        }
    }

    private void addCard(string cardName)
    {
        GameObject newCard = Instantiate(CardPrefab, this.transform.position, this.transform.rotation);
        newCard.name = cardName;
        newCard.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Sprites/{cardName}");

        newCard.transform.SetParent(this.transform, false);
    }


}
