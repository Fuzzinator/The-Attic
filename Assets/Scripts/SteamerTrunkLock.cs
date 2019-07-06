using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class SteamerTrunkLock : MonoBehaviour
{
   public Collider trunkLock;
   public Animator anim;

   public void UnlockTrunk()
   {
      anim.SetBool("Locked", false);
      trunkLock.enabled = false;
   }
}
