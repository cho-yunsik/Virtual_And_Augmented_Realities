using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cshTileControl : MonoBehaviour
{
    private cshHandsVRController controller; 
    private GameObject controlTile; 
    private int iControl = 0; // 타일 제어 상태인지 체크 0:제어 안함, 1:제어 중, 2:타일 이동중
    private Transform tileTrans; // control tile transform

    private Transform thisTrans; // hand transform
    private Vector3 basisRot; // 손의 기준 벡터 (basisRot을 기준으로 현재 손이 90도 가량 회전하였는지를 체크)
    private float rotAngle; // 현재 손의 회전각
    private int iTileRot = 0;//0: 기본 상태, 1: 손목을 회전하는 순간 상태, 2: 손목을 처음으로 되돌린 상태
    private bool bCheckRotCtrl = false; // 한번이라도 컨트롤 타일을 제어했더면 true, 컨트롤 타일 생성 후 한번도 제어하지 않았다면 false
    private int iTileSetup = 1;

    public AudioClip tileTurn;
    public AudioClip tileAssemble;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<cshHandsVRController>();
        //controlTile = GameObject.FindGameObjectWithTag("ControlTile");

        thisTrans = GetComponent<Transform>(); //hand의 Transform
        basisRot = thisTrans.up;
    }
    // Update is called once per frame
    void Update()
    {
        // 주먹을 쥐면, 제어할 수 있는 타일이 있는지를 체크
        if(controller.handState == 3)
        {
            controlTile = GameObject.FindGameObjectWithTag("ControlTile"); //태그가 ControlTile인 오브젝트
            if (controlTile != null)
            {
                // 타일을 제어할 수 있는 상태로 전환
                iControl = 1; // 1=>제어 상태
                tileTrans = controlTile.GetComponent<Transform>(); //controlTile의 Transform

                if(iTileRot == 0) { // 0이면 기본 상태
                    //기본 상태에서 주먹을 쥐면 그 순간의 x축좌표를 기준벡터에 저장
                    basisRot = thisTrans.up;
                }
            }
            else //태그가 ControlTile인 object가 없으면
            {
                iControl = 0; // 0=>제어 안함 
                bCheckRotCtrl = false;
            }
        }else if (controller.handState == 0 || controller.handState == 1 && iControl == 1)
        {
            // 타일제어가 끝나고 회전에 들어가는 상태
          
            iControl = 2; // 2=>타일 이동중
        }
        else
        {
            //iControl = 0;
            //손을 떼면, 다시 기준 좌표를 손의 x좌표로 초기화
            basisRot = thisTrans.up;
        }

        // 타일 제어 상태가 되면 > 손을 좌우로 회전하면 타일이 같이 이동
        if (iControl == 1)
        {
            // 손의 X축이 월드좌표 기준 Y축이기 때문에 thisTrans.up 벡터를 활용
            rotAngle = Vector3.Angle(thisTrans.up, basisRot); //

            if (rotAngle > 70.0f && iTileRot == 2) //사이각이 70 이상이고 손목을 처음으로 되돌린 상태
            {
                // 현재 제어 상태가 손목이 움직이는 중이 아닌(iTileRot == 2) 조건에서 손목을 일정각도 이상 회전되면
                // 타일이 회전 중인 상태로 변경
                iTileRot = 1;

            }else if(Mathf.Abs(rotAngle) < 10.0f)
            {
                // 손목 회전각이 작은 즉, 손목 움직임이 미비하여 회전을 멈췄다면
                // 타일 회전이 멈춘 상태로 변경
                iTileRot = 2;
                // basisRot = thisTrans.up; < 손목 한번 회전시 한번씩만 움직이게 하려면 주석해제
            }

            // 타일 회전 상태에서
            if(iTileRot == 1)
            {
                // 회전 방향을 계산
                // 움직인 손의 x축과 움직이기 전 x축을 외적
                // 외적 벡터가 손의 z축과 같은 방향인지 여부를 확인하기 위해 내적을 계산
                Vector3 cross = Vector3.Cross(thisTrans.up, basisRot);
                float dot = Vector3.Dot(thisTrans.right, cross);

                // 내적이 0이상이면 같은 방향으로 이 경우 오른쪽으로 회전
                // 아니면 반대방향으로 이 경우 왼쪽으로 회전                
                if (dot > 0.0f)
                {
                    controlTile.GetComponent<AudioSource>().clip = tileTurn;
                    controlTile.GetComponent<AudioSource>().Play();
                    tileTrans.Rotate(new Vector3(0.0f, 90.0f, 0.0f));
                }
                else
                {
                    controlTile.GetComponent<AudioSource>().clip = tileTurn;
                    controlTile.GetComponent<AudioSource>().Play();
                    tileTrans.Rotate(new Vector3(0.0f, -90.0f, 0.0f));
                }

                // 한번 회전 후 타일제어 상태를 기본 상태로 전환
                iTileRot = 0;

                // 타일이 제어되기전에 세팅되는것을 막기 위해 bCheckRotCtrl변수 설정
                bCheckRotCtrl = true;
            }
        }else if(iControl == 2) // 타일 제어가 완료되고 배치되는 상태
        {
            //타일 셋팅 과정 (컨트롤 타일을 제어하고 난 후 이동)
            //1. 90도 회전 (타일 배치 상태 1)
            if (bCheckRotCtrl && iTileSetup == 1)
            {

                GetComponent<AudioSource>().clip = tileAssemble;
                GetComponent<AudioSource>().Play();

                // 타일이 회전되는 축을 계산
                // 월드좌표의 -z축과 타일의 y축을 외적을 취해 x축을 계산하고 회전
                Vector3 rotAxis = Vector3.Cross(Vector3.down, tileTrans.up);
                rotAxis = rotAxis.normalized;
                tileTrans.Rotate(rotAxis * 80.0f * Time.deltaTime, Space.World);

                // 타일의 y축이 월드의 y축 즉, 위를 향한다면
                float angle = Vector3.Angle(tileTrans.up, Vector3.up);
                if (angle < 2.0f)
                {
                    //tileTrans.Rotate(rotAxis, Space.World);
                    // 타일이 이동하였음을 체크
                    bCheckRotCtrl = false;
                    // 타일 배치 상태 2
                    iTileSetup = 2;
                }
            }else if(iTileSetup == 2) //2. Rigidbody > Gravity true
            {
                // 중력을 활성화
                tileTrans.gameObject.GetComponent<Rigidbody>().useGravity = true;
                tileTrans.gameObject.GetComponent<Rigidbody>().isKinematic = false;
                // 트리거 비활성화
                tileTrans.gameObject.GetComponent<Collider>().isTrigger = true;
                
                // 타일 배치 상태를 다시 1로 변경 (잘못된 방향으로 배치되면 타이 제어를 해야하기 때문)
                iTileSetup = 1;
                // 타일 제어 상태도 초기화
                iControl = 0;
            }

            //3. 충돌검사 확인 > Tag 비교 > 맞으면 기준타일 좌표에 현재 타일 세팅 아니면 초기위치로
            // 완료 후 tag > floorTile로 변경
            // cshTile, cshTileSide 스크립트에서 구현

        }


    }
}
