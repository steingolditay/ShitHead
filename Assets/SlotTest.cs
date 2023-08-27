using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotTest : MonoBehaviour
{

    [SerializeField] private Button slotPlayer;
    [SerializeField] private Button slotOpponent;

    
    private GameMaster gameMaster;


    private void Awake()
    {
        slotPlayer.onClick.AddListener(() =>
        {
            gameMaster.SlotPlayer();
        });
        
        slotOpponent.onClick.AddListener(() =>
        {
            gameMaster.SlotOpponent();
        });
    }

    void Start()
    {
        gameMaster = GameMaster.Singleton;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
