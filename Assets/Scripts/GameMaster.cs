
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameMaster : MonoBehaviour
{
    public static GameMaster Singleton { get; private set; }

    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Sprite cover;
    [SerializeField] private Sprite[] cardFronts;

    [Header("Placements")] 
    [SerializeField] private Transform cardHeight;
    [SerializeField] private Transform deckSpawnPoint;
    [SerializeField] private Transform playerHand;
    [SerializeField] private Transform opponentHand;
    [SerializeField] private Transform playerTableCard1;
    [SerializeField] private Transform playerTableCard2;
    [SerializeField] private Transform playerTableCard3;
    [SerializeField] private Transform opponentTableCard1;
    [SerializeField] private Transform opponentTableCard2;
    [SerializeField] private Transform opponentTableCard3;
    [SerializeField] private Transform pile;
    [SerializeField] private Transform deck;

    [SerializeField] private Transform graveyard;

    private Collection<CardModel> cardModels = new Collection<CardModel>();


    private const float initialTimeBetweenPutCardInDeck = 0.2f;
    private const float deckCardDistance = 0.005f;
    private const float putCardInDeckAnimationSpeed = 0.5f;


    private void OnEnable()
    {
        if (Singleton == null)
        {
            Singleton = this;
        }

        ShuffleCards();
        StartCoroutine(PutCardInDeck());

    }

    private void OnDestroy()
    {
        if (Singleton == this)
        {
            Singleton = null;
        }
    }
    
    
    public IEnumerator PutCardInDeck()
    {
        Quaternion rotation = Quaternion.Euler(new Vector3(0, 0, 180));
        Vector3 destination = deck.position;
    
        for (int i = 0; i < cardModels.Count; i++)
        {
            
            float spead = 1 / (i == 0 ? 1f : i);
            yield return new WaitForSeconds(spead * initialTimeBetweenPutCardInDeck);
            destination.y = deck.position.y + (i * deckCardDistance);
            GameObject card = Instantiate(cardPrefab, deckSpawnPoint.position, rotation);
            card.transform.SetParent(deck, true);
            card.GetComponent<Card>().SetCover(cover);
            LeanTween.move(card, destination, putCardInDeckAnimationSpeed);  
        }
        
        yield return new WaitForSeconds(1.5f);
    }
    
    public void ShuffleCards()
    {
        cardModels = Utils.GetAllCardModels();

        for (int i = 0; i < cardModels.Count - 1; i++)
        {
            CardModel temp = cardModels[i];
            int random = Random.Range(i, cardModels.Count);
            cardModels[i] = cardModels[random];
            cardModels[random] = temp;
        }
        Debug.Log("");
    }

    public void SlotPlayer()
    {
        
        Debug.Log("Slot a card to player");
        CardModel cardModel = GetTopDeckCardModel();
        Transform cardTransform = GetTopDeckCard();
        Card card = cardTransform.GetComponent<Card>();
        card.SetFront(GetSpriteForCardModel(cardModel));
        card.SetCardClass(cardModel.cardClass);
        cardTransform.SetParent(playerHand, true);
        
        Vector3 cardPosition = Utils.SortPlayerHand(playerHand, cardTransform.gameObject);
        
        LeanTween.moveLocalY(cardTransform.gameObject, cardPosition.y, 0.3f)
            .setEase(LeanTweenType.easeInOutQuad)
            .setOnComplete(() => {
            cardTransform.LeanMoveLocal(cardPosition, 0.5f)
                .setEase(LeanTweenType.easeInOutQuad);
            });
        // rotate
        LeanTween.rotateZ(cardTransform.gameObject, 0, 0.4f);
    }

    public void SlotOpponent()
    {
        Transform cardTransform = GetTopDeckCard();
        cardTransform.SetParent(opponentHand, true);
        Vector3 cardPosition = Utils.SortOpponentHand(opponentHand, cardTransform.gameObject);

        // move 
        LeanTween.moveLocalY(cardTransform.gameObject, cardPosition.y, 0.3f)
            .setEase(LeanTweenType.easeInOutQuad)
            .setOnComplete(() => {
                cardTransform.LeanMoveLocal(cardPosition, 0.5f)
                    .setEase(LeanTweenType.easeInOutQuad);
            });
    }
    

    public Collection<CardModel> GetDeck()
    {
        return cardModels;
    }

    private Sprite GetSpriteForCardModel(CardModel cardModel)
    {
        if (cardModel.cardFlavour == CardFlavour.Joker01 || cardModel.cardFlavour == CardFlavour.Joker02)
        {
            foreach (Sprite sprite in cardFronts)
            {
                if (sprite.name == cardModel.cardFlavour.ToString())
                {
                    return sprite;
                }
            }
        }
        else
        {
            string cardClass = cardModel.cardClass < 10 ? "0" + cardModel.cardClass : cardModel.cardClass.ToString();
            foreach (Sprite sprite in cardFronts)
            {
                if (sprite.name == cardModel.cardFlavour + cardClass)
                {
                    return sprite;
                }
            }
        }

        return null;
    }

    private CardModel GetTopDeckCardModel()
    {
        CardModel cardModel = cardModels[0];
        cardModels.RemoveAt(0);
        return cardModel;
    }

    private Transform GetTopDeckCard()
    {
        return deck.GetChild(deck.childCount - 1);
    }
}
