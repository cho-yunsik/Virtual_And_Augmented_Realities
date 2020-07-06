using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class cshCreateEvent : MonoBehaviourPun
{
    private cshVRPlayer m_VR = null;

    public GameObject eventObj;
    private bool create = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (m_VR == null)
        {
            if (GameObject.FindGameObjectWithTag("VRPlayer") != null)
            {
                m_VR = GameObject.FindGameObjectWithTag("VRPlayer").GetComponent<cshVRPlayer>();
                //Debug.Log("VR");
            }
            else
            {
                m_VR = null;
            }
        }

        CreateEventObj();
    }

    public void CreateEventObj()
    {
        if (m_VR == null || !m_VR.photonView.IsMine)
            return;

        if (this.tag == "FloorTile" && create == false)
        {
            Transform tilePos = this.GetComponent<Transform>();
            Vector3 eventPos = new Vector3(tilePos.position.x, tilePos.position.y + 0.1f, tilePos.position.z);
            PhotonNetwork.Instantiate(eventObj.name, eventPos, Quaternion.identity);

            create = true;
        }
    }
}
