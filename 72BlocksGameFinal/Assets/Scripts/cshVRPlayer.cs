using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class cshVRPlayer : MonoBehaviourPun
{
    public GameObject obj;

    GameObject[] fbase; //프로즌베이스 
    float dis; //떨어진 위치와 프로즌베이스 사이의 거리
    Transform fbasePos; //가까운 프로즌베이스 위치
    public GameObject find; //가장 가까운 프로즌베이스
    //float speed = 10.0f;
    public bool falling = false;

    // Start is called before the first frame update
    void Start()
    {
        if (!photonView.IsMine)
        {
            // VR 카메라 FALSE
            GetComponentInChildren<OVRCameraRig>().disableEyeAnchorCameras = true;

            Camera[] cameras;
            cameras = transform.gameObject.GetComponentsInChildren<Camera>();
            foreach (Camera c in cameras)
            {
                c.enabled = false;
            }

            // VR 사용자가 네트워크 상에서 자신이 아닐 경우
            // 오큘러스 충돌을 방지하기 위해 관련된 속성들을 모두 비활성화
            GetComponent<CharacterController>().enabled = false;
            GetComponent<OVRPlayerController>().enabled = false;
            GetComponentInChildren<OVRCameraRig>().enabled = false;
            GetComponentInChildren<OVRManager>().enabled = false;
            GetComponentInChildren<OVRHeadsetEmulator>().enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            PhotonNetwork.Instantiate("temp/" + obj.name, new Vector3(1.0f, 0.5f, 1.0f), Quaternion.identity);
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            GameObject dst = GameObject.FindGameObjectWithTag("Respawn");
            PhotonNetwork.Destroy(dst);
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("afd");
            GameObject dst = GameObject.FindGameObjectWithTag("Respawn");
            dst.transform.Translate(new Vector3(10.0f, 0.0f, 0.0f) * Time.deltaTime);
        }

        if (falling == true)
        {
            InitPos();
            GetComponent<CharacterController>().enabled = true;
            falling = false;
        }

    }


    private void OnTriggerEnter(Collider other)
    {

        if (other.tag == "Fall")
        {
            GetComponent<CharacterController>().enabled = false;

            fbase = GameObject.FindGameObjectsWithTag("FloorTile");
            Debug.Log("fbase : " + fbase.Length);

            float shortdist = 1000;//Vector3.Distance(this.transform.position, fbase[0].transform.position);
            Debug.Log("shortdist : " + shortdist);

            for (int i = 0; i < fbase.Length; i++)
            {
                dis = Vector3.Distance(this.transform.position, fbase[i].transform.position);
                Debug.Log("distance" + dis);

                if (dis <= shortdist)
                {
                    shortdist = dis;
                    fbasePos = fbase[i].transform;
                    find = fbase[i];
                    Debug.Log(find);
                }

            }

            falling = true;

        }
    }

    public void InitPos()
    {

        transform.position = new Vector3(find.transform.position.x, find.transform.position.y + 1.0f, find.transform.position.z);
        //this.transform.position = Vector3.Lerp(transform.position, find.transform.position, speed * Time.deltaTime);
        Debug.Log("player pos " + transform.position.x + transform.position.y + transform.position.z);

    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine)
            return;
    }


}
