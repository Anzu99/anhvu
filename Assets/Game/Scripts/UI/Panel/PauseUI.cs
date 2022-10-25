using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseUI : UIBase
{
    [Header("Pause UI")]
    [SerializeField] Button resumeBtn;
    [SerializeField] Button restartBtn;

    protected override void SetUp()
    {
        base.SetUp();
        resumeBtn.onClick.AddListener(OnResume);
        restartBtn.onClick.AddListener(OnClickRestart);
    }

    protected override void OnClickClose()
    {
        base.OnClickClose();
    }
    public override void Show()
    {
        base.Show();
    }

    void OnResume()
    {
        Hide();
    }

    void OnClickRestart()
    {
        Hide();
    }
}
