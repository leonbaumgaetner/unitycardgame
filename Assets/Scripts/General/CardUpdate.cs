using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
public class CardUpdate : MonoBehaviour
{
    public TextMeshPro dataText;
    float offset = 17;
    float duration = 2f;
    public void SetText(string s, Color color)
    {
        dataText.text = s;
        dataText.color = color;
        float currentY = transform.position.y;
        Vector3 pos = transform.position;
        pos.z = 60;
        transform.position = pos;
        transform.DOMoveY(currentY + offset, duration).OnComplete(()=>Destroy(this.gameObject));
      //  dataText.text.DOFade(0, 2);
    }
}
