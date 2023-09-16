using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{

    private bool isSelectedTableCardsLayed = false;
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
        if (isSelectedTableCardsLayed)
        {
            return;
        }
        if (GameMaster.Singleton.playerSelectedTableCards)
        {
            isSelectedTableCardsLayed = true;
            for (int i = 0; i < 3; i++)
            {
                ulong id = NetworkManager.Singleton.LocalClientId;
                CardModel cardModel = gameMaster.selectedTableCards[i].GetComponent<CardModel>();
                SetSelectedTableCards_ServerRpc(id, cardModel.cardClass, cardModel.cardFlavour.ToString(), i);
            }
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
    void SetSelectedTableCards_ServerRpc(ulong id, int cardClass, string cardFlavor, int number)
    {
        DealSelectedTableCard_ClientRpc(id, cardClass, cardFlavor, number);
    }


    [ClientRpc]
    void DealSelectedTableCard_ClientRpc(ulong id, int cardClass, string cardFlavor, int number)
    {
        if (id != NetworkManager.Singleton.LocalClientId)
        {
            
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    void DealHands_ServerRpc()
    {
        Debug.Log("Finished Dealing Deck");
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
                Debug.Log( "I GOT " + cardClass + cardFlavor);
            }
        }
        else
        {
            gameMaster.DealOpponentTableCard(number);
            Debug.Log( "Opponent GOT " + cardClass + cardFlavor);
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
                Debug.Log( "I GOT " + cardClass + cardFlavor);
            }
        }
        else
        {
            gameMaster.DealOpponentSelectionCard();
            Debug.Log( "Opponent GOT " + cardClass + cardFlavor);
        }
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
