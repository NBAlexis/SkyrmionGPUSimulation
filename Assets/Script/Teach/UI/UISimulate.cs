using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ESimulateTabs
{
    Common,
    JTab,
    DTab,
    BTab,
    KTab,
    Electric,
}

public class COneParameterConfig
{
    public float m_fDefault;
    public bool m_bEnable;
    public float m_fMin;
    public bool m_bEnableMax;
    public float m_fMax;
    public float m_fDefaultMin;
    public float m_fDefaultMax;
    public bool m_bEnableImage;
    public string m_sImage;
}


public class CUIPageConfigSimulate : CUIPageConfig
{
    public string m_sTitle;
    public COneParameterConfig m_ConfigAlpha;
    public COneParameterConfig m_ConfigStep;
    public COneParameterConfig m_ConfigBoundary;
    public COneParameterConfig m_ConfigJ;
    public COneParameterConfig m_ConfigD;
    public COneParameterConfig m_ConfigB;
    public COneParameterConfig m_ConfigK;
    public COneParameterConfig m_ConfigElect;

    public static CUIPageConfigSimulate GetDefault()
    {
        COneParameterConfig alpha = new COneParameterConfig
        {
            m_sImage = "",
            m_bEnable = true,
            m_bEnableMax = true,
            m_bEnableImage = true,
            m_fDefault = 0.1f,
            m_fMin = 0.0001f,
            m_fMax = 1.0f,
            m_fDefaultMax = 1.0f,
            m_fDefaultMin = 0.0001f
        };

        COneParameterConfig step = new COneParameterConfig
        {
            m_sImage = "",
            m_bEnable = true,
            m_bEnableMax = true,
            m_bEnableImage = true,
            m_fDefault = 1.0f,
            m_fMin = 0.00001f,
            m_fMax = 10.0f,
            m_fDefaultMax = 10.0f,
            m_fDefaultMin = 0.00001f
        };

        COneParameterConfig boundary = new COneParameterConfig
        {
            m_sImage = "Periodic",
            m_bEnable = true,
            m_bEnableMax = true,
            m_bEnableImage = true,
            m_fDefault = 0.1f,
            m_fMin = 0.0001f,
            m_fMax = 1.0f,
            m_fDefaultMax = 1.0f,
            m_fDefaultMin = 0.0001f
        };

        COneParameterConfig cfgj = new COneParameterConfig
        {
            m_sImage = "0",
            m_bEnable = true,
            m_bEnableMax = true,
            m_bEnableImage = true,
            m_fDefault = 0.1f,
            m_fMin = -10.0f,
            m_fMax = 10.0f,
            m_fDefaultMax = 0.0f,
            m_fDefaultMin = 0.1f
        };

        COneParameterConfig cfgd = new COneParameterConfig
        {
            m_sImage = "0",
            m_bEnable = true,
            m_bEnableMax = true,
            m_bEnableImage = true,
            m_fDefault = 0.1f,
            m_fMin = -10.0f,
            m_fMax = 10.0f,
            m_fDefaultMax = 0.0f,
            m_fDefaultMin = 0.1f
        };

        COneParameterConfig cfgb = new COneParameterConfig
        {
            m_sImage = "0",
            m_bEnable = true,
            m_bEnableMax = true,
            m_bEnableImage = true,
            m_fDefault = 0.1f,
            m_fMin = -10.0f,
            m_fMax = 10.0f,
            m_fDefaultMax = 0.0f,
            m_fDefaultMin = 0.1f
        };

        COneParameterConfig cfgk = new COneParameterConfig
        {
            m_sImage = "0",
            m_bEnable = true,
            m_bEnableMax = true,
            m_bEnableImage = true,
            m_fDefault = 0.0f,
            m_fMin = -10.0f,
            m_fMax = 10.0f,
            m_fDefaultMax = 0.0f,
            m_fDefaultMin = 0.0f
        };

        CUIPageConfigSimulate freeExp = new CUIPageConfigSimulate();
        freeExp.m_ConfigAlpha = alpha;
        freeExp.m_ConfigStep = step;
        freeExp.m_ConfigBoundary = boundary;
        freeExp.m_ConfigJ = cfgj;
        freeExp.m_ConfigD = cfgd;
        freeExp.m_ConfigB = cfgb;
        freeExp.m_ConfigK = cfgk;

        return freeExp;
    }
}



