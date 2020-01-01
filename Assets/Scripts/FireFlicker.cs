using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class FireFlicker : MonoBehaviour
{
    public Light thisLight;

    public float minSpeed = .05f;
    public float maxSpeed = .3f;
    public float minBrightness = .35f;
    public float maxBrightness = 1;

    private float flickerSpeed;
    private float origBrightness;

    private void OnValidate()
    {
        if (thisLight == null)
        {
            thisLight = GetComponent<Light>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        origBrightness = thisLight.intensity;
        StartCoroutine(FlickerLight());
    }

    private IEnumerator FlickerLight()
    {
        yield return null;
        while (true)
        {
            var targetIntense = Random.Range(minBrightness, maxBrightness);
            var changeLength = Random.Range(minSpeed, maxSpeed);

            //var normalized = (changeLength - minSpeed) / (changeLength - minSpeed);
            var currentIntense = thisLight.intensity;
            for (var i = 0f; i < 1; i += changeLength)
            {
                var newBrightness = Mathf.Lerp(currentIntense, targetIntense, i);
                thisLight.intensity = newBrightness;
                yield return null;
            }

            yield return null;
        }
    }
}
