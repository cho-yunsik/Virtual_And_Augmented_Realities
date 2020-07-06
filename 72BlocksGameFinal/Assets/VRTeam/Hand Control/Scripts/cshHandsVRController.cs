using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cshHandsVRController : MonoBehaviour
{
    public OVRInput.Controller o_controller;
    // OVRInput Hand Button : middle finger
    private OVRInput.Button o_middleButton = OVRInput.Button.PrimaryHandTrigger;
    // OVRInput Hand Button : index finger
    private OVRInput.Button o_indexButton = OVRInput.Button.PrimaryIndexTrigger;

    private bool middleButtonDown;    // middle finger button down
    private bool middleButtonUp;      // middle finger button up
    private bool indexButtonDown; // index finger button down
    private bool indexButtonUp; // index finger button up

    // Hand Animator Control Variable
    private Animator animHand;
    public int handState = -1;

    private bool bHandMotionOn = false;

    void Awake()
    {
        animHand = gameObject.GetComponent<Animator>();
    }


    // Start is called before the first frame update
    void Start()
    {
        handState = -1;
    }

    void GetVrInput()
    {
        // middle finger button down or up
        middleButtonDown = OVRInput.Get(o_middleButton, o_controller);
        // index finger button down or up
        indexButtonDown = OVRInput.Get(o_indexButton, o_controller);

        if (middleButtonDown)
        {
            bHandMotionOn = true;
            
            if (OVRInput.Get(OVRInput.Touch.Two, o_controller))
            {
                if (indexButtonDown)
                {
                    animHand.SetFloat("Hands", 3.0f);
                    //Fist
                    handState = 3;
                }
                else
                {
                    animHand.SetFloat("Hands", 1.0f);
                    //Point
                    handState = 1;
                }
            }else if (indexButtonDown)
            {
                animHand.SetFloat("Hands", 2.0f);
                //Thumb
                handState = 2;
            }
            else
            {
                animHand.SetFloat("Hands", 0.0f);
                //Gun
                handState = 0;
            }

        }else 
        {
            bHandMotionOn = false;
            handState = -1;
        }

        animHand.SetBool("Motion", bHandMotionOn);


        /*
        if (triggerButtonDown)
        {
            // Fist state
            animHand.SetInteger("iHandState", 1);
            iHandState = 1;
            // grab only in fist state
            if (!grabbing) GrabObject();
        }
        else if (gripButtonDown)
        {
            // Index state
            animHand.SetInteger("iHandState", 2);
            iHandState = 2;
        }
        else
        {
            // Open state
            animHand.SetInteger("iHandState", 0);
            iHandState = 0;
            // drop only in open state
            if (grabbing) DropObject();
        }
        */

    }

    // Update is called once per frame
    void Update()
    {
        GetVrInput();
    }
}
