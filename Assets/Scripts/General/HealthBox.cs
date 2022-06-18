using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
public class HealthBox : MonoBehaviour
{
    public PLAYER_TYPE thisPlayerType;

    public TextMeshProUGUI healthText;
    public TextMeshProUGUI deductText;

    public Transform refGotoPoint;
    public void ShowTextDeduction(int amount)
    {
        deductText.gameObject.SetActive(true);
        deductText.text = "" + amount;
        deductText.transform.localPosition = Vector3.zero;
        deductText.transform.DOMove(refGotoPoint.position, 0.6f).OnComplete(()=> deductText.gameObject.SetActive(false));
    }
}
