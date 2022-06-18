using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using DG.Tweening;
public class TurnManager : MonoBehaviour
{
    public Turn thisPlayersTurn = Turn.LOCAL;

    public static UnityAction<Turn> OnTurnChangedTo;


    public TextMeshProUGUI whoseTurnText;
    public static bool isPlayerTurn = false;

    public static UnityAction<Turn> FinishedTurn;

    public static PHASE currentPhase = PHASE.NORMAL;

    
    private void Start()
    {
        GameManager.OnLocalCardSet += SetLocalPlayer;


        OnTurnChangedTo += ChangingTurn;
        FinishedTurn += OnFinishedTurn;
        //Invoke("StartTurn", 3.8f);
    }

    private void OnDestroy()
    {
        GameManager.OnLocalCardSet -= SetLocalPlayer;
        OnTurnChangedTo -= ChangingTurn;
    }

    private void StartTurn()
    {
        int t = Random.Range(0, 2);
        if (t == 0)
            isPlayerTurn = true;
        else
            isPlayerTurn = false;

       // SwitchTurn();
    }


    private void SwitchTurn()
    {        
        isPlayerTurn = !isPlayerTurn;

        if (isPlayerTurn)
        {
            // ChangingTurn(Turn.LOCAL);
            OnTurnChangedTo?.Invoke(Turn.LOCAL);
        }
        else
        {
            // ChangingTurn(Turn.ENEMY);
            OnTurnChangedTo?.Invoke(Turn.ENEMY);

        }
            
    }




    private void OnFinishedTurn(Turn t)
    {
        SwitchTurn();
    }

    private void ChangingTurn(Turn currentTurn)
    {
        thisPlayersTurn = currentTurn;

        if (currentTurn == Turn.ENEMY)
        {
            whoseTurnText.text = "OPPONENT'S TURN";
            whoseTurnText.gameObject.SetActive(true);
            whoseTurnText.transform.localScale = Vector3.one;

            whoseTurnText.transform.DOScale(new Vector3(2, 2, 2), 2).OnComplete(()=> {

                whoseTurnText.gameObject.SetActive(false);
                isPlayerTurn = false;
                //OnTurnChangedTo?.Invoke(Turn.ENEMY);
                
            } );
        }
        else
        {
            whoseTurnText.text = "YOUR TURN";
            whoseTurnText.gameObject.SetActive(true);
            whoseTurnText.transform.localScale = Vector3.one;

            whoseTurnText.transform.DOScale(new Vector3(2, 2, 2), 2).OnComplete(() => {

                whoseTurnText.gameObject.SetActive(false);
                isPlayerTurn = true;
               // OnTurnChangedTo?.Invoke(Turn.LOCAL);
            });
        }
    }
    private CardPlayer localCardPlayer;
    public void SetLocalPlayer(CardPlayer cardPlayer)
    {
        localCardPlayer = cardPlayer;
    }

}
