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
            ulong id = NetworkManager.Singleton.LocalClientId;
            for (int i = 0; i < 3; i++)
            {
                Card card = gameMaster.selectedTableCards[i].GetComponent<Card>();
                SetOpponentSelectedTableCards_ServerRpc(id, card.GetCardClass(), card.GetCardFlavour().ToString(), i);
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
        if (id != NetworkManager.Singleton.LocalClientId)
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
        if (id == NetworkManager.Singleton.LocalClientId)
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
        
        if (id == NetworkManager.Singleton.LocalClientId)
        {
            CardFlavour flavour;
            if (Enum.TryParse(cardFlavor, out flavour))
            {
                gameMaster.DealPlayerSelectionCard(new CardModel(cardClass, flavour), number);
            }
        }
        else
        {
            gameMaster.DealOpponentSelectionCard();
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
        bool isMyTurn = NetworkManager.Singleton.LocalClientId == playerId;
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
