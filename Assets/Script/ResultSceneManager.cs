using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResultSceneManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI hpText;

    void Start()
    {
        if (GameData.Instance != null)
        {
            scoreText.text = $"Score: {GameData.Instance.Score}";
            hpText.text = $"HP: {GameData.Instance.HP}";
        }
        else
        {
            scoreText.text = "SCORE : 0";
            hpText.text =    "HP    : 0";
        }
    }
}
