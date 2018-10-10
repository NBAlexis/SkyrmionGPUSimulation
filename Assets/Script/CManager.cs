using System;
using System.Collections.Generic;
using System.IO;
using LuaInterface;
using SFB;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Skyrmion/Scripts/Manager")]
public class CManager : MonoBehaviour
{
    #region Show

    public RawImage OutPutSingle;
    public RawImage Indicator;

    public Text FrameCount;
    public Text SimDate;

    public Button Bt;

    public InputField K;
    public InputField D;
    public InputField D0;
    public InputField B;
    public InputField alpha;
    public InputField timestep;
    public InputField stopstep;
    public InputField savestep;

    public Text JButtonName;
    public Text MagButtonName;
    public Text BoundaryName;
    public Text JxPeroidFunctionName;

    public Button JButton;
    public Button MagButton;
    public Button CondButton;
    public Button JxButton;
    public Button RemoveJxButton;

    public Button ResetButton;
    public Button SaveButton;

    public Material MatShow;
    public Material MatGetXYZ;

    public Toggle ToggleInverseNz;

    #endregion

    #region Compute Shader

    public ComputeShader CmpShader;
    private int m_iK1Handle;
    private int m_iK2Handle;
    private int m_iK3Handle;
    private int m_iKernelHandle;

    #endregion

    #region Calculators

    private RenderTexture K1X;
    private RenderTexture K1Y;
    private RenderTexture K1Z;

    private RenderTexture InternalRTR;
    private RenderTexture InternalRTG;
    private RenderTexture InternalRTB;
    private Texture2D JTexture;
    private Texture2D JxTexture;

    #endregion

    #region Std Dev

    public Text StandardDevationText;
    public Toggle StandardDevationToggle;

    private int m_iLastPreserveStep = -1;

    private Texture2D PreData = null;

    private static readonly List<string> m_lstStdDevRes = new List<string>();

    private void CalculateStdDev()
    {
        RenderTexture rt = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32);

        MatGetXYZ.SetTexture("_Nx", InternalRTR);
        MatGetXYZ.SetTexture("_Ny", InternalRTG);
        MatGetXYZ.SetTexture("_Nz", InternalRTB);
        Graphics.Blit(null, rt, MatGetXYZ);
        Texture2D dataReader = new Texture2D(512, 512, TextureFormat.ARGB32, false);
        RenderTexture.active = rt;
        dataReader.ReadPixels(new Rect(0, 0, 512, 512), 0, 0);
        dataReader.Apply();
        RenderTexture.active = null;
        rt.Release();

        //Data obtained in dataReader
        if (m_iLastPreserveStep != m_iFrame - 1 || null == PreData)
        {
            if (null != PreData)
            {
                Destroy(PreData);
                PreData = null;
            }
            PreData = dataReader;
            m_iLastPreserveStep = m_iFrame;
            return;
        }

        double err = 0.0f;
        for (int i = 0; i < 512; ++i)
        {
            for (int j = 0; j < 512; ++j)
            {
                Color c1 = PreData.GetPixel(i, j);
                Color c2 = dataReader.GetPixel(i, j);
                err += new Vector3(c1.r - c2.r, c1.g - c2.g, c1.b - c2.b).sqrMagnitude;
            }
        }
        err = err/(Mathf.Sqrt(512*512 - 1)* m_fTimeStep);

        m_lstStdDevRes.Add(string.Format("step{0}:\n{1}\n", m_iFrame, err));
        if (m_lstStdDevRes.Count > 80)
        {
            m_lstStdDevRes.RemoveAt(0);
        }

        string sOutPut = "";
        for (int i = 0; i < m_lstStdDevRes.Count; ++i)
        {
            sOutPut = sOutPut + m_lstStdDevRes[m_lstStdDevRes.Count - i - 1];
        }
        StandardDevationText.text = sOutPut;

