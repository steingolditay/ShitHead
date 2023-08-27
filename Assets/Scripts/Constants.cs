using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public static class Constants
{

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
    
}
