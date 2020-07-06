using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cshTileSide : MonoBehaviour
{
    public int iAssembly; //0: 기본, 1: 정상, 2: 비정상 조립
    //성 조립에 사용되는 변수
    public int idCastle; // 현재 조립되는 성의 id
    public int idRoad;
    public int idCandidate; // 조립이 정상 완료될때까지 임시 성 id

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (transform.parent.tag == "ControlTile")
        {
            if (other.tag == "ForestSide" || other.tag == "CastleSide" || other.tag == "RoadSide")
            {
                if (other.tag == gameObject.tag)
                {
                    //태그 이름 같을때
                    //GetComponentInParent<cshTile>().bCorrect = true;
                    //GetComponentInParent<cshTile>().iCorrect++; 
                    iAssembly = 1; // 현재 성면이 정상 조립되었음
                    if(gameObject.tag == "CastleSide") { 
                        GetComponentInParent<cshTile>().bCastle = true;
                        // 일단 조립된 면의 id를 임시로 저장
                        idCandidate = other.GetComponent<cshTileSide>().idCastle;
                    }
                }
                else
                {
                    //GetComponentInParent<cshTile>().iCorrect--;
                    //GetComponentInParent<cshTile>().bCorrect = false;
                    iAssembly = 2;
                    //GetComponentInParent<cshTile>().InitTile();
                }

            }
        }
    }
}
