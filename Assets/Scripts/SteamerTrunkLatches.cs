using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class SteamerTrunkLatches : MonoBehaviour
{
   public Collider latch1;
   public Collider latch2;
   public Animator anim;

   public void UnlatchTrunk()
   {
      anim.SetBool("Latched", false);
      latch1.enabled = false;
      latch2.enabled = false;
   }
}
