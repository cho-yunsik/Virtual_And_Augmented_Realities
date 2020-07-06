using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cshChickenAnimation : MonoBehaviour
{
    Animator anim;

    public int time;
    public float timePass;

    // Start is called before the first frame update
    void Start()
    {
        anim = gameObject.GetComponent<Animator>();
       
 
    }
    private void Update()
    {
        time = Random.Range(0, 6);

        timePass += Time.deltaTime;

        if(timePass <= time)
        {
            anim.SetBool("Turn Head", true);
            anim.SetBool("Eat", false);
        }
        else
        {
            anim.SetBool("Turn Head", false);
            anim.SetBool("Eat", true);
        }
        if(timePass >= 10.0f)
        {
            timePass = 0.0f;
        }
    }
}
