using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class cshHead : MonoBehaviourPun
{
    public Transform target;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine) return;

        transform.LookAt(transform.position + target.forward);
    }
}