public class UISimulate : UIPage
{
    private CUIPageConfigSimulate m_pConfigSimulation = null;
    private CSimulateParameter m_pParameter = null;

    public override void Start()
    {
        m_pConfigSimulation = CUIPageConfigSimulate.GetDefault();
        SetupTabs(m_pConfigSimulation);
        m_pParameter = GetSimulateParameter();
    }

    public override void Show(CUIPageConfig config)
    {
        base.Show(config);

        CUIPageConfigSimulate cfgSim = config as CUIPageConfigSimulate;
        m_pConfigSimulation = cfgSim;
        m_pParameter = new CSimulateParameter();

        SetupTabs(cfgSim);
        m_txtTitle.text = cfgSim.m_sTitle;
        m_txtInfo.text = "步数: 0";

        m_pParameter = GetSimulateParameter();

        for (int i = 0; i < m_pAnims.Length; ++i)
        {
            if (0 == i)
            {
                m_pAnims[i]["LeftSlideIn"].normalizedTime = 0.0001f;
                m_pAnims[i]["LeftSlideIn"].speed = 0.0001f;
                m_pAnims[i]["LeftSlideIn"].wrapMode = WrapMode.Clamp;
                m_pAnims[i].Play("LeftSlideIn");
            }
            else
            {
                m_pAnims[i]["RightSlideIn"].normalizedTime = 0.0001f;
                m_pAnims[i]["RightSlideIn"].speed = 0.0001f;
                m_pAnims[i]["RightSlideIn"].wrapMode = WrapMode.Clamp;
                m_pAnims[i].Play("RightSlideIn");
            }
        }

        OnEnterPage();
    }

    public override void ToHide()
    {
        base.ToHide();

        for (int i = 0; i < m_pAnims.Length; ++i)
        {
            if (0 == i)
            {
                m_pAnims[i]["LeftSlideIn"].normalizedTime = 1.0f;
                m_pAnims[i]["LeftSlideIn"].speed = -1.0f;
                m_pAnims[i]["LeftSlideIn"].wrapMode = WrapMode.Clamp;
                m_pAnims[i].Play("LeftSlideIn");
            }
            else
            {
                m_pAnims[i]["RightSlideIn"].normalizedTime = 1.0f;
                m_pAnims[i]["RightSlideIn"].speed = -1.0f;
                m_pAnims[i]["RightSlideIn"].wrapMode = WrapMode.Clamp;
                m_pAnims[i].Play("RightSlideIn");
            }
        }
    }

    public override void FadeIn(float fAnimTime)
    {
        base.FadeIn(fAnimTime);

        float fSep = 0.1f;
        for (int i = 0; i < m_pAnims.Length; ++i)
        {
            if (0 == i)
            {
                if (m_pAnims[i]["LeftSlideIn"].speed < 0.5f && m_fAnimTime + fSep * i < 1.0f)
                {
                    m_pAnims[i]["LeftSlideIn"].normalizedTime = 0.0f;
                    m_pAnims[i]["LeftSlideIn"].speed = 1.0f;
                    m_pAnims[i]["LeftSlideIn"].wrapMode = WrapMode.Clamp;
                    m_pAnims[i].Play("LeftSlideIn");
                }
            }
            else
            {
                if (m_pAnims[i]["RightSlideIn"].speed < 0.5f && m_fAnimTime + fSep * i < 1.0f)
                {
                    m_pAnims[i]["RightSlideIn"].normalizedTime = 0.0f;
                    m_pAnims[i]["RightSlideIn"].speed = 1.0f;
                    m_pAnims[i]["RightSlideIn"].wrapMode = WrapMode.Clamp;
                    m_pAnims[i].Play("RightSlideIn");
                }
            }
        }
    }

