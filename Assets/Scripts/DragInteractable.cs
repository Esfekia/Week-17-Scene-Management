using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;

[Serializable]
public class DragEvent : UnityEvent<float> { }

public class DragInteractable : XRBaseInteractable
{
    public Transform startDragPosition = null;
    public Transform endDragPosition = null;
    
    [HideInInspector]//because it will be publicly available to other scripts but we dont want to worry about setting it in ui
    public float dragPercent = 0.0f; // percentage between 0 and 1

    // with protected inherited classes can still access m_interactor
    protected XRBaseInteractor m_interactor = null;

    // to be able to tie into these from the unity editor, like other interactables
    public UnityEvent onDragStart = new UnityEvent();
    public UnityEvent onDragEnd = new UnityEvent();
    public DragEvent onDragUpdate = new DragEvent();

    Coroutine m_drag = null;

    void StartDrag()
    {
        if (m_drag != null)
        {
            StopCoroutine(m_drag);
        }
        m_drag = StartCoroutine(CalculateDrag());
        // as long as there is something to invoke, invoke it (thats what ? means)
        onDragStart?.Invoke();
    }

    void EndDrag()
    {
        if (m_drag != null)
        {
            StopCoroutine(m_drag);
            m_drag = null;
            onDragEnd?.Invoke();
        }
    }

    // see below where the inverselerp is used
    public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
    {
        Vector3 AB = b - a;
        Vector3 AV = value - a;
        // the dot of a to value divided by thedot of the total range
        // gives the normalized 0-1 distance of value between a and b
        return Mathf.Clamp01(Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB));
    }


    // coroutine to do the drag calculation

    IEnumerator CalculateDrag()
    {
        while (m_interactor != null)
        {
            // get a line in local space
            Vector3 line = startDragPosition.localPosition - endDragPosition.localPosition;

            // convert our interactor position to local space
            // move from interactor's worldspace into this object's local space
            Vector3 interactorLocalPosition = startDragPosition.parent.InverseTransformPoint(m_interactor.transform.position);

            // project the interactor position onto the line
            // use line direction as normal and get interactor local position projected on that line
            Vector3 projectedPoint = Vector3.Project(interactorLocalPosition, line.normalized);

            // reverse interpolate that position on the line to get a percentage of how far the drag has moved
            // lerp interpolates between 2 points, we have the opposite (inverselerp above!), the point is somewhere between them.
            dragPercent = InverseLerp(startDragPosition.localPosition, endDragPosition.localPosition, projectedPoint);

            onDragUpdate?.Invoke(dragPercent);

            yield return null;
        }
        
    }
    
    // when an interactor interacts with this draggable object, we can just tie into the event right here:
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        // first catch our interactor
        m_interactor = args.interactorObject as XRBaseInteractor;

        // then start dragging
        StartDrag();
        base.OnSelectEntered(args);
    
    }

    // and do the same thing when select is exited / handle is let go.
    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        EndDrag();
        // clear out the interactor once the interaction is over
        m_interactor = null;

        base.OnSelectExited(args);
    }
}
