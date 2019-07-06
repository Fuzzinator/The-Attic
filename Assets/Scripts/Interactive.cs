using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.XPath;
using FuzzyTools;
using UnityEngine;
using Random = UnityEngine.Random;

public enum InteractType
{
    Basic,
    Story,
    Key,
    Lock,
    Puzzle
}
public class Interactive : FuzzyMonoBehaviour
{
    public InteractType type;
    public KeyTarget target;
    public AudioClip clip_1;
    public AudioClip clip_2;
    //public bool triggerMemory = false;
    public bool useCustomWaitTimes = false;
    public float customWaitTime1 = 1;
    public float customWaitTime2 = 1;

    //public bool collectible = false;
    [ReadOnly]public Vector3 origPos;
    [ReadOnly] public Quaternion origRot;
    [AutoGet] public AudioSource source;
    [AutoGet] public Animator animator;
    public AnimationClip animClip;
    [ReadOnly] [AutoGet] public Rigidbody thisRigid;
    [AutoGet] [ReadOnly] public BoxCollider thisCollider;
    [ReadOnly] [SerializeField] private bool _interacted = false;
    private WaitForSeconds _wait1;
    private WaitForSeconds _wait2;
    private AudioPool _pool;
    private int _invalidLength;
    private int _lockedLength;
    private int _keysLength;
    

    public enum KeyTarget
    {
        none,
        Briefcase,
        FilingCabinet,
        RollerTop,
        SteamerTrunk
    }
    
    private void Start()
    {
        if(!useCustomWaitTimes)
        {
            //animator.GetNextAnimatorClipInfo()
            if(animClip)_wait1 = new WaitForSeconds(animClip.length);//clip_1.length);
            else _wait1 = new WaitForSeconds(clip_1.length);
            _wait2 = _wait1;//new WaitForSeconds(clip_2.length);
        }
        else
        {
            _wait1 = new WaitForSeconds(customWaitTime1);
            _wait2 = new WaitForSeconds(customWaitTime2);
        }
        //print(Animator.StringToHash("Interact"));
        //-662453572

        //print(Animator.StringToHash("Locked"));
        //-310456921

        //print(Animator.StringToHash("Unlocked"));
        //-125993919

        
//        if (collectible && thisRigid != null)
//        {
//            
//        }
        var t = transform;
        origPos = t.position;
        origRot = t.rotation;

        _pool = AudioPool.Instance;
        _invalidLength = _pool.invalidInteract.Count - 1;
        _lockedLength = _pool.targetLocked.Count - 1;
        _keysLength = _pool.manyKeys.Count - 1;
    }

    public WaitForSeconds Interact(Interactions player)
    {
        
        //if (collectible)
        //{
        //    thisRigid.isKinematic = true;
        //    
        //    //gameObject.AddComponent<FixedJoint>();
        //}

        var held = player.heldObj;

        switch (type)
        {
            case InteractType.Basic:
                if (held)
                {
                    var rand = _pool.invalidInteract[Random.Range(0, _invalidLength)];
                    source.PlayOneShot(rand);
                    print("That doesn't do anything");
                    return _pool.invalidWaits[rand];
                }
                if (_interacted)
                {
                    animator.SetTrigger(-662453572);
                    //if (_wait1 == null) _wait1 = new WaitForSeconds(animator.GetCurrentAnimatorClipInfo(0)[0].clip.length);
                    _interacted = !_interacted;
                    source.PlayOneShot(clip_1);
                    print("Playing Clip 1");
                    return _wait1;
                }
                else
                {
                    animator.SetTrigger(-662453572);
                    //if (_wait2 == null) _wait2 = new WaitForSeconds(animator.GetCurrentAnimatorClipInfo(0)[0].clip.length);
                    _interacted = !_interacted;
                    source.PlayOneShot(clip_2);
                    print("Playing Clip 2");
                    return _wait2;
                }
            case InteractType.Story:

                if (_interacted)
                {
                    //undecided what to do here possibly activate expanding bloom like screen whiting out and playing audio.
                    //maybe visually show stuff or isolate the interacted object. 
                }
                else
                {
                    //Secondary story
                }

                break;
            case InteractType.Key:
                if (!held)
                {
                    var t = transform;
                    t.SetParent(player.holdPos, true);
                    t.localPosition = Vector3.zero;
                    thisRigid.isKinematic = true;
                    thisCollider.enabled = false;
                    player.heldObj = this;
                    source.PlayOneShot(clip_1);
                    print("Playing Clip 1");
                    player.DisableInteract();
                    return _wait1;
                }
                else
                {
                    var rand = _pool.manyKeys[Random.Range(0, _keysLength)];
                    source.PlayOneShot(rand);
                    return _pool.invalidWaits[rand];
                    //transform.SetParent(null, true);
                    //thisRigid.isKinematic = false;
                    //source.PlayOneShot(clip_2);
                    //print("Playing Clip 2");
                    //return _wait2;
                }
            case InteractType.Lock:
                if (held != null)
                {
                    if (held.type == InteractType.Key && held.target == target)
                    {
                        held.gameObject.SetActive(false);
                        player.heldObj = null;
                        type = InteractType.Basic;
                        var rand = _pool.unlocks[Random.Range(0, _invalidLength)];
                        source.PlayOneShot(rand);
                        animator.SetTrigger(-125993919);
                        print("You unlocked the thing!");
                        return _pool.unlockWaits[rand];
                    }
                    if( held.type == InteractType.Key )
                    {
                        var rand = _pool.invalidInteract[Random.Range(0, _lockedLength)];
                        source.PlayOneShot(rand);
                        print("The key doesnt fit");
                        return _pool.invalidWaits[rand];
                    }
                    else
                    {
                        var rand = _pool.invalidInteract[Random.Range(0, _lockedLength)];
                        source.PlayOneShot(rand);
                        print("The thing you're holding is bad");
                        return _pool.invalidWaits[rand];
                    }
                }
                else
                {
                    var rand = _pool.targetLocked[Random.Range(0, _invalidLength)];
                    source.PlayOneShot(rand);
                    animator.SetTrigger(-310456921);
                    print("It is locked");
                    return _pool.lockedWaits[rand];
                }
            case InteractType.Puzzle:
                
                break;

        }

        

        return null;
    }
}
