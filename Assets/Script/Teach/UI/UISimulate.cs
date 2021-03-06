using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

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
    public float m_fDefault = 0.0f;
    public bool m_bEnable = false;
    public float m_fMin = -10.0f;
    public bool m_bEnableMax;
    public float m_fMax = 10.0f;
    public float m_fDefaultMin = 0.0f;
    public float m_fDefaultMax = 0.0f;
    public bool m_bEnableImage = false;
    public string m_sImage = "";
}


public class CUIPageConfigSimulate : CUIPageConfig
{
    public string m_sTitle;
    public string m_sConfiguration;
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
            m_fDefault = 0.2f,
            m_fMin = 0.0001f,
            m_fMax = 3.0f,
            m_fDefaultMax = 3.0f,
            m_fDefaultMin = 0.0001f
        };

        COneParameterConfig step = new COneParameterConfig
        {
            m_sImage = "",
            m_bEnable = true,
            m_bEnableMax = true,
            m_bEnableImage = true,
            m_fDefault = 0.1f,
            m_fMin = 0.000001f,
            m_fMax = 1.0f,
            m_fDefaultMax = 0.1f,
            m_fDefaultMin = 0.000001f
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
            m_fDefault = 1.0f,
            m_fMin = -10.0f,
            m_fMax = 10.0f,
            m_fDefaultMax = 0.0f,
            m_fDefaultMin = 1.0f
        };

        COneParameterConfig cfgd = new COneParameterConfig
        {
            m_sImage = "0",
            m_bEnable = true,
            m_bEnableMax = true,
            m_bEnableImage = true,
            m_fDefault = 0.2f,
            m_fMin = -10.0f,
            m_fMax = 10.0f,
            m_fDefaultMax = 0.0f,
            m_fDefaultMin = 0.2f
        };

        COneParameterConfig cfgb = new COneParameterConfig
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

        COneParameterConfig el = new COneParameterConfig
        {
            m_sImage = "",
            m_bEnable = false,
            m_bEnableMax = true,
            m_bEnableImage = true,
            m_fDefault = 0.01f,
            m_fMin = 0.0001f,
            m_fMax = 3.0f,
            m_fDefaultMax = 3.0f,
            m_fDefaultMin = 0.0001f
        };

        CUIPageConfigSimulate freeExp = new CUIPageConfigSimulate();
        freeExp.m_sConfiguration = "Out";
        freeExp.m_ConfigAlpha = alpha;
        freeExp.m_ConfigStep = step;
        freeExp.m_ConfigBoundary = boundary;
        freeExp.m_ConfigJ = cfgj;
        freeExp.m_ConfigD = cfgd;
        freeExp.m_ConfigB = cfgb;
        freeExp.m_ConfigK = cfgk;
        freeExp.m_ConfigElect = el;

        return freeExp;
    }
}



public class UISimulate : UIPage
{
    public const int reso = 256;

    private CUIPageConfigSimulate m_pConfigSimulation = null;
    private CSimulateParameter m_pParameter = null;

    //public override void Start()
    //{
    //    m_pConfigSimulation = CUIPageConfigSimulate.GetDefault();
    //    SetupTabs(m_pConfigSimulation);
    //    m_pParameter = GetSimulateParameter();
    //}

