using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    public DeckType deckType;
    public PLAYER_TYPE thisPlayerType;


    protected DeckManager deckManager;
    public virtual void Start()
    {
        deckManager = FindObjectOfType<DeckManager>();
    }
}
