using UnityEngine;
using CriWare;

public class AudioManager : MonoBehaviour
{
    void Start()
    {

        // ACF ‚Ì“o˜^
        CriAtomEx.RegisterAcf(null, "NewProject1.acf");

        // CueSheet ‚Ì“o˜^
        CriAtom.AddCueSheet(
            "maou_bgm_fantasy13", // ”CˆÓ‚Ì¯•Ê–¼
            "Assets/StreamingAssets/CriWare/maou_bgm_fantasy13.acb",      // o—Í‚µ‚½ acb
            null       // o—Í‚µ‚½ awbi‚È‚¯‚ê‚Î nullj
        );

    }
}
