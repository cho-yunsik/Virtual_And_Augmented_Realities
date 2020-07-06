using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cshCastleShader : MonoBehaviour
{
    private GameObject player;
    private float distance;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //player = GameObject.Find("VR user(Clone)");
        player = GameObject.FindGameObjectWithTag("VRPlayer");
        
        distance = Vector3.Distance(player.transform.position, transform.position); //castle과 플레이어 사이 거리

        if (distance < 3.0f) //castle과 플레이어 사이 거리가 3.0미만일때
        {
            this.GetComponent<Renderer>().material.shader = Shader.Find("Legacy Shaders/Transparent/Diffuse");
            this.GetComponent<Renderer>().material.color = new Color32(142, 142, 142, 60);  //투명도60
        }
        else
        {
            this.GetComponent<Renderer>().material.shader = Shader.Find("Legacy Shaders/Diffuse");
            //this.GetComponent<Renderer>().material.color = new Color32(255, 8, 8, 255);
        }
        
    }
}
