using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBase : MonoBehaviour
{
    public Button closeButton;
    internal bool isEnd;
    void Awake()
    {
        SetUp();
    }
    protected virtual void SetUp()
    {
        if (closeButton != null) closeButton.onClick.AddListener(OnClickClose);
    }

    protected virtual void OnClickClose()
    {
        Hide();
    }
    public virtual void Show()
    {
        gameObject.SetActive(true);
    }

    protected virtual void Hide(bool hasShowOutAnim = false)
    {
        if (hasShowOutAnim)
        {
            GetComponent<Animation>().Play("Show_Out");
            StartCoroutine(DelayShowOut());
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    IEnumerator DelayShowOut()
    {
        yield return new WaitForSeconds(0.5f);
        gameObject.SetActive(false);
    }
}
