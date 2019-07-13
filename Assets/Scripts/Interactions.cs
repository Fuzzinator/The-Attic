using System;
using System.Collections;
using System.Collections.Generic;
using FuzzyTools;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class Interactions : FuzzyMonoBehaviour
{
    public Transform holdPos;
    public GameObject interactIcon;
    public Material highlight;
    [ReadOnly] public GameObject currentObj;
    [ReadOnly] public Interactive heldObj;
    [SerializeField] [ReadOnly] private Interactive activeInteractive;
    [AutoGet] [SerializeField] [ReadOnly] private BoxCollider _trigger;
    private Renderer[] _currentRends;

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
        
        DisableInteract();
    }

    private void EnableInteract(Collider other)
    {
        if (currentObj != null) return;
        print("Press E to interact");
        currentObj = other.gameObject;
        activeInteractive = currentObj.GetComponent<Interactive>();
        if (activeInteractive == null)
        {
            currentObj = other.transform.parent.gameObject;
            activeInteractive = currentObj.GetComponent<Interactive>();
        }

        if (activeInteractive == null)
        {
            DisableInteract();
            return;
        }
        interactIcon.SetActive(true);
        _currentRends = activeInteractive.rends;
        foreach (var rend in _currentRends)
        {
            var mats = new List<Material>();
            mats.AddRange(rend.sharedMaterials);
            mats.Add(highlight);
            rend.sharedMaterials = mats.ToArray();
        }
        _controls.Main.Interact.performed += InteractOnPerformed;
     }

    public void DisableInteract()
    {
        print("Cant interact with " + currentObj);
        currentObj = null;
        foreach (var rend in _currentRends)
        {
            var mats = new List<Material>();
            mats.AddRange(rend.sharedMaterials);
            mats.Remove(highlight);
            rend.sharedMaterials = mats.ToArray();
        }
        activeInteractive = null;
        interactIcon.SetActive(false);
        _controls.Main.Interact.performed -= InteractOnPerformed;
    }

    private void InteractOnPerformed(InputAction.CallbackContext context)
    {
        
        StartCoroutine(Interact());
    }

    private IEnumerator Interact()
    {
        //_controls.Main.Interact.Disable();
        
        print("You interacted!");
        yield return null;
        var waitTime = activeInteractive.Interact(this);
        DisableInteract();
        yield return waitTime;
        print("You can interact again.");
        _controls.Main.Interact.Enable();
    }

    private IEnumerator ReturnItem()
    {
        var obj = heldObj;
        heldObj = null;
        yield return null;
        yield return FuzzyWait.ForFiveSeconds();
        obj.thisRigid.isKinematic = true;
        obj.thisRigid.velocity = Vector3.zero;
        var t = obj.transform;
        t.position = obj.origPos;
        t.rotation = obj.origRot;
        print("Return!");
    }
}
