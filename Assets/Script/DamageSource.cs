using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageSource : MonoBehaviour
{
    public int damage = 1;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(DamageSequence());

        }

    }


    IEnumerator DamageSequence()
    {
        // ì_ñ≈Åi2ïbÅj
        yield return StartCoroutine(Blink(1f));

    }

    IEnumerator Blink(float duration)
    {
        float t = 0f;
        Renderer rend = GetComponentInChildren<Renderer>();

        while (t < duration)
        {
            rend.enabled = !rend.enabled;
            yield return new WaitForSeconds(0.1f);
            t += 0.1f;
        }

        rend.enabled = true;
    }

}