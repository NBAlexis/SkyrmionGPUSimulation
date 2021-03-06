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

        CUIPageConfigSimulate exp1 = GetExp1();
        GoPage(EPage.Simulate, exp1);
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

    #region Experiments

    private static CUIPageConfigSimulate GetExp1()
    {
        CUIPageConfigSimulate config = new CUIPageConfigSimulate();

        COneParameterConfig alpha = new COneParameterConfig();
        alpha.m_fDefault = 0.2f;
        alpha.m_bEnable = false;

        COneParameterConfig step = new COneParameterConfig();
        step.m_fDefault = 0.1f;
        step.m_bEnable = false;

        COneParameterConfig j = new COneParameterConfig();
        j.m_fDefaultMin = 1.0f;
        j.m_sImage = "0";
        j.m_bEnable = false;

        COneParameterConfig d = new COneParameterConfig();
        d.m_fDefaultMin = 0.2f;
        d.m_sImage = "0";
        d.m_bEnable = true;
        d.m_bEnableMax = false;
        d.m_bEnableImage = false;

        COneParameterConfig b = new COneParameterConfig();
        b.m_fDefaultMin = 0.0f;
        b.m_sImage = "0";
        b.m_bEnable = true;
        b.m_bEnableMax = false;
        b.m_bEnableImage = false;

        COneParameterConfig k = new COneParameterConfig();
        k.m_fDefaultMin = 0.0f;
        k.m_sImage = "0";
        k.m_bEnable = false;
        k.m_bEnableMax = false;
        k.m_bEnableImage = false;

        COneParameterConfig el = new COneParameterConfig();
        el.m_bEnable = false;

        COneParameterConfig bound = new COneParameterConfig();
        bound.m_sImage = "Open";
        bound.m_bEnable = false;

        config.m_sTitle = "实验1：Skyrmion相变";
        config.m_sConfiguration = "Noise";
        config.m_ConfigAlpha = alpha;
        config.m_ConfigStep = step;
        config.m_ConfigBoundary = bound;
        config.m_ConfigJ = j;
        config.m_ConfigD = d;
        config.m_ConfigB = b;
        config.m_ConfigK = k;
        config.m_ConfigElect = el;

        return config;
    }

    #endregion
}
