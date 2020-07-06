using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cshTest : MonoBehaviour
{
    public List<GameObject> tower = new List<GameObject>();
    private int num;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            num = Random.Range(0, 3);

            Instantiate(tower[num], new Vector3(0,0,0), Quaternion.identity);
        }
    }
}
