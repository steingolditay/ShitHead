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
        putCardsInDeck.Finished += delegate(bool manual)
        {
            if (!manual && IsHost)
            {
                DealHands_ServerRpc();
            }
        };
    }
    
    [ServerRpc(RequireOwnership = false)]
    void DealHands_ServerRpc()
    {
        Debug.Log("Finished Dealing Deck");
        StartCoroutine(DealHandsToClients());


    }
    //
    [ClientRpc]
    void DealBottomCard1_ClientRpc(ulong id, int cardClass, string cardFlavor)
    {
        if (id == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log( "I GOT " + cardClass + cardFlavor);
        }
        else
        {
            Debug.Log( "Opponent GOT " + cardClass + cardFlavor);
        }

    }

    private IEnumerator DealHandsToClients()
    {
        IReadOnlyList<ulong> clientIds = NetworkManager.Singleton.ConnectedClientsIds;
        for (int i = 0; i < clientIds.Count; i++)
        {
            CardModel cardModel = gameMaster.GetDeck()[i];
            DealBottomCard1_ClientRpc(clientIds[i], cardModel.cardClass, cardModel.cardFlavour.ToString());
            
        }

        yield return null;
    }
}
