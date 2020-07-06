using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class cshTileManager : MonoBehaviour
{
    public List<int> linkedCntCastle = new List<int>(); // i번째 연결된 성의 수
    public List<Vector3> castlePos = new List<Vector3>(); // i번째 연결된 성의 평균 위치
    public List<int> openCastle = new List<int>(10); // i번째 미 조립된 성의 면 수
    public List<bool> continueCastle = new List<bool>(); // 성 조립 완료 여부 (openCastle == 0이면 조립 끝, false) 
    public int iCastle = 1; //현재 바닥에 깔려있는 성 타일의 수 

    public List<GameObject> castleLv1 = new List<GameObject>();
    public List<GameObject> castleLv2 = new List<GameObject>();
    public List<GameObject> castleLv3 = new List<GameObject>();
    public List<GameObject> castleLv4 = new List<GameObject>();
    public List<GameObject> castleLv5 = new List<GameObject>();
    public List<GameObject> castleLv6 = new List<GameObject>();

    private cshVRPlayer m_VR = null;  

    public int castlenum;
    public List<Vector3> treePos = new List<Vector3>(); // i번째 연결된 성의 평균 위치
    public List<GameObject> treePrefabs = new List<GameObject>();

    public float timePass;
    public float time = 2.0f;

    // Start is called before the first frame update
    void Start()
    {
        //성 open 수
        for (int i = 0; i < 30; i++)
        {
            openCastle[i] = 1;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(m_VR == null)
        {
            if(GameObject.FindGameObjectWithTag("VRPlayer") != null)
            {
                m_VR = GameObject.FindGameObjectWithTag("VRPlayer").GetComponent<cshVRPlayer>();
            }
            else
            {
                m_VR = null;
            }
        }

        //if(m_VR != null && !m_VR.photonView.IsMine) CreateCastle();
       
    }

    public void CreateTree()
    {
        if (m_VR == null || !m_VR.photonView.IsMine) return;

        int rnd = Random.Range(0, treePos.Count);
        Vector3 pos = new Vector3(treePos[rnd].x + Random.Range(-1.2f, 1.2f), 0.0f, treePos[rnd].z + Random.Range(-1.2f, 1.2f));

        int tRnd = Random.Range(0, treePrefabs.Count);

        GameObject obj = PhotonNetwork.Instantiate("Tree/" + treePrefabs[tRnd].name, pos, Quaternion.Euler(-90.0f, 0.0f,0.0f));
        float scRnd = Random.Range(0.2f, 0.3f);
        obj.transform.localScale = new Vector3(scRnd, scRnd, scRnd);

        treePos.RemoveAt(rnd);
    }



    public void CreateCastle(int id, Vector3 GenPos)
    {
        if (m_VR == null || !m_VR.photonView.IsMine) return;

        //새로 생성되는 성이 플레이어를 향하도록
        Transform player = GameObject.FindGameObjectWithTag("VRPlayer").GetComponent<Transform>();
        Vector3 dir = GenPos - player.position; // 플레이어에서 타일까지 향하는 벡터
        Vector3 foward = Vector3.forward;
        float angle = Vector3.SignedAngle(foward, dir, Vector3.up);

        // 플레이어와 타일 생성위치에 따라 회전을 적용
        Quaternion CaslteRot = Quaternion.identity;
        if (angle > -45.0f && angle < 45.0f)
        {
            CaslteRot = Quaternion.Euler(Vector3.up * 180.0f);
        }
        else if (angle >= 45.0f && angle < 135.0f)
        {
            CaslteRot = Quaternion.Euler(Vector3.up * -90.0f);
        }
        else if (angle > -135.0f && angle < -45.0f)
        {
            CaslteRot = Quaternion.Euler(Vector3.up * 90.0f);
        }

        // 성이 생성될 때, 현재 성과 성 주변 타일에게 같은 id를 부여하고, List에 저장 (몬스터 패널티로 활용)


        int rnd = 0;
        if(linkedCntCastle[id] == 2)
        {
            rnd = Random.Range(0, castleLv1.Count);
            PhotonNetwork.Instantiate("NewCastleEffect/" + castleLv1[0].name, castlePos[id], CaslteRot);

            openCastle[id] = -1;
        }
        //조립된 성의 수가 3~4
        else if (linkedCntCastle[id] >= 3 && linkedCntCastle[id] <= 4)
        {
            rnd = Random.Range(0, castleLv2.Count);
            PhotonNetwork.Instantiate("NewCastleEffect/" + castleLv2[rnd].name, castlePos[id], CaslteRot);

            openCastle[id] = -1;
        }
        //조립된 성의 수가 5~7
        else if (linkedCntCastle[id] >= 5 && linkedCntCastle[id] <= 7)
        {
            rnd = Random.Range(0, castleLv3.Count);
            PhotonNetwork.Instantiate("NewCastleEffect/" + castleLv3[rnd].name, castlePos[id], CaslteRot);

            openCastle[id] = -1;
        }

        //조립된 성의 수가 8~10
        else if (linkedCntCastle[id] >= 8 && linkedCntCastle[id] <= 10)
        {
            rnd = Random.Range(0, castleLv4.Count);
            PhotonNetwork.Instantiate("NewCastleEffect/" + castleLv4[rnd].name, castlePos[id], CaslteRot);

            openCastle[id] = -1;
        }
        //조립된 성의 수가 11~14
        else if (linkedCntCastle[id] >= 11 && linkedCntCastle[id] <= 14)
        {
            rnd = Random.Range(0, castleLv5.Count);
            PhotonNetwork.Instantiate("NewCastleEffect/" + castleLv5[rnd].name, castlePos[id], CaslteRot);

            openCastle[id] = -1;
        }

        //조립된 성의 수가 15~
        else if (linkedCntCastle[id] >= 15)
        {
            rnd = Random.Range(0, castleLv6.Count);
            PhotonNetwork.Instantiate("NewCastleEffect/" + castleLv6[rnd].name, castlePos[id], CaslteRot);

            openCastle[id] = -1;
        }
        /*
        for (int i = 0; i < 30; i++)
        {
            if (openCastle[i] == 0 && linkedCntCastle[i] == 2)
            {
                rnd = Random.Range(0, castleLv1.Count);
                PhotonNetwork.Instantiate("NewCastle/" + castleLv1[rnd].name, castlePos[i], Quaternion.identity);

                openCastle[i] = -1;
                break;
            }
            //조립된 성의 수가 3~4
            else if (openCastle[i] == 0 && linkedCntCastle[i] >= 3 && linkedCntCastle[i] <= 4)
            {
                rnd = Random.Range(0, castleLv2.Count);
                PhotonNetwork.Instantiate("NewCastle/" + castleLv2[rnd].name, castlePos[i], Quaternion.identity);

                openCastle[i] = -1;
                break;
            }
            //조립된 성의 수가 5~7
            else if (openCastle[i] == 0 && linkedCntCastle[i] >= 5 && linkedCntCastle[i] <= 7)
            {
                rnd = Random.Range(0, castleLv3.Count);
                PhotonNetwork.Instantiate("NewCastle/" + castleLv3[rnd].name, castlePos[i], Quaternion.identity);

                openCastle[i] = -1;
                break;
            }

            //조립된 성의 수가 8~10
            else if (openCastle[i] == 0 && linkedCntCastle[i] >= 8 && linkedCntCastle[i] <= 10)
            {
                rnd = Random.Range(0, castleLv4.Count);
                PhotonNetwork.Instantiate("NewCastle/" + castleLv4[rnd].name, castlePos[i], Quaternion.identity);

                openCastle[i] = -1;
                break;
            }
            //조립된 성의 수가 11~14
            else if (openCastle[i] == 0 && linkedCntCastle[i] >= 11 && linkedCntCastle[i] <= 14)
            {
                rnd = Random.Range(0, castleLv5.Count);
                PhotonNetwork.Instantiate("NewCastle/" + castleLv5[rnd].name, castlePos[i], Quaternion.identity);

                openCastle[i] = -1;
                break;
            }

            //조립된 성의 수가 15~
            else if (openCastle[i] == 0 && linkedCntCastle[i] >= 15)
            {
                rnd = Random.Range(0, castleLv6.Count);
                PhotonNetwork.Instantiate("NewCastle/" + castleLv6[rnd].name, castlePos[i], Quaternion.identity);

                openCastle[i] = -1;
                break;
            }
        }
        */
    }
}