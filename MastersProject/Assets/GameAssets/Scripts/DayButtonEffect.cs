using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;
using Valve.VR.InteractionSystem;

public class DayButtonEffect : MonoBehaviour
{
    private bool isActive;
    private bool oneShot = true;
    private string buttonText = "";

    public void OnButtonDown(Hand fromHand)
    {
        GetComponentInParent<MessageManager>().NextDay();
        isActive = false;
    }

    public void OnButtonUp(Hand fromHand)
    {
        gameObject.SetActive(isActive);
        if (oneShot) GetComponentInChildren<TextMesh>().text = buttonText;
        oneShot = false;
    }

    public void ChangeText(String newText)
    {
        buttonText = newText;
        oneShot = true;
    }
}