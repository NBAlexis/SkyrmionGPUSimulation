using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public enum EPage
{
    None,
    Main,
    Menu,
    Simulate,
}

public enum EDialog
{
    None,
    Msg,
    Image,
    Detail,
    SaveFile,
}

public enum EPreviewMatType
{
    EPMT_Mask,
    EPMT_Config,
}


public class CImageDialogConfig
{
    public string m_sNow;
    public string m_sFolder;
    public string m_sMsg;

    public EPreviewMatType m_eMat;
    public Action<string> m_pCallback;
}

public class CDetailDialogConfig
{
    public COneParameterConfig m_Config;
    public string m_sMsg;
    public string m_sImageFolder;
    public Action<COneParameterConfig> m_pCallback;
}

public class CSaveDialogConfig
{
    public string m_sImageFolder;
    public byte[] m_pToSave;
    public string m_sParameter;
}



public class CSimulateParameter
{
    public float m_fAlpha = 0.1f;
    public float m_fStep = 0.02f;
    public string m_sBoundaryImage = "Periodic";

    public float m_fJMin = 0.1f;
    public float m_fJMax = 0.0f;
    public string m_sJImage = "0";

    public float m_fDMin = 0.2f;
    public float m_fDMax = 0.0f;
    public string m_sDImage = "0";

    public float m_fBMin = 0.2f;
    public float m_fBMax = 0.0f;
    public string m_sBImage = "0";

    public float m_fKMin = 0.0f;
    public float m_fKMax = 0.0f;
    public string m_sKImage = "0";

    //Electric not implement yet

    public string GetDescription()
    {
        string sRet = "";
        sRet = sRet + string.Format("Alpha: {0}, Step: {1}, Boundary: {2}\n", m_fAlpha, m_fStep, m_sBoundaryImage);
        sRet = sRet + string.Format("J: {0} + ({1} - {0}) * {2}\n", m_fJMin, m_fJMax, m_sJImage);
        sRet = sRet + string.Format("D: {0} + ({1} - {0}) * {2}\n", m_fDMin, m_fDMax, m_sDImage);
        sRet = sRet + string.Format("B: {0} + ({1} - {0}) * {2}\n", m_fBMin, m_fBMax, m_sBImage);
        sRet = sRet + string.Format("K: {0} + ({1} - {0}) * {2}\n", m_fKMin, m_fKMax, m_sKImage);

        return sRet;
    }
}

public class UIManager : MonoBehaviour
{
    public static CSimulateParameter SimulateParameter;

    public static UIManager _Instance;
    private bool m_bStart = false;

    public RawImage m_tester;

    // Start is called before the first frame update
    void Start()
    {
        _Instance = this;
        m_bStart = false;
        for (int i = 0; i < m_pPages.Length; ++i)
        {
            if (null != m_pPages[i])
            {
                m_pPages[i].Intial(this, (EPage)i);
                m_pPages[i].Hide();
            }
        }

        for (int i = 0; i < m_pDialogPlanes.Length; ++i)
        {
            if (null != m_pDialogPlanes[i])
            {
                m_pDialogPlanes[i].gameObject.SetActive(false);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_bStart)
        {
            m_bStart = true;
            GoPage(EPage.Main, null);
        }
    }

    public EPage m_eCurrent = EPage.None;
    public UIPage[] m_pPages;

    public void GoPage(EPage ePage, CUIPageConfig config)
    {
        if (null != m_pPages[(int)m_eCurrent])
        {
            m_pPages[(int)m_eCurrent].Hide();
        }

        m_eCurrent = ePage;
        m_pPages[(int)m_eCurrent].Show(config);
    }

    public Image m_imgBackground;

    public void ShowBackground(bool bShow)
    {
        m_imgBackground.gameObject.SetActive(bShow);
    }


    #region Right Top Panel

    public Text m_txtNavLoginButton;
    public Text m_txtNavBackButton;

    public Button m_btBack;
    public Button m_btHelp;
    public Button m_btLogin;

    public void NavigationLoginButton()
    {

    }

    public void NavigationBackButton()
    {
        UISoundManager.UISound(EUISound.Button);
        if (null != m_pPages[(int)m_eCurrent])
        {
            m_pPages[(int)m_eCurrent].OnButtonBack();
        }
    }

    public void NavigationHelpButton()
    {
        UISoundManager.UISound(EUISound.Button);
    }

    public void ChangeInteract(bool bCanWork)
    {
        m_btBack.interactable = bCanWork;
        m_btHelp.interactable = bCanWork;
        m_btLogin.interactable = bCanWork;
    }

    #endregion

    #region Dialog

    public GameObject m_pDialogBG;

    public void OnDialogClosed(EDialog eDialog)
    {
        m_pDialogPlanes[(int)eDialog].gameObject.SetActive(false);
    }

    public void OnDialogPopUp(EDialog eDialog)
    {
        switch (eDialog)
        {
            case EDialog.Image:
                OnImageChooserShown();
                break;
            default:
                break;
        }
    }

    public void ShowDialog(EDialog eDialog)
    {
        m_pDialogBG.SetActive(true);
        UISoundManager.UISound(EUISound.Popup);
        m_pDialogPlanes[(int)eDialog].gameObject.SetActive(true);
        m_pDialogAnims[(int)eDialog]["PlaneIn"].normalizedTime = 0.0f;
        m_pDialogAnims[(int)eDialog]["PlaneIn"].speed = 1.0f;
        m_pDialogAnims[(int)eDialog]["PlaneIn"].wrapMode = WrapMode.Clamp;
        m_pDialogAnims[(int)eDialog].Play("PlaneIn");
    }

    public void HideDialog(EDialog eDialog)
    {
        m_pDialogBG.SetActive(false);
        UISoundManager.UISound(EUISound.Popup);
        m_pDialogAnims[(int)eDialog]["PlaneOut"].normalizedTime = 0.0f;
        m_pDialogAnims[(int)eDialog]["PlaneOut"].speed = 1.0f;
        m_pDialogAnims[(int)eDialog]["PlaneOut"].wrapMode = WrapMode.Clamp;
        m_pDialogAnims[(int)eDialog].Play("PlaneOut");
    }

    public Animation[] m_pDialogAnims;
    public Image[] m_pDialogPlanes;

    #region Two Button

    public Text m_txtSimpleDialogMsg;
    public Text m_txtSimpleDialogOKMid;
    public Text m_txtSimpleDialogOK;
    public Text m_txtSimpleDialogCancel;

    public Button m_btSimpleDialogOKMid;
    public Button m_btSimpleDialogOK;
    public Button m_btmpleDialogCancel;

    private Action m_SimpleDialogOK;
    private Action<bool> m_SimpleDialogOKCancel;


    public void ShowOneButtonDialog(string sMsg, Action ok = null, string sOK = "确定")
    {
        ShowDialog(EDialog.Msg);
        m_btSimpleDialogOKMid.gameObject.SetActive(true);
        m_btSimpleDialogOK.gameObject.SetActive(false);
        m_btmpleDialogCancel.gameObject.SetActive(false);

        m_txtSimpleDialogMsg.text = sMsg;
        m_txtSimpleDialogOKMid.text = sOK;
        m_SimpleDialogOK = ok;
        m_SimpleDialogOKCancel = null;
    }

    public void ShowTwoButtonDialog(string sMsg, Action<bool> ok = null, string sOK = "确定", string sCancel = "取消")
    {
        ShowDialog(EDialog.Msg);
        m_btSimpleDialogOKMid.gameObject.SetActive(false);
        m_btSimpleDialogOK.gameObject.SetActive(true);
        m_btmpleDialogCancel.gameObject.SetActive(true);

        m_txtSimpleDialogMsg.text = sMsg;
        m_txtSimpleDialogOK.text = sOK;
        m_txtSimpleDialogCancel.text = sCancel;
        m_SimpleDialogOK = null;
        m_SimpleDialogOKCancel = ok;
    }


    public void OnSimpleDialogOKMid()
    {

        HideDialog(EDialog.Msg);
        m_SimpleDialogOK?.Invoke();
    }

    public void OnSimpleDialogOK()
    {
        HideDialog(EDialog.Msg);
        m_SimpleDialogOKCancel?.Invoke(true);
    }

    public void OnSimpleDialogCancel()
    {
        HideDialog(EDialog.Msg);
        m_SimpleDialogOKCancel?.Invoke(false);
    }

    #endregion

    #region Image Choose

    private CImageDialogConfig m_ImageChooseConfig;

    public Material[] m_pDialogIamgeMats;

    private int m_iImageCurrentChoose;
    private int m_iImageMaxChoose;

    public RectTransform m_pContentRect;
    public UIChooseImage m_pOneImagePrefab;
    public List<UIChooseImage> m_lstImagePool = new List<UIChooseImage>();
    public Text m_txtImageChooserMsg;

    public void ShowImageChoose(CImageDialogConfig config)
    {
        ShowDialog(EDialog.Image);
        m_ImageChooseConfig = config;

        DirectoryInfo info = new DirectoryInfo(Application.streamingAssetsPath + "/" + m_ImageChooseConfig.m_sFolder);
        FileInfo[] fileInfo = info.GetFiles("*.png");
        //foreach (FileInfo file in fileInfo)
        //{
        //    Debug.Log(file.FullName);
        //}

        if (0 == m_lstImagePool.Count)
        {
            m_lstImagePool.Add(m_pOneImagePrefab);
        }

        for (int i = 0; i < m_lstImagePool.Count; ++i)
        {
            m_lstImagePool[i].Hide();
        }

        m_iImageCurrentChoose = -1;
        m_iImageMaxChoose = fileInfo.Length;
        for (int i = 0; i < fileInfo.Length; ++i)
        {
            if (i >= m_lstImagePool.Count)
            {
                GameObject newObj = GameObject.Instantiate<GameObject>(m_pOneImagePrefab.gameObject, m_pOneImagePrefab.transform.parent);
                UIChooseImage img = newObj.GetComponent<UIChooseImage>();
                m_lstImagePool.Add(img);
            }

            m_lstImagePool[i].m_iIndex = i;
            Texture2D tx2D = new Texture2D(256, 256, TextureFormat.RGB24, false);
            tx2D.LoadImage(File.ReadAllBytes(fileInfo[i].FullName), true);
            string sName = fileInfo[i].Name;
            int iDotPos = sName.IndexOf('.');
            if (iDotPos > 0)
            {
                sName = sName.Substring(0, iDotPos);
            }
            m_lstImagePool[i].Show(tx2D, m_pDialogIamgeMats[(int)m_ImageChooseConfig.m_eMat], sName);
            m_lstImagePool[i].gameObject.name = sName;
            if (sName.Equals(m_ImageChooseConfig.m_sNow))
            {
                m_iImageCurrentChoose = i;
                m_lstImagePool[i].ChooseMe(true);
            }
            else
            {
                m_lstImagePool[i].ChooseMe(false);
            }

            //Debug.Log(m_lstImagePool[i].transform.localPosition.y);
        }

        if (m_iImageCurrentChoose < 0)
        {
            m_iImageCurrentChoose = 0;
            m_ImageChooseConfig.m_sNow = m_lstImagePool[m_iImageCurrentChoose].m_pText.text;
            m_lstImagePool[0].ChooseMe(true);
        }

        int iLength = 60 + 170 * (1 + (m_iImageMaxChoose - 1) / 4);
        m_pContentRect.sizeDelta = new Vector2(m_pContentRect.sizeDelta.x, iLength);
        m_txtImageChooserMsg.text = m_ImageChooseConfig.m_sMsg;
    }

    public void OnBtImageChooseOK()
    {
        HideDialog(EDialog.Image);
        if (m_iImageCurrentChoose >= 0 && m_iImageCurrentChoose < m_iImageMaxChoose)
        {
            m_ImageChooseConfig?.m_pCallback?.Invoke(m_lstImagePool[m_iImageCurrentChoose].m_pText.text);
        }
    }

    public void OnBtImageChooseCancel()
    {
        HideDialog(EDialog.Image);
    }

    public void OnChooseImage(int iIndex)
    {
        for (int i = 0; i < m_iImageMaxChoose; ++i)
        {
            m_lstImagePool[i].ChooseMe(false);
        }
        m_iImageCurrentChoose = iIndex;
        m_ImageChooseConfig.m_sNow = m_lstImagePool[m_iImageCurrentChoose].m_pText.text;
        m_lstImagePool[m_iImageCurrentChoose].ChooseMe(true);
    }

    private void OnImageChooserShown()
    {
        float iHeight = -m_lstImagePool[m_iImageCurrentChoose].transform.localPosition.y - 100.0f;
        m_pContentRect.anchoredPosition = new Vector2(0.0f, iHeight);
    }

    #endregion

    #region Detail Parameter

    private CDetailDialogConfig m_pDetailConfig;
    public Text m_txtDetailMsg;
    public InputField m_inDetailA;
    public InputField m_inDetailB;
    public Button m_btDetailImage;
    public RawImage m_imgDetailImg;

    public void ShowDetailChoose(CDetailDialogConfig config)
    {
        ShowDialog(EDialog.Detail);
        m_pDetailConfig = config;
        m_txtDetailMsg.text = m_pDetailConfig.m_sMsg;

        //set up image
        Texture2D tx2D = new Texture2D(256, 256, TextureFormat.RGB24, false);
        tx2D.LoadImage(File.ReadAllBytes(
            Application.streamingAssetsPath + "/" 
            + m_pDetailConfig.m_sImageFolder + "/" 
            + m_pDetailConfig.m_Config.m_sImage + ".png"), true);
        m_imgDetailImg.texture = tx2D;

        m_inDetailA.SetTextWithoutNotify(m_pDetailConfig.m_Config.m_fDefaultMin.ToString());
        m_inDetailB.SetTextWithoutNotify(m_pDetailConfig.m_Config.m_fDefaultMax.ToString());

        //Debug.Log(m_pDetailConfig.m_Config.m_bEnableImage);
        m_btDetailImage.interactable = m_pDetailConfig.m_Config.m_bEnableImage;
        m_inDetailB.interactable = m_pDetailConfig.m_Config.m_bEnableMax;
    }


    public void OnDetailDialogOK()
    {
        HideDialog(EDialog.Detail);
        if (null != m_pDetailConfig.m_pCallback)
        {
            COneParameterConfig newcfg = new COneParameterConfig();
            newcfg.m_fDefaultMin = UISimulate.TryParse(m_inDetailA.text, m_pDetailConfig.m_Config.m_fDefaultMin, m_pDetailConfig.m_Config.m_fMin, m_pDetailConfig.m_Config.m_fMax);
            newcfg.m_fDefaultMax = UISimulate.TryParse(m_inDetailB.text, m_pDetailConfig.m_Config.m_fDefaultMax, m_pDetailConfig.m_Config.m_fMin, m_pDetailConfig.m_Config.m_fMax);
            newcfg.m_sImage = m_pDetailConfig.m_Config.m_sImage;
            m_pDetailConfig.m_pCallback(newcfg);
        }
    }

    public void OnDetailDialogCancel()
    {
        HideDialog(EDialog.Detail);
    }

    public void OnDetailDialogImage()
    {
        CImageDialogConfig imageConfig = new CImageDialogConfig();
        imageConfig.m_sFolder = m_pDetailConfig.m_sImageFolder;
        imageConfig.m_eMat = EPreviewMatType.EPMT_Mask;
        imageConfig.m_sNow = m_pDetailConfig.m_Config.m_sImage;
        imageConfig.m_pCallback = OnDetailDialogImageChanged;
        imageConfig.m_sMsg = "";
        ShowImageChoose(imageConfig);
    }

    public void OnDetailDialogInputChangeMin()
    {
        float fNewAlpha = UISimulate.TryParse(m_inDetailA.text,
            m_pDetailConfig.m_Config.m_fDefaultMin,
            m_pDetailConfig.m_Config.m_fMin,
            m_pDetailConfig.m_Config.m_fMax);

        m_pDetailConfig.m_Config.m_fDefaultMin = fNewAlpha;
        m_inDetailA.SetTextWithoutNotify(fNewAlpha.ToString());
    }

    public void OnDetailDialogInputChangeMax()
    {
        float fNewAlpha = UISimulate.TryParse(m_inDetailB.text,
            m_pDetailConfig.m_Config.m_fDefaultMax,
            m_pDetailConfig.m_Config.m_fMin,
            m_pDetailConfig.m_Config.m_fMax);

        m_pDetailConfig.m_Config.m_fDefaultMax = fNewAlpha;
        m_inDetailB.SetTextWithoutNotify(fNewAlpha.ToString());
    }

    private void OnDetailDialogImageChanged(string sNewImage)
    {
        //Debug.Log(sNewImage);
        m_pDetailConfig.m_Config.m_sImage = sNewImage;
        Texture2D tx2D = new Texture2D(256, 256, TextureFormat.RGB24, false);
        tx2D.LoadImage(File.ReadAllBytes(
            Application.streamingAssetsPath + "/"
                                            + m_pDetailConfig.m_sImageFolder + "/"
                                            + m_pDetailConfig.m_Config.m_sImage + ".png"), true);
        m_imgDetailImg.texture = tx2D;
    }


    #endregion

    #region Save File

    private CSaveDialogConfig m_pSaveConfig;
    public Text m_txtSaveMsg;
    public InputField m_inputSaveFileName;
    public InputField m_inputSaveFileDiscription;

    public void ShowSDialogSave(CSaveDialogConfig config)
    {
        m_pSaveConfig = config;
        ShowDialog(EDialog.SaveFile);
        m_txtSaveMsg.text = "";
    }

    public void OnBtDialogSaveOK()
    {
        if (string.IsNullOrEmpty(m_inputSaveFileName.text))
        {
            m_txtSaveMsg.text = "<color=#FF0000>文件名不能为空</color>";
            return;
        }

        if (string.IsNullOrEmpty(m_inputSaveFileDiscription.text))
        {
            m_txtSaveMsg.text = "<color=#FF0000>文件描述不能为空</color>";
            return;
        }

        string sFile = m_pSaveConfig.m_sImageFolder + m_inputSaveFileName.text + ".png";
        if (File.Exists(sFile))
        {
            m_txtSaveMsg.text = "<color=#FF0000>文件名不能覆盖已有文件</color>";
            return;
        }

        HideDialog(EDialog.SaveFile);
        string sDescFile = m_pSaveConfig.m_sImageFolder + m_inputSaveFileName.text + ".txt";
        File.WriteAllBytes(sFile, m_pSaveConfig.m_pToSave);
        File.WriteAllText(sDescFile, m_inputSaveFileDiscription + "\n" + m_pSaveConfig.m_sParameter);
    }

    public void OnBtDialogSaveCancel()
    {
        HideDialog(EDialog.SaveFile);
    }

    #endregion

    #endregion

}
