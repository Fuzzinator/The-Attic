using System.Collections.Generic;
using UnityEngine;

namespace FuzzyTools
{
    public class MeshMergerPolicyList : MonoBehaviour
    {
        public enum PolicyType
        {
            Ignore,
            Include
        }

        public enum CheckTypes
        {
            Tag = 1,
            Script = 2,
            Layer = 4
        }

        public PolicyType policyType = PolicyType.Ignore;
        public CheckTypes checkType = CheckTypes.Layer;
        public List<string> identifiers = new List<string>();

        public bool CheckPolicy(GameObject obj)
        {
            return CheckType(obj, policyType == PolicyType.Include);
        }

        private bool CheckType(GameObject obj, bool state)
        {
            var sel = 0;
            if (((int) checkType & (int) CheckTypes.Tag) != 0)
            {
                sel += 1;
            }

            if (((int) checkType & (int) CheckTypes.Script) != 0)
            {
                sel += 2;
            }

            if (((int) checkType & (int) CheckTypes.Layer) != 0)
            {
                sel += 4;
            }

            switch (sel)
            {
                case 1:
                    return CheckTag(obj, state);
                case 2:
                    return CheckScript(obj, state);
                case 3:
                    var tagCheck3 = CheckTag(obj, state);
                    if ((state && tagCheck3) || (!state && !tagCheck3)) return state;
                    var scriptCheck3 = CheckScript(obj, state);
                    if ((state && scriptCheck3) || (!state && !scriptCheck3)) return state;
                    break;
                case 4:
                    return CheckLayer(obj, state);
                case 5:
                    var tagCheck5 = CheckTag(obj, state);
                    if ((state && tagCheck5) || (!state && !tagCheck5)) return state;
                    var layerCheck5 = CheckLayer(obj, state);
                    if ((state && layerCheck5) || (!state && !layerCheck5)) return state;
                    break;
                case 6:
                    var scriptCheck6 = CheckScript(obj, state);
                    if ((state && scriptCheck6) || (!state && !scriptCheck6)) return state;
                    var layerCheck6 = CheckLayer(obj, state);
                    if ((state && layerCheck6) || (!state && !layerCheck6)) return state;
                    break;
                case 7:
                    var tagCheck7 = CheckTag(obj, state);
                    if ((state && tagCheck7) || (!state && !tagCheck7)) return state;
                    var scriptCheck7 = CheckScript(obj, state);
                    if ((state && scriptCheck7) || (!state && !scriptCheck7)) return state;
                    var layerCheck7 = CheckLayer(obj, state);
                    if ((state && layerCheck7) || (!state && !layerCheck7)) return state;
                    break;
            }

            return !state;
        }

        private bool CheckTag(GameObject obj, bool state)
        {
            return state ? identifiers.Contains(obj.tag) : !identifiers.Contains(obj.tag);
        }

        private bool CheckLayer(GameObject obj, bool state)
        {
            return state
                ? identifiers.Contains(LayerMask.LayerToName(obj.layer))
                : !identifiers.Contains(LayerMask.LayerToName(obj.layer));
        }

        private bool CheckScript(GameObject obj, bool state)
        {
            foreach (var ident in identifiers)
            {
                if (!obj.GetComponent(ident)) continue;
                return state;
            }

            return !state;
        }

    }
}