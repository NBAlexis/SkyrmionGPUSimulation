using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMenu : UIPage
{


    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }

    public override void Show(CUIPageConfig config)
    {
        base.Show(config);
        m_pOwner.m_txtNavBackButton.text = "返回";

        for (int i = 0; i < m_pAnims.Length; ++i)
        {
            m_pAnims[i]["LeftSlideIn"].normalizedTime = 0.0001f;
            m_pAnims[i]["LeftSlideIn"].speed = 0.0001f;
            m_pAnims[i]["LeftSlideIn"].wrapMode = WrapMode.Clamp;
            m_pAnims[i].Play("LeftSlideIn");
        }
    }

    public override void ToHide()
    {
        base.ToHide();

        for (int i = 0; i < m_pAnims.Length; ++i)
        {
            m_pAnims[i]["LeftSlideIn"].normalizedTime = 1.0f;
            m_pAnims[i]["LeftSlideIn"].speed = -1.0f;
            m_pAnims[i]["LeftSlideIn"].wrapMode = WrapMode.Clamp;
            m_pAnims[i].Play("LeftSlideIn");
        }
    }

    #region Events

    public override void OnButtonBack()
    {
        if (m_fAnimTime > 0.0f)
        {
            return;
        }

        base.OnButtonBack();
        GoPage(EPage.Main, null);
    }

    public void OnEnterExp1()
    {
        if (m_fAnimTime > 0.0f)
        {
            return;
        }
    }

    public void OnEnterExp2()
    {
        if (m_fAnimTime > 0.0f)
        {
            return;
        }
    }

    public void OnEnterExpFree()
    {
        if (m_fAnimTime > 0.0f)
        {
            return;
        }

        CUIPageConfigSimulate freeExp = CUIPageConfigSimulate.GetDefault();
        freeExp.m_sTitle = "自由探索";
        GoPage(EPage.Simulate, freeExp);
    }

    public override void FadeIn(float fAnimTime)
    {
        base.FadeIn(fAnimTime);

        float fSep = 0.1f;
        for (int i = 0; i < m_pAnims.Length; ++i)
        {
            if (m_pAnims[i]["LeftSlideIn"].speed < 0.5f && m_fAnimTime + fSep * i < 1.0f)
            {
                m_pAnims[i]["LeftSlideIn"].normalizedTime = 0.0f;
                m_pAnims[i]["LeftSlideIn"].speed = 1.0f;
                m_pAnims[i]["LeftSlideIn"].wrapMode = WrapMode.Clamp;
                m_pAnims[i].Play("LeftSlideIn");
            }
        }
    }

    #endregion
}
