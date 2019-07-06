﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FuzzyTools;

public class Page1 : MonoBehaviour
{
    public BoxCollider lastCollider;
    public BoxCollider thisCollider;
    public BoxCollider nextCollider;
    public bool interacted = false;
    public void ToggleCollider1()
    {
        
        if(interacted)
        {
            lastCollider.enabled = false;
            thisCollider.enabled = false;
            nextCollider.enabled = true;
        }
        else
        {
            lastCollider.enabled = true;
            thisCollider.enabled = false;
            nextCollider.enabled = false;
        }
        interacted = !interacted;
    }
}
