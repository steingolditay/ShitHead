using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    
    private Image frontImage, coverImage;
    private GameObject highlight;
    private Vector3 originalScale = Vector3.zero;
    private CardModel cardModel;

    private bool isHoverdOn = false;
    private bool isHighlighted = false;
    private bool isOnSelectionStage = false;
    
    void Awake()
    {
        frontImage = transform.Find("Front").transform.Find("Image").GetComponent<Image>();
        coverImage = transform.Find("Cover").transform.Find("Image").GetComponent<Image>();
        highlight = transform.Find("Highlight").gameObject;
        
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

    public bool GetIsOnSelectedStage()
    {
        return isOnSelectionStage;
    }

    private void OnMouseDown()
    {
        if (isOnSelectionStage)
        {
            if (isHighlighted)
            {
                GameMaster.Singleton.RemoveCardFromSelectedTableCards(this);
            }
            else
            {
                GameMaster.Singleton.AddCardToSelectedTableCards(this);

            }
            isHighlighted = !isHighlighted;
            ToggleHighlight(isHighlighted);
        }
    }

    public void ToggleHighlight(bool state)
    {
        highlight.SetActive(state);
    }


    private void OnMouseEnter()
    {
        if (transform.parent == GameMaster.Singleton.GetPlayerHand() && !isHoverdOn)
        {
            isHoverdOn = true;
            transform.LeanScale(new Vector3(originalScale.x * 1.1f, originalScale.y , originalScale.z * 1.1f), 0.1f);
            if (Utils.CanPlayCard(cardModel.cardClass, GameMaster.Singleton.GetTopPileCardClass()))
            {
                ToggleHighlight(true);
            }
        }
    }
    
    private void OnMouseExit()
    {
        if (transform.parent == GameMaster.Singleton.GetPlayerHand() && isHoverdOn)
        {
            isHoverdOn = false;
            transform.LeanScale(new Vector3(originalScale.x, originalScale.y, originalScale.z), 0.1f);
            ToggleHighlight(false);
        }    
    }
}
