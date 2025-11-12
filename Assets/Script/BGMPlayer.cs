using UnityEngine;
using System;
using CriWare;

public class BGMPlayer : MonoBehaviour
{
    public CriAtomSource bgmSource;
    public CriAtomExPlayback pb;


    void Start()
    {
        Debug.Log("bgmSource:start");
        //bgmSource.cueSheet = "BGM_CueSheet";
        //bgmSource.cueName = "StageBGM01";
        //bgmSource.cueSheet = "BGM_Maou_CueSheet";//AudioManagerÇ≈ìoò^ÇµÇΩéØï ñº
        //bgmSource.cueName = "maou_bgm_fantasy13";//AtomcraftÇ≈ê›íËÇµÇΩacb ì‡ÇÃ Cue ñº

        pb = bgmSource.Play();
        Debug.Log(pb.status);
    }
}
