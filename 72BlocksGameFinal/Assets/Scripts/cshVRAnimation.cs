using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class cshVRAnimation : MonoBehaviourPun
{
    float horizontalMove;
    float verticalMove;
    public Animator anim;

    public Transform target;
    public OVRInput.Controller controller;
    // Start is called before the first frame update
    void Start()
    {
        if (!photonView.IsMine)
            return;

        anim = GetComponent<Animator>();
      
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine)
            return;

        Vector3 project = Vector3.Project(target.forward, Vector3.up);
        Vector3 look = target.forward - project;
        look = look.normalized;

        transform.LookAt(transform.position + look);


        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 moveHorizontal = Vector3.right * h;
        Vector3 moveVertical = Vector3.forward * v;
        Vector3 velocity = (moveHorizontal + moveVertical).normalized;

        transform.LookAt(transform.position + velocity); //
        //transform.Translate( velocity* Time.deltaTime*0.1f, Space.World);
        horizontalMove = Input.GetAxisRaw("Horizontal");
        verticalMove = Input.GetAxisRaw("Vertical");


        AnimationUpdate();
    }
    public void AnimationUpdate()
    {
        //Vector2 primaryAxis = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, controller);
        //if (Mathf.Abs(primaryAxis.x) > 0 || Mathf.Abs(primaryAxis.y) > 0)
        //{
        //    anim.SetBool("isRun", true);
        //}
        //else
        //{
        //    anim.SetBool("isRun", false);
        //}

        if (!photonView.IsMine)
            return;
        if (horizontalMove == 0 && verticalMove == 0)
        {
            anim.SetBool("isRun", false);
        }
        else
        {
            anim.SetBool("isRun", true);
        }

    }
}
