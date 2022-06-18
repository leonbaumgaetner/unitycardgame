using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
public class RightDeck : Deck
{
   public void AddACard(GameObject go, UnityAction callback)
   {
        go.transform.SetParent(this.transform);
        go.transform.DOLocalMove(Vector3.zero, 0.2f).OnComplete(()=> { go.GetComponent<Collider>().enabled = true; callback?.Invoke(); });
        go.transform.DOScale(Vector3.one, 0.2f);
        go.transform.DORotate(transform.rotation.eulerAngles, 0.2f);

        //CHECK IF THE CARD HAS ANY ABILITY
        var co = go.GetComponent<CardObject>();
        if(co.thisCardData.thisCardAbility != AbilityType.NONE)
        {
            //process the card ability
            AbilitiesManager.ExecuteAbilty?.Invoke(co.thisCardData.thisCardAbility,go);
        }
   }

    public PlayerHandCard GetCardInDeck()
    {
        return GetComponentInChildren<PlayerHandCard>();
    }

    public void ReturnCardToPlayer()
    {
        PlayerInsteraction playerInsteraction = FindObjectOfType<PlayerInsteraction>();
        Transform card = playerInsteraction.GetPlayerDeck(DeckType.RIGHT_SIDE).GetComponentInChildren<PlayerHandCard>().transform;

        playerInsteraction.MoveCardToDeck(DeckType.PLAYER_HAND, card.gameObject, PLAYER_TYPE.ENEMY);
        card.GetComponent<PlayingCard>().ShowCard(false);
    }

    public void ProcessAddedCard()
    {
        //step 1 use resources
       
    }

    
}
