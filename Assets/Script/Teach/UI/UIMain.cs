using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMain : UIPage
{

    public override void Show(CUIPageConfig config)
    {
        base.Show(config);
        m_pOwner.m_txtNavBackButton.text = "退出";
        Debug.Log(m_pAnims[0]);
        Debug.Log(m_pAnims[0]["TitleDown"]);
        for (int i = 0; i < m_pAnims.Length; ++i)
        {
            m_pAnims[i]["TitleDown"].normalizedTime = 0.0f;
            m_pAnims[i]["TitleDown"].speed = 1.0f;
            m_pAnims[i]["TitleDown"].wrapMode = WrapMode.Clamp;
            m_pAnims[i].Play("TitleDown");
        }
    }

    public override void ToHide()
    {
        base.ToHide();

        for (int i = 0; i < m_pAnims.Length; ++i)
        {
            m_pAnims[i]["TitleDown"].normalizedTime = 1.0f;
            m_pAnims[i]["TitleDown"].speed = -1.0f;
            m_pAnims[i]["TitleDown"].wrapMode = WrapMode.Clamp;
            m_pAnims[i].Play("TitleDown");
        }
    }

    #region Components



    #endregion

    #region Events

    public void OnButtonStart()
    {
        if (m_fAnimTime > 0.0f)
        {
            return;
        }

        UISoundManager.UISound(EUISound.Button);
        GoPage(EPage.Menu, null);
    }

    public override void OnButtonBack() 
    {
        if (m_fAnimTime > 0.0f)
        {
            return;
        }

        base.OnButtonBack();
        m_pOwner.ShowTwoButtonDialog("真的要退出吗?", RealQuit, "是的", "不是");
    }

    public void RealQuit(bool bQuit)
    {
        if (bQuit)
        {
            Application.Quit();
        }
    }

    #endregion
}
