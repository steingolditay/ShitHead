


using System;
using UnityEngine;

public class DeckScript : MonoBehaviour
{
    private GameMaster gameMaster;

    private void Start()
    {
        gameMaster = GameMaster.Singleton;
    }


    private void OnMouseEnter()
    {
        if (IsMyTurn() && DeckHasCards())
        {
            HighlightTopDeckCard(true);

        }
    }

    private void OnMouseUp()
    {
        if (IsMyTurn() && DeckHasCards())
        {
            gameMaster.PlayerDrawCardFromDeck();
        }
    }

    private void OnMouseExit()
    {
        if (IsMyTurn() && DeckHasCards())
        {
            HighlightTopDeckCard(false);
        }
        
    }

    private bool IsMyTurn()
    {
        return gameMaster.GetCurrentPlayerTurn() == GameMaster.PlayerTurn.Player;
    }

    private bool DeckHasCards()
    {
        return transform.childCount > 0;
    }

    private void HighlightTopDeckCard(bool state)
    {
        gameMaster.GetTopDeckCard().GetComponent<Card>().ToggleHighlight(state);

    }
}
