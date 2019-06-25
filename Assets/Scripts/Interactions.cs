using System;
using System.Collections;
using System.Collections.Generic;
using FuzzyTools;
using UnityEditor.Experimental.Rendering.HDPipeline;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class Interactions : FuzzyMonoBehaviour
{
    public Transform holdPos;
    [ReadOnly] public GameObject currentObj;
    [ReadOnly] public Interactive heldObj;
    [SerializeField] [ReadOnly] private Interactive activeInteractive;
    [AutoGet] [SerializeField] [ReadOnly] private BoxCollider trigger;

    private PlayerControlls _controls;

    private void Start()
    {
        _controls = Controller.Controls;
        _controls.Main.Interact.Enable();
        _controls.Main.DropItem.performed += DropItemOnPerformed;
        _controls.Main.DropItem.Enable();
    }

    private void DropItemOnPerformed(InputAction.CallbackContext obj)
    {
        heldObj.transform.SetParent(null);
        heldObj.thisRigid.isKinematic = false;
        heldObj.thisCollider.enabled = true;
        StartCoroutine(ReturnItem());
    }

    private void OnTriggerEnter(Collider other)
    {
        EnableInteract(other);
    }

    private void OnTriggerStay(Collider other)
    {
        EnableInteract(other);
    }

    private void OnTriggerExit(Collider other)
    {
        print("Cant interact with " + currentObj);
        currentObj = null;
        _controls.Main.Interact.performed -= InteractOnPerformed;
    }

    private void EnableInteract(Collider other)
    {
        if (currentObj != null) return;
        print("Press E to interact");
        currentObj = other.gameObject;
        activeInteractive = currentObj.GetComponent<Interactive>();
        _controls.Main.Interact.performed += InteractOnPerformed;
     }

    private void InteractOnPerformed(InputAction.CallbackContext context)
    {
        
        StartCoroutine(Interact());
    }

    private IEnumerator Interact()
    {
        _controls.Main.Interact.Disable();
        print("You interacted!");
        yield return null;
        var waitTime = activeInteractive.Interact(this);
        yield return waitTime;
        print("You can interact again.");
        _controls.Main.Interact.Enable();
    }

    private IEnumerator ReturnItem()
    {
        var obj = heldObj;
        yield return null;
        yield return FuzzyWait.ForFiveSeconds();
        obj.thisRigid.velocity = Vector3.zero;
        obj.transform.position = obj.origPos;
        print("Return!");
    }
}
