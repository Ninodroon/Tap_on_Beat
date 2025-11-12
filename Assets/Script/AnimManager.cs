using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimManager : MonoBehaviour
{
    private Animator animator;
    void OnEnable()
    {
        animator = GetComponent<Animator>();
        AdxMarkerBroadcaster.OnMarkerReceived += OnMarker;
    }

    void OnDisable()
    {
        AdxMarkerBroadcaster.OnMarkerReceived -= OnMarker;
    }

    private void OnMarker(string tag)
    {
        if (tag == "JUMP") animator.SetTrigger("Jump");
        if (tag == "BACK") animator.SetTrigger("BackBeat");
    }
}
