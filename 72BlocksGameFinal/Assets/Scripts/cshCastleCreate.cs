using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cshCastleCreate : MonoBehaviour
{
    public float time = 2.0f;
    public float timePass;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timePass += Time.deltaTime;

        if(timePass > time)
        {
            gameObject.transform.GetChild(0).gameObject.SetActive(false);
            gameObject.transform.GetChild(1).gameObject.SetActive(true);
        }
    }
}
