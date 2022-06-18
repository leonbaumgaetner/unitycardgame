using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefendLine : MonoBehaviour
{
    public LineRenderer lineRenderer;

    public Transform startPointRef, endPointRef;

    public bool isLineSet = false;

    private LayerMask layerMask;

    private DefendableCard forCard;
    private DefendableCard toCard;
    private void Start()
    {
        layerMask = LayerMask.GetMask("backwall");
    }

    public DefendableCard GetDefendableOwner()
    {
        return forCard;
    }

    public DefendableCard GetAttackingOwnder()
    {
        return toCard;
    }

    public void SetAttackingOwner(DefendableCard defendableCard)
    {        
        toCard = defendableCard;
        lineRenderer.SetPosition(1, defendableCard.transform.position);
    }

    public void Update()
    {
        lineRenderer.SetPosition(0, startPointRef.position);
        lineRenderer.SetPosition(1, endPointRef.position);

        if (isLineSet == false)
        {
            //follow mouse cursor
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                Vector3 followPoint = hit.point;
                followPoint.z -= 20;
                endPointRef.position = followPoint;

            }

            if (Input.GetMouseButtonUp(0))
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
       // isLineSet = true;
       // DefendManager.FinishedSettingLine?.Invoke(this);

    }



    public void SetLineOwner(DefendableCard cardObject)
    {
        forCard = cardObject;
    }

    public Transform GetOwner()
    {
        return forCard.transform;
    }
}
