using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioPool : MonoBehaviour
{
    public static AudioPool Instance;

    public List<AudioClip> invalidInteract = new List<AudioClip>();
    public Dictionary<AudioClip, WaitForSeconds> invalidWaits = new Dictionary<AudioClip, WaitForSeconds>();
    public List<AudioClip> targetLocked = new List<AudioClip>();
    public Dictionary<AudioClip, WaitForSeconds> lockedWaits = new Dictionary<AudioClip, WaitForSeconds>();
    public List<AudioClip> unlocks = new List<AudioClip>();
    public Dictionary<AudioClip, WaitForSeconds> unlockWaits = new Dictionary<AudioClip, WaitForSeconds>();
    public List<AudioClip> manyKeys = new List<AudioClip>();
    public Dictionary<AudioClip, WaitForSeconds> keysWaits = new Dictionary<AudioClip, WaitForSeconds>();
    private void OnValidate()
    {
        foreach (var sound in invalidInteract)
        {
            if (!sound || invalidWaits.ContainsKey(sound)) continue;
            invalidWaits.Add(sound, new WaitForSeconds(sound.length));
        }
        foreach (var sound in targetLocked)
        {
            if (!sound || lockedWaits.ContainsKey(sound)) continue;
            lockedWaits.Add(sound, new WaitForSeconds(sound.length));
        }
        foreach (var sound in unlocks)
        {
            if (!sound || unlockWaits.ContainsKey(sound)) continue;
            unlockWaits.Add(sound, new WaitForSeconds(sound.length));
        }
        foreach (var sound in manyKeys)
        {
            if (!sound || keysWaits.ContainsKey(sound)) continue;
            keysWaits.Add(sound, new WaitForSeconds(sound.length));
        }
    }

    private void Awake()
    {
        Instance = this;
    }
}
