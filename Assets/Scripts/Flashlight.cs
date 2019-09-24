using System.Collections;
using System.Collections.Generic;
using FuzzyTools;
using UnityEngine;
using UnityEngine.InputSystem;

public class Flashlight : MonoBehaviour
{
   public Light flashlight;
   [SerializeField]
   [ReadOnly]
   private AudioSource audioSource;
   
   private bool _flashlightOn;

   private void Awake()
   {
      Controller.Controls.Main.Flashlight.Enable();
      Controller.Controls.Main.Flashlight.performed += UseFlashlight;
   }

   private void UseFlashlight(InputAction.CallbackContext obj)
   {
      _flashlightOn = !_flashlightOn;
      flashlight.enabled = _flashlightOn;
   }
}