        Destroy(PreData);
        PreData = null;
        PreData = dataReader;
        m_iLastPreserveStep = m_iFrame;
    }

    #endregion

    private Texture2D m_pTestT2;
    private int m_iFrame = 0;
    private int m_iStopFrame = -1;
    private int m_iSaveFrame = -1;
    private bool m_bRunning = false;
    private float m_fTimeStep = 1.0f;
    private DateTime m_dtSim;

    private bool m_bJSet = false;
    private bool m_bMagSet = false;
    private bool m_bCondSet = false;
    private bool m_bInverseNz = false;
    private bool m_bJxPeroidSet = false;

    private static readonly Color[] _color512 = new Color[512 * 512];

    //private float m_fShowSep = 0.3f;
    //private float m_fShowSepTicker = 0.0f;

    // Use this for initialization
    void Start ()
	{
        Application.runInBackground = true;
        Application.targetFrameRate = -1;

        Texture2D indit = new Texture2D(128, 128, TextureFormat.ARGB32, false);
        Color[] colorsi = new Color[128 * 128];
        for (int i = 0; i < 128 * 128; ++i)
        {
            Vector3 v = new Vector3(i % 128, i / 128, 0.0f) * (1.0f / 127.0f);
            colorsi[i] = new Color(v.x, v.y, 0.0f, 1.0f);
        }
        indit.SetPixels(colorsi);
        indit.Apply(true);

        Indicator.texture = indit;

        m_iK1Handle = CmpShader.FindKernel("CaclK1");
        m_iK2Handle = CmpShader.FindKernel("CaclK2");
        m_iK3Handle = CmpShader.FindKernel("CaclK3");
        m_iKernelHandle = CmpShader.FindKernel("CSMain");
        CmpShader.SetInts("size", new int[] { 512, 512 });

        K1X = new RenderTexture(1024, 1024, 0, RenderTextureFormat.RFloat);
        K1X.enableRandomWrite = true;
        K1X.Create();
        K1Y = new RenderTexture(1024, 1024, 0, RenderTextureFormat.RFloat);
        K1Y.enableRandomWrite = true;
        K1Y.Create();
        K1Z = new RenderTexture(1024, 1024, 0, RenderTextureFormat.RFloat);
        K1Z.enableRandomWrite = true;
        K1Z.Create();

        CmpShader.SetTexture(m_iK1Handle, "k1x", K1X);
        CmpShader.SetTexture(m_iK1Handle, "k1y", K1Y);
        CmpShader.SetTexture(m_iK1Handle, "k1z", K1Z);

        CmpShader.SetTexture(m_iK2Handle, "k1x", K1X);
        CmpShader.SetTexture(m_iK2Handle, "k1y", K1Y);
        CmpShader.SetTexture(m_iK2Handle, "k1z", K1Z);

        CmpShader.SetTexture(m_iK3Handle, "k1x", K1X);
        CmpShader.SetTexture(m_iK3Handle, "k1y", K1Y);
        CmpShader.SetTexture(m_iK3Handle, "k1z", K1Z);

        CmpShader.SetTexture(m_iKernelHandle, "k1x", K1X);
        CmpShader.SetTexture(m_iKernelHandle, "k1y", K1Y);
        CmpShader.SetTexture(m_iKernelHandle, "k1z", K1Z);


        Bt.GetComponentInChildren<Text>().text = "Start";

        //ShowRT = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB64, RenderTextureReadWrite.sRGB);
        //OutPutSingle.texture = ShowRT;
    }
	
	// Update is called once per frame
	void Update ()
    {
	    if (m_bRunning)
	    {
            CmpShader.Dispatch(m_iK1Handle, 512 / 8, 512 / 8, 1);
            CmpShader.Dispatch(m_iK2Handle, 512 / 8, 512 / 8, 1);
            CmpShader.Dispatch(m_iK3Handle, 512 / 8, 512 / 8, 1);
            CmpShader.Dispatch(m_iKernelHandle, 512 / 8, 512 / 8, 1);

            ++m_iFrame;
            FrameCount.text = m_iFrame.ToString();

	        //m_fShowSepTicker += Time.deltaTime;
	        //if (m_fShowSepTicker > m_fShowSep)
	        //{
	        //    m_fShowSepTicker = 0.0f;
            //    Graphics.Blit(null, ShowRT, GetXYZ);
	        //}

	        if (StandardDevationToggle.isOn)
	        {
	            CalculateStdDev();
	        }

            if (m_iStopFrame == m_iFrame)
	        {
	            OnBtGo();
	        }

	        if (m_iFrame > 0 
             && m_iSaveFrame > 0
             && 0 == (m_iFrame%m_iSaveFrame))
	        {
	            SaveBt();
	        }
        }
    }

    public void OnBtGo()
    {
        if (m_bRunning)
        {
            m_bRunning = false;
            Bt.GetComponentInChildren<Text>().text = "Start";
            K.interactable = true;
            D.interactable = true;
            D0.interactable = true;
            B.interactable = true;
            alpha.interactable = true;
            timestep.interactable = true;
            stopstep.interactable = true;
            savestep.interactable = true;

            JButton.interactable = true;
            CondButton.interactable = true;
            ResetButton.interactable = true;
            SaveButton.interactable = true;
            MagButton.interactable = true;
            JxButton.interactable = true;
            RemoveJxButton.interactable = true;
        }
        else
        {
            Bt.GetComponentInChildren<Text>().text = "Stop";
            K.interactable = false;
            D.interactable = false;
            D0.interactable = false;
            B.interactable = false;
            alpha.interactable = false;
            timestep.interactable = false;
            stopstep.interactable = false;
            savestep.interactable = false;

            JButton.interactable = false;
            CondButton.interactable = false;
            ResetButton.interactable = false;
            SaveButton.interactable = false;
            MagButton.interactable = false;
            JxButton.interactable = false;
            RemoveJxButton.interactable = false;

            CmpShader.SetFloat("K", float.Parse(K.text));
            CmpShader.SetFloat("D", float.Parse(D.text));
            CmpShader.SetFloat("D0", float.Parse(D0.text));
            CmpShader.SetFloat("B", float.Parse(B.text));
            m_fTimeStep = float.Parse(timestep.text);
            CmpShader.SetFloat("timestep", m_fTimeStep);
            CmpShader.SetFloat("alpha", float.Parse(alpha.text));

            m_bRunning = true;
            m_iStopFrame = int.Parse(stopstep.text);
            m_iSaveFrame = int.Parse(savestep.text);

            CmpShader.SetInt("jxstep", 0);
            if (!m_bJxPeroidSet)
            {
                SetJXTexture(0, null);
            }

            if (0 == m_iFrame)
            {
                m_dtSim = DateTime.Now;
                SimDate.text = string.Format("{0}-{1}-{2}-{3}", m_dtSim.Month, m_dtSim.Day, m_dtSim.Hour, m_dtSim.Minute);

                if (m_iSaveFrame > 0)
                {
                    SaveBt();
                }
            }
        }
    }

