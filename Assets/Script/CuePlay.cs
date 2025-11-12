using UnityEngine;
using System;
using CriWare;

public class CuePlay : MonoBehaviour
{
    private CriAtomSource atomSrc;

    public UnityEngine.UI.Slider volSlider;
    public UnityEngine.UI.Slider pitchSlider;

    void Start() {
        /* CriAtomSource を取得 */
        atomSrc = (CriAtomSource)GetComponent("CriAtomSource");

    }

    public void PlaySound() {
        if (atomSrc != null) {
            atomSrc.Play();
        }

        if (atomSrc == null) { 
            Debug.Log("atomSrc空");
        }
    }

    public void PlayAndStopSound() {
        if (atomSrc != null) {
            /* CriAtomSource の状態を取得 */
            CriAtomSource.Status status = atomSrc.status;
            if ((status == CriAtomSource.Status.Stop) || (status == CriAtomSource.Status.PlayEnd)) {
                /* 停止状態なので再生 */
                atomSrc.Play();
            } else {
                /* 再生中なので停止 */
                atomSrc.Stop();
            }
        }
    }

    /* イベントコールバック用関数を追加 */
    public void OnVolSliderChanged()
    {
        atomSrc.volume = volSlider.value;
    }

    public void OnPitchSliderChanged()
    {
        atomSrc.pitch = pitchSlider.value;
    }
}
