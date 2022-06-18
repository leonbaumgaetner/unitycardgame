using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class TargetLine : MonoBehaviour
{
    public LineRenderer lineRenderer;

    public Transform startPointRef, endPointRef;

    public bool isLineSet = false;

    private LayerMask layerMask;

    public UnityAction<GameObject> OnTargetCardClicked;

    private void Start()
    {
        layerMask = LayerMask.GetMask("backwall");
    }

    

    private void OnDestroy()
    {
        OnTargetCardClicked = null;
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

         }
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
}
