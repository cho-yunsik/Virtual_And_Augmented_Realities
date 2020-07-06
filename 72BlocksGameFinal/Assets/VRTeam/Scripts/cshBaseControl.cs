using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class cshBaseControl : MonoBehaviourPun
{
    private Camera cam;
    private GameObject hitBase;
    private Color originColor;
    // Start is called before the first frame update

    //VR 모드를 위해 추가 (AR 작업시 삭제)
    public Transform indexFingerPoint; // VR 임시 변수

    public Transform tile;

    // 임시작업
    public Transform[] tiles;
    public int itile = 0;

    public OVRInput.Controller controller;

    public AudioClip lazor;
    void Start()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetMouseButtonDown(0)) 

        if (OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, controller))
        {
            Debug.Log("1");
            if (OVRInput.GetDown(OVRInput.Button.Two, controller))
            {
                Debug.Log("2");
                // if left button pressed...
                Ray ray = cam.ScreenPointToRay(Input.mousePosition); // AR 카메라로 설정
                RaycastHit hit;
                //if (Physics.Raycast(ray, out hit)) // AR 모드일때
                int layerMask = (1 << LayerMask.NameToLayer("Portal"));  // Everything에서 Player 레이어만 제외하고 충돌 체크함
                layerMask |= (1 << LayerMask.NameToLayer("Player"));
                layerMask |= (1 << LayerMask.NameToLayer("HMD_Player"));
                layerMask |= (1 << LayerMask.NameToLayer("Monster"));
                layerMask |= (1 << LayerMask.NameToLayer("ControlTile"));
                layerMask = ~layerMask;
                if (Physics.Raycast(indexFingerPoint.position, indexFingerPoint.forward, out hit, Mathf.Infinity, layerMask)) // VR 임시 모드
                {
                    // the object identified by hit.transform was clicked
                    // do whatever you want
                    Debug.Log(hit.transform.gameObject);
                    if (hit.transform.gameObject.tag == "SelectBase")
                    {
                        Debug.Log("3");
                        hitBase = hit.transform.gameObject;
                        originColor = hitBase.GetComponent<Renderer>().material.color; //흰색
                        hitBase.GetComponent<Renderer>().material.color = Color.green; //초록색 

                        GetComponent<AudioSource>().clip = lazor;
                        GetComponent<AudioSource>().Play();

                        // 동기화
             
                        photonView.RPC("ChangeColor", RpcTarget.Others, hitBase.name, 0.0f, 1.0f, 0.0f, 1.0f); //빨간색
                    }
                }
            }
            else if (OVRInput.GetUp(OVRInput.Button.Two, controller))
            {
                if (hitBase != null)
                {
                    hitBase.GetComponent<Renderer>().material.color = originColor; //원래 색(흰색)으로 
                                                                                   // 동기화
                    photonView.RPC("ChangeColor", RpcTarget.Others, hitBase.name, originColor.r, originColor.g, originColor.b, originColor.a); //빨간색

                    // 컨트롤 타일이 없다면 생성
                    // GameObject ctile = GameObject.FindGameObjectWithTag("ControlTile");
                    //if(ctile == null) ControlTileCreation(hitBase.transform.position); //컨트롤 타일이 없으면 hitBase위치에 타일 생성

                    hitBase = null;
                }
            }

        }
    }

    ////////////////////////////색상변경 동기화///////////////////////////////////
    [PunRPC]
    public void ChangeColor(string name, float r, float g, float b, float a)
    {
        Color c = new Color(r, g, b, a);
        GameObject.Find(name).GetComponent<Renderer>().material.color = c;
        //obj.GetComponent<Renderer>().material.color = color;
    }


    //타일 생성
    void ControlTileCreation(Vector3 pos)
    {
        Vector3 tilePos = new Vector3(pos.x, 2.0f, pos.z);
        Quaternion tileRot = Quaternion.Euler(90.0f, 0.0f, 0.0f);
        //Transform newTile = Instantiate(tile, tilePos, tileRot);
        Transform newTile = Instantiate(tiles[itile], tilePos, tileRot);
        itile++;
        if (itile >= tiles.Length) itile = tiles.Length - 1;

        newTile.gameObject.tag = "ControlTile";

        //새로 생성되는 타일이 플레이어를 바로볼수 있게
        Transform player = GameObject.FindGameObjectWithTag("VRPlayer").GetComponent<Transform>();
        Vector3 dir = pos - player.position; // 플레이어에서 타일까지 향하는 벡터
        Vector3 foward = Vector3.forward;
        float angle = Vector3.SignedAngle(foward, dir, Vector3.up);

        // 플레이어와 타일 생성위치에 따라 회전을 적용
        if (angle > -45.0f && angle < 45.0f)
        {
            newTile.Rotate(Vector3.up * 180.0f, Space.World);
        }
        else if (angle >= 45.0f && angle < 135.0f)
        {
            newTile.Rotate(Vector3.up * -90.0f, Space.World);
        }
        else if (angle > -135.0f && angle < -45.0f)
        {
            newTile.Rotate(Vector3.up * 90.0f, Space.World);
        }

    }
}
