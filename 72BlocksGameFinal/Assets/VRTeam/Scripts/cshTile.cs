using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cshTile : MonoBehaviour
{
    public bool bCorrect = false;
    public int iCorrect = 0;
    private Transform thisTrans;
    private Vector3 initPos;
    private Quaternion initRot;

    // 성타일 조립에 사용되는 변수
    private int iRoadCnt;
    private int iCastleCnt; // 현재 타일에 존재하는 성의 면 수 
    public bool bContCastle; // 현재 타일에 존재하는 성타일이 연결되어있는지 체크 *수동으로 타일에 직접 다 체크해야됨
    public bool bCastle; // 현재 캐슬이 조립되었는지 여부
    public bool bRoad; // 현재 길이 조립되었는지 여부 
    public AudioClip clip;
    public AudioClip tileError;
    public cshTileManager tileManager;

    // Start is called before the first frame update
    void Start()
    {
        thisTrans = GetComponent<Transform>();
        initPos = thisTrans.position;
        initRot = thisTrans.rotation;
        bCorrect = false;
        iCorrect = 0;

        // 성 조립 관련 변수 설정
        tileManager = GameObject.Find("TileManager").GetComponent<cshTileManager>();
        iCastleCnt = 0;
        for (int i = 0; i < 4; i++)
        {
            if (transform.GetChild(i).gameObject.tag == "CastleSide")
                iCastleCnt++;
        }

    }

    public void SetupInit()
    {
        initPos = thisTrans.position;
        initRot = thisTrans.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        // 제대로 타일이 부착된경우 > 움직임 없애고, 타일의 위치를 이쁘게 정력 후 태그를 수정
        //if(iCorrect > 0)
        if (gameObject.tag == "ControlTile")
        {
            // Layer 변경
            gameObject.layer = 10;

            CheckAssembly();

            if (bCorrect)
            {
                GetComponent<AudioSource>().clip = clip;
                GetComponent<AudioSource>().Play();

                GetComponent<Rigidbody>().useGravity = false;
                GetComponent<Rigidbody>().isKinematic = true;
                GetComponent<Collider>().isTrigger = false;
                thisTrans.position = new Vector3(thisTrans.position.x, 0.0f, thisTrans.position.z);
                thisTrans.eulerAngles = new Vector3(0.0f, thisTrans.eulerAngles.y, 0.0f);
                gameObject.tag = "FloorTile";

              
                // FrozeBase
                ChangeFrozeBase(thisTrans.position);

                // Tree 생성
                if (GameObject.FindGameObjectWithTag("VRPlayer") != null)
                {
                    if (GameObject.FindGameObjectWithTag("VRPlayer").GetComponent<cshVRPlayer>().photonView.IsMine)
                    {
                        if (iCastleCnt == 0)
                        {
                            tileManager.castlenum++;
                            tileManager.treePos.Add(transform.position);

                            if (tileManager.castlenum % 2 == 0)
                            {
                                tileManager.CreateTree();
                            }
                        }

                    }
                }


                Destroy(GetComponent<Rigidbody>());
                // 현재 타일위치를 중심으로 주변 네 방향 base(tag>CandidateBase) 타일을 찾아서 선택 가능한 base타일로 변경(tag>SelectBase)
                // 이때, base에 타일이 있다면(FrozenBase) 제외
                Select4SideBase(thisTrans.position);

                bCorrect = false;
                //iCorrect = 0;
            }
            InitAssembly();
        }
        else
        {
            // Layer 변경
            gameObject.layer = 8;
        }

    }
    public void CheckAssembly()
    {
        cshTileSide[] childs = GetComponentsInChildren<cshTileSide>();
        int check = 0;
        for (int i = 0; i < childs.Length; i++)
        {
            //Debug.Log(childs[i].iAssembly);
            if (childs[i].iAssembly == 2)
            {
                bCorrect = false;
                check = 0;

                this.GetComponent<AudioSource>().clip = tileError;
                this.GetComponent<AudioSource>().Play();
                InitTile();
                break;
            }
            else if (childs[i].iAssembly == 1)
            {
                check++;
            }
        }

        if (check > 0) bCorrect = true;


        // 성 조립 관련 코드
        // 1. Side 조립 시 하나의 타일이 조립되었고, 조립된 타일의 면이 성이라면
        int cntCastle = 0;
        for (int i = 0; i < 4; i++)
        {
            GameObject obj = transform.GetChild(i).gameObject;
            if (obj.tag == "CastleSide" && obj.GetComponent<cshTileSide>().iAssembly == 1)
            {
                cntCastle++;
            }
        }

        if (cntCastle == 1 && bCastle == true)
        {
            AssemblyCastle(1, childs, cntCastle);
        }

        // 2. Side가 조립 되었지만, 성 side가 조립된게 아니라면 (새로운 성 id를 부여)
        if (cntCastle == 0 && bCastle == false && check > 0 && iCastleCnt > 0)
        {
            AssemblyCastle(2, childs, cntCastle);
        }

        // 3. 2개 이상의 다른 id를 가진 성 side가 조립되는 경우
        if (cntCastle >= 2 && bCastle == true)
        {
            AssemblyCastle(3, childs, cntCastle);
        }
        /////////////////////////////////////////////////////////////////////////////////////////////

    }
    public void InitAssembly()
    {
        cshTileSide[] childs = GetComponentsInChildren<cshTileSide>();
        for (int i = 0; i < childs.Length; i++)
        {
            childs[i].iAssembly = 0;
        }
    }

    /// <summary> /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // 성 조립에 대한 처리
    public void AssemblyCastle(int type, cshTileSide[] childs, int castleAssemble)
    {
        // 조립된 타일의 면이 하나이고, 조립된 면이 성인 경우
        if (type == 1)
        {
            // 충돌된 SIDE 중 성 SIDE를 찾아서 그 SIDE의 id를 현재 성 SIDE의 id로 복사
            int id = 0;
            for (int i = 0; i < childs.Length; i++)
            {
                GameObject obj = transform.GetChild(i).gameObject;
                if (childs[i].iAssembly == 1 && obj.tag == "CastleSide")
                {
                    childs[i].idCastle = childs[i].idCandidate;
                    id = childs[i].idCastle;
                    break;
                }
            }
            if (bContCastle == true || iCastleCnt >= 2)
            {
                for (int i = 0; i < 4; i++)
                {
                    GameObject obj = transform.GetChild(i).gameObject;
                    if (obj.tag == "CastleSide" && obj.GetComponent<cshTileSide>().iAssembly == 0)
                    {
                        obj.GetComponent<cshTileSide>().idCastle = id;
                    }
                }
            }

            // TileManager
            tileManager.linkedCntCastle[id]++;
            float x = (tileManager.castlePos[id].x + thisTrans.position.x);
            float z = (tileManager.castlePos[id].z + thisTrans.position.z);
            tileManager.castlePos[id] = new Vector3(x, 0.0f, z);

            if (iCastleCnt >= 2 && bContCastle == true)
            {
                tileManager.openCastle[id] = tileManager.openCastle[id] + iCastleCnt - 2;
            }
            else if (iCastleCnt == 1)
            {
                tileManager.openCastle[id]--;
                if (tileManager.openCastle[id] == 0)
                {
                    tileManager.castlePos[id] = tileManager.castlePos[id] / tileManager.linkedCntCastle[id];
                    //tileManager.castlePos[id] 위치에 tileManager.linkedCntCastle[id]수를 고려하여 성을 생성
                    tileManager.continueCastle[id] = false;
                    Debug.Log("성 생성");
                   
                    tileManager.CreateCastle(id, tileManager.castlePos[id]);
                }
            }
            else if (iCastleCnt >= 2 && bContCastle == false)
            {
                tileManager.openCastle[id] = tileManager.openCastle[id] - 1;
                if (tileManager.openCastle[id] == 0)
                {
                    tileManager.castlePos[id] = tileManager.castlePos[id] / tileManager.linkedCntCastle[id];
                    //tileManager.castlePos[id] 위치에 tileManager.linkedCntCastle[id]수를 고려하여 성을 생성
                    tileManager.continueCastle[id] = false;
                    Debug.Log("성 생성");
                    tileManager.CreateCastle(id, tileManager.castlePos[id]);
                }
            }

            // bContCaltle == false인 경우에 대한 처리 추가
            if (bContCastle == false)
            {
                // 다른 한 면의 아이디를 새롭게 추가
                tileManager.iCastle++;
                for (int i = 0; i < 4; i++)
                {
                    GameObject obj = transform.GetChild(i).gameObject;
                    if (obj.tag == "CastleSide" && obj.GetComponent<cshTileSide>().iAssembly == 0)
                    {
                        obj.GetComponent<cshTileSide>().idCastle = tileManager.iCastle;
                        break;
                    }
                }

                // TileManager
                tileManager.linkedCntCastle[tileManager.iCastle] = 1;
                tileManager.castlePos[tileManager.iCastle] = new Vector3(thisTrans.position.x, 0.0f, thisTrans.position.z);
                tileManager.openCastle[tileManager.iCastle] = 1;

            }
        }
        else if (type == 2) // 조립은 되었지만, 성이 아닌 경우 현재 성타일의 id를 새롭게 부여
        {
            tileManager.iCastle++;
            if (bContCastle == true) // 성이 연결된 경우 같은 id를 부여
            {
                for (int i = 0; i < 4; i++)
                {
                    GameObject obj = transform.GetChild(i).gameObject;
                    if (obj.tag == "CastleSide" && obj.GetComponent<cshTileSide>().iAssembly == 0)
                    {
                        obj.GetComponent<cshTileSide>().idCastle = tileManager.iCastle;
                    }
                }

                // TileManager
                tileManager.linkedCntCastle[tileManager.iCastle] = 1;
                tileManager.castlePos[tileManager.iCastle] = new Vector3(thisTrans.position.x, 0.0f, thisTrans.position.z);
                tileManager.openCastle[tileManager.iCastle] = iCastleCnt;

            }
            else // 성이 분리된 경우 다른 id를 부여
            {
                for (int i = 0; i < 4; i++)
                {
                    GameObject obj = transform.GetChild(i).gameObject;
                    if (obj.tag == "CastleSide" && obj.GetComponent<cshTileSide>().iAssembly == 0)
                    {
                        obj.GetComponent<cshTileSide>().idCastle = tileManager.iCastle;

                        // TileManager
                        tileManager.linkedCntCastle[tileManager.iCastle] = 1;
                        tileManager.castlePos[tileManager.iCastle] = new Vector3(thisTrans.position.x, 0.0f, thisTrans.position.z);
                        tileManager.openCastle[tileManager.iCastle] = 1;

                        tileManager.iCastle++;
                    }
                }
            }

        }
        else if (type == 3) // 서로 다른 id의 성이 연결되는 경우
        {
            if (castleAssemble == iCastleCnt) // 성 side가 모두 조립된경우
            {
                if (castleAssemble == 2 && bContCastle == false) // side가 모두 조립되었는데 분리된 성인 경우
                {
                    // 충돌된 side의 id로 각각 지정
                    for (int i = 0; i < childs.Length; i++)
                    {
                        GameObject obj = transform.GetChild(i).gameObject;
                        if (childs[i].iAssembly == 1 && obj.tag == "CastleSide")
                        {
                            childs[i].idCastle = childs[i].idCandidate;

                            // TileManager
                            tileManager.linkedCntCastle[childs[i].idCastle]++;
                            tileManager.castlePos[childs[i].idCastle] = (tileManager.castlePos[childs[i].idCastle] + thisTrans.position);
                            tileManager.openCastle[childs[i].idCastle]--;

                            if (tileManager.openCastle[childs[i].idCastle] == 0)
                            {
                                tileManager.castlePos[childs[i].idCastle] = tileManager.castlePos[childs[i].idCastle] / tileManager.linkedCntCastle[childs[i].idCastle];

                                //tileManager.castlePos[childs[i].idCastle] 위치에 tileManager.linkedCntCastle[childs[i].idCastle]수를 고려하여 성을 생성
                                tileManager.continueCastle[childs[i].idCastle] = false;
                                Debug.Log("성 생성");
                                tileManager.CreateCastle(childs[i].idCastle, tileManager.castlePos[childs[i].idCastle]);
                            }

                        }
                    }
                }
                else // 일반적인 성 타일의 경우
                {
                    int minid = 10000;
                    List<int> removeId = new List<int>(3);
                    // 충돌된 여러개의 성 side id중 최소 id로 통일하기 위해 최소 id를 탐색
                    //
                    for (int i = 0; i < childs.Length; i++)
                    {
                        GameObject obj = transform.GetChild(i).gameObject;
                        if (childs[i].iAssembly == 1 && obj.tag == "CastleSide")
                        {
                            if (minid > childs[i].idCandidate)
                            {
                                minid = childs[i].idCandidate;
                            }
                        }
                    }
                    // 충돌된 모든 성 side의 id를 minid로 통일
                    for (int i = 0; i < childs.Length; i++)
                    {
                        GameObject obj = transform.GetChild(i).gameObject;
                        if (childs[i].iAssembly == 1 && obj.tag == "CastleSide")
                        {
                            childs[i].idCastle = minid;
                            // 최소 id로 통일하게 되면 나머지 id정보는 필요없어지는 상황
                            // 따라서 만약 현재 충돌된 면의 id가 minid가 아닌 경우 이를 List에 저장하여 추후 관련 정보를 가져와야 함
                            if (minid != childs[i].idCandidate)
                                removeId.Add(childs[i].idCandidate);
                        }
                    }
                    //removeId에 있는 모든 성 side를 minid로 교체
                    GameObject[] sides = GameObject.FindGameObjectsWithTag("CastleSide");

                    for (int i = 0; i < sides.Length; i++)
                    {
                        for (int j = 0; j < removeId.Count; j++)
                        {
                            if (sides[i].GetComponent<cshTileSide>().idCastle == removeId[j])
                            {
                                sides[i].GetComponent<cshTileSide>().idCastle = minid;
                            }
                        }
                    }

                    //removeId의 position을 minid로 누적
                    //removeId의 연결성수를 minid에 누적
                    //removeId의 open수를 minid에 누적
                    // TileManager
                    for (int i = 0; i < removeId.Count; i++)
                    {
                        tileManager.linkedCntCastle[minid] = tileManager.linkedCntCastle[minid] + tileManager.linkedCntCastle[removeId[i]];
                        //tileManager.linkedCntCastle.RemoveAt(removeId[i]);

                        tileManager.castlePos[minid] = (tileManager.castlePos[minid] + tileManager.castlePos[removeId[i]]);
                        //tileManager.castlePos.RemoveAt(removeId[i]);

                        tileManager.openCastle[minid] = tileManager.openCastle[minid] + tileManager.openCastle[removeId[i]];
                        //tileManager.openCastle.RemoveAt(removeId[i]);

                        tileManager.iCastle--;
                    }
                    tileManager.castlePos[minid] = (tileManager.castlePos[minid] + thisTrans.position);
                    tileManager.linkedCntCastle[minid] = tileManager.linkedCntCastle[minid] + 1;
                    tileManager.openCastle[minid] = tileManager.openCastle[minid] - castleAssemble;

                    if (tileManager.openCastle[minid] == 0)
                    {
                        tileManager.castlePos[minid] = tileManager.castlePos[minid] / tileManager.linkedCntCastle[minid];
                        //tileManager.castlePos[minid] 위치에 tileManager.linkedCntCastle[minid]수를 고려하여 성을 생성
                        tileManager.continueCastle[minid] = false;
                        Debug.Log("성 생성");
                        tileManager.CreateCastle(minid, tileManager.castlePos[minid]);
                    }

                }
            }
            else if (castleAssemble == iCastleCnt - 1) // 성 side중 일부만 조립된 경우
            {
                int minid = 10000;
                List<int> removeId = new List<int>(3);
                // 충돌된 여러개의 성 side id중 최소 id로 통일하기 위해 최소 id를 탐색
                //
                for (int i = 0; i < childs.Length; i++)
                {
                    GameObject obj = transform.GetChild(i).gameObject;
                    if (childs[i].iAssembly == 1 && obj.tag == "CastleSide")
                    {
                        if (minid > childs[i].idCandidate)
                        {
                            minid = childs[i].idCandidate;
                        }
                    }
                }
                // 충돌된 모든 성 side의 id를 minid로 통일
                for (int i = 0; i < childs.Length; i++)
                {
                    GameObject obj = transform.GetChild(i).gameObject;
                    if (childs[i].iAssembly == 1 && obj.tag == "CastleSide")
                    {
                        childs[i].idCastle = minid;
                        // 최소 id로 통일하게 되면 나머지 id정보는 필요없어지는 상황
                        // 따라서 만약 현재 충돌된 면의 id가 minid가 아닌 경우 이를 List에 저장하여 추후 관련 정보를 가져와야 함
                        if (minid != childs[i].idCandidate)
                            removeId.Add(childs[i].idCandidate);
                    }
                }
                //removeId에 있는 모든 성 side를 minid로 교체
                GameObject[] sides = GameObject.FindGameObjectsWithTag("CastleSide");

                for (int i = 0; i < sides.Length; i++)
                {
                    for (int j = 0; j < removeId.Count; j++)
                    {
                        if (sides[i].GetComponent<cshTileSide>().idCastle == removeId[j])
                        {
                            sides[i].GetComponent<cshTileSide>().idCastle = minid;
                        }
                    }
                }

                for (int i = 0; i < childs.Length; i++)
                {
                    GameObject obj = transform.GetChild(i).gameObject;
                    if (childs[i].iAssembly == 0 && obj.tag == "CastleSide")
                    {
                        childs[i].idCastle = minid;
                    }
                }
                //removeId의 position을 minid로 누적
                //removeId의 연결성수를 minid에 누적
                //removeId의 open수를 minid에 누적
                // TileManager
                for (int i = 0; i < removeId.Count; i++)
                {
                    tileManager.linkedCntCastle[minid] = tileManager.linkedCntCastle[minid] + tileManager.linkedCntCastle[removeId[i]];
                    //tileManager.linkedCntCastle.RemoveAt(removeId[i]);

                    tileManager.castlePos[minid] = (tileManager.castlePos[minid] + tileManager.castlePos[removeId[i]]);
                    //tileManager.castlePos.RemoveAt(removeId[i]);

                    tileManager.openCastle[minid] = tileManager.openCastle[minid] + tileManager.openCastle[removeId[i]];
                    //tileManager.openCastle.RemoveAt(removeId[i]);

                    tileManager.iCastle--;
                }
                tileManager.castlePos[minid] = (tileManager.castlePos[minid] + thisTrans.position);
                tileManager.linkedCntCastle[minid] = tileManager.linkedCntCastle[minid] + 1;
                tileManager.openCastle[minid] = tileManager.openCastle[minid] - castleAssemble + 1;
            }
        }

    }
    /// </summary>/////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    // 선택 타일 좌표 및 설정 초기화
    public void InitTile()
    {
        GetComponent<Rigidbody>().useGravity = false;
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<Collider>().isTrigger = false;
        thisTrans.position = initPos;
        thisTrans.rotation = initRot;
    }

    private void Select4SideBase(Vector3 pos)
    {
        GameObject[] bases = GameObject.FindGameObjectsWithTag("CandidateBase");
        Vector3[] lrtb = new Vector3[4];
        lrtb[0] = new Vector3(pos.x + 2.5f, pos.y, pos.z);
        lrtb[1] = new Vector3(pos.x - 2.5f, pos.y, pos.z);
        lrtb[2] = new Vector3(pos.x, pos.y, pos.z + 2.5f);
        lrtb[3] = new Vector3(pos.x, pos.y, pos.z - 2.5f);

        for (int i = 0; i < bases.Length; i++)
        {
            for (int j = 0; j < lrtb.Length; j++)
            {
                float dist = Vector3.Distance(lrtb[j], bases[i].transform.position);
                if (dist < 0.1f)
                {
                    bases[i].tag = "SelectBase";
                    break;
                }

            }
        }
        /*
        //OnTriggerEnter 함수가 정상작동 안하는 경우가 발생
        GameObject[] selects = GameObject.FindGameObjectsWithTag("SelectBase");
        for (int i = 0; i < selects.Length; i++)
        {
            float dist = Vector3.Distance(pos, selects[i].transform.position);
            if (dist < 0.1f)
            {
                selects[i].tag = "FrozenBase";
                selects[i].GetComponent<Renderer>().enabled = false;
                break;
            }
        }
        */

    }

    void ChangeFrozeBase(Vector3 pos)
    {
        //OnTriggerEnter 함수가 정상작동 안하는 경우가 발생
        GameObject[] selects = GameObject.FindGameObjectsWithTag("SelectBase");
        for (int i = 0; i < selects.Length; i++)
        {
            float dist = Vector3.Distance(pos, selects[i].transform.position);
            if (dist < 0.1f)
            {
                selects[i].tag = "FrozenBase";
                selects[i].GetComponent<Renderer>().enabled = false;
                break;
            }
        }
    }
    /*
    private void OnTriggerEnter(Collider other)
    {
        // 타일 조립이 완성된 base타일은 투명 재질을 disable 시키고, tag를 변경
        if (other.gameObject.tag == "SelectBase" && bCorrect)
        {
            float dist = Vector3.Distance(thisTrans.position, other.transform.position);
            if (dist < 0.1f)
            {
                other.gameObject.tag = "FrozenBase";
                other.gameObject.GetComponent<Renderer>().enabled = false;
            }
        }
    }*/

}
