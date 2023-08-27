using System.Collections;
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
        if (evenNumberOfCards)
        {
            int count = 0;
            float startPosition = 0.01f;
            int leftMiddlePosition = numberOfCards / 2;
            int rightMiddlePosition = (numberOfCards / 2) + 1;
            for (int i = leftMiddlePosition - 1; i >= 0 ; i--)
            {
                GameObject cardObject = slotCards[i];
                Vector3 destPosition = new Vector3();
                destPosition.x = -startPosition + (count * -horizontalDistance);
                destPosition.y = count * -baseVerticalDistance;
                if (cardObject == newCard)
                {
                    newCardPosition = destPosition;
                }
                else
                {
                    cardObject.LeanMoveLocal(destPosition, 0.3f);
                }
                count++;
            }

            count = 0;
            for (int i = rightMiddlePosition - 1; i < numberOfCards; i++)
            {
                GameObject cardObject = slotCards[i];
                Vector3 destPosition = new Vector3();
                destPosition.x = startPosition + (count * horizontalDistance);
                destPosition.y = (count + 1) * baseVerticalDistance;
                if (cardObject == newCard)
                {
                    newCardPosition = destPosition;
                }
                else
                {
                    cardObject.LeanMoveLocal(destPosition, 0.3f);
                }

                count++;
            }
        }
        else
        {
            int count = 0;
            int middlePosition = (numberOfCards / 2) + 1;
            for (int i = middlePosition - 1; i >= 0 ; i--)
            {
                GameObject cardObject = slotCards[i];
                Vector3 destPosition = new Vector3();
                destPosition.x = count * -horizontalDistance;
                destPosition.y = count * -baseVerticalDistance;
                if (cardObject == newCard)
                {
                    newCardPosition = destPosition;
                }
                else
                {
                    cardObject.LeanMoveLocal(destPosition, 0.3f);
                }
                count++;
            }
            count = 1;
            for (int i = middlePosition; i < numberOfCards; i++)
            {
                GameObject cardObject = slotCards[i];
                Vector3 destPosition = new Vector3();
                destPosition.x = (count * horizontalDistance);
                destPosition.y = count * baseVerticalDistance;
                if (cardObject == newCard)
                {
                    newCardPosition = destPosition;
                }
                else
                {
                    cardObject.LeanMoveLocal(destPosition, 0.3f);
                }
                count++;
            }
        }

        return newCardPosition;
    }
    

    
}