    public override void Update()
    {
        base.Update();

        if (m_bRunning)
        {
            UpdateShaderOneStep();
            m_iStep++;
            m_txtInfo.text = "步数: " + m_iStep.ToString();
        }
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
        m_iStep = 0;

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
    public Text m_txtButtonStart;
    private int m_iStep = 0;

    public Button m_btBtSave;
    public Button m_btBtLoad;
    public Button m_btBt3D;

    public Image m_TabCommon;
    public InputField m_InputAlpha;
    public InputField m_InputStep;
    public Text m_txtBoundary;
    public Button m_btBoundary;

    public Image m_TabJ;
    public Text m_txtJ;
    public Slider m_sliderJMin;
    public Slider m_sliderJMax;
    public Button m_ButtonJ;

    public Image m_TabD;
    public Text m_txtD;
    public Slider m_sliderDMin;
    public Slider m_sliderDMax;
    public Button m_ButtonD;

    public Image m_TabB;
    public Text m_txtB;
    public Slider m_sliderBMin;
    public Slider m_sliderBMax;
    public Button m_ButtonB;

    public Image m_TabK;
    public Text m_txtK;
    public Slider m_sliderKMin;
    public Slider m_sliderKMax;
    public Button m_ButtonK;

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
        max.value = config.m_fDefaultMax;

        min.interactable = config.m_bEnable;
        max.interactable = config.m_bEnableMax;
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
        m_pParameter.m_sJImage = config.m_ConfigJ.m_sImage;
        ConfigOneTab(m_TabD, m_txtD, m_sliderDMin, m_sliderDMax, m_ButtonD, config.m_ConfigD, "D");
        m_pParameter.m_sDImage = config.m_ConfigD.m_sImage;
        ConfigOneTab(m_TabB, m_txtB, m_sliderBMin, m_sliderBMax, m_ButtonB, config.m_ConfigB, "B");
        m_pParameter.m_sBImage = config.m_ConfigB.m_sImage;
        ConfigOneTab(m_TabK, m_txtK, m_sliderKMin, m_sliderKMax, m_ButtonK, config.m_ConfigK, "K");
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

        m_sliderDMin.value = parameter.m_fDMin;
        m_sliderDMax.value = parameter.m_fDMax;

        m_sliderBMin.value = parameter.m_fBMin;
        m_sliderBMax.value = parameter.m_fBMax;

        m_sliderKMin.value = parameter.m_fKMin;
        m_sliderKMax.value = parameter.m_fKMax;
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
        ret.m_sBoundaryImage = m_pConfigSimulation.m_ConfigBoundary.m_sImage;

        ret.m_fJMin = m_sliderJMin.value;
        ret.m_fJMax = m_sliderJMax.value;
        ret.m_sJImage = m_pParameter.m_sJImage;
        ret.m_fDMin = m_sliderDMin.value;
        ret.m_fDMax = m_sliderDMax.value;
        ret.m_sDImage = m_pParameter.m_sDImage;

        ret.m_fBMin = m_sliderBMin.value;
        ret.m_fBMax = m_sliderBMax.value;
        ret.m_sBImage = m_pParameter.m_sBImage;

        ret.m_fKMin = m_sliderKMin.value;
        ret.m_fKMax = m_sliderKMax.value;
        ret.m_sKImage = m_pParameter.m_sKImage;

        return ret;
    }

    public void SetUpInteractableWithCfg()
    {
        m_btBtSave.interactable = true;
        m_btBtLoad.interactable = true;
        m_btBt3D.interactable = true;

        m_InputAlpha.interactable = m_pConfigSimulation.m_ConfigAlpha.m_bEnable;
        m_InputStep.interactable = m_pConfigSimulation.m_ConfigStep.m_bEnable;
        m_btBoundary.interactable = m_pConfigSimulation.m_ConfigBoundary.m_bEnable;

        m_sliderJMin.interactable = m_pConfigSimulation.m_ConfigJ.m_bEnable;
        m_sliderJMax.interactable = 
            m_pConfigSimulation.m_ConfigJ.m_bEnableMax && m_pConfigSimulation.m_ConfigJ.m_bEnable;
        m_ButtonJ.interactable = m_pConfigSimulation.m_ConfigJ.m_bEnable;

        m_sliderDMin.interactable = m_pConfigSimulation.m_ConfigD.m_bEnable;
        m_sliderDMax.interactable = 
            m_pConfigSimulation.m_ConfigD.m_bEnableMax && m_pConfigSimulation.m_ConfigD.m_bEnable;
        m_ButtonD.interactable = m_pConfigSimulation.m_ConfigD.m_bEnable;

        m_sliderBMin.interactable = m_pConfigSimulation.m_ConfigB.m_bEnable;
        m_sliderBMax.interactable = 
            m_pConfigSimulation.m_ConfigB.m_bEnableMax && m_pConfigSimulation.m_ConfigB.m_bEnable;
        m_ButtonB.interactable = m_pConfigSimulation.m_ConfigB.m_bEnable;

        m_sliderKMin.interactable = m_pConfigSimulation.m_ConfigK.m_bEnable;
        m_sliderKMax.interactable = 
            m_pConfigSimulation.m_ConfigK.m_bEnableMax && m_pConfigSimulation.m_ConfigK.m_bEnable;
        m_ButtonK.interactable = m_pConfigSimulation.m_ConfigK.m_bEnable;

        m_ButtonEl.interactable = m_pConfigSimulation.m_ConfigElect.m_bEnable;

        m_TabCommon.color = m_cTabEnable;
        m_TabJ.color = m_pConfigSimulation.m_ConfigJ.m_bEnable ? m_cTabEnable : m_cTabDisable;
        m_TabD.color = m_pConfigSimulation.m_ConfigD.m_bEnable ? m_cTabEnable : m_cTabDisable;
        m_TabB.color = m_pConfigSimulation.m_ConfigB.m_bEnable ? m_cTabEnable : m_cTabDisable;
        m_TabK.color = m_pConfigSimulation.m_ConfigK.m_bEnable ? m_cTabEnable : m_cTabDisable;
        m_TabEl.color = m_pConfigSimulation.m_ConfigElect.m_bEnable ? m_cTabEnable : m_cTabDisable;

        m_pOwner.ChangeInteract(true);
    }

