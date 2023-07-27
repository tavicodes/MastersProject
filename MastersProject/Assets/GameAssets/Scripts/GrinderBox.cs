using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrinderBox : MonoBehaviour
{
    public Transform[] grinders;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        grinders[0].Rotate(new Vector3(1, 0, 0), Space.World);
        grinders[1].Rotate(new Vector3(-1, 0, 0), Space.World);
    }
}