    #region Tabs

    public static Color m_cTabEnable = new Color(217.0f / 255.0f, 213.0f / 255.0f, 203.0f / 255.0f);
    public static Color m_cTabDisable = new Color(147.0f / 255.0f, 145.0f / 255.0f, 135.0f / 255.0f);

    public Text m_txtTitle;
    public Text m_txtInfo;

    public InputField m_InputAlpha;
    public InputField m_InputStep;
    public Text m_txtBoundary;
    public Button m_btBoundary;

    public Image m_TabJ;
    public Text m_txtJ;
    public Slider m_sliderJMin;
    public Slider m_sliderJMax;
    public Button m_ButtonJ;
    public string m_sImageJ;

    public Image m_TabD;
    public Text m_txtD;
    public Slider m_sliderDMin;
    public Slider m_sliderDMax;
    public Button m_ButtonD;
    public string m_sImageD;

    public Image m_TabB;
    public Text m_txtB;
    public Slider m_sliderBMin;
    public Slider m_sliderBMax;
    public Button m_ButtonB;
    public string m_sImageB;

    public Image m_TabK;
    public Text m_txtK;
    public Slider m_sliderKMin;
    public Slider m_sliderKMax;
    public Button m_ButtonK;
    public string m_sImageK;

    public Image m_TabEl;
    public Button m_ButtonEl;

    private static void ConfigOneTab(Image tab, 
        Text txt, Slider min, Slider max, Button detal, 
        COneParameterConfig config, string sHead)
    {
        tab.color = config.m_bEnable ? m_cTabEnable : m_cTabDisable;
        txt.text = string.Format("{5}:{0:0.00}+({1:0.00}{3}{4:0.00})*{2}",
            config.m_fDefaultMin,
            config.m_fDefaultMax,
            config.m_sImage,
            config.m_fDefaultMin < 0.0f ? "+" : "-",
            Mathf.Abs(config.m_fDefaultMin),
            sHead
        );

        min.minValue = config.m_fMin;
        min.maxValue = config.m_fMax;
        min.value = config.m_fDefaultMin;
        max.minValue = config.m_fMin;
        max.maxValue = config.m_fMax;
        max.interactable = config.m_bEnableMax;
        max.value = config.m_fDefaultMax;
        min.interactable = config.m_bEnable;
        max.interactable = config.m_bEnable;
        detal.interactable = config.m_bEnable;
    }

    public static float TryParse(string sCotent, float fDefault, float fMin, float fMax)
    {
        if (float.TryParse(sCotent, out var fV))
        {
            if (fV <= fMax && fV >= fMin)
            {
                return fV;
            }
        }

        return fDefault;
    }

    private static string SafeImage(string sContent, string sDefault)
    {
        return sDefault;
    }

