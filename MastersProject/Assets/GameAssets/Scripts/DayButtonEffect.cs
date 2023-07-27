using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;
using Valve.VR.InteractionSystem;

public class DayButtonEffect : MonoBehaviour
{
    private bool isActive;

    public void OnButtonDown(Hand fromHand)
    {
        GetComponentInParent<MessageManager>().NextDay();
        isActive = false;
    }

    public void OnButtonUp(Hand fromHand)
    {
        gameObject.SetActive(isActive);
    }
}