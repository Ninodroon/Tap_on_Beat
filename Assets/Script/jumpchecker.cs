using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class jumpchecker : MonoBehaviour
{
    private Renderer rend;
    private Color originalColor;
    private Color beatColor = Color.red;
    public DededeJump2 player;

   
     void Awake()
    {
        rend = GetComponent<Renderer>();
        originalColor = Color.white;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        //UnityEngine.Debug.Log($"èÛë‘{player.currentJumpPhase}");

        if (player.currentJumpPhase == DededeJump2.JumpPhase.Rising)
            {
                rend.material.color = Color.green;
            }
            else
            {
                rend.material.color = originalColor;
            }
        
            if (player.currentJumpPhase == DededeJump2.JumpPhase.Stay)
            {
                rend.material.color = Color.blue;
            }
            else
            {
                rend.material.color = originalColor;
            }
        

            if (player.currentJumpPhase == DededeJump2.JumpPhase.Falling)
            {
                rend.material.color = Color.red;
            }
            else
            {
                rend.material.color = originalColor;
            }
        


    }
}
