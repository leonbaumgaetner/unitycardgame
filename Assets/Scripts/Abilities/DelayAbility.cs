using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class DelayAbility : MonoBehaviour
{
    public int delayCount = 0;

    public TextMeshProUGUI delayText;

    private void Start()
    {
        TurnManager.OnTurnChangedTo += TurnChanged;
    }

    private void OnDisable()
    {
        TurnManager.OnTurnChangedTo -= TurnChanged;
    }

    private void TurnChanged(Turn newTurn)
    {
        var playerType = GetComponentInParent<PlayerHandCard>().thisPlayerType;

        if(newTurn == Turn.LOCAL && playerType == PLAYER_TYPE.LOCAL)
        {
            delayCount--;
        }
        else if(newTurn == Turn.ENEMY && playerType == PLAYER_TYPE.ENEMY)
        {
            delayCount--;
        }

        UpdateDelay(delayCount);

        if(delayCount == 0)
        {
            GetComponentInParent<PlayingCard>().SetCardState?.Invoke(true);
            Destroy(this.gameObject);
        }

       
    }

    public void UpdateDelay(int dc)
    {       
        delayText.text = "DELAY " + dc;
    }

    public void SetDelay(int delay)
    {
        delayCount = delay + 1;
        UpdateDelay(delay);
    }
}