    private void SetupTabs(CUIPageConfigSimulate config)
    {
        //Alpha
        m_InputAlpha.text = config.m_ConfigAlpha.m_fDefault.ToString();
        m_InputAlpha.interactable = config.m_ConfigAlpha.m_bEnable;

        //Step
        m_InputStep.text = config.m_ConfigStep.m_fDefault.ToString();
        m_InputStep.interactable = config.m_ConfigStep.m_bEnable;

        //Boundary
        m_txtBoundary.text = config.m_ConfigBoundary.m_sImage;
        m_btBoundary.interactable = config.m_ConfigBoundary.m_bEnable;

        //j
        ConfigOneTab(m_TabJ, m_txtJ, m_sliderJMin, m_sliderJMax, m_ButtonJ, config.m_ConfigJ, "J");
        m_sImageJ = config.m_ConfigJ.m_sImage;
        m_pParameter.m_sJImage = config.m_ConfigJ.m_sImage;
        ConfigOneTab(m_TabD, m_txtD, m_sliderDMin, m_sliderDMax, m_ButtonD, config.m_ConfigD, "D");
        m_sImageD = config.m_ConfigD.m_sImage;
        m_pParameter.m_sDImage = config.m_ConfigD.m_sImage;
        ConfigOneTab(m_TabB, m_txtB, m_sliderBMin, m_sliderBMax, m_ButtonB, config.m_ConfigB, "B");
        m_sImageB = config.m_ConfigB.m_sImage;
        m_pParameter.m_sBImage = config.m_ConfigB.m_sImage;
        ConfigOneTab(m_TabK, m_txtK, m_sliderKMin, m_sliderKMax, m_ButtonK, config.m_ConfigK, "K");
        m_sImageK = config.m_ConfigK.m_sImage;
        m_pParameter.m_sKImage = config.m_ConfigK.m_sImage;

        m_TabEl.color = m_cTabDisable;
        m_ButtonEl.interactable = false;
    }

    private void UpdateWithParameters(CSimulateParameter parameter)
    {
        m_InputAlpha.text = parameter.m_fAlpha.ToString();
        m_InputStep.text = parameter.m_fStep.ToString();
        m_txtBoundary.text = parameter.m_sBoundaryImage;

        m_sliderJMin.value = parameter.m_fJMin;
        m_sliderJMax.value = parameter.m_fJMax;
        m_sImageJ = parameter.m_sJImage;

        m_sliderDMin.value = parameter.m_fDMin;
        m_sliderDMax.value = parameter.m_fDMax;
        m_sImageD = parameter.m_sDImage;

        m_sliderBMin.value = parameter.m_fBMin;
        m_sliderBMax.value = parameter.m_fBMax;
        m_sImageB = parameter.m_sBImage;

        m_sliderKMin.value = parameter.m_fKMin;
        m_sliderKMax.value = parameter.m_fKMax;
        m_sImageK = parameter.m_sKImage;
    }

    public CSimulateParameter GetSimulateParameter()
    {
        CSimulateParameter ret = new CSimulateParameter();

        ret.m_fAlpha = TryParse(m_InputAlpha.text,
            m_pConfigSimulation.m_ConfigAlpha.m_fDefault,
            m_pConfigSimulation.m_ConfigAlpha.m_fMin,
            m_pConfigSimulation.m_ConfigAlpha.m_fMax);
        ret.m_fStep = TryParse(m_InputStep.text,
            m_pConfigSimulation.m_ConfigStep.m_fDefault,
            m_pConfigSimulation.m_ConfigStep.m_fMin,
            m_pConfigSimulation.m_ConfigStep.m_fMax);
        ret.m_sBoundaryImage = SafeImage(m_txtBoundary.text, m_pConfigSimulation.m_ConfigBoundary.m_sImage);

        ret.m_fJMin = m_sliderJMin.value;
        ret.m_fJMax = m_sliderJMax.value;
        ret.m_sJImage = SafeImage(m_sImageJ, m_pConfigSimulation.m_ConfigJ.m_sImage);
        ret.m_fDMin = m_sliderDMin.value;
        ret.m_fDMax = m_sliderDMax.value;
        ret.m_sDImage = SafeImage(m_sImageD, m_pConfigSimulation.m_ConfigD.m_sImage);

        ret.m_fBMin = m_sliderBMin.value;
        ret.m_fBMax = m_sliderBMax.value;
        ret.m_sBImage = SafeImage(m_sImageB, m_pConfigSimulation.m_ConfigB.m_sImage);

        ret.m_fKMin = m_sliderKMin.value;
        ret.m_fKMax = m_sliderKMax.value;
        ret.m_sKImage = SafeImage(m_sImageK, m_pConfigSimulation.m_ConfigK.m_sImage);

        return ret;
    }


