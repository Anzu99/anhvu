using mygame.sdk;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingUI : UIBase
{
    [Header("Setting UI")]
    [SerializeField] private Button soundBtn;
    [SerializeField] private Button music;
    [SerializeField] private Button vibrate;
    [SerializeField] private Button rate;

    [SerializeField] private GameObject tickSound;
    [SerializeField] private GameObject tickMusic;
    [SerializeField] private GameObject tickVibrate;

    [SerializeField] Text version;
    protected override void SetUp()
    {
        base.SetUp();
        soundBtn.onClick.AddListener(OnClickSound);
        music.onClick.AddListener(OnClickMusic);
        vibrate.onClick.AddListener(OnClickVibrate);
        rate.onClick.AddListener(OnClickRate);
        version.text = "v " + Application.version;
    }

    public override void Show()
    {
        base.Show();
        CheckStatus();
    }

    void OnClickSound()
    {
        SoundManager.soundEnable = !SoundManager.soundEnable;
        CheckStatus();
    }

    void OnClickVibrate()
    {
        PlayerPrefsUtil.VibrateSetting = PlayerPrefsUtil.VibrateSetting < 1 ? 1 : 0;
        CheckStatus();
    }

    void OnClickMusic()
    {
        SoundManager.musicEnable = !SoundManager.musicEnable;
        CheckStatus();
    }
    void OnClickRate()
    {
        //ratePopup.SetActive(true);
    }

    void CheckStatus()
    {
        tickSound.SetActive(SoundManager.soundEnable);
        tickMusic.SetActive(SoundManager.musicEnable);
        if (PlayerPrefsUtil.VibrateSetting == 0)
        {
            tickVibrate.SetActive(false);
        }
        else
        {
            tickVibrate.SetActive(true);
        }
    }
}