    public void DisableAllItems()
    {
        m_btBtSave.interactable = false;
        m_btBtLoad.interactable = false;
        m_btBt3D.interactable = false;

        m_InputAlpha.interactable = false;
        m_InputStep.interactable = false;
        m_btBoundary.interactable = false;

        m_sliderJMin.interactable = false;
        m_sliderJMax.interactable = false;
        m_ButtonJ.interactable = false;

        m_sliderDMin.interactable = false;
        m_sliderDMax.interactable = false;
        m_ButtonD.interactable = false;

        m_sliderBMin.interactable = false;
        m_sliderBMax.interactable = false;
        m_ButtonB.interactable = false;

        m_sliderKMin.interactable = false;
        m_sliderKMax.interactable = false;
        m_ButtonK.interactable = false;

        m_ButtonEl.interactable = false;

        m_TabCommon.color = m_cTabDisable;
        m_TabJ.color = m_cTabDisable;
        m_TabD.color = m_cTabDisable;
        m_TabB.color = m_cTabDisable;
        m_TabK.color = m_cTabDisable;
        m_TabEl.color = m_cTabDisable;

        m_pOwner.ChangeInteract(false);
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
        m_pParameter.m_sBoundaryImage = sFile;
        m_txtBoundary.text = sFile;
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
        UISoundManager.UISound(EUISound.Button);
        //m_pParameter = GetSimulateParameter();
        if (!m_bRunning)
        {
            SetupParametersToShader();
            DisableAllItems();
        }
        else
        {
            SetUpInteractableWithCfg();
        }

        m_bRunning = !m_bRunning;
        m_txtButtonStart.text = m_bRunning ? "暂停" : "开始";
    }

    public void OnBtSave()
    {
        if (m_fAnimTime > 0.0f)
        {
            return;
        }
        GetConfigurationOut();

        CSaveDialogConfig config = new CSaveDialogConfig();
        config.m_pToSave = m_pLastConfig.EncodeToPNG();
        config.m_sImageFolder = Application.streamingAssetsPath + "/Configuration/";
        config.m_sParameter = m_pParameter.GetDescription() + string.Format("Time: {0}\n", m_iStep);
        m_pOwner.ShowSDialogSave(config);
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
        SetupConfigurationToShader(sNew);
    }

    public void OnBt3D()
    {
        if (m_fAnimTime > 0.0f)
        {
            return;
        }

        Show3D();
    }

    #endregion

    #region Simulate

    public RawImage m_imgShow;
    public ComputeShader CmpShader;
    public ComputeShader GetXYZ;
    public Material DegreeToN;
    private int m_iK1Handle;
    private int m_iK2Handle;
    private int m_iK3Handle;
    private int m_iKernelHandle;
    private int m_iKernelGetXYZ;

    private RenderTexture m_pConfiguration = null;
    private Texture2D m_pConfigurationTmp2 = null;
    private RenderTexture m_pRKMiddle = null;
    private Texture2D m_pJDBKTmp = null;
    private Texture2D m_pJ = null;
    private Texture2D m_pD = null;
    private Texture2D m_pB = null;
    private Texture2D m_pK = null;
    private Texture2D m_pBoundTexture = null;
    private Texture2D m_pElecTexture = null;
    private RenderTexture m_pLastConfigRT = null;
    private Texture2D m_pLastConfig = null;

