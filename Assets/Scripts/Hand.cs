using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine.XR.Interaction.Toolkit;

public enum HandType
{
    Left,
    Right
};

public class Hand : MonoBehaviour
{

    public HandType type = HandType.Left;

    // public getter, private setter for our isHidden boolean
    public bool isHidden { get; private set; } = false;

    // we want to get updated when this device's action updates so we create a binding to look into this input
    public InputAction trackedAction = null;

    // find out if the grip button is held or not

    public InputAction gripAction = null;
    public InputAction triggerAction = null;

    public Animator handAnimator = null;

    private int m_gripAmountParameter = 0;
    private int m_pointAmountParameter = 0;

    bool m_isCurrentlyTracked = false;

    List<Renderer> m_currentRenderers = new List<Renderer>();

    // an array to store all colliders so that we can use this script to enable or disable them
    Collider[] m_colliders = null;

    public bool isCollisionEnabled { get; private set; } = false;

    // need an interactor with HandControl script
    public XRBaseInteractor interactor = null;

    private void Awake()
    {
        if (interactor == null)
        {
            interactor = GetComponentInParent<XRBaseInteractor>();
        }
    }

    private void OnEnable()
    {
        interactor.selectEntered.AddListener(OnGrab);
        interactor.selectExited.AddListener(OnRelease);
    }

    private void OnDisable()
    {
        interactor.selectEntered.RemoveListener(OnGrab);
        interactor.selectExited.RemoveListener(OnRelease);
    }


    // Start is called before the first frame update
    void Start()
    {
        //grab all colliders, look thru all children. however use a lambda function to filter the results for triggers
        m_colliders = GetComponentsInChildren<Collider>().Where(childCollider => !childCollider.isTrigger).ToArray();
        
        // need to enable it before it can be used. It will give us a float
        trackedAction.Enable();

        m_gripAmountParameter = Animator.StringToHash("GripAmount");
        m_pointAmountParameter = Animator.StringToHash("PointAmount");
        gripAction.Enable();

        triggerAction.Enable();

        // this will hide the hand in the beginning and the function will populate our m_currentRenderers for use in Show()
        Hide();
    }

    void UpdateAnimations()
    {

        // get the point amount from the pointAction binding
        float pointAmount = triggerAction.ReadValue<float>();
        handAnimator.SetFloat(m_pointAmountParameter, pointAmount);

        // get the grip amount from the gripAction binding
        float gripAmount = gripAction.ReadValue<float>();

        // if the trigger is the only thing down we still are going to use the grip animation
        // if the point amount is zero, we are still just using the grip amount alone
        // if the point amount is one, it gets grip amount added but clamped to 1 
        handAnimator.SetFloat(m_gripAmountParameter, Mathf.Clamp01(gripAmount + pointAmount));
    }

    // Update is called once per frame
    void Update()
    {
        float isTracked = trackedAction.ReadValue<float>();

        // if it is tracked, then lets show our hand
        if (isTracked == 1.0f && !m_isCurrentlyTracked)
        {
            m_isCurrentlyTracked = true;
            Show();
        }
        // if it is no longer tracked but still showing, lets hide it
        else if(isTracked == 0 && m_isCurrentlyTracked)
        {
            m_isCurrentlyTracked = false;
            Hide();
        }
        UpdateAnimations();
    }

    public void Show()
    {
        // currentRenderers got all the skin/meshrenderers added to it in Hide() below!
        foreach (Renderer renderer in m_currentRenderers)
        {
            renderer.enabled = true;                       
        }
        isHidden = false;
        EnableCollisions(true);
    }
    public void Hide()
    {
        m_currentRenderers.Clear();

        // get all the possible skin/meshrenderers in the children and go through each of them
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = false;

            // add it to the currentRenderers list so that you can show them again when they are turned back on
            m_currentRenderers.Add(renderer);
        }
        isHidden = true;
        EnableCollisions(false);
    }

    public void EnableCollisions(bool enabled)
    {
        if (isCollisionEnabled == enabled) return;
        
        isCollisionEnabled = enabled;
        foreach(Collider collider in m_colliders)
        {
            collider.enabled = isCollisionEnabled;
        }
    }

    void OnGrab(SelectEnterEventArgs grabbedObject)
    {
        HandControl ctrl = grabbedObject.interactableObject.transform.gameObject.GetComponent<HandControl>();
        if (ctrl != null)
        {
            if (ctrl.hideHand)
            {
                Hide();
            }
        }
    }

    void OnRelease(SelectExitEventArgs releasedObject)
    {
        HandControl ctrl = releasedObject.interactableObject.transform.gameObject.GetComponent<HandControl>();
        if (ctrl != null)
        {
            if (ctrl.hideHand)
            {
                Show();
            }
        }
    }

    public void InteractionCleanup()
    {
        XRDirectInteractor directInteractior = interactor as XRDirectInteractor;
        if (directInteractior != null)
        {
            List<XRBaseInteractable> validTargets = new List<XRBaseInteractable>();
            interactor.GetValidTargets(validTargets);

            //tell the interaction manager that we want to clear this selection from happening 
            interactor.interactionManager.ClearInteractorSelection(interactor);

            // make sure nothing is happening if it is hovered
            interactor.interactionManager.ClearInteractorHover(interactor, validTargets);

            foreach (var target in validTargets)
            {
                if (target.gameObject.scene != interactor.gameObject.scene)
                {
                    // make sure this object will trigger an OnTriggerExit
                    // best way to do that is moving the object out of the scene, really far away.
                    target.transform.position = new Vector3(100, 100, 100);
                }
            }
        }
    }
    
}
