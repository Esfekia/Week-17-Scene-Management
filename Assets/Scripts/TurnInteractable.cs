using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;

[Serializable]
public class TurnEvent : UnityEvent<float> { };

public class TurnInteractable : XRBaseInteractable
{
    XRBaseInteractor m_interactor = null;

    Coroutine m_turn = null;

    // the thing that it is going to update is the turn angle
    [HideInInspector]
    public float turnAngle = 0.0f;

    Vector3 m_startingRotation = Vector3.zero;

    Quaternion GetLocalRotation(Quaternion targetWorld)
    {
        // invert the targetWorld and multiply it by the current objects world rotation
        // that gives us the rotation of the target in terms of the local rotation of this transform
        return Quaternion.Inverse(targetWorld) * transform.rotation;
    }

    public UnityEvent onTurnStart = new UnityEvent();
    public UnityEvent onTurnStop = new UnityEvent();

    public TurnEvent onTurnUpdate = new TurnEvent();

    void StartTurn()
    {
        if (m_turn != null)
        {
            StopCoroutine(m_turn);
        }
        // use the interactors transfrom where the hand currently is when it is interacting with this object
        Quaternion localRotation = GetLocalRotation(m_interactor.transform.rotation);

        // and the starting rotation will just be the Euler version of that
        m_startingRotation = localRotation.eulerAngles;

        //before we start the coroutine below, lets hit this event for onTurnStart
        onTurnStart?.Invoke();

        m_turn = StartCoroutine(UpdateTurn());
    }

    void StopTurn()
    {
        if (m_turn != null)
        {
            StopCoroutine(m_turn);

            // right after we stop the coroutine, lets invoke onTurnStop
            onTurnStop?.Invoke();

            m_turn = null;
        }
    }

    IEnumerator UpdateTurn()
    {
        while(m_interactor != null)
        {
            // get the local rotation agian everytime we are updating this coroutine
            Quaternion localRotation = GetLocalRotation(m_interactor.transform.rotation);

            // the angle that is happening now
            turnAngle = m_startingRotation.y - localRotation.eulerAngles.y;

            // as we update, lets invoke onTurnUpdate, passing the turn angle.
            onTurnUpdate?.Invoke(turnAngle);

            yield return null;
        }
        
    }
    
    // (same as DragInteractable.cs) when an interactor interacts with this draggable object, we can just tie into the event right here:
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        // first catch our interactor
        m_interactor = args.interactorObject as XRBaseInteractor;

        // then start dragging
        StartTurn();
        base.OnSelectEntered(args);

    }

    // and do the same thing when select is exited / handle is let go.
    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        StopTurn();
        // clear out the interactor once the interaction is over
        m_interactor = null;

        base.OnSelectExited(args);
    }
}
