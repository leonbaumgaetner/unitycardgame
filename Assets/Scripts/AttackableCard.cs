using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AttackableCard : MonoBehaviour
{
   


    CardObject handCard;
    


    private bool canAttack = false;

    private UnityAction<AttackLine> CurrentAttackLine;
    AttackLine cardsAttackLine;
    DefendableCard otherCardDefending;
    private void Start()
    {
        handCard = GetComponent<CardObject>();  

        GameManager.OnGameModeChanged += CheckGameMode;

        CurrentAttackLine += SetCurrentAttackLine;
    }

    public void SetDefendableCard(DefendableCard defendableCard)
    {
        otherCardDefending = defendableCard;
    }

    public DefendableCard GetDefendableCard()
    {
        return otherCardDefending;
    }

    private void OnDestroy()
    {
        GameManager.OnGameModeChanged -= CheckGameMode;

        CurrentAttackLine -= SetCurrentAttackLine;
    }

    void CheckGameMode(GAME_MODE gAME_MODE)
    {
        if(gAME_MODE == GAME_MODE.ATTACKING)
        {
            SetAttackable(true);
        }
        else
        {
            SetAttackable(false);
        }
    }

    public void OnMouseDown()
    {
        if (handCard.thisCardsDeck == DeckType.CENTER_DECK && canAttack)
        {
            if(cardsAttackLine)
            {
                Destroy(cardsAttackLine.gameObject);
            }
           // Debug.Log("Trying to draw line");
            AttackManager.LineForAttackableCard?.Invoke(this, CurrentAttackLine);
        }
    }

   



    public void SetAttackable(bool state)
    {
        canAttack = state;
    }

    private void SetCurrentAttackLine(AttackLine line)
    {
        cardsAttackLine = line;
    }

}
