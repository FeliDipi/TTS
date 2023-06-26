using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testUpdate : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.Rotate(new Vector3(0,0,5)*Time.deltaTime);
    }
}