    #endregion

    #region Events

    public void OnSliderJMin()
    {
        m_pParameter.m_fJMin = m_sliderJMin.value;
        m_txtJ.text = string.Format("J:{0:0.00}+({1:0.00}{3}{4:0.00})*{2}",
            m_pParameter.m_fJMin,
            m_pParameter.m_fJMax,
            m_pParameter.m_sJImage,
            m_pParameter.m_fJMin < 0.0f ? "+" : "-",
            Mathf.Abs(m_pParameter.m_fJMin)
            );
    }

    public void OnSliderJMax()
    {
        m_pParameter.m_fJMax = m_sliderJMax.value;
        m_txtJ.text = string.Format("J:{0:0.00}+({1:0.00}{3}{4:0.00})*{2}",
            m_pParameter.m_fJMin,
            m_pParameter.m_fJMax,
            m_pParameter.m_sJImage,
            m_pParameter.m_fJMin < 0.0f ? "+" : "-",
            Mathf.Abs(m_pParameter.m_fJMin)
        );
    }

    public void OnSliderDMin()
    {
        m_pParameter.m_fDMin = m_sliderDMin.value;
        m_txtD.text = string.Format("D:{0:0.00}+({1:0.00}{3}{4:0.00})*{2}",
            m_pParameter.m_fDMin,
            m_pParameter.m_fDMax,
            m_pParameter.m_sDImage,
            m_pParameter.m_fDMin < 0.0f ? "+" : "-",
            Mathf.Abs(m_pParameter.m_fDMin)
        );
    }

    public void OnSliderDMax()
    {
        m_pParameter.m_fDMax = m_sliderDMax.value;
        m_txtD.text = string.Format("D:{0:0.00}+({1:0.00}{3}{4:0.00})*{2}",
            m_pParameter.m_fDMin,
            m_pParameter.m_fDMax,
            m_pParameter.m_sDImage,
            m_pParameter.m_fDMin < 0.0f ? "+" : "-",
            Mathf.Abs(m_pParameter.m_fDMin)
        );
    }

    public void OnSliderBMin()
    {
        m_pParameter.m_fBMin = m_sliderBMin.value;
        m_txtB.text = string.Format("B:{0:0.00}+({1:0.00}{3}{4:0.00})*{2}",
            m_pParameter.m_fBMin,
            m_pParameter.m_fBMax,
            m_pParameter.m_sBImage,
            m_pParameter.m_fBMin < 0.0f ? "+" : "-",
            Mathf.Abs(m_pParameter.m_fBMin)
        );
    }

    public void OnSliderBMax()
    {
        m_pParameter.m_fBMax = m_sliderBMax.value;
        m_txtB.text = string.Format("B:{0:0.00}+({1:0.00}{3}{4:0.00})*{2}",
            m_pParameter.m_fBMin,
            m_pParameter.m_fBMax,
            m_pParameter.m_sBImage,
            m_pParameter.m_fBMin < 0.0f ? "+" : "-",
            Mathf.Abs(m_pParameter.m_fBMin)
        );
    }

    public void OnSliderKMin()
    {
        m_pParameter.m_fKMin = m_sliderKMin.value;
        m_txtK.text = string.Format("K:{0:0.00}+({1:0.00}{3}{4:0.00})*{2}",
            m_pParameter.m_fKMin,
            m_pParameter.m_fKMax,
            m_pParameter.m_sKImage,
            m_pParameter.m_fKMin < 0.0f ? "+" : "-",
            Mathf.Abs(m_pParameter.m_fKMin)
        );
    }

    public void OnSliderKMax()
    {
        m_pParameter.m_fKMax = m_sliderJMax.value;
        m_txtK.text = string.Format("K:{0:0.00}+({1:0.00}{3}{4:0.00})*{2}",
            m_pParameter.m_fKMin,
            m_pParameter.m_fKMax,
            m_pParameter.m_sKImage,
            m_pParameter.m_fKMin < 0.0f ? "+" : "-",
            Mathf.Abs(m_pParameter.m_fKMin)
        );
    }

