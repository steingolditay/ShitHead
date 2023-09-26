using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{

    private GameMaster gameMaster;

    private void Start()
    {
        gameMaster = GameMaster.Singleton;
        gameMaster.SetPlayerController(this);
        if (!IsHost)
        {
            DealDeck_ServerRpc();
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (GameMaster.Singleton.playerSelectedTableCards)
        {
            GameMaster.Singleton.playerSelectedTableCards = false;
            ulong id = GetId();
            for (int i = 0; i < 3; i++)
            {
                Card card = gameMaster.selectedTableCards[i].GetComponent<Card>();
                SetOpponentSelectedTableCards_ServerRpc(GetId(), card.GetCardClass(), card.GetCardFlavour().ToString(), i);
            }

            AddPlayerReady_ServerRpc(id);
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    void DealDeck_ServerRpc()
    {
        gameMaster.ShuffleCards();
        DealDeck_ClientRpc();
    }
    
    [ClientRpc]
    void DealDeck_ClientRpc()
    {
        if (!IsOwner) return;
        Task putCardsInDeck = new Task(gameMaster.PutCardInDeck());
        putCardsInDeck.Finished += delegate
        {
            if (IsHost)
            {
                DealHands_ServerRpc();
            }
        };
    }
    

    [ServerRpc(RequireOwnership = false)]
    void SetOpponentSelectedTableCards_ServerRpc(ulong id, int cardClass, string cardFlavor, int number)
    {
        DealOpponentSelectedTableCard_ClientRpc(id, cardClass, cardFlavor, number);
    }
    
    [ClientRpc]
    void DealOpponentSelectedTableCard_ClientRpc(ulong id, int cardClass, string cardFlavor, int number)
    {
        if (id != GetId())
        {
            CardModel cardModel = new CardModel(cardClass, Utils.GetCardFlavourForString(cardFlavor));
            gameMaster.SetOpponentSelectedTableCards(cardModel, number);
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    void DealHands_ServerRpc()
    {
        StartCoroutine(DealHandsToClients());
    }

    [ClientRpc]
    void DealTableCard_ClientRpc(ulong id, int cardClass, string cardFlavor, int number)
    {
        if (id == GetId())
        {
            CardFlavour flavour;
            if (Enum.TryParse(cardFlavor, out flavour))
            {
                gameMaster.DealPlayerTableCard(new CardModel(cardClass, flavour), number);
            }
        }
        else
        {
            gameMaster.DealOpponentTableCard(number);
        }
    }
    
    [ClientRpc]
    void DealSelectionCard_ClientRpc(ulong id, int cardClass, string cardFlavor, int number)
    {
        if (number == 1)
        {
            gameMaster.ToggleSelectCardsDialog(true);
        }
        
        if (id == GetId())
        {
            CardFlavour flavour;
            if (Enum.TryParse(cardFlavor, out flavour))
            {
                gameMaster.DealPlayerSelectionCard(new CardModel(cardClass, flavour), number);
            }
        }
        else
        {
            gameMaster.OpponentDrawCardFromDeck();
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    void AddPlayerReady_ServerRpc(ulong id)
    {
        AddPlayerReady_ClientRpc(id);
    }
    [ClientRpc]
    void AddPlayerReady_ClientRpc(ulong id)
    {
        gameMaster.playersReady++;
        if (!IsHost) return;

        if (gameMaster.playersReady == 1)
        {
            gameMaster.firstPlayerToStart = id;
        }
        if (gameMaster.playersReady == 2)
        {
            SetFirstCardFromDeck_ServerRpc();
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    void SetPlayerTurn_ServerRpc(ulong playerId)
    {
        SetPlayerTurn_ClientRpc(playerId);
    }

    [ClientRpc]
    void SetPlayerTurn_ClientRpc(ulong playerId)
    {
        bool isMyTurn = GetId() == playerId;
        gameMaster.SetCurrentPlayerTurn(isMyTurn ? GameMaster.PlayerTurn.Player : GameMaster.PlayerTurn.Opponent);
    }
    
    [ServerRpc(RequireOwnership = false)]
    void SetFirstCardFromDeck_ServerRpc()
    {
        CardModel cardModel = gameMaster.GetTopDeckCardModel();
        SetFirstCardFromDeck_ClientRpc(cardModel.cardClass, cardModel.cardFlavour.ToString());
    }
    
    [ClientRpc] 
    void SetFirstCardFromDeck_ClientRpc(int cardClass, string cardFlavor)
    {
        gameMaster.SetFirstCardFromDeck(new CardModel(cardClass, Utils.GetCardFlavourForString(cardFlavor)));
        SetPlayerTurn_ServerRpc(gameMaster.firstPlayerToStart);

    }

    [ServerRpc(RequireOwnership = false)]
    void SetOpponentHandCardInPile_ServerRpc(ulong id, int cardClass, string cardFlavor)
    {
        SetOpponentHandCardInPile_ClientRpc(id, cardClass, cardFlavor);
    }

    [ClientRpc]
    void SetOpponentHandCardInPile_ClientRpc(ulong id, int cardClass, string cardFlavour)
    {
        if (id != GetId())
        {
            Transform cardTransform = gameMaster.GetOpponentHandCard();
            cardTransform.GetComponent<Card>().SetCardModel(new CardModel(cardClass, cardFlavour));
            gameMaster.SetOpponentHandCardInPile(cardTransform);
        }
    }

    
    [ServerRpc(RequireOwnership = false)]
    void TurnFinished_ServerRpc(ulong id)
    {
        IReadOnlyList<ulong> clientIds = NetworkManager.Singleton.ConnectedClientsIds;
        foreach (ulong clientId in clientIds)
        {
            if (clientId != id)
            {
                SetPlayerTurn_ClientRpc(clientId);
                return;
            }
        }
    }
    
    
    [ServerRpc(RequireOwnership = false)]
    void DrawMissingCards_ServerRpc(ulong id)
    {
        CardModel cardModel = gameMaster.GetTopDeckCardModel();
        DrawMissingCards_ClientRpc(id, cardModel.cardClass, cardModel.cardFlavour.ToString());
    }

    [ClientRpc]
    void DrawMissingCards_ClientRpc(ulong id, int cardClass, string cardFlavour)
    {
        if (id == GetId())
        {
            gameMaster.PlayerDrawMissingCardsFromDeck(new CardModel(cardClass, cardFlavour));
        }
        else
        {
            gameMaster.OpponentDrawCardFromDeck();
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    void DrawCard_ServerRpc(ulong id)
    {
        CardModel cardModel = gameMaster.GetTopDeckCardModel();
        DrawCard_ClientRpc(id, cardModel.cardClass, cardModel.cardFlavour.ToString());
    }

    [ClientRpc]
    void DrawCard_ClientRpc(ulong id, int cardClass, string cardFlavour)
    {
        if (id == GetId())
        {
            gameMaster.PlayerDrawMissingCardsFromDeck(new CardModel(cardClass, cardFlavour));
        }
        else
        {
            gameMaster.OpponentDrawCardFromDeck();
        }
    }


    public void OnPutCardInPile(Card card)
    {
        SetOpponentHandCardInPile_ServerRpc(GetId(), card.GetCardClass(), card.GetCardFlavour().ToString());
    }

    public void OnDrawCard()
    {
        ulong id = GetId();
        DrawCard_ServerRpc(id);
        TurnFinished_ServerRpc(id);

    }

    private void OnDrawMissingCardsFromDeck(ulong id)
    {
        DrawMissingCards_ServerRpc(id);
    }

    public void OnTurnFinished()
    {
        ulong id = GetId();
        OnDrawMissingCardsFromDeck(id);
        TurnFinished_ServerRpc(id);
    }

    private ulong GetId()
    {
        return NetworkManager.Singleton.LocalClientId;
    }

    
    private IEnumerator DealHandsToClients()
    {
        IReadOnlyList<ulong> clientIds = NetworkManager.Singleton.ConnectedClientsIds;
        for (int i = 0; i < clientIds.Count; i++)
        {
            CardModel cardModel = gameMaster.GetTopDeckCardModel();
            DealTableCard_ClientRpc(clientIds[i], cardModel.cardClass, cardModel.cardFlavour.ToString(), 1);
            yield return new WaitForSeconds(0.3f);
        }

        for (int i = 0; i < clientIds.Count; i++)
        {
            CardModel cardModel = gameMaster.GetTopDeckCardModel();
            DealTableCard_ClientRpc(clientIds[i], cardModel.cardClass, cardModel.cardFlavour.ToString(), 2);
            yield return new WaitForSeconds(0.3f);
        }
        for (int i = 0; i < clientIds.Count; i++)
        {
            CardModel cardModel = gameMaster.GetTopDeckCardModel();
            DealTableCard_ClientRpc(clientIds[i], cardModel.cardClass, cardModel.cardFlavour.ToString(), 3);
            yield return new WaitForSeconds(0.3f);
        }
        
        for (int i = 0; i < clientIds.Count; i++)
        {
            CardModel cardModel = gameMaster.GetTopDeckCardModel();
            DealSelectionCard_ClientRpc(clientIds[i], cardModel.cardClass, cardModel.cardFlavour.ToString(), 1);
            yield return new WaitForSeconds(0.3f);
        }
        for (int i = 0; i < clientIds.Count; i++)
        {
            CardModel cardModel = gameMaster.GetTopDeckCardModel();
            DealSelectionCard_ClientRpc(clientIds[i], cardModel.cardClass, cardModel.cardFlavour.ToString(), 2);
            yield return new WaitForSeconds(0.3f);
        }
        for (int i = 0; i < clientIds.Count; i++)
        {
            CardModel cardModel = gameMaster.GetTopDeckCardModel();
            DealSelectionCard_ClientRpc(clientIds[i], cardModel.cardClass, cardModel.cardFlavour.ToString(), 3);
            yield return new WaitForSeconds(0.3f);
        }
        for (int i = 0; i < clientIds.Count; i++)
        {
            CardModel cardModel = gameMaster.GetTopDeckCardModel();
            DealSelectionCard_ClientRpc(clientIds[i], cardModel.cardClass, cardModel.cardFlavour.ToString(), 4);
            yield return new WaitForSeconds(0.3f);
        }
        for (int i = 0; i < clientIds.Count; i++)
        {
            CardModel cardModel = gameMaster.GetTopDeckCardModel();
            DealSelectionCard_ClientRpc(clientIds[i], cardModel.cardClass, cardModel.cardFlavour.ToString(), 5);
            yield return new WaitForSeconds(0.3f);
        }
        for (int i = 0; i < clientIds.Count; i++)
        {
            CardModel cardModel = gameMaster.GetTopDeckCardModel();
            DealSelectionCard_ClientRpc(clientIds[i], cardModel.cardClass, cardModel.cardFlavour.ToString(), 6);
            yield return new WaitForSeconds(0.3f);
        }
        yield return null;
    }
}
