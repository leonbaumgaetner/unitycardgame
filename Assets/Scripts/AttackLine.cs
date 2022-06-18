using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AttackLine : MonoBehaviour
{
    public LineRenderer lineRenderer;

    public Transform startPointRef, endPointRef;

    bool isLineSet = false;

    private LayerMask layerMask;

    //the card which is attacking
    private AttackableCard forCard;
    private void Start()
    {
        layerMask = LayerMask.GetMask("backwall");
    }

    public void Update()
    {
        lineRenderer.SetPosition(0, startPointRef.position);
        lineRenderer.SetPosition(1, endPointRef.position);

        if(isLineSet == false)
        {
            //follow mouse cursor
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                Vector3 followPoint = hit.point;
                followPoint.z -= 20;
                endPointRef.position = followPoint;
                
            }

            if(Input.GetMouseButtonUp(0))
            {
                
                Destroy(this.gameObject);
            }
        }
    }


    private void OnDestroy()
    {
        UIManager.LineDestroyed?.Invoke();
    }

    public void SetStart(Transform t)
    {
        startPointRef.transform.position = t.position;
    }

    public void SetEndPoint(Transform t)
    {
        isLineSet = true;
        endPointRef.position = t.position;
        //forCard.SetAttackable(false);
    }

    public void ClickedUp()
    {
        isLineSet = true;
        AttackManager.FinishedSettingLine?.Invoke(this);

    }



    public void SetLineOwner(AttackableCard cardObject)
    {
        forCard = cardObject;
    }

    public Transform GetOwner()
    {
        return forCard.transform;
    }

}
