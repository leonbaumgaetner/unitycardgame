using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
public class DiscardDeck : Deck
{
    public UnityAction<GameObject> DiscardPileCard;


    private void Start()
    {
        DiscardPileCard += DiscardACard;
    }

    public void DiscardACard(GameObject go)
    {
        go.transform.SetParent(transform);
        go.transform.DOLocalMove(Vector3.zero, 0.2f).SetEase(Ease.InOutSine);
        go.transform.DOScale(transform.GetChild(0).localScale, 0.2f);
    }
}
