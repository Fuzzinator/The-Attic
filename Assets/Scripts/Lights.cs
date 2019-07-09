using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FuzzyTools;

public class Lights : MonoBehaviour
{
    public GameObject lights;
    public Renderer rend;
    public Material offLight;
    public Material lightBulb;
    [ReadOnly] public bool on = true;
    [ReadOnly] public Color offColor = Color.black;
    [ReadOnly] public Color onColor;
    [ReadOnly] public Color currentColor;

    private void OnValidate()
    {
        if (lightBulb == null && rend != null)
        {
            lightBulb = rend.sharedMaterial;
        }
        if (lightBulb != null)
        {
            onColor = lightBulb.GetColor("_EmissiveColor"); //GetInt("_EmissiveIntensity");
        }
    }

    public void ToggleLight()
    {
        lights.SetActive(!lights.activeSelf);
        on = !on;
        if (on)
        {
            rend.sharedMaterial = lightBulb;
            //StartCoroutine(TurnOn());
        }
        else
        {
            rend.sharedMaterial = offLight;
            //StartCoroutine(TurnOff());
        }
    }

    private IEnumerator TurnOn()
    {
        var current = 0f;
        var tempColor = currentColor;
        while (on && current < 1)
        {
            yield return null;
            currentColor = Color.Lerp(tempColor, onColor, current);
            current += .1f;
            DynamicGI.SetEmissive(rend, currentColor);
            //current += 100;
            //lightBulb.SetInt("_EmissiveIntensity",  current);
            //DynamicGI.UpdateEnvironment();
        }
    }
    
    private IEnumerator TurnOff()
    {
        var current = 0f;
        var tempColor = currentColor;
        while (on && current < 1)
        {
            yield return null;
            currentColor = Color.Lerp(tempColor, offColor, current);
            current += .1f;
            DynamicGI.SetEmissive(rend, currentColor);
            //current += 100;
            //lightBulb.SetInt("_EmissiveIntensity",  current);
            //DynamicGI.UpdateEnvironment();
        }
        /*var current = onColor;
        while (!on && current > 0)
        {
            yield return null;
            print(current);
            current -= 1000;
            lightBulb.SetInt("_EmissiveIntensity",  current);
            DynamicGI.UpdateEnvironment();
            //RendererExtensions.UpdateGIMaterials(rend);
        }*/
    }
}
