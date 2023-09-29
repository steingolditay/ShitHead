
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameMaster : MonoBehaviour
{
    public static GameMaster Singleton { get; private set; }
    private PlayerController playerController;

    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Sprite cover;
    [SerializeField] private Sprite[] cardFronts;

    [Header("Placements")] [SerializeField]
    private Transform cardHeight;

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

    [SerializeField] private Transform playerSelectionCard1;
    [SerializeField] private Transform playerSelectionCard2;
    [SerializeField] private Transform playerSelectionCard3;
    [SerializeField] private Transform playerSelectionCard4;
    [SerializeField] private Transform playerSelectionCard5;
    [SerializeField] private Transform playerSelectionCard6;

    [Header("Turn Indicators")] 
    [SerializeField] private Renderer playerTurnIndicator;
    [SerializeField] private Renderer opponentTurnIndicator;
    [SerializeField] private Material playerTurnIndicatorMaterial;
    [SerializeField] private Material opponentTurnIndicatorMaterial;
    [SerializeField] private Material turnIndicatorOffMaterial;

    [Header("Dialogs")] 
    [SerializeField] private GameObject selectCardsDialog;
    
    public enum PlayerTurn
    {
        Player,
        Opponent,
        None
    }

    private Collection<CardModel> cardModels = new Collection<CardModel>();
    private const float initialTimeBetweenPutCardInDeck = 0.2f;
    private const float deckCardDistance = 0.005f;
    private const float putCardInDeckAnimationSpeed = 0.5f;
    
    public bool playerSelectedTableCards = false;
    public List<Card> selectedTableCards = new List<Card>();
    public List<Card> unselectedTableCards = new List<Card>();

    public List<Card> selectedCards = new List<Card>();

    public int playersReady = 0;
    public ulong firstPlayerToStart = 0;
    private PlayerTurn currentPlayerTurn = PlayerTurn.None;
    public bool opponentPlayedTurn = false;


    private void OnEnable()
    {
        if (Singleton == null)
        {
            Singleton = this;
        }

        selectCardsDialog.transform.localScale = Vector3.zero;
        selectCardsDialog.SetActive(true);
    }

    private void OnDestroy()
    {
        if (Singleton == this)
        {
            Singleton = null;
        }
    }

    public void SetPlayerController(PlayerController callback)
    {
        playerController = callback;
    }

    public void AddCardToSelectedTableCards(Card card)
    {
        selectedTableCards.Add(card);
        unselectedTableCards.Remove(card);
        selectCardsDialog.GetComponent<SelectTableCardsDialog>().SetActive(selectedTableCards.Count == 3);
    }

    public void RemoveCardFromSelectedTableCards(Card card)
    {
        selectedTableCards.Remove(card);
        unselectedTableCards.Add(card);
        selectCardsDialog.GetComponent<SelectTableCardsDialog>().SetActive(selectedTableCards.Count == 3);
    }


    public void SetCurrentPlayerTurn(PlayerTurn player)
    {
        currentPlayerTurn = player;
        playerTurnIndicator.material =
            player == PlayerTurn.Player ? playerTurnIndicatorMaterial : turnIndicatorOffMaterial;

        opponentTurnIndicator.material =
            player == PlayerTurn.Player ? turnIndicatorOffMaterial : opponentTurnIndicatorMaterial;

        if (currentPlayerTurn == PlayerTurn.Opponent)
        {
            opponentPlayedTurn = false;
        }
        else
        {
            for (int i = 0; i < playerHand.childCount; i++)
            {
                Card card = playerHand.GetChild(i).GetComponent<Card>();
                card.SetCanStick(false);
            }

        }

    }

    public PlayerTurn GetCurrentPlayerTurn()
    {
        return currentPlayerTurn;
    }

    public Transform GetPlayerHand()
    {
        return playerHand;
    }

    public IEnumerator PutCardInDeck()
    {
        Quaternion rotation = Quaternion.Euler(new Vector3(0, 0, 180));
        Vector3 destination = deck.position;

        for (int i = 0; i < 54; i++)
        {

            float speed = 1 / (i == 0 ? 1f : i);
            yield return new WaitForSeconds(speed * initialTimeBetweenPutCardInDeck);
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
        
        
        // test
        
        // Table Cards //
        // Collection<CardModel> list = new Collection<CardModel>();
        // list.Add(new CardModel(4, CardFlavour.Diamond));
        // list.Add(new CardModel(4, CardFlavour.Club));
        // list.Add(new CardModel(5, CardFlavour.Diamond));
        // list.Add(new CardModel(5, CardFlavour.Club));
        // list.Add(new CardModel(6, CardFlavour.Diamond));
        // list.Add(new CardModel(6, CardFlavour.Club));
        //
        // // Selection Cards //
        // list.Add(new CardModel(7, CardFlavour.Diamond));
        // list.Add(new CardModel(7, CardFlavour.Club));
        //
        // list.Add(new CardModel(8, CardFlavour.Diamond));
        // list.Add(new CardModel(8, CardFlavour.Club));
        //
        // list.Add(new CardModel(9, CardFlavour.Diamond));
        // list.Add(new CardModel(9, CardFlavour.Club));
        //
        // list.Add(new CardModel(11, CardFlavour.Diamond));
        // list.Add(new CardModel(12, CardFlavour.Diamond));
        //
        // list.Add(new CardModel(11, CardFlavour.Club));
        // list.Add(new CardModel(12, CardFlavour.Club));
        //
        // list.Add(new CardModel(11, CardFlavour.Spade));
        // list.Add(new CardModel(12, CardFlavour.Spade));
        //
        // // First Card //
        // list.Add(new CardModel(8, CardFlavour.Heart));
        //
        // // Deck Cards //
        //
        // list.Add(new CardModel(11, CardFlavour.Heart));
        // list.Add(new CardModel(12, CardFlavour.Heart));
        //
        // list.Add(new CardModel(10, CardFlavour.Diamond));
        // list.Add(new CardModel(10, CardFlavour.Club));
        // list.Add(new CardModel(10, CardFlavour.Spade));
        // list.Add(new CardModel(10, CardFlavour.Heart));
        // list.Add(new CardModel(9, CardFlavour.Diamond));
        // list.Add(new CardModel(9, CardFlavour.Club));
        // list.Add(new CardModel(9, CardFlavour.Spade));
        // list.Add(new CardModel(9, CardFlavour.Heart));
        // list.Add(new CardModel(1, CardFlavour.Diamond));
        // list.Add(new CardModel(1, CardFlavour.Club));
        // list.Add(new CardModel(1, CardFlavour.Spade));
        // list.Add(new CardModel(1, CardFlavour.Heart));

        
        cardModels = list;
    }

    public void DealPlayerTableCard(CardModel cardModel, int number)
    {
        Transform placement;
        switch (number)
        {
            case 1:
                placement = playerTableCard1;
                break;
            case 2:
                placement = playerTableCard2;
                break;
            case 3:
                placement = playerTableCard3;
                break;
            default:
                placement = playerTableCard1;
                break;
        }

        Transform cardTransform = GetTopDeckCard();
        Card card = cardTransform.GetComponent<Card>();
        card.SetCardModel(cardModel);
        cardTransform.SetParent(placement, true);
        cardTransform.LeanMoveLocal(new Vector3(0, 0, 0), 0.3f);
    }

    public void DealPlayerSelectionCard(CardModel cardModel, int number)
    {
        Transform placement;
        switch (number)
        {
            case 1:
                placement = playerSelectionCard1;
                break;
            case 2:
                placement = playerSelectionCard2;
                break;
            case 3:
                placement = playerSelectionCard3;
                break;
            case 4:
                placement = playerSelectionCard4;
                break;
            case 5:
                placement = playerSelectionCard5;
                break;
            case 6:
                placement = playerSelectionCard6;
                break;
            default:
                placement = playerSelectionCard1;
                break;
        }

        Transform cardTransform = GetTopDeckCard();
        Card card = cardTransform.GetComponent<Card>();
        card.SetCardModel(cardModel);
        card.SetIsOnSelectionStage(true);
        cardTransform.SetParent(placement, true);
        unselectedTableCards.Add(card);
        cardTransform.LeanMoveLocal(new Vector3(0, 0, 0), 0.3f);
        LeanTween.rotateZ(cardTransform.gameObject, 0, 0.3f);
    }

    public void DealOpponentTableCard(int number)
    {
        Transform placement;
        switch (number)
        {
            case 1:
                placement = opponentTableCard1;
                break;
            case 2:
                placement = opponentTableCard2;
                break;
            case 3:
                placement = opponentTableCard3;
                break;
            default:
                placement = opponentTableCard1;
                break;
        }

        Transform cardTransform = GetTopDeckCard();
        cardTransform.SetParent(placement, true);
        cardTransform.LeanMoveLocal(new Vector3(0, 0, 0), 0.3f);

    }

    public void PlayerDrawCardFromDeck()
    {
        playerController.OnDrawCard();
    }

    public void PlayerDrawMissingCardsFromDeck(CardModel cardModel)
    {
        Transform cardTransform = GetTopDeckCard();
        Card card = cardTransform.GetComponent<Card>();
        card.SetCardModel(cardModel);
        if (cardModel.cardClass == GetAbsoluteTopPileCardClass())
        {
            card.SetCanStick(true);
        }
        cardTransform.SetParent(playerHand, true);

        Vector3 cardPosition = Utils.SortPlayerHand(playerHand, cardTransform.gameObject);

        LeanTween.moveLocalY(cardTransform.gameObject, cardPosition.y, 0.3f)
            .setEase(LeanTweenType.easeInOutQuad)
            .setOnComplete(() =>
            {
                cardTransform.LeanMoveLocal(cardPosition, 0.5f)
                    .setEase(LeanTweenType.easeInOutQuad);
            });
        LeanTween.rotateZ(cardTransform.gameObject, 0, 0.4f);
    }

    public void OpponentDrawCardFromDeck()
    {
        Transform cardTransform = GetTopDeckCard();
        cardTransform.SetParent(opponentHand, true);
        Vector3 cardPosition = Utils.SortOpponentHand(opponentHand, cardTransform.gameObject);

        LeanTween.moveLocalY(cardTransform.gameObject, cardPosition.y, 0.3f)
            .setEase(LeanTweenType.easeInOutQuad)
            .setOnComplete(() =>
            {
                cardTransform.LeanMoveLocal(cardPosition, 0.5f)
                    .setEase(LeanTweenType.easeInOutQuad);
            });
    }

    public Sprite[] GetCardFronts()
    {
        return cardFronts;
    }

    public void ToggleSelectCardsDialog(bool state)
    {
        selectCardsDialog.LeanScale(state ? new Vector3(1, 1, 1) : Vector3.zero, 0.3f)
            .setEase(LeanTweenType.easeInCubic)
            .setOvershoot(0.2f);
    }

    public IEnumerator SetPlayerSelectedTableCards()
    {
        playerSelectedTableCards = true;
        ToggleSelectCardsDialog(false);
        for (int i = 0; i < 3; i++)
        {
            Card card = selectedTableCards[i];
            Transform cardTransform = card.transform;
            Transform parent = GetPlayerTableCardPositionForIndex(i);

            cardTransform.SetParent(parent, true);
            card.SetIsOnSelectionStage(false);
            card.ToggleHighlight(false);
            Vector3 position = new Vector3(0, deckCardDistance, 0);
            cardTransform.LeanMoveLocal(position, 0.3f);
            yield return new WaitForSeconds(0.3f);
        }

        yield return new WaitForSeconds(0.3f);


        for (int i = 0; i < 3; i++)
        {
            Card card = unselectedTableCards[i];
            card.SetIsOnSelectionStage(false);
            Transform cardTransform = card.transform;
            cardTransform.SetParent(playerHand, true);

            Vector3 cardPosition = Utils.SortPlayerHand(playerHand, cardTransform.gameObject);
            cardTransform.LeanMoveLocal(cardPosition, 0.3f)
                .setEase(LeanTweenType.easeInOutQuad);
            yield return new WaitForSeconds(0.3f);

        }

        yield return null;
    }

    public void SetOpponentSelectedTableCards(CardModel cardModel, int number)
    {
        Transform cardTransform = GetOpponentCard();
        Card card = cardTransform.GetComponent<Card>();
        card.SetCardModel(cardModel);

        Transform destination = GetOpponentTableCardPositionForIndex(number);
        cardTransform.SetParent(destination, true);

        Utils.SortOpponentHand(opponentHand, null);

        cardTransform.LeanMoveLocal(new Vector3(0, deckCardDistance, 0), 0.3f)
            .setEase(LeanTweenType.easeInOutQuad);
        LeanTween.rotateZ(cardTransform.gameObject, 0, 0.3f);
    }

    private Transform GetPlayerTableCardPositionForIndex(int index)
    {
        Transform position;
        switch (index)
        {
            case 0:
                position = playerTableCard1;
                break;
            case 1:
                position = playerTableCard2;
                break;
            case 2:
                position = playerTableCard3;
                break;
            default:
                position = playerTableCard1;
                break;
        }

        return position;
    }

    private Transform GetOpponentTableCardPositionForIndex(int index)
    {
        Transform position;
        switch (index)
        {
            case 0:
                position = opponentTableCard1;
                break;
            case 1:
                position = opponentTableCard2;
                break;
            case 2:
                position = opponentTableCard3;
                break;
            default:
                position = opponentTableCard1;
                break;
        }

        return position;
    }

    private Transform GetOpponentCard()
    {
        return opponentHand.GetChild(opponentHand.childCount - 1);
    }

    public void SetFirstCardFromDeck(CardModel cardModel)
    {
        Transform topDeckCard = GetTopDeckCard();
        topDeckCard.GetComponent<Card>().SetCardModel(cardModel);
        topDeckCard.SetParent(pile, true);
        topDeckCard.LeanMoveLocal(Vector3.zero, 0.3f)
            .setEase(LeanTweenType.easeInOutQuad);
        LeanTween.rotateZ(topDeckCard.gameObject, 0, 0.3f);

    }

    public void OnCardSelected(Card card, bool state)
    {
        if (!state)
        {
            selectedCards.Remove(card);
            return;
        }
        //
        if (selectedCards.Count == 0 || selectedCards[0].GetCardClass() == card.GetCardClass())
        {
            selectedCards.Add(card);
            return;
        }

        foreach (Card selectedCard in selectedCards)
        {
            selectedCard.DisableSelection();
        }
        selectedCards.Clear();
        selectedCards.Add(card);
    }

    public CardModel GetTopDeckCardModel()
    {
        CardModel cardModel = cardModels[0];
        cardModels.RemoveAt(0);
        return cardModel;
    }

    public Transform GetTopDeckCard()
    {
        return deck.GetChild(deck.childCount - 1);
    }

    public void PutCardFromPlayerHandInPile(Card card, bool isStick)
    {

        List<Card> cardsToPlay = new List<Card>();
        if (selectedCards.Count > 0 && selectedCards[0].GetCardClass() == card.GetCardClass())
        {
            cardsToPlay = selectedCards;
        }
        else
        {
            cardsToPlay.Add(card);
        }

        foreach (Card cardToPlay in cardsToPlay)
        {
            cardToPlay.DisableSelection();
        }

        StartCoroutine(PlayCards(cardsToPlay, isStick));
    }

    private IEnumerator PlayCards(List<Card> cards, bool isStick)
    {
        bool isEights = cards[0].GetCardClass() == 8;
        bool isTens = cards[0].GetCardClass() == 10;
        bool isJoker = cards[0].GetCardClass() == 14;
        
        foreach (Card card in cards)
        {
            card.ToggleHighlight(false);
            card.ToggleSelection(false);
            card.ToggleRaised(false);
            Vector3 destination = new Vector3(0, deckCardDistance * pile.childCount, 0);
            card.transform.SetParent(pile, true);

            card.transform.LeanMoveLocal(destination, 0.3f);
            playerController.OnPutCardInPile(card);
            yield return new WaitForSeconds(0.3f);
        }
        yield return new WaitForSeconds(0.1f);
        // if top 4 are the same > boom
        if (IsBoom() || isTens)
        {
            playerController.OnClearPileToGraveyard();
            yield break;
        }

        if (!isStick)
        {
            if (!isEights || cards.Count == 1 || cards.Count == 3)
            {
                playerController.OnTurnFinished();
            }
            else if (ShouldDrawCards())
            {
                StartCoroutine(playerController.DrawMissingCards());
            }
            yield break;
        }


        if (ShouldDrawCards())
        {
            StartCoroutine(playerController.DrawMissingCards());
        }
        else
        {
            Utils.SortPlayerHand(playerHand, null);
        }
    }

    private bool ShouldDrawCards()
    {
        int deckCards = GetDeckCardsCount();
        int cardsInHand = GetPlayerHand().childCount;
        return deckCards > 0 && cardsInHand < 3;
    }

    private bool IsBoom()
    {
        if (pile.childCount < 4)
        {
            return false;
        }

        List<Card> topFourCards = new List<Card>();
        for (int i = pile.childCount - 1; i >= 0; i--)
        {
            topFourCards.Add(pile.GetChild(i).GetComponent<Card>());
        }
        List<Card> filteredList = topFourCards.FindAll(cards => cards.GetCardClass() == topFourCards[0].GetCardClass());
        return filteredList.Count == 4;
    }

    public IEnumerator ClearPileToGraveyard(bool isMe)
    {

        for (int i = pile.childCount - 1; i >= 0; i--)
        {
            Transform cardTransform = pile.GetChild(i);
            cardTransform.SetParent(graveyard, true);
            Vector3 destination = Vector3.zero;
            destination.y = graveyard.childCount * deckCardDistance;
            cardTransform.LeanMoveLocal(destination, 0.3f);
            cardTransform.LeanRotateZ(180, 0.3f);
            yield return new WaitForSeconds(0.2f);
        }


        if (isMe && ShouldDrawCards())
        {
            StartCoroutine(playerController.DrawMissingCards());
        }
    }


    public void SetOpponentHandCardInPile(Transform cardTransform)
    {
        Vector3 destination = new Vector3(0, deckCardDistance * pile.childCount, 0);
        cardTransform.SetParent(pile, true);
        cardTransform.LeanMoveLocal(destination, 0.3f);
        cardTransform.LeanRotateZ(0, 0.3f);
    }

    public Transform GetOpponentHandCard()
    {
        return opponentHand.GetChild(opponentHand.childCount - 1);
    }

    public int GetDeckCardsCount()
    {
        return deck.childCount;
    }
    
    private int GetAbsoluteTopPileCardClass()
    {
        if (pile.childCount > 0)
        {
            return pile.GetChild(pile.childCount - 1).GetComponent<Card>().GetCardClass();
        }

        return 0;

    }


    public int GetTopPileCardClass()
    {
        if (pile.childCount == 0)
        {
            return 0;
        }

        for (int i = pile.childCount - 1; i >= 0; i--)
        {
            int cardClass = pile.GetChild(i).GetComponent<Card>().GetCardClass();
            if (cardClass != 3)
            {
                return cardClass;
            }
        }
        return 3;

    }
}
