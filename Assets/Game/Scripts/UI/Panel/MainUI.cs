using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainUI : UIBase
{
    [Header("Main UI")]
    [SerializeField] private Button playBtn;
    [SerializeField] private Button settingBtn;
    [SerializeField] private Button shopBtn;

    protected override void SetUp()
    {
        base.SetUp();
        settingBtn.onClick.AddListener(OnClickSetting);
        shopBtn.onClick.AddListener(OnClickShop);
        playBtn.onClick.AddListener(OnClickPlay);
    }

    void OnClickSetting()
    {
        UIManager.Instance.ShowUI<SettingUI>(UIManager.UIName.SETTING);
    }

    void OnClickPlay()
    {
        UIManager.Instance.ShowUI<GameplayUI>(UIManager.UIName.GAMEPLAY);
    }

    void OnClickShop()
    {
        UIManager.Instance.ShowUI<GameplayUI>(UIManager.UIName.SHOP);
    }
}
