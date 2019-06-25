using UnityEngine;

namespace FuzzyTools
{
   public class FuzzyMonoBehaviour : MonoBehaviour
   {
      private MonoBehaviour[] _thisScript;
      private void OnValidate()
      {
         _thisScript = FindObjectsOfType<MonoBehaviour>();
         Getter.GetThatComponent<AutoGetAttribute>(_thisScript, "GetComponent");
         
      }
   }
}