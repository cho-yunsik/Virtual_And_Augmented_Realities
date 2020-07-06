using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class cshARPlayer : MonoBehaviourPun
{
    private GameObject target;
    private Camera ARCamera;

    // Tile Generator
    public List<Sprite> ImgTiles = new List<Sprite>();
    public List<GameObject> prefabsTiles = new List<GameObject>();
    public Image UITile; 
    private int iTiles;
    private bool bTileGen = false;

    private GameObject hitBase;
    private Color originColor;

    public Button btnGen;
    public Button btnDone;

    public int hitCount = 0;

    public GameObject portalOut;
    public GameObject portalVR;
    //public GameObject portalEnter;
    public GameObject PortalOutCreate;
    //public GameObject PortalEnterCreate;

    private float TouchTime;
    private bool bTouched = false;
    private GameObject hitObj;

    public AudioClip clip;

    // Start is called before the first frame update
    void Start()
    {
        // 네트워크 환경에서 현재 제어중인 사용자(AR/VR)인지 유무 판단
        if (!photonView.IsMine)
        {
            Camera[] cameras;
            cameras = transform.gameObject.GetComponentsInChildren<Camera>();
            foreach (Camera c in cameras)
            {
                c.enabled = false;
            }
        }
        else
        {
            target = GameObject.Find("Cube");
            ARCamera = GameObject.Find("ARCamera").GetComponent<Camera>();
            btnGen.interactable = true;
            btnDone.interactable = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine)
        {
            transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
            return;
        }
        else
        {
            // 이미지 타깃이 인식이 되면, 배경 객체를 화면에 그리고
            // 그렇지 않다면 그리지 않게 만드는 과정
            // ARObject/ControlTile/Player 레이어를 AR카메라의 Culling Mask와 연동하여 설정
            if (target.GetComponent<Renderer>().enabled)
            {
                ARCamera.cullingMask |= 1 << LayerMask.NameToLayer("ARObject");
                ARCamera.cullingMask |= 1 << LayerMask.NameToLayer("ControlTile");
                ARCamera.cullingMask |= 1 << LayerMask.NameToLayer("Player");
                ARCamera.cullingMask |= 1 << LayerMask.NameToLayer("HMD_Player");
                ARCamera.cullingMask |= 1 << LayerMask.NameToLayer("Portal");
                ARCamera.cullingMask |= 1 << LayerMask.NameToLayer("Monster");
            }
            else
            {
                ARCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("ARObject"));
                ARCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("ControlTile"));
                ARCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Player"));
                ARCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("HMD_Player"));
                ARCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Portal"));
                ARCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Monster"));
            }


            if (bTileGen)
            {
                // 타일이 생성되고 나면, 클릭위치에 타일을 배치
                if (Input.GetMouseButtonDown(0))
                { // if left button pressed...
                    Debug.Log("test");
                    Ray ray = ARCamera.ScreenPointToRay(Input.mousePosition); // AR 카메라로 설정
                    RaycastHit hit;
                    int layerMask = (1 << LayerMask.NameToLayer("Portal"));  // Everything에서 Player 레이어만 제외하고 충돌 체크함
                    layerMask = ~layerMask;
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask)) // AR 모드일때
                    {
                        // the object identified by hit.transform was clicked
                        // do whatever you want
                        Debug.Log(hit.transform.gameObject);
                        // SelectBase 태그를 가진 바닥 타일만 선택 가능
                        if (hit.transform.gameObject.tag == "SelectBase")
                        {
                            hitBase = hit.transform.gameObject;
                            originColor = hitBase.GetComponent<Renderer>().material.color; //흰색
                            hitBase.GetComponent<Renderer>().material.color = Color.red; //빨간색 


                            // 동기화
                            photonView.RPC("ChangeColor", RpcTarget.Others, hitBase.name, 1.0f, 0.0f, 0.0f, 1.0f); //빨간색
                        }

                    }
                }
                if (Input.GetMouseButtonUp(0))
                {
                    if (hitBase != null)
                    {
                        hitBase.GetComponent<Renderer>().material.color = originColor; //원래 색(흰색)으로 

                        // 동기화
                        photonView.RPC("ChangeColor", RpcTarget.Others, hitBase.name, originColor.r, originColor.g, originColor.b , originColor.a); //빨간색

                        // 컨트롤 타일이 없다면 생성
                        GameObject ctile = GameObject.FindGameObjectWithTag("CandidateTile");
                        if (ctile != null) ArrangeCandidateTile(ctile.transform, hitBase.transform.position); //컨트롤 타일이 없으면 hitBase위치에 타일 생성

                        hitBase = null;

                        btnGen.interactable = false;
                        btnDone.interactable = true;
                    }
                }
            }

            ///////////////몬스터 Hit //////////////////////
            if (Input.GetMouseButtonDown(0))
            { // if left button pressed...
                Ray ray = ARCamera.ScreenPointToRay(Input.mousePosition); // AR 카메라로 설정
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit)) // AR 모드일때
                {
                    if (hit.transform.gameObject.tag == "Monster")
                    {
                        hitCount++;
                        Debug.Log("hit" + hitCount);

                        if (hitCount > 5) //5번 이상 몬스터 클릭하면 죽음
                        {

                            photonView.RPC("hitMonster", RpcTarget.Others, hitCount); //몇번 클릭했는지 동기화
                            hitMonster(hitCount);

                            hitCount = 0;
                        }
                    }
       
                }
            }
            // 롱클릭으로 포탈 설치
            if (Input.GetMouseButtonDown(0) && !bTouched)
            {
                Ray rayTouch = ARCamera.ScreenPointToRay(Input.mousePosition); // AR 카메라로 설정
                RaycastHit hitTouch;
                int layerMask = (1 << LayerMask.NameToLayer("Portal"));  // Everything에서 Player, Potal 레이어만 제외하고 충돌 체크함
                layerMask |= 1 << LayerMask.NameToLayer("Player");
                layerMask = ~layerMask;
                if (Physics.Raycast(rayTouch, out hitTouch, Mathf.Infinity, layerMask)) // AR 모드일때
                {
                    hitObj = hitTouch.transform.gameObject;
                    if (hitTouch.transform.gameObject.tag == "FloorTile" || hitTouch.transform.gameObject.tag == "FrozenBase")  //FloorTile 누르면 PortalOut 생성
                    {
                        TouchTime = Time.time;
                        bTouched = true;
                    }
                }
            }
            if (bTouched)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    if (Time.time - TouchTime > 0.4)
                    {
                        if (hitObj != null)
                        {
                            //GameObject currPortal = GameObject.FindGameObjectWithTag("PortalOut"); //현재 맵에 PortalOut포탈이 있는지 먼저 확인
                            GameObject currPortal = GameObject.Find("PortalVFX(Clone)");
                            GameObject VRplayer = GameObject.FindGameObjectWithTag("VRPlayer");

                            //새로 생성되는 포탈 or 기 생성된 포탈이 플레이어를 향하도록
                            Transform player = VRplayer.GetComponent<Transform>();
                            Vector3 dir = hitObj.transform.position - player.position; // 플레이어에서 포탈까지 향하는 벡터
                            Vector3 foward = Vector3.forward;
                            float angle = Vector3.SignedAngle(foward, dir, Vector3.up);

                            // 플레이어와 포탈 생성위치에 따라 회전을 적용
                            Quaternion PortalRot = Quaternion.identity;
                            if (angle > -45.0f && angle < 45.0f)
                            {
                                PortalRot = Quaternion.Euler(Vector3.up * 180.0f);
                            }
                            else if (angle >= 45.0f && angle < 135.0f)
                            {
                                PortalRot = Quaternion.Euler(Vector3.up * -90.0f);
                            }
                            else if (angle > -135.0f && angle < -45.0f)
                            {
                                PortalRot = Quaternion.Euler(Vector3.up * 90.0f);
                            }

                            if (currPortal == null) //현재 맵에 PortalOut이 없으면 생성
                            {
                                PhotonNetwork.Instantiate(portalOut.name, new Vector3(hitObj.transform.position.x, hitObj.transform.position.y + 1.5f, hitObj.transform.position.z), PortalRot);
                                GameObject VRFX = PhotonNetwork.Instantiate(portalVR.name, new Vector3(VRplayer.transform.position.x, VRplayer.transform.position.y + 2.5f, VRplayer.transform.position.z), Quaternion.identity);
                                VRFX.GetComponent<AudioSource>().clip = clip;
                                VRFX.GetComponent<AudioSource>().Play();

                                VRFX.transform.parent = player;
                            }
                            else
                            {
                                PortalOutCreate = currPortal;
                                //PortalOut 포탈이 이미 있으면 해당 포탈을 클릭한 FloorTile위치로 이동시켜줌 
                                PortalOutCreate.transform.position = new Vector3(hitObj.transform.position.x, hitObj.transform.position.y + 1.5f, hitObj.transform.position.z);
                                PortalOutCreate.transform.rotation = PortalRot;

                                PortalOutCreate.GetComponent<AudioSource>().clip = clip;
                                PortalOutCreate.GetComponent<AudioSource>().Play();
                            }
                        }
                        //do stuff as a tap​
                        bTouched = false;
                    }
                    bTouched = false;
                }
            }

        }

    }
    private void FixedUpdate()
    {
        if (!photonView.IsMine)
            return;

    }

    ////////////////////////////색상변경 동기화///////////////////////////////////
    [PunRPC]
    public void ChangeColor(string name, float r, float g, float b, float a)
    {
        Color c = new Color(r, g, b, a);
        GameObject.Find(name).GetComponent<Renderer>().material.color = c;
        //obj.GetComponent<Renderer>().material.color = color;
    }
    ///////////////////////////몬스터 hit 동기화//////////////////////////////////
    [PunRPC]
    public void hitMonster(int hitnum)
    {
        cshMonster monster = GameObject.FindGameObjectWithTag("Monster").GetComponent<cshMonster>();
        monster.monHit = hitnum;
        Debug.Log("monhit" + monster.monHit);
    }

    // 생성된 타일의 태그를 정의 후 동기화
    // 이 과정에서 VR 캐릭터와 타일의 충돌로 인한 문제를 해결하기 위해 타일의 Rigidbody 충돌 속성(detectCollisions)을 해제
    [PunRPC]
    public void setTileTag(int id, string tag)
    {
        Transform target = PhotonView.Find(id).transform;
        target.GetComponent<Rigidbody>().detectCollisions = false;
        target.tag = tag;
    }

    public void TileGen()
    {
        // 타일 생성
        // 타일 생성시 장면안에 타일이 남아있지 않아야 함
        GameObject ctile = GameObject.FindGameObjectWithTag("ControlTile");
        if (!bTileGen && ctile == null)
        {
            // List 길이만큼 랜덤 수 생성 후 타일과 이미지를 추출
            iTiles = Random.Range(0, ImgTiles.Count);
            UITile.sprite = ImgTiles[iTiles];

            // 네트워크를 통한 타일 생성 및 태그 동기화
            GameObject newTile = PhotonNetwork.Instantiate("tiles/" + prefabsTiles[iTiles].name, new Vector3(0.0f, 100000.0f, 0.0f), Quaternion.identity);
            photonView.RPC("setTileTag", RpcTarget.All, newTile.GetPhotonView().ViewID, "CandidateTile");


            // 타일의 레이어를 ControlTile로 설정
            newTile.layer = 10;
            bTileGen = true;

            
        }

    }

    // 배치가 완료된 타일의 초기 위치를 동기화
    // 배치가 완료된 타일은 VR 사용자가 제어하기 때문에 충돌 속성을 다시 활성화
    [PunRPC]
    public void SetupTileInit(int id)
    {
        PhotonView.Find(id).GetComponent<cshTile>().SetupInit();
        PhotonView.Find(id).GetComponent<Rigidbody>().detectCollisions = true;
    }

    public void TileDone()
    {
        if (bTileGen)
        {
            // 한번 생성한 타일은 List에서 제거
            ImgTiles.RemoveAt(iTiles);
            prefabsTiles.RemoveAt(iTiles);

            // 장면 안에 후보타일이 있는 경우 생성이 완료 가능
            GameObject ctile = GameObject.FindGameObjectWithTag("CandidateTile");
            if (ctile != null)
            {
                // 타일의 레이어를 ControlTile로 설정
                ctile.layer = 10;

                // 배치가 완료된 타일의 초기 위치 동기화
                photonView.RPC("SetupTileInit", RpcTarget.All, ctile.GetPhotonView().ViewID);
                // 배치가 완료된 타일의 태그를 변경
                photonView.RPC("setTileTag", RpcTarget.All, ctile.GetPhotonView().ViewID, "ControlTile");
                // 컨트롤타일의 네트워크 주인을 VR 사용자로 전환
                Photon.Realtime.Player[] plys = PhotonNetwork.PlayerListOthers;
                ctile.GetPhotonView().TransferOwnership(plys[0].ActorNumber);

                btnGen.interactable = true;
                btnDone.interactable = false;
            }

            bTileGen = false;
        }
    }
    // AR유저가 선택한 곳으로 타일의 위치를 변경
    // 이때 타일의 방향은 VR 사용자의 위치를 고려하여 회전
    void ArrangeCandidateTile(Transform newTile, Vector3 pos)
    {
        Vector3 tilePos = new Vector3(pos.x, 2.0f, pos.z);
        Quaternion tileRot = Quaternion.Euler(90.0f, 0.0f, 0.0f);
        newTile.position = tilePos;
        newTile.rotation = tileRot;

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
