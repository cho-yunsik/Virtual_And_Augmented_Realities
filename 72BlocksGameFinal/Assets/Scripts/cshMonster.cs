using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class cshMonster : MonoBehaviourPun
{
    private GameObject player;
    public Transform LookAtPlayer;
    private float distance;
    Animator animator;

    //public GameObject MonText;
    //public Transform TextPos;
    //private int text = 0;
    private GameObject PlayerTarget;

    int rotateSpeed = 3; //몬스터 회전 
    public bool isAlive = true;
    public int monHit = 0;
    public float dPass = 3.0f;
    public float dPres = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        if (!photonView.IsMine)
            return;
       
    }

    // Update is called once per frame
    void Update()
    {
        animator = GetComponent<Animator>();
        cshARPlayer arHit = GameObject.FindGameObjectWithTag("ARPlayer").GetComponent<cshARPlayer>();

        player = GameObject.FindGameObjectWithTag("VRPlayer");
        PlayerTarget = GameObject.FindGameObjectWithTag("MonsterTarget");

        distance = Vector3.Distance(player.transform.position, this.transform.position);

        //일정 거리에 들어온 VR 바라보는 몬스터 회전
        Vector3 rot = PlayerTarget.GetComponent<Transform>().position - transform.position;
        rot.y = 0.0f;
        rot.Normalize();

        if (distance <= 5.0f)
        {
           Quaternion monrot = Quaternion.LookRotation(rot);
           transform.rotation = Quaternion.Lerp(transform.rotation, monrot, Time.deltaTime * rotateSpeed); //VR 바라보는 몬스터 회전 
            //transform.rotation = Quaternion.Lerp(player.transform.rotation, this.transform.rotation, Time.time * speed);
            //transform.LookAt(PlayerTarget.transform);
            //this.transform.GetChild(0).gameObject.SetActive(true);
  
        }
        //else
        //{
        //    //this.transform.GetChild(0).gameObject.SetActive(false);
        //}
       
        MonAttack();

        cshARPlayer monsterHit = GameObject.FindGameObjectWithTag("ARPlayer").GetComponent<cshARPlayer>();

        if (monHit > 5)
        {
            MonDead();
        }

    }

    public void MonAttack() //몬스터 공격
    {
        if (!photonView.IsMine)
            return;

        if (distance <= 3.0f)
        {
            animator.SetBool("isAttacking", true); 
        }
        else
        {
            animator.SetBool("isAttacking", false);
        }
    }

    public void MonDead() //몬스터 죽음
    {
        if (!photonView.IsMine)
            return;

        animator.SetBool("isDead", true);

        isAlive = false;

        if (isAlive == false)
        {
            dPres += Time.deltaTime;
            if(dPres >= dPass)
            {
                PhotonNetwork.Destroy(this.gameObject);
            }
        }
    }
}
