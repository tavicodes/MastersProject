using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageDestination : MonoBehaviour
{
    public GameObject playerCameraObj;
    public Color[] lightColor;
    public Light spotLight;

    public bool isAllow;
    public GameObject[] allowMeshes;
    public GameObject tempMesh;


    void Awake()
    {
        float[] tempIntens = new float[2] {1.5f, 2.0f};
        tempMesh.SetActive(false);
        spotLight.color = lightColor[Convert.ToInt32(isAllow)];
        spotLight.intensity = tempIntens[Convert.ToInt32(isAllow)];
        allowMeshes[Convert.ToInt32(isAllow)].SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
       
    }
}
