using System.Collections;
using FuzzyTools;
using UnityEngine;
using UnityEngine.InputSystem;

public class Controller : MonoBehaviour
{
    public Transform mainCamera;
    public Transform headPos;
    public Transform crouchPos;
    [ReadOnly] [SerializeField] private bool crouching = false;
    public static PlayerControlls Controls;
    private const float MoveSpeed = .1f;
    private void Awake()
    {
        Controls = new PlayerControlls();
        Controls.Main.Crouch.Enable();
        Controls.Main.Crouch.performed += CrouchOnPerformed;
    }

    private void CrouchOnPerformed(InputAction.CallbackContext obj)
    {
        Controls.Main.Crouch.Disable();
        StartCoroutine(Crouch());
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
        Controls.Main.Crouch.Enable();
    }
}
