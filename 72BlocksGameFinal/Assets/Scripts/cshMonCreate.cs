using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class cshMonCreate : MonoBehaviour
{
    private cshVRPlayer m_VR = null;

   // public GameObject monster;
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

        CreateMonster();

       
    }

    public void CreateMonster()
    {
        if (m_VR == null || !m_VR.photonView.IsMine) 
            return;

        if (this.tag == "FloorTile" && create == false)
        {
            //새로 생성되는 몬스터가 플레이어를 향하도록
            Transform player = GameObject.FindGameObjectWithTag("VRPlayer").GetComponent<Transform>();
            Vector3 dir = transform.position - player.position; // 플레이어에서 몬스터까지 향하는 벡터
            dir.y = 0.0f;
            Vector3 foward = Vector3.forward;
            float angle = Vector3.SignedAngle(foward, dir, Vector3.up);

            // 플레이어와 몬스터 생성위치에 따라 회전을 적용
            Quaternion MonsterRot = Quaternion.identity;
            if (angle > -45.0f && angle < 45.0f)
            {
                MonsterRot = Quaternion.Euler(Vector3.up * 180.0f);
            }
            else if (angle >= 45.0f && angle < 135.0f)
            {
                MonsterRot = Quaternion.Euler(Vector3.up * -90.0f);
            }
            else if (angle > -135.0f && angle < -45.0f)
            {
                MonsterRot = Quaternion.Euler(Vector3.up * 90.0f);
            }



            Transform tilePos = this.GetComponent<Transform>();
            Vector3 monPos = new Vector3(tilePos.position.x, tilePos.position.y + 0.1f, tilePos.position.z);
            PhotonNetwork.Instantiate("Orc", monPos, MonsterRot);
            create = true;
        }
    }

  
}