    public override void Intial(UIManager pManager, EPage ePage)
    {
        base.Intial(pManager, ePage);

        CmpShader.SetInt("size", reso);

        m_iK1Handle = CmpShader.FindKernel("CaclK1");
        m_iK2Handle = CmpShader.FindKernel("CaclK2");
        m_iK3Handle = CmpShader.FindKernel("CaclK3");
        m_iKernelHandle = CmpShader.FindKernel("CSMain");

        m_iKernelGetXYZ = GetXYZ.FindKernel("CSMain");

        m_pRKMiddle = new RenderTexture(reso * 2, reso * 2, 0, RenderTextureFormat.ARGBFloat);
        m_pRKMiddle.enableRandomWrite = true;
        m_pRKMiddle.Create();

        CmpShader.SetTexture(m_iK1Handle, "k1", m_pRKMiddle);
        CmpShader.SetTexture(m_iK2Handle, "k1", m_pRKMiddle);
        CmpShader.SetTexture(m_iK3Handle, "k1", m_pRKMiddle);
        CmpShader.SetTexture(m_iKernelHandle, "k1", m_pRKMiddle);

        m_pConfiguration = new RenderTexture(reso, reso, 0, RenderTextureFormat.ARGBFloat);
        m_pConfiguration.enableRandomWrite = true;
        m_pConfiguration.Create();

        m_imgShow.material.SetTexture("_Using", m_pConfiguration);

        CmpShader.SetTexture(m_iK1Handle, "magneticMomentum", m_pConfiguration);
        CmpShader.SetTexture(m_iK2Handle, "magneticMomentum", m_pConfiguration);
        CmpShader.SetTexture(m_iK3Handle, "magneticMomentum", m_pConfiguration);
        CmpShader.SetTexture(m_iKernelHandle, "magneticMomentum", m_pConfiguration);

        m_pLastConfig = new Texture2D(reso, reso, TextureFormat.RGB24, false);
        m_pConfigurationTmp2 = new Texture2D(reso, reso, TextureFormat.RGB24, false);

        m_pJDBKTmp = new Texture2D(reso, reso, TextureFormat.RGBAFloat, false);
        m_pJ = new Texture2D(reso, reso, TextureFormat.RGBA32, false);
        m_pD = new Texture2D(reso, reso, TextureFormat.RGBA32, false);
        m_pB = new Texture2D(reso, reso, TextureFormat.RGBA32, false);
        m_pK = new Texture2D(reso, reso, TextureFormat.RGBA32, false);

        m_pBoundTexture = new Texture2D(reso, reso, TextureFormat.RGBA4444, false);

        m_pLastConfigRT = new RenderTexture(reso, reso, 0, RenderTextureFormat.ARGB32);
        m_pLastConfigRT.enableRandomWrite = true;
        m_pLastConfigRT.Create();

        GetXYZ.SetTexture(m_iKernelGetXYZ, "Mag", m_pConfiguration);
        GetXYZ.SetTexture(m_iKernelGetXYZ, "Result", m_pLastConfigRT);

        Debug.Log("All texture created");
    }

    public bool m_bRunning = false;
    public void OnEnterPage()
    {
        //Fill All paramters with default
        SetupConfigurationToShader(m_pConfigSimulation.m_sConfiguration);
        SetupParametersToShader();
    }

