using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cshBirdAnimation : MonoBehaviour
{
    public AudioClip song1;
    public AudioClip song2;

    public float time = 10.0f;
    public float time2 = 20.0f;
    public float timePass;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timePass += Time.deltaTime;

        if(timePass >= time)
        {
            gameObject.GetComponent<AudioSource>().clip = song1;
            gameObject.GetComponent<AudioSource>().Play();
        }
        if(timePass >= time2)
        {
            gameObject.GetComponent<AudioSource>().clip = song2;
            gameObject.GetComponent<AudioSource>().Play();
        }
        if (timePass >= 25.0f)
        {
            timePass = 0.0f;
        }
    }
}
