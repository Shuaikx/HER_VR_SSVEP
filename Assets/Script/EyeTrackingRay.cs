using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class EyeTrackingRay : MonoBehaviour
{
    [SerializeField]
    private GameObject cursor;
    [SerializeField]// Start is called before the first frame update
    private float rayDistance = 1.0f;
    [SerializeField]
    private float rayWidth = 0.01f;

    [SerializeField]
    private LayerMask  layerToInclude;

    [SerializeField]
    private Color rayCorlorDefultState = Color.red;
    [SerializeField]
    private Color rayCorlorHoverState = Color.yellow;

    private LineRenderer lineRenderer;
    private List<EyeInteractable> eyeInteractables = new List<EyeInteractable>();
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        SetupRay();
    }

    void SetupRay()
    {
        lineRenderer.useWorldSpace = false;
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = rayWidth;
        lineRenderer.endWidth = rayWidth;
        lineRenderer.startColor = rayCorlorDefultState;
        lineRenderer.endColor = rayCorlorDefultState;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, new Vector3(transform.position.x, transform.position.y, transform.position.z+rayDistance));
    }

    void FixedUpdate() 
    {
        RaycastHit hit;
        Vector3 rayCastDirection = transform.TransformDirection(Vector3.forward) * rayDistance;
        if(Physics.Raycast(transform.position, rayCastDirection, out hit, Mathf.Infinity, layerToInclude))
        {
            UnSelect();
            lineRenderer.startColor = rayCorlorHoverState;
            lineRenderer.endColor = rayCorlorHoverState;
            var eyeInteractable = hit.transform.GetComponent<EyeInteractable>();
            eyeInteractables.Add(eyeInteractable);
            eyeInteractable.IsHovered = true;
        }
        else
        {
            lineRenderer.startColor = rayCorlorDefultState;
            lineRenderer.endColor = rayCorlorDefultState;
            UnSelect(true);
        }
        cursor.transform.position = transform.position + rayCastDirection.normalized * 2.0f;
    }

    void UnSelect(bool clear = false)//如果不赋值则默认为false
    {
        foreach(var interactable in eyeInteractables)
        {
            interactable.IsHovered = false;
        }
        if(clear)
        {
            eyeInteractables.Clear();//移除所有元素
        }
    }
}