    public void OnInputChangeAlpha(string sAlpha)
    {
        float fNewAlpha = TryParse(m_InputAlpha.text,
            m_pParameter.m_fAlpha,
            m_pConfigSimulation.m_ConfigAlpha.m_fMin,
            m_pConfigSimulation.m_ConfigAlpha.m_fMax);

        m_pParameter.m_fAlpha = fNewAlpha;
        m_InputAlpha.SetTextWithoutNotify(fNewAlpha.ToString());
    }

    public void OnInputChangeStep(string sStep)
    {
        float fNewStep = TryParse(m_InputStep.text,
            m_pParameter.m_fStep,
            m_pConfigSimulation.m_ConfigStep.m_fMin,
            m_pConfigSimulation.m_ConfigStep.m_fMax);

        m_pParameter.m_fStep = fNewStep;
        m_InputStep.SetTextWithoutNotify(fNewStep.ToString());
    }

    public void OnBtBoundary()
    {
        if (m_fAnimTime > 0.0f)
        {
            return;
        }

        CImageDialogConfig imageConfig = new CImageDialogConfig();
        imageConfig.m_sFolder = "Boundary";
        imageConfig.m_eMat = EPreviewMatType.EPMT_Mask;
        imageConfig.m_sNow = "Periodic";
        imageConfig.m_pCallback = OnChangeBoundary;
        imageConfig.m_sMsg = "";
        m_pOwner.ShowImageChoose(imageConfig);
    }

    private void OnChangeBoundary(string sFile)
    {
        Debug.Log(sFile);
    }

    public void OnBtJ()
    {
        if (m_fAnimTime > 0.0f)
        {
            return;
        }

        COneParameterConfig jNow = new COneParameterConfig();
        jNow.m_fDefaultMin = m_pParameter.m_fJMin;
        jNow.m_fDefaultMax = m_pParameter.m_fJMax;
        jNow.m_fMin = m_pConfigSimulation.m_ConfigJ.m_fMin;
        jNow.m_fMax = m_pConfigSimulation.m_ConfigJ.m_fMax;
        jNow.m_sImage = m_pParameter.m_sJImage;
        jNow.m_bEnableImage = m_pConfigSimulation.m_ConfigJ.m_bEnableImage;
        jNow.m_bEnableMax = m_pConfigSimulation.m_ConfigJ.m_bEnableMax;

        CDetailDialogConfig dialog = new CDetailDialogConfig();
        dialog.m_sMsg = "结果J=a + (b-a) x 图片灰度\n图片灰度黑色为0，白色为1";
        dialog.m_Config = jNow;
        dialog.m_sImageFolder = "Mask";
        dialog.m_pCallback = OnChangeJ;

        m_pOwner.ShowDetailChoose(dialog);
    }

    private void OnChangeJ(COneParameterConfig change)
    {
        m_pParameter.m_fJMin = change.m_fDefaultMin;
        m_pParameter.m_fJMax = change.m_fDefaultMax;
        m_sliderJMin.SetValueWithoutNotify(m_pParameter.m_fJMin);
        m_sliderJMax.SetValueWithoutNotify(m_pParameter.m_fJMax);
        m_pParameter.m_sJImage = change.m_sImage;
        m_sImageJ = change.m_sImage;
        m_txtJ.text = string.Format("J:{0:0.00}+({1:0.00}{3}{4:0.00})*{2}",
            m_pParameter.m_fJMin,
            m_pParameter.m_fJMax,
            m_pParameter.m_sJImage,
            m_pParameter.m_fJMin < 0.0f ? "+" : "-",
            Mathf.Abs(m_pParameter.m_fJMin)
        );
    }

