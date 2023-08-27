using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    private int cardClass;
    private Image frontImage, coverImage;
    
    void Awake()
    {
        frontImage = transform.Find("Front").transform.Find("Image").GetComponent<Image>();
        coverImage = transform.Find("Cover").transform.Find("Image").GetComponent<Image>();
        
        Color frontImageColor = frontImage.color;
        frontImageColor.a = 0;
        frontImage.color = frontImageColor;
    }

    public int GetCardClass()
    {
        return cardClass;
    }

    public void SetFront(Sprite front)
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

    public void SetCardClass(int number)
    {
        this.cardClass = number;
    }
}
