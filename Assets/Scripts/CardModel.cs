

public class CardModel
{ 
    public int cardClass;
    public CardFlavour cardFlavour;

    public CardModel(int cardClass, CardFlavour cardFlavour)
    {
        this.cardClass = cardClass;
        this.cardFlavour = cardFlavour;
    }
}


public enum CardFlavour
{
    Heart, Spade, Diamond, Club, Joker01, Joker02
}