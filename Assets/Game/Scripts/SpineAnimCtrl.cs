using Spine.Unity;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpineAnimCtrl : MonoBehaviour
{
    //private PlayerCtrl playerCtrl;
    public SkeletonAnimation skeletonAnimation;
    public AnimState animState { get; set; }

    public string curAnimName = "";
    public string curAnimName2 = "";

    private void Awake()
    {

    }
    private void Start()
    {
        //playerCtrl = GetComponent<PlayerCtrl>();
        animState = AnimState.IDLE;
    }

    public void PlayAnim(string curAnim, int track = 0)
    {
        if (curAnim == curAnimName) return;
        curAnimName = curAnim;
        skeletonAnimation.state.SetAnimation(track, curAnim, true);
    }

    public void PlayAnim2(string curAnim, int track = 1)
    {
        if (curAnim == curAnimName2) return;
        curAnimName2 = curAnim;
        this.skeletonAnimation.state.SetAnimation(track, curAnim, true);
    }
    void ClearTrack2()
    {
        if (!isClear)
        {
            isClear = true;
            skeletonAnimation.state.ClearTrack(1);
        }
    }

    bool isClear = false;
    int _index = 0;
    private void Update()
    {
        //if (playerCtrl.rb2D.velocity.magnitude > 0)
        //{
        //    animState = AnimState.RUN;
        //}
        //else
        //{
        //    animState = AnimState.IDLE;
        //}
        switch (animState)
        {
            case AnimState.IDLE: 
                {
                    PlayAnim("Idle");
                    break;
                }
            case AnimState.RUN:
                {
                    PlayAnim("Run_loop");
                    break;
                }
            case AnimState.JUMP:
                {
                    break;
                }
            case AnimState.CELEBRATE:
                {

                    break;
                }
            case AnimState.CHEER:
                {
                    PlayAnim("Cheer");
                    break;
                }
            case AnimState.PUSHING:
                {
                    PlayAnim("Push");
                    break;
                }
            case AnimState.DEAD:
                {
                    PlayAnim("Die");
                    break;
                }
        }
    }
}

public enum AnimState
{
    IDLE,
    RUN,
    JUMP,
    CHEER,
    DEAD,
    PUSHING,
    CELEBRATE
}
