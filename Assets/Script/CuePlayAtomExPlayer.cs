using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CriWare;

public class CuePlayAtomExPlayer : MonoBehaviour
{
    private CriAtomEx.CueInfo[] cueInfoList;
    private CriAtomExPlayer atomExPlayer;
    private CriAtomExAcb atomExAcb;

    IEnumerator Start()
    {
        /* キューシートファイルのロード待ち */
        while (CriAtom.CueSheetsAreLoading)
        {
            yield return null;
        }

        /* AtomExPlayerの生成 */
        atomExPlayer = new CriAtomExPlayer();

        /* Cue情報の取得 */
        atomExAcb = CriAtom.GetAcb("maou_bgm_fantasy13");
        cueInfoList = atomExAcb.GetCueInfoList();

        /* プレーヤーにキューを設定して再生開始 */
        for (int i = 0; i <= 1; i++)
        {
            atomExPlayer.SetCue(atomExAcb, cueInfoList[i].name);
            atomExPlayer.Start();
        }
    }
    private void OnDestroy()
    {
        atomExPlayer.Dispose();
    }

    void OnGUI()
    {
        /* キュー名再生ボタンの生成 */
        for (int i = 0; i < cueInfoList.Length; i++)
        {
            if (GUI.Button(new Rect(Screen.width - 150, (Screen.height / cueInfoList.Length) * i, 150, Screen.height / cueInfoList.Length), cueInfoList[i].name))
            {
                /* 再生中の場合は停止 */
                if (atomExPlayer.GetStatus() == CriAtomExPlayer.Status.Playing)
                {
                    atomExPlayer.Stop();
                }
                atomExPlayer.SetCue(atomExAcb, cueInfoList[i].name);
                atomExPlayer.Start();
            }
        }
    }
}