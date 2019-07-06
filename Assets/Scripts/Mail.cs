using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FuzzyTools;

public class Mail : MonoBehaviour
{
    public SphereCollider thisCollider;
    public MeshCollider nextCollider;
    public void ToggleMail()
    {
        thisCollider.enabled = false;
        nextCollider.enabled = true;
    }
}
