using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

public static class Utils
{
    
    private const float baseHorizontalDistance = 0.02f;
    private const float baseVerticalDistance = 0.005f;

    public static Collection<CardModel> GetAllCardModels()
    {
        Collection<CardModel> list = new Collection<CardModel>();

        for (int i = 1; i <= 13; i++)
        {
            list.Add(new CardModel(i, CardFlavour.Club));
            list.Add(new CardModel(i, CardFlavour.Heart));
            list.Add(new CardModel(i, CardFlavour.Spade));
            list.Add(new CardModel(i, CardFlavour.Diamond));
        }
        list.Add(new CardModel(14, CardFlavour.Joker01));
        list.Add(new CardModel(14, CardFlavour.Joker02));
        
        return list;
    }


    public static Vector3 SortOpponentHand(Transform opponentHand, GameObject newCard) 
    {
        Collection<GameObject> opponentCards = GetExistingCardObjectsInSlot(opponentHand);
        return SortHand(opponentCards, newCard);
    }
    
    public static Vector3 SortPlayerHand(Transform playerHand, GameObject newCard)
    {
        Collection<GameObject> playerCards = GetExistingCardObjectsInSlot(playerHand);
        List<GameObject> orderedList = playerCards.OrderBy(card => card.GetComponent<Card>().GetCardClass()).ToList();
        return SortHand(orderedList, newCard);
    }
    
    private static Collection<GameObject> GetExistingCardObjectsInSlot(Transform slot)
    {
        Collection<GameObject> opponentCards = new Collection<GameObject>();
        for (int i = 0; i < slot.childCount; i++)
        {
            opponentCards.Add(slot.GetChild(i).gameObject);
        }

        return opponentCards;
    }


    private static Vector3 SortHand(IList<GameObject> slotCards, GameObject newCard)
    { 
        Vector3 newCardPosition = new Vector3();
        int numberOfCards = slotCards.Count;
        float horizontalDistance = numberOfCards > 24 ? baseHorizontalDistance / 1.3f : baseHorizontalDistance;
        bool evenNumberOfCards = numberOfCards % 2 == 0;
        int middleCardIndex = numberOfCards / 2;
        
        GameObject middleCard = slotCards[middleCardIndex];
        Vector3 middleCardPosition = Vector3.zero;
        middleCardPosition.x = evenNumberOfCards ? horizontalDistance / 2 : 0;
        if (middleCard != newCard)
        {
            middleCard.LeanMoveLocal(middleCardPosition, 0.3f);
        }
        else
        {
            newCardPosition = middleCardPosition;
        }
        
        for (int i = 0; i < numberOfCards; i++)
        {
            if (i != middleCardIndex)
            {
                GameObject cardObject = slotCards[i];

                int relativePosition = Mathf.Abs(i - middleCardIndex);
                Vector3 destination = Vector3.zero;
                destination.x = middleCardPosition.x + relativePosition * (i < middleCardIndex ? -horizontalDistance : horizontalDistance);
                destination.y = middleCardPosition.y + relativePosition * (i < middleCardIndex ? -baseVerticalDistance : baseVerticalDistance);
                if (cardObject == newCard)
                {
                    newCardPosition = destination;
                }
                else
                {
                    cardObject.LeanMoveLocal(destination, 0.3f);
                }
            }
        }
        
        return newCardPosition;
    }
}