    public void OnBtD()
    {
        if (m_fAnimTime > 0.0f)
        {
            return;
        }

        COneParameterConfig jNow = new COneParameterConfig();
        jNow.m_fDefaultMin = m_pParameter.m_fDMin;
        jNow.m_fDefaultMax = m_pParameter.m_fDMax;
        jNow.m_fMin = m_pConfigSimulation.m_ConfigD.m_fMin;
        jNow.m_fMax = m_pConfigSimulation.m_ConfigD.m_fMax;
        jNow.m_sImage = m_pParameter.m_sDImage;
        jNow.m_bEnableImage = m_pConfigSimulation.m_ConfigD.m_bEnableImage;
        jNow.m_bEnableMax = m_pConfigSimulation.m_ConfigD.m_bEnableMax;

        CDetailDialogConfig dialog = new CDetailDialogConfig();
        dialog.m_sMsg = "结果D=a + (b-a) x 图片灰度\n图片灰度黑色为0，白色为1";
        dialog.m_Config = jNow;
        dialog.m_sImageFolder = "Mask";
        dialog.m_pCallback = OnChangeD;

        m_pOwner.ShowDetailChoose(dialog);
    }

    private void OnChangeD(COneParameterConfig change)
    {
        m_pParameter.m_fDMin = change.m_fDefaultMin;
        m_pParameter.m_fDMax = change.m_fDefaultMax;
        m_sliderDMin.SetValueWithoutNotify(m_pParameter.m_fDMin);
        m_sliderDMax.SetValueWithoutNotify(m_pParameter.m_fDMax);
        m_pParameter.m_sDImage = change.m_sImage;
        m_sImageD = change.m_sImage;
        m_txtD.text = string.Format("D:{0:0.00}+({1:0.00}{3}{4:0.00})*{2}",
            m_pParameter.m_fDMin,
            m_pParameter.m_fDMax,
            m_pParameter.m_sDImage,
            m_pParameter.m_fDMin < 0.0f ? "+" : "-",
            Mathf.Abs(m_pParameter.m_fDMin)
        );
    }

    public void OnBtB()
    {
        if (m_fAnimTime > 0.0f)
        {
            return;
        }

        COneParameterConfig jNow = new COneParameterConfig();
        jNow.m_fDefaultMin = m_pParameter.m_fBMin;
        jNow.m_fDefaultMax = m_pParameter.m_fBMax;
        jNow.m_fMin = m_pConfigSimulation.m_ConfigB.m_fMin;
        jNow.m_fMax = m_pConfigSimulation.m_ConfigB.m_fMax;
        jNow.m_sImage = m_pParameter.m_sBImage;
        jNow.m_bEnableImage = m_pConfigSimulation.m_ConfigB.m_bEnableImage;
        jNow.m_bEnableMax = m_pConfigSimulation.m_ConfigB.m_bEnableMax;

        CDetailDialogConfig dialog = new CDetailDialogConfig();
        dialog.m_sMsg = "结果B=a + (b-a) x 图片灰度\n图片灰度黑色为0，白色为1";
        dialog.m_Config = jNow;
        dialog.m_sImageFolder = "Mask";
        dialog.m_pCallback = OnChangeB;

        m_pOwner.ShowDetailChoose(dialog);
    }

    private void OnChangeB(COneParameterConfig change)
    {
        m_pParameter.m_fBMin = change.m_fDefaultMin;
        m_pParameter.m_fBMax = change.m_fDefaultMax;
        m_sliderBMin.SetValueWithoutNotify(m_pParameter.m_fBMin);
        m_sliderBMax.SetValueWithoutNotify(m_pParameter.m_fBMax);
        m_pParameter.m_sBImage = change.m_sImage;
        m_sImageB = change.m_sImage;
        m_txtB.text = string.Format("B:{0:0.00}+({1:0.00}{3}{4:0.00})*{2}",
            m_pParameter.m_fBMin,
            m_pParameter.m_fBMax,
            m_pParameter.m_sBImage,
            m_pParameter.m_fBMin < 0.0f ? "+" : "-",
            Mathf.Abs(m_pParameter.m_fBMin)
        );
    }

