using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class Pack
{
    public static List<PlayingCard> GetCards()
    {
        string[] suits = new string[] {"Diamond"}; //, "Club", "Heart", "Spade"};

        List<PlayingCard> cards = new List<PlayingCard>();

        string cardName = string.Empty;
        string id = string.Empty;
        int sortOrder = 0;

        foreach(var suit in suits)
        {
            for(int i = 1; i < 14; i++)
            {
                id = $"0{i}";
                cardName = suit + id.Substring(id.Length - 2);
                cards.Add(new PlayingCard {
                    Suit = suit,
                    Number = i,
                    SortOrder = sortOrder++
                });
            }
        }

        return cards;
    }
}