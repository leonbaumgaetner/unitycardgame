using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DefendableCard : MonoBehaviour
{
    CardObject handCard;



    private bool canDefend = false;

    private UnityAction<DefendLine> CurrentDefendLine;
    DefendLine cardsDefendLine;
    private void Start()
    {
        handCard = GetComponent<CardObject>();

        GameManager.OnGameModeChanged += CheckGameMode;

        CurrentDefendLine += SetCurrentDefendLine;
    }

    private void SetCurrentDefendLine(DefendLine line)
    {
        cardsDefendLine = line;
    }

    public DefendLine GetCurrentDefendLine()
    {
        return cardsDefendLine;
    }

    private void CheckGameMode(GAME_MODE gAME_MODE)
    {
        if (gAME_MODE == GAME_MODE.DEFENDING)
        {
            SetDefendable(true);
        }
        else
        {
            SetDefendable(false);
        }
    }

    private void SetDefendable(bool state)
    {
        var b = GetComponent<PlayerHandCard>().isActive;
        if(b && state)
        {
            canDefend = state;
        }
        else
        {
            canDefend = false;
        }
        
    }

    public void OnMouseDown()
    {
        if (handCard.thisCardsDeck == DeckType.CENTER_DECK && canDefend && handCard.thisPlayerType == PLAYER_TYPE.LOCAL)
        {
            if (cardsDefendLine)
            {               

                Destroy(cardsDefendLine.gameObject);
                DefendManager.OnDefendLineDeleted?.Invoke();
            }
            // Debug.Log("Trying to draw line");
            DefendManager.LineForDefendableCard?.Invoke(this, CurrentDefendLine);
        }
    }

    public void OnMouseEnter()
    {
        if(handCard.thisCardsDeck == DeckType.CENTER_DECK && handCard.thisPlayerType == PLAYER_TYPE.ENEMY)
        {
            if(GetComponent<PlayerHandCard>().isActive)
            {
                Debug.LogError("defender card up");
                DefendManager.OnCardUp?.Invoke(this);
            }
           
        }
    }


    private void OnDestroy()
    {
        GameManager.OnGameModeChanged -= CheckGameMode;

        CurrentDefendLine -= SetCurrentDefendLine;
    }
}