    public void SetupParametersToShader()
    {
        CmpShader.SetInt("size", reso);
        CmpShader.SetFloat("timestep", m_pParameter.m_fStep);
        CmpShader.SetFloat("alpha", m_pParameter.m_fAlpha);


        m_pJ.LoadImage(File.ReadAllBytes(Application.streamingAssetsPath + "/Mask/" + m_pParameter.m_sJImage + ".png"));
        m_pD.LoadImage(File.ReadAllBytes(Application.streamingAssetsPath + "/Mask/" + m_pParameter.m_sDImage + ".png"));
        m_pB.LoadImage(File.ReadAllBytes(Application.streamingAssetsPath + "/Mask/" + m_pParameter.m_sBImage + ".png"));
        m_pK.LoadImage(File.ReadAllBytes(Application.streamingAssetsPath + "/Mask/" + m_pParameter.m_sKImage + ".png"));

        for (int i = 0; i < reso; ++i)
        {
            for (int j = 0; j < reso; ++j)
            {
                m_pJDBKTmp.SetPixel(i, j, new Color(
                    m_pJ.GetPixel(i, j).r * (m_pParameter.m_fJMax - m_pParameter.m_fJMin) + m_pParameter.m_fJMin,
                    m_pD.GetPixel(i, j).r * (m_pParameter.m_fDMax - m_pParameter.m_fDMin) + m_pParameter.m_fDMin,
                    m_pB.GetPixel(i, j).r * (m_pParameter.m_fBMax - m_pParameter.m_fBMin) + m_pParameter.m_fBMin,
                    m_pK.GetPixel(i, j).r * (m_pParameter.m_fKMax - m_pParameter.m_fKMin) + m_pParameter.m_fKMin
                    ));
            }
        }
        m_pJDBKTmp.Apply(false);

        CmpShader.SetTexture(m_iK1Handle, "jdbkStrength", m_pJDBKTmp);
        CmpShader.SetTexture(m_iK2Handle, "jdbkStrength", m_pJDBKTmp);
        CmpShader.SetTexture(m_iK3Handle, "jdbkStrength", m_pJDBKTmp);
        CmpShader.SetTexture(m_iKernelHandle, "jdbkStrength", m_pJDBKTmp);

        m_pBoundTexture.LoadImage(File.ReadAllBytes(Application.streamingAssetsPath + "/Boundary/" + m_pParameter.m_sBoundaryImage + ".png"));
        CmpShader.SetTexture(m_iK1Handle, "boundaryCondition", m_pBoundTexture);
        CmpShader.SetTexture(m_iK2Handle, "boundaryCondition", m_pBoundTexture);
        CmpShader.SetTexture(m_iK3Handle, "boundaryCondition", m_pBoundTexture);
        CmpShader.SetTexture(m_iKernelHandle, "boundaryCondition", m_pBoundTexture);

        CmpShader.SetInt("jxperoid", 0);
        CmpShader.SetInt("jxstep", 0);

        m_pElecTexture = new Texture2D(1, 1, TextureFormat.RFloat, false);
        m_pElecTexture.Apply(false);

        CmpShader.SetTexture(m_iK1Handle, "jxPeroidFunction", m_pElecTexture);
        CmpShader.SetTexture(m_iK2Handle, "jxPeroidFunction", m_pElecTexture);
        CmpShader.SetTexture(m_iK3Handle, "jxPeroidFunction", m_pElecTexture);
        CmpShader.SetTexture(m_iKernelHandle, "jxPeroidFunction", m_pElecTexture);
    }

    public void SetupConfigurationToShader(string sFileName)
    {
        byte[] fileContent = File.ReadAllBytes(Application.streamingAssetsPath + "/Configuration/" + sFileName + ".png");
        m_pConfigurationTmp2.LoadImage(fileContent);
        Graphics.Blit(m_pConfigurationTmp2, m_pConfiguration, DegreeToN);

        //CmpShader.SetTexture(m_iK1Handle, "magneticMomentum", m_pConfiguration);
        //CmpShader.SetTexture(m_iK2Handle, "magneticMomentum", m_pConfiguration);
        //CmpShader.SetTexture(m_iK3Handle, "magneticMomentum", m_pConfiguration);
        //CmpShader.SetTexture(m_iKernelHandle, "magneticMomentum", m_pConfiguration);
    }

    public void UpdateShaderOneStep()
    {
        CmpShader.Dispatch(m_iK1Handle, reso / 16, reso / 16, 1);
        CmpShader.Dispatch(m_iK2Handle, reso / 16, reso / 16, 1);
        CmpShader.Dispatch(m_iK3Handle, reso / 16, reso / 16, 1);
        CmpShader.Dispatch(m_iKernelHandle, reso / 16, reso / 16, 1);
    }

    public override void OnButtonBack()
    {
        if (!gameObject.activeSelf)
        {
            //In 3D Mode
            Hide3D();
            return;
        }

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

    public Material NtoDegree;

    public void GetConfigurationOut()
    {
        GetXYZ.Dispatch(m_iKernelGetXYZ, reso / 16, reso / 16, 1);
        RenderTexture.active = m_pLastConfigRT;
        
        m_pLastConfig.ReadPixels(new Rect(0, 0, reso, reso), 0, 0);
        m_pLastConfig.Apply();
        RenderTexture.active = null;

        m_pOwner.m_tester.texture = m_pLastConfig;
    }

    #endregion

    #region 3D Show

    public AArraws m_Arraws;

    public void Show3D()
    {
        GetConfigurationOut();
        m_pOwner.ShowBackground(false);
        gameObject.SetActive(false);
        m_Arraws.Show(m_pLastConfig);
    }

    public void Hide3D()
    {
        m_pOwner.ShowBackground(true);
        gameObject.SetActive(true);
        m_Arraws.Hide();
    }

    #endregion
}