#if UNITY_EDITOR
    public static readonly string _outfolder = "/";
#else
    public static readonly string _outfolder = "/../";
#endif

    public void LoadMagneticBt()
    {
        string[] sPath = StandaloneFileBrowser.OpenFilePanel("Choose the script or image", 
            Application.dataPath + _outfolder,
            new[] { new ExtensionFilter("Lua Script", "lua"), new ExtensionFilter("Image Files", "png", "jpg", "jpeg"), }, 
            false);

        if (sPath.Length > 0 && !string.IsNullOrEmpty(sPath[0]))
        {
            if (sPath[0].Contains(".lua"))
            {
                if (LoadManetic(sPath[0]))
                {
                    string sFileName = sPath[0];
                    sFileName = sFileName.Replace("\\", "/");
                    int iLastSlash = Mathf.Max(0, sFileName.LastIndexOf("/"));
                    MagButtonName.text = sFileName.Substring(iLastSlash);
                    m_bMagSet = true;

                    if (m_bMagSet && m_bJSet && m_bCondSet)
                    {
                        Bt.interactable = true;
                    }
                }
            }
            else
            {
                if (LoadMagneticPic(sPath[0]))
                {
                    string sFileName = sPath[0];
                    sFileName = sFileName.Replace("\\", "/");
                    int iLastSlash = Mathf.Max(0, sFileName.LastIndexOf("/"));
                    MagButtonName.text = sFileName.Substring(iLastSlash);
                    m_bMagSet = true;

                    if (m_bMagSet && m_bJSet && m_bCondSet)
                    {
                        Bt.interactable = true;
                    }
                }
            }
        }
    }

    public void LoadJValueBt()
    {
        string[] sPath = StandaloneFileBrowser.OpenFilePanel("Choose the script", 
            Application.dataPath + _outfolder, 
            "lua",
            false);

        if (sPath.Length > 0)
        {
            if (LoadJValue(sPath[0]))
            {
                string sFileName = sPath[0];
                sFileName = sFileName.Replace("\\", "/");
                int iLastSlash = Mathf.Max(0, sFileName.LastIndexOf("/"));
                JButtonName.text = sFileName.Substring(iLastSlash);
                m_bJSet = true;

                if (m_bMagSet && m_bJSet && m_bCondSet)
                {
                    Bt.interactable = true;
                }
            }
        }
    }

    public void LoadEdgeBt()
    {
        string[] sPath = StandaloneFileBrowser.OpenFilePanel("Choose the picture", Application.dataPath + _outfolder, new [] { new ExtensionFilter("Image Files", "png", "jpg", "jpeg")}, false);
        if (sPath.Length > 0)
        {
            if (SetEdge(sPath[0]))
            {
                string sFileName = sPath[0];
                sFileName = sFileName.Replace("\\", "/");
                int iLastSlash = Mathf.Max(0, sFileName.LastIndexOf("/"));
                BoundaryName.text = sFileName.Substring(iLastSlash);

                m_bCondSet = true;

                if (m_bMagSet && m_bJSet && m_bCondSet)
                {
                    Bt.interactable = true;
                }
            }
        }
    }

    public void LoadJXPeroidFunction()
    {
        string[] sPath = StandaloneFileBrowser.OpenFilePanel("Choose the script",
                                                            Application.dataPath + _outfolder,
                                                            "lua",
                                                            false);

        if (sPath.Length > 0)
        {
            if (LoadJXValue(sPath[0]))
            {
                string sFileName = sPath[0];
                sFileName = sFileName.Replace("\\", "/");
                int iLastSlash = Mathf.Max(0, sFileName.LastIndexOf("/"));
                JxPeroidFunctionName.text = sFileName.Substring(iLastSlash);
                m_bJxPeroidSet = true;
            }
        }
    }

    public void RemoveJxBtFunc()
    {
        m_bJxPeroidSet = false;
        JxPeroidFunctionName.text = "none";
    }

    public void SaveBt()
    {
        string sDate = SimDate.text;
        int iStep = m_iFrame;
        CExportData.Save(sDate, iStep, 
            string.Format(@"
start time={0}
step={1}
exchange strength J value={2}
initla magnetic={3}
boundary condition={4}
electrical current={5}
K={6}
D={7} + J * {8}
B={9}
Gilbert alpha={10}
time step={11}",
            m_dtSim.ToString("MM-dd-yyyy hh:mm:ss"),
            m_iFrame,
            JButtonName.text,
            MagButtonName.text,
            BoundaryName.text,
            JxPeroidFunctionName.text,
            K.text,
            D0.text,
            D.text,
            B.text,
            alpha.text,
            m_fTimeStep
            ), 
            InternalRTR,
            InternalRTG,
            InternalRTB,
            MatShow,
            MatGetXYZ);
    }

    public void ResetBt()
    {
        m_iFrame = 0;
        FrameCount.text = "0";
    }

    public void OnInverseNz()
    {
        m_bInverseNz = ToggleInverseNz.isOn;
        MatShow.SetFloat("_InverseNz", m_bInverseNz ? 1.0f : 0.0f);
        OutPutSingle.material.SetFloat("_InverseNz", m_bInverseNz ? 1.0f : 0.0f);
    }

    private static readonly Vector3[] _mags = new Vector3[512 * 512];
    private bool LoadManetic(string sLuaFileName)
    {
        Util.ClearMemory();
        string sLuaCode = File.ReadAllText(sLuaFileName);
        LuaScriptMgr mgr = new LuaScriptMgr();
        mgr.DoString(sLuaCode);
        LuaFunction func = mgr.GetLuaFunction("GetMagneticByLatticeIndex");
        if (null == func)
        {
            ShowErrorMessage("GetMagneticByLatticeIndex function cannot been found.");
            mgr.Destroy();
            return false;
        }

        for (int x = 0; x < 512; ++x)
        {
            for (int y = 0; y < 512; ++y)
            {
                object[] r = func.Call2(x, y);
                if (3 != r.Length 
                 || !(r[0] is double)
                 || !(r[1] is double)
                 || !(r[2] is double))
                {
                    ShowErrorMessage("GetMagneticByLatticeIndex should return 3 numbers.");
                    mgr.Destroy();
                    return false;
                }

                _mags[y * 512 + x] = new Vector3(
                    (float)(double)r[0], 
                    (float)(double)r[1], 
                    (float)(double)r[2]);
            }
        }
        mgr.Destroy();
        SetCurrentState(_mags);
        return true;
    }

    private Texture2D _txR, _txG, _txB;
    private readonly Color[] _txcR = new Color[512 * 512];
    private readonly Color[] _txcG = new Color[512 * 512];
    private readonly Color[] _txcB = new Color[512 * 512];

    private bool LoadMagneticPic(string sPicFileName)
    {
        if (null == m_pTestT2)
        {
            m_pTestT2 = new Texture2D(512, 512, TextureFormat.RGBA32, false);
        }
        if (!m_pTestT2.LoadImage(File.ReadAllBytes(sPicFileName), false))
        {
            ShowErrorMessage("Not support this file format.");
            m_pTestT2 = new Texture2D(512, 512, TextureFormat.RGBA32, false);
            return false;
        }

        if (512 != m_pTestT2.width || 512 != m_pTestT2.height)
        {
            ShowErrorMessage("Only support 512 x 512 file.");
            m_pTestT2 = new Texture2D(512, 512, TextureFormat.RGBA32, false);
            return false;
        }

        for (int x = 0; x < 512; ++x)
        {
            for (int y = 0; y < 512; ++y)
            {
                Color c = m_pTestT2.GetPixel(x, y);
                _mags[y*512 + x] = new Vector3(2.0f*c.r - 1.0f, 2.0f*c.g - 1.0f, 2.0f*c.b - 1.0f);
            }
        }

        SetCurrentState(_mags);

        return true;
    }

    private void SetCurrentState(Vector3[] magnetic)
    {
        if (null == _txR)
        {
            _txR = new Texture2D(512, 512, TextureFormat.RFloat, false);
        }
        if (null == _txG)
        {
            _txG = new Texture2D(512, 512, TextureFormat.RFloat, false);
        }
        if (null == _txB)
        {
            _txB = new Texture2D(512, 512, TextureFormat.RFloat, false);
        }

        for (int i = 0; i < 512 * 512; ++i)
        {
            magnetic[i].Normalize();
            _txcR[i] = new Color(magnetic[i].x, 0.0f, 0.0f);
            _txcG[i] = new Color(magnetic[i].y, 0.0f, 0.0f);
            _txcB[i] = new Color(magnetic[i].z, 0.0f, 0.0f);
        }

        _txR.SetPixels(_txcR);
        _txR.Apply(false);
        _txG.SetPixels(_txcG);
        _txG.Apply(false);
        _txB.SetPixels(_txcB);
        _txB.Apply(false);

        if (null == InternalRTR)
        {
            InternalRTR = new RenderTexture(512, 512, 0, RenderTextureFormat.RFloat);
            InternalRTR.enableRandomWrite = true;
            InternalRTR.Create();
            OutPutSingle.material.SetTexture("_Nx", InternalRTR);
        }
        if (null == InternalRTG)
        {
            InternalRTG = new RenderTexture(512, 512, 0, RenderTextureFormat.RFloat);
            InternalRTG.enableRandomWrite = true;
            InternalRTG.Create();
            OutPutSingle.material.SetTexture("_Ny", InternalRTG);
        }
        if (null == InternalRTB)
        {
            InternalRTB = new RenderTexture(512, 512, 0, RenderTextureFormat.RFloat);
            InternalRTB.enableRandomWrite = true;
            InternalRTB.Create();
            OutPutSingle.material.SetTexture("_Nz", InternalRTB);
        }

        Graphics.Blit(_txR, InternalRTR);
        Graphics.Blit(_txG, InternalRTG);
        Graphics.Blit(_txB, InternalRTB);

        CmpShader.SetTexture(m_iK1Handle, "magneticMomentumX", InternalRTR);
        CmpShader.SetTexture(m_iK1Handle, "magneticMomentumY", InternalRTG);
        CmpShader.SetTexture(m_iK1Handle, "magneticMomentumZ", InternalRTB);
        CmpShader.SetTexture(m_iK2Handle, "magneticMomentumX", InternalRTR);
        CmpShader.SetTexture(m_iK2Handle, "magneticMomentumY", InternalRTG);
        CmpShader.SetTexture(m_iK2Handle, "magneticMomentumZ", InternalRTB);
        CmpShader.SetTexture(m_iK3Handle, "magneticMomentumX", InternalRTR);
        CmpShader.SetTexture(m_iK3Handle, "magneticMomentumY", InternalRTG);
        CmpShader.SetTexture(m_iK3Handle, "magneticMomentumZ", InternalRTB);

        CmpShader.SetTexture(m_iKernelHandle, "magneticMomentumX", InternalRTR);
        CmpShader.SetTexture(m_iKernelHandle, "magneticMomentumY", InternalRTG);
        CmpShader.SetTexture(m_iKernelHandle, "magneticMomentumZ", InternalRTB);

        //Graphics.Blit(null, ShowRT, GetXYZ);
    }

    private static readonly float[] _jvalues = new float[512 * 512];
    private bool LoadJValue(string sLuaFileName)
    {
        Util.ClearMemory();
        string sLuaCode = File.ReadAllText(sLuaFileName);
        LuaScriptMgr mgr = new LuaScriptMgr();
        mgr.DoString(sLuaCode);
        LuaFunction func = mgr.GetLuaFunction("GetJValueByLatticeIndex");
        if (null == func)
        {
            ShowErrorMessage("GetJValueByLatticeIndex function cannot been found.");
            mgr.Destroy();
            return false;
        }

        for (int x = 0; x < 512; ++x)
        {
            for (int y = 0; y < 512; ++y)
            {
                object[] r = func.Call2(x, y);
                if (1 != r.Length || !(r[0] is double))
                {
                    ShowErrorMessage("Call function GetJValueByLatticeIndex failed, function may have error.");
                    mgr.Destroy();
                    return false;
                }
                _jvalues[y * 512 + x] = (float)(double)r[0];
            }
        }
        mgr.Destroy();
        SetJTexture(_jvalues);
        return true;
    }

    private void SetJTexture(float[] js)
    {
        if (null == JTexture)
        {
            JTexture = new Texture2D(512, 512, TextureFormat.RFloat, false);
        }

        for (int i = 0; i < 512 * 512; ++i)
        {
            _color512[i] = new Color(js[i], 0.0f, 0.0f);
        }
        JTexture.SetPixels(_color512);
        JTexture.Apply(false);

        CmpShader.SetTexture(m_iK1Handle, "exchangeStrength", JTexture);
        CmpShader.SetTexture(m_iK2Handle, "exchangeStrength", JTexture);
        CmpShader.SetTexture(m_iK3Handle, "exchangeStrength", JTexture);
        CmpShader.SetTexture(m_iKernelHandle, "exchangeStrength", JTexture);
    }

    private Texture2D _theEdgeTexture = null;
    private bool SetEdge(string sPNGFileName)
    {
        if (null == _theEdgeTexture)
        {
            _theEdgeTexture = new Texture2D(512, 512, TextureFormat.Alpha8, false);
        }
        if (!_theEdgeTexture.LoadImage(File.ReadAllBytes(sPNGFileName), true))
        {
            ShowErrorMessage("Not support this file format.");
            return false;
        }

        if (512 != _theEdgeTexture.width || 512 != _theEdgeTexture.height)
        {
            ShowErrorMessage("Only support 512 x 512 file.");
            return false;
        }
        CmpShader.SetTexture(m_iKernelHandle, "boundaryCondition", _theEdgeTexture);
        return true;
    }

    private bool LoadJXValue(string sLuaFileName)
    {
        Util.ClearMemory();
        string sLuaCode = File.ReadAllText(sLuaFileName);
        LuaScriptMgr mgr = new LuaScriptMgr();
        mgr.DoString(sLuaCode);

        LuaFunction func1 = mgr.GetLuaFunction("GetJxPeroidLength");
        if (null == func1)
        {
            ShowErrorMessage("GetJxPeroidLength function cannot been found.");
            mgr.Destroy();
            return false;
        }

        object[] r1 = func1.Call();
        if (1 != r1.Length || !(r1[0] is double))
        {
            ShowErrorMessage("GetJxPeroidLength function should return a number.");
            mgr.Destroy();
            return false;
        }

        //The 2.0f multipler is For Runge–Kutta, we need t + dt and t + 0.5 dt both!
        int iStepLength = Mathf.RoundToInt((float)(double)r1[0] * 2.0f);

        float[] jxvalue = null;
        if (iStepLength > 0)
        {
            LuaFunction func2 = mgr.GetLuaFunction("GetJxValueInPeroid");
            if (null == func2)
            {
                ShowErrorMessage("GetJxValueInPeroid function cannot been found.");
                mgr.Destroy();
                return false;
            }

            jxvalue = new float[iStepLength];
            for (int y = 0; y < iStepLength; ++y)
            {
                object[] r2 = func2.Call(y / (float)iStepLength);
                if (1 != r2.Length || !(r2[0] is double))
                {
                    ShowErrorMessage("GetJxValueInPeroid function should return a number.");
                    mgr.Destroy();
                    return false;
                }

                jxvalue[y] = (float)(double)r2[0];
            }
        }
        mgr.Destroy();
        SetJXTexture(iStepLength, jxvalue);
        return true;
    }

    private void SetJXTexture(int jxStep, float[] jxValue)
    {
        if (jxStep < 1 || null == jxValue || 0 == jxValue.Length)
        {
            CmpShader.SetInt("jxperoid", 0);

            Color[] nonejxcolor = new Color[1];
            nonejxcolor[0] = new Color(0.0f, 0.0f, 0.0f);
            if (null == JxTexture || JxTexture.width != 1)
            {
                JxTexture = new Texture2D(1, 1, TextureFormat.RFloat, false);
            }
            JxTexture.SetPixels(nonejxcolor);
            JxTexture.Apply(false);

            CmpShader.SetTexture(m_iK1Handle, "jxPeroidFunction", JxTexture);
            CmpShader.SetTexture(m_iK2Handle, "jxPeroidFunction", JxTexture);
            CmpShader.SetTexture(m_iK3Handle, "jxPeroidFunction", JxTexture);

            CmpShader.SetTexture(m_iKernelHandle, "jxPeroidFunction", JxTexture);

            return;
        }

        if (null == JxTexture || JxTexture.width != jxStep)
        {
            JxTexture = new Texture2D(jxStep, 1, TextureFormat.RFloat, false);
        }

        Color[] jxcolors = new Color[jxStep];
        for (int i = 0; i < jxStep; ++i)
        {
            jxcolors[i] = new Color(jxValue[i], 0.0f, 0.0f);
        }
        JxTexture.SetPixels(jxcolors);
        JxTexture.Apply(false);
        CmpShader.SetInt("jxperoid", jxStep);

        CmpShader.SetTexture(m_iK1Handle, "jxPeroidFunction", JxTexture);
        CmpShader.SetTexture(m_iK2Handle, "jxPeroidFunction", JxTexture);
        CmpShader.SetTexture(m_iK3Handle, "jxPeroidFunction", JxTexture);

        CmpShader.SetTexture(m_iKernelHandle, "jxPeroidFunction", JxTexture);
    }

    public void ShowErrorMessage(string sMsg)
    {
        m_sMessage = sMsg;
        m_bMsgShow = true;
    }

#region Old GUI MessageBox

    private Rect m_rcWindowRect = new Rect(Screen.width * 0.05f, Screen.height * 0.05f, Screen.width * 0.9f, Screen.height * 0.9f);
    private bool m_bMsgShow = false;
    private string m_sMessage = "";

    private void OnGUI()
    {
        if (m_bMsgShow)
        {
            m_rcWindowRect = GUI.Window(0, m_rcWindowRect, DialogWindow, "Error");
        }
    }

    private void DialogWindow(int windowID)
    {
        GUI.Label(new Rect(5, 5, m_rcWindowRect.width - 10, 50), m_sMessage);

        if (GUI.Button(new Rect(m_rcWindowRect.width * 0.3f, m_rcWindowRect.height - 30, m_rcWindowRect.width * 0.4f, 20), "OK"))
        {
            m_bMsgShow = false;
        }
    }

#endregion
}
