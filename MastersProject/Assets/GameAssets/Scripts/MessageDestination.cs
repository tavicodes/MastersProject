using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageDestination : MonoBehaviour
{
    public GameObject playerCameraObj;
    public Color[] cubeColor;
    private string[] cubeText;
    private TextMesh triggerTextMesh;

    public bool isAllow;

    void Awake()
    {
        cubeText = new string[2] {"Spam", "Allow"};
        GetComponentInChildren<MeshRenderer>().material.color = cubeColor[Convert.ToInt32(isAllow)];
        triggerTextMesh = GetComponentInChildren<TextMesh>();
        triggerTextMesh.text = cubeText[Convert.ToInt32(isAllow)];
    }

    // Update is called once per frame
    void Update()
    {
        FaceTextMeshToCamera();
    }

    void FaceTextMeshToCamera()
    {
        Vector3 origRot = triggerTextMesh.transform.eulerAngles;
        triggerTextMesh.transform.LookAt(playerCameraObj.transform);
        origRot.y = triggerTextMesh.transform.eulerAngles.y - 180;
        triggerTextMesh.transform.eulerAngles = origRot;
    }
}
