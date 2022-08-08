using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;

// this will use regular collision system rather than our interactor/interactable system
public class PushButton : MonoBehaviour
{
    public UnityEvent onPressed = new UnityEvent();
    public UnityEvent onReset = new UnityEvent();

    public UnityEvent onInteractionStart = new UnityEvent();
    public UnityEvent onInteractionEnd = new UnityEvent();

    // how deep the button can go when pressed
    [Min(0.01f)]
    public float depressionDepth = 0.015f;

    [Min(0.0001f)]
    public float pressThreshold = 0.001f;

    [Min(0.0001f)]
    public float resetThreshold = 0.001f;

    [Min(0.01f)]
    public float returnSpeed = 1.0f;

    float m_currentPressDepth = 0.0f;
    private float m_yMax = 0.0f; // resting position
    private float m_yMin = 0.0f; // all the way pressed in+

    bool m_wasPressed = false;


    
    // use this list to add the capsule colliders we put in the fingers/hands, as they touch and leave the button's collider.
    List<Collider> m_currentColliders = new List<Collider>();

    XRBaseInteractor m_interactor = null;
    
    // Start is called before the first frame update
    void Start()
    {
        // set our max range for our button (current - minimum)
        m_yMax = transform.localPosition.y;
    }

    void SetMinRange()
    {
        // full range
        m_yMin = m_yMax - depressionDepth;
    }

    void SetHeight(float newHeight)
    {
        Vector3 currentPosition = transform.localPosition;
        currentPosition.y = newHeight;

        // clamp it within our min and maxrange
        currentPosition.y = Mathf.Clamp(currentPosition.y, m_yMin, m_yMax);

        transform.localPosition = currentPosition;
    }

    bool isPressed()
    {
        // as long as we are greater than the minimum and within the "press zone"
        return transform.localPosition.y >= m_yMin && transform.localPosition.y <= m_yMin + pressThreshold;
    }

    bool isReset()
    {
        // whether button returned to the upper threshold range and is below the very maximum
        return transform.localPosition.y >= m_yMax - resetThreshold && transform.localPosition.y <= m_yMax;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_interactor != null)
        {
            // get the current depth of our interactor every update
            float newPressHeight = GetPressedDepth(m_interactor.transform.position);
            
            // get height of where interactor is at. start at zero and move up and down based on where the interactor currently is
            float deltaHeight = m_currentPressDepth - newPressHeight;

            // use the deltaHeight to get our new "pressed" position
            float newPressedPosition = transform.localPosition.y - deltaHeight;

            // figure out if we reached a position that results in a button press
            SetHeight(newPressedPosition);

            if (!m_wasPressed && isPressed())
            {
                // we pressed the button!
                onPressed?.Invoke();

                m_wasPressed = true;
            }

            m_currentPressDepth = newPressHeight;
        }
        else
        {
            // this is when we need to release the height of the button back to the original resting place, which is the m_yMax
            
            //as long as the button is not in the resting state
            if (!Mathf.Approximately(transform.localPosition.y, m_yMin))
            {
                float returnHeight = Mathf.MoveTowards(transform.localPosition.y, m_yMax, Time.deltaTime * returnSpeed);
                SetHeight(returnHeight);
            }
        }

        // can also happen outside of the press, i.e. on the button's way up to its resting position
        if (m_wasPressed && isReset())
        {
            onReset?.Invoke();
            m_wasPressed = false;
        }


    }
    // we want to be calculating all the positions how the button is being pressed in local space of the button itself.
    float GetPressedDepth(Vector3 interactorWorldPosition)
    {
        return transform.parent.InverseTransformPoint(interactorWorldPosition).y;
    }

    private void OnTriggerEnter(Collider other)
    {
        XRBaseInteractor interactor = other.GetComponentInParent<XRBaseInteractor>();
        
        // for this, we are looking for the non-trigger colliders, the capsule colliders we made inside our fingers/hands!
        if (interactor != null && !other.isTrigger)
        {
            // remember, there can be multiple colliders from fingers/hands touching the button
            m_currentColliders.Add(other);

            // check if we are starting a new button press and if so set the beginning of the button press
            if (m_interactor == null)
            {
                m_interactor = interactor;
                SetMinRange();
                m_currentPressDepth = GetPressedDepth(m_interactor.transform.position);
                onInteractionStart?.Invoke();
            }
        }

    }

    void EndPress()
    {
        // to reset our state
        m_currentColliders.Clear(); // just in case
        m_currentPressDepth = 0.0f; // reset press
        m_interactor = null; // no more interaction
    }

    // ending the interaction

    private void OnTriggerExit(Collider other)
    {
        if (m_currentColliders.Contains(other))
        {
            m_currentColliders.Remove(other);
            if (m_currentColliders.Count == 0)
            {
                onInteractionEnd?.Invoke();
                EndPress();
            }
        }
    }
}
