using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class cshVRPortalMove : MonoBehaviourPun
{
    private Camera cam;
    public Transform indexFingerPoint;

    public AudioClip portalEnterClip;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine) return;
        //if (Input.GetMouseButtonDown(0)) 
        if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger))
        {
            RaycastHit hit;

            if (Physics.Raycast(indexFingerPoint.position, indexFingerPoint.forward, out hit)) 
            {
               
               if(hit.transform.gameObject.tag == "PortalOut")
                {

                    if (GameObject.FindGameObjectWithTag("PortalOut") != null)
                    {
                        gameObject.GetComponent<AudioSource>().clip = portalEnterClip;
                        gameObject.GetComponent<AudioSource>().Play();
                        //GameObject portalEnter = GameObject.FindGameObjectWithTag("PortalEnter");
                        //GameObject []portalOut = GameObject.FindGameObjectsWithTag("PortalOut");
                        //GameObject portalOut = GameObject.FindGameObjectWithTag("PortalOut");
                        GameObject portalOut = GameObject.Find("PortalVFX(Clone)");
                        GameObject portalVROut = GameObject.Find("PortalVRVFX(Clone)");

                        Debug.Log("portalEnterCheck");
                        this.GetComponent<CharacterController>().enabled = false;
                        //this.transform.position = new Vector3(portalOut[0].transform.position.x, portalOut[0].transform.position.y, portalOut[0].transform.position.z); //portalout 위치로 이동
                        this.transform.position = new Vector3(portalOut.transform.position.x, portalOut.transform.position.y, portalOut.transform.position.z); //portalout 위치로 이동
                        this.GetComponent<CharacterController>().enabled = true;

                        //vr이 포탈 이동 마치면 포탈 두개 destroy
                        //photonView.RPC("DestroyPortal", RpcTarget.AllBuffered, portalOut[0].GetPhotonView().ViewID, portalOut[1].GetPhotonView().ViewID);


                        photonView.RPC("DestroySinglePortal", RpcTarget.Others, portalOut.GetPhotonView().ViewID);
                        photonView.RPC("DestroySinglePortal", RpcTarget.Others, portalVROut.GetPhotonView().ViewID);
                    }
                }
            }
        }
    }

    [PunRPC]
    public void DestroyPortal(int id1, int id2)
    {
        PhotonNetwork.Destroy(PhotonView.Find(id1));
        PhotonNetwork.Destroy(PhotonView.Find(id2));
    }

    [PunRPC]
    public void DestroySinglePortal(int id)
    {
        PhotonNetwork.Destroy(PhotonView.Find(id));
    }
}
