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
        ColorSelf(Color.cyan);
        GetComponentInParent<MessageManager>().NextDay();
        isActive = false;
    }

    public void OnButtonUp(Hand fromHand)
    {
        ColorSelf(Color.white);
        gameObject.SetActive(isActive);
    }

    private void ColorSelf(Color newColor)
    {
        Renderer[] renderers = this.GetComponentsInChildren<Renderer>();
        for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
        {
            renderers[rendererIndex].material.color = newColor;
        }
    }
}