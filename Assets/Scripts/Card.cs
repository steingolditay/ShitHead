using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{

    private GameMaster gameMaster;
    private Image frontImage, coverImage;
    private GameObject highlight;
    private GameObject selection;
    private Vector3 originalScale = Vector3.zero;
    private CardModel cardModel;

    private bool isHoverdOn = false;
    private bool isHighlighted = false;
    private bool isOnSelectionStage = false;
    private bool isSelected = false;
    private bool canStick = false;
    
    void Awake()
    {
        gameMaster = GameMaster.Singleton;

        frontImage = transform.Find("Front").transform.Find("Image").GetComponent<Image>();
        coverImage = transform.Find("Cover").transform.Find("Image").GetComponent<Image>();
        highlight = transform.Find("Highlight").gameObject;
        selection = transform.Find("Selection").gameObject;

        ToggleHighlight(false);
        originalScale.x = transform.localScale.x / 20;
        originalScale.y = transform.localScale.y;
        originalScale.z = transform.localScale.z / 11.24f;
            
        Color frontImageColor = frontImage.color;
        frontImageColor.a = 0;
        frontImage.color = frontImageColor;

    }

    public int GetCardClass()
    {
        return cardModel.cardClass;
    }
    
    public CardFlavour GetCardFlavour()
    {
        return cardModel.cardFlavour;
    }

    public void SetCardModel(CardModel model)
    {
        cardModel = model;
        SetFront( Utils.GetSpriteForCardModel(model));;
        
    }

    public void SetCanStick(bool state)
    {
        canStick = state;
    }

    private void SetFront(Sprite front)
    {
        frontImage.sprite = front;
        
        Color frontImageColor = frontImage.color;
        frontImageColor.a = 1;
        frontImage.color = frontImageColor;
    }
    
    public void SetCover(Sprite cover)
    {
        coverImage.sprite = cover;
    }


    public void SetIsOnSelectionStage(bool state)
    {
        isOnSelectionStage = state;
    }

    private void OnMouseDown()
    {
        if (isOnSelectionStage)
        {
            if (isHighlighted)
            {
                gameMaster.RemoveCardFromSelectedTableCards(this);
            }
            else
            {
                gameMaster.AddCardToSelectedTableCards(this);

            }
            isHighlighted = !isHighlighted;
            ToggleHighlight(isHighlighted);
            return;
        }
        
        if (IsMyTurn() && CardIsInMyHand() && CanPlayerCard())
        {
            ToggleHighlight(false);
            ToggleSelection(false);
            gameMaster.PutCardFromPlayerHandInPile(this, false);

        } 
        else if (!IsMyTurn() && !gameMaster.opponentPlayedTurn && canStick)
        {
            ToggleHighlight(false);
            ToggleSelection(false);
            gameMaster.PutCardFromPlayerHandInPile(this, true);
        } 
        else if (CardIsVisibleOnMyTable() && CanPlayTableCard())
        {
            ToggleHighlight(false);
            ToggleSelection(false);
            gameMaster.PutCardFromPlayerTableInPile(this);
        }
    }

    public void ToggleHighlight(bool state)
    {
        highlight.SetActive(state);
    }

    public void ToggleSelection(bool state)
    {
        selection.SetActive(state);
    }


    private void OnMouseEnter()
    {
        if (CardIsInMyHand() && !isHoverdOn)
        {
            isHoverdOn = true;
            if (!isSelected)
            {
                ToggleRaised(true);
            }
            if ((IsMyTurn() && CanPlayerCard() && !isSelected) ||
                (!IsMyTurn() && !gameMaster.opponentPlayedTurn && canStick))
            {
                ToggleHighlight(!isSelected);
            }
        } else if (CardIsVisibleOnMyTable() && CanPlayTableCard() && !isHoverdOn && IsMyTurn())
        {
            isHoverdOn = true;
            ToggleHighlight(!isSelected);
        }
    }
//
    private void OnMouseOver()
    {
        if (IsMyTurn())
        {
            if (CardIsInMyHand() && CanPlayerCard())
            {
                if (Input.GetMouseButtonUp(1))
                {
                    isSelected = !isSelected;
                    ToggleSelection(isSelected);
                    ToggleHighlight(false);
                    ToggleRaised(true);
                    gameMaster.OnCardSelected(this, isSelected);
                }
            } else if (CardIsVisibleOnMyTable() && CanPlayTableCard())
            {
                if (Input.GetMouseButtonUp(1))
                {
                    isSelected = !isSelected;
                    ToggleSelection(isSelected);
                    ToggleHighlight(false);
                    gameMaster.OnCardSelected(this, isSelected);
                }
            }
        }
    }

    public void DisableSelection()
    {
        isSelected = false;
        ToggleSelection(false);
        ToggleHighlight(false);
        ToggleRaised(false);
    }

    private void OnMouseExit()
    {
        if (CardIsInMyHand() && isHoverdOn)
        {
            isHoverdOn = false;
            if (!isSelected)
            {
                ToggleRaised(false);
                transform.LeanScale(new Vector3(originalScale.x, originalScale.y, originalScale.z), 0.1f);
            }
            ToggleHighlight(false);
        } else if (CardIsVisibleOnMyTable() && CanPlayTableCard() && isHoverdOn)
        {
            isHoverdOn = false;
            ToggleHighlight(false);
        }
    }

    private bool IsMyTurn()
    {
        return gameMaster.GetCurrentPlayerTurn() == GameMaster.PlayerTurn.Player;
    }

    public void ToggleRaised(bool state)
    {
        float x = state ? originalScale.x * 1.1f : originalScale.x;
        float z = state ? originalScale.z * 1.1f : originalScale.z;
        transform.LeanScale(new Vector3(x, originalScale.y, z), 0.1f);

    }

    private bool CardIsInMyHand()
    {
        return transform.parent == gameMaster.GetPlayerHand();
    }

    private bool CardIsVisibleOnMyTable()
    {
        Transform parent = transform.parent;
        bool isTableCard = parent == gameMaster.playerTableCard1 || 
                           parent == gameMaster.playerTableCard2 || 
                           parent == gameMaster.playerTableCard3;
        if (!isTableCard || parent.childCount == 1)
        {
            return false;
        }
        return parent.GetChild(1) == transform;
    }

    private bool CanPlayTableCard()
    {
        return gameMaster.GetDeckCardsCount() == 0 && gameMaster.GetPlayerHand().childCount == 0 && CanPlayerCard();
    }

    private bool CanPlayerCard()
    {
        return Utils.CanPlayCard(GetCardClass(), gameMaster.GetTopPileCardClass());
    }
}
