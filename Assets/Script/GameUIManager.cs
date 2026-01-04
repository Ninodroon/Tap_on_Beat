using CriWare;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameUIManager : MonoBehaviour
{
    // Start is called before the first frame update
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timeText;

    private float elapsedTime = 0f;
    private bool isCounting = false;

    void Start()
    {
        UpdateUI();
    }

    void Update()
    {
        // 音楽が再生中ならカウント
        if (DededeJump2.Instance != null && DededeJump2.Instance.music.status == CriAtomSource.Status.Playing)
        {
            if (!isCounting)
            {
                isCounting = true;
            }
            elapsedTime += Time.deltaTime;
        }
        else
        {
            isCounting = false;
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        if (DededeJump2.Instance != null)
        {
            hpText.text =    $"HP: {DededeJump2.Instance.GetHP()}";
            scoreText.text = $"Score: {DededeJump2.Instance.GetScore()}";
        }

        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        timeText.text =      $"Time: {minutes:00}:{seconds:00}";
    }
}