    public void OnBtK()
    {
        if (m_fAnimTime > 0.0f)
        {
            return;
        }

        COneParameterConfig jNow = new COneParameterConfig();
        jNow.m_fDefaultMin = m_pParameter.m_fKMin;
        jNow.m_fDefaultMax = m_pParameter.m_fKMax;
        jNow.m_fMin = m_pConfigSimulation.m_ConfigK.m_fMin;
        jNow.m_fMax = m_pConfigSimulation.m_ConfigK.m_fMax;
        jNow.m_sImage = m_pParameter.m_sKImage;
        jNow.m_bEnableImage = m_pConfigSimulation.m_ConfigK.m_bEnableImage;
        jNow.m_bEnableMax = m_pConfigSimulation.m_ConfigK.m_bEnableMax;

        CDetailDialogConfig dialog = new CDetailDialogConfig();
        dialog.m_sMsg = "结果K=a + (b-a) x 图片灰度\n图片灰度黑色为0，白色为1";
        dialog.m_Config = jNow;
        dialog.m_sImageFolder = "Mask";
        dialog.m_pCallback = OnChangeK;

        m_pOwner.ShowDetailChoose(dialog);
    }

    private void OnChangeK(COneParameterConfig change)
    {
        m_pParameter.m_fKMin = change.m_fDefaultMin;
        m_pParameter.m_fKMax = change.m_fDefaultMax;
        m_sliderKMin.SetValueWithoutNotify(m_pParameter.m_fKMin);
        m_sliderKMax.SetValueWithoutNotify(m_pParameter.m_fKMax);
        m_pParameter.m_sKImage = change.m_sImage;
        m_sImageK = change.m_sImage;
        m_txtK.text = string.Format("K:{0:0.00}+({1:0.00}{3}{4:0.00})*{2}",
            m_pParameter.m_fKMin,
            m_pParameter.m_fKMax,
            m_pParameter.m_sKImage,
            m_pParameter.m_fKMin < 0.0f ? "+" : "-",
            Mathf.Abs(m_pParameter.m_fKMin)
        );
    }

    public void OnBtElectric()
    {

    }

    public void OnBtStart()
    {
        if (m_fAnimTime > 0.0f)
        {
            return;
        }
    }

    public void OnBtSave()
    {
        if (m_fAnimTime > 0.0f)
        {
            return;
        }
    }

    public void OnBtLoad()
    {
        if (m_fAnimTime > 0.0f)
        {
            return;
        }

        CImageDialogConfig imageConfig = new CImageDialogConfig();
        imageConfig.m_sFolder = "Configuration";
        imageConfig.m_eMat = EPreviewMatType.EPMT_Config;
        imageConfig.m_sNow = "Noise";
        imageConfig.m_pCallback = OnChangeConfiguration;
        imageConfig.m_sMsg = "<color=#FF0000>未保存的Configuration将丢失！</color>";
        m_pOwner.ShowImageChoose(imageConfig);
    }

    private void OnChangeConfiguration(string sNew)
    {
        Debug.Log(sNew);
    }

    #endregion

    #region Simulate

    public bool m_bRunning = false;
    public void OnEnterPage()
    {

    }

    public override void OnButtonBack()
    {
        if (m_fAnimTime > 0.0f)
        {
            return;
        }

        if (m_bRunning)
        {
            m_pOwner.ShowOneButtonDialog("先暂停计算后，才能退出");
        }
        else
        {
            m_pOwner.ShowTwoButtonDialog("真的要退出吗？如果没有保存Configuration，这次计算的结果会丢失！", RealQuit);
        }
    }

    public void RealQuit(bool bQuit)
    {
        if (bQuit)
        {
            GoPage(EPage.Menu, null);
        }
    }

    #endregion
}
