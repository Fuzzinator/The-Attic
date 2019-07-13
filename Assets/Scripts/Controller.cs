using System.Collections;
using FuzzyTools;
using UnityEngine;
using UnityEngine.InputSystem;

public class Controller : MonoBehaviour
{
    public Transform mainCamera;
    public Transform headPos;
    public Transform crouchPos;
    public Transform pronePos;
    [ReadOnly] [SerializeField] private bool crouching = false;
    [ReadOnly] [SerializeField] private bool prone = false;
    public static PlayerControlls Controls;
    private const float MoveSpeed = .1f;
    private void Awake()
    {
        Controls = new PlayerControlls();
        Controls.Main.Crouch.Enable();
        Controls.Main.Prone.Enable();
        Controls.Main.Crouch.performed += CrouchOnPerformed;
        Controls.Main.Prone.performed += ProneOnPerformed;
    }

    private void CrouchOnPerformed(InputAction.CallbackContext obj)
    {
        Controls.Main.Crouch.Disable();
        StartCoroutine(Crouch());
    }

    private void ProneOnPerformed(InputAction.CallbackContext obj)
    {
        print("Prone");
        Controls.Main.Prone.Disable();
        StartCoroutine(Prone());
    }

    private IEnumerator Crouch()
    {
        yield return null;
        
        var current = mainCamera.localPosition;
        var target = crouching ? headPos.localPosition : crouchPos.localPosition;
        
        for (var f = 0f; f < 1; f+= MoveSpeed)
        {
            yield return null;
            mainCamera.localPosition = Vector3.Lerp(current, target, f);
        }

        crouching = !crouching;
        if (prone) prone = false;
        Controls.Main.Crouch.Enable();
    }
    
    private IEnumerator Prone()
    {
        yield return null;
        
        var current = mainCamera.localPosition;
        var target = prone ? headPos.localPosition : pronePos.localPosition;
        
        for (var f = 0f; f < 1; f+= MoveSpeed)
        {
            yield return null;
            mainCamera.localPosition = Vector3.Lerp(current, target, f);
        }

        prone = !prone;
        if (crouching) crouching = false;
        Controls.Main.Crouch.Enable();
    }
}
