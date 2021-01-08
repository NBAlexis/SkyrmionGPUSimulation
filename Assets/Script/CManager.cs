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
    public Toggle TogglePeriodic;

    #endregion

    #region Compute Shader

    public ComputeShader CmpShader;
    private int m_iK1Handle;
    private int m_iK2Handle;
    private int m_iK3Handle;
    private int m_iKernelHandle;

    private int m_iK1HandleP;
    private int m_iK2HandleP;
    private int m_iK3HandleP;
    private int m_iKernelHandleP;

    private int m_iJxStep = 0;

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
        RenderTexture rt = new RenderTexture(m_iResolution, m_iResolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);

        MatGetXYZ.SetTexture("_Nx", InternalRTR);
        MatGetXYZ.SetTexture("_Ny", InternalRTG);
        MatGetXYZ.SetTexture("_Nz", InternalRTB);
        Graphics.Blit(null, rt, MatGetXYZ);
        Texture2D dataReader = new Texture2D(m_iResolution, m_iResolution, TextureFormat.ARGB32, false, true);
        RenderTexture.active = rt;
        dataReader.ReadPixels(new Rect(0, 0, m_iResolution, m_iResolution), 0, 0);
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
        for (int i = 0; i < m_iResolution; ++i)
        {
            for (int j = 0; j < m_iResolution; ++j)
            {
                Color c1 = PreData.GetPixel(i, j);
                Color c2 = dataReader.GetPixel(i, j);
                err += new Vector3(c1.r - c2.r, c1.g - c2.g, c1.b - c2.b).sqrMagnitude;
            }
        }
        err = err/(Mathf.Sqrt(m_iResolution * m_iResolution - 1)* m_fTimeStep);

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

    #region Resolution

    public InputField InputResolution;
    public Button BtResolution;
    public Text TxtResolution;

    [HideInInspector]
    public int m_iResolution = 512;

    #endregion

    private Texture2D m_pTestT2 = null;
    private int m_iFrame = 0;
    private int m_iStopFrame = -1;
    private int m_iSaveFrame = -1;
    private bool m_bRunning = false;
    private float m_fTimeStep = 1.0f;
    private DateTime m_dtSim;

    private bool m_bJSet = false;
    private bool m_bMagSet = false;
    private string m_sCondName = "none";
    private bool m_bCondSet = false;
    private bool m_bInverseNz = false;
    private bool m_bJxPeroidSet = false;

    private static Color[] _color512 = new Color[512 * 512];

    //private float m_fShowSep = 0.3f;
    //private float m_fShowSepTicker = 0.0f;

    // Use this for initialization
    void Start ()
	{
        Application.runInBackground = true;
        Application.targetFrameRate = -1;

        Texture2D indit = new Texture2D(128, 128, TextureFormat.ARGB32, false, true);
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
        m_iK1HandleP = CmpShader.FindKernel("CaclK1p");
        m_iK2HandleP = CmpShader.FindKernel("CaclK2p");
        m_iK3HandleP = CmpShader.FindKernel("CaclK3p");
        m_iKernelHandleP = CmpShader.FindKernel("CSMainp");

        BuildInitialTexture(512);

        Bt.GetComponentInChildren<Text>().text = "Start";

        //ShowRT = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB64, RenderTextureReadWrite.sRGB);
        //OutPutSingle.texture = ShowRT;
    }

    private void BuildInitialTexture(int iRes)
    {
        CmpShader.SetInts("size", new int[] { iRes, iRes });

        K1X = new RenderTexture(iRes * 2, iRes * 2, 0, RenderTextureFormat.RFloat);
        K1X.enableRandomWrite = true;
        K1X.Create();
        K1Y = new RenderTexture(iRes * 2, iRes * 2, 0, RenderTextureFormat.RFloat);
        K1Y.enableRandomWrite = true;
        K1Y.Create();
        K1Z = new RenderTexture(iRes * 2, iRes * 2, 0, RenderTextureFormat.RFloat);
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

        CmpShader.SetTexture(m_iK1HandleP, "k1x", K1X);
        CmpShader.SetTexture(m_iK1HandleP, "k1y", K1Y);
        CmpShader.SetTexture(m_iK1HandleP, "k1z", K1Z);

        CmpShader.SetTexture(m_iK2HandleP, "k1x", K1X);
        CmpShader.SetTexture(m_iK2HandleP, "k1y", K1Y);
        CmpShader.SetTexture(m_iK2HandleP, "k1z", K1Z);

        CmpShader.SetTexture(m_iK3HandleP, "k1x", K1X);
        CmpShader.SetTexture(m_iK3HandleP, "k1y", K1Y);
        CmpShader.SetTexture(m_iK3HandleP, "k1z", K1Z);

        CmpShader.SetTexture(m_iKernelHandleP, "k1x", K1X);
        CmpShader.SetTexture(m_iKernelHandleP, "k1y", K1Y);
        CmpShader.SetTexture(m_iKernelHandleP, "k1z", K1Z);
    }
	
	// Update is called once per frame
	void Update ()
    {
	    if (m_bRunning)
	    {
	        m_iJxStep += 2;
            CmpShader.SetInt("jxstep", m_iJxStep);
	        if (TogglePeriodic.isOn)
	        {
                CmpShader.Dispatch(m_iK1HandleP, m_iResolution / 16, m_iResolution / 16, 1);
                CmpShader.Dispatch(m_iK2HandleP, m_iResolution / 16, m_iResolution / 16, 1);
                CmpShader.Dispatch(m_iK3HandleP, m_iResolution / 16, m_iResolution / 16, 1);
                CmpShader.Dispatch(m_iKernelHandleP, m_iResolution / 16, m_iResolution / 16, 1);
            }
	        else
	        {
                CmpShader.Dispatch(m_iK1Handle, m_iResolution / 16, m_iResolution / 16, 1);
                CmpShader.Dispatch(m_iK2Handle, m_iResolution / 16, m_iResolution / 16, 1);
                CmpShader.Dispatch(m_iK3Handle, m_iResolution / 16, m_iResolution / 16, 1);
                CmpShader.Dispatch(m_iKernelHandle, m_iResolution / 16, m_iResolution / 16, 1);
            }


            ++m_iFrame;
            FrameCount.text = m_iFrame.ToString();

	        //m_fShowSepTicker += Time.deltaTime;
	        //if (m_fShowSepTicker > m_fShowSep)
	        //{
	        //    m_fShowSepTicker = 0.0f;
            //    Graphics.Blit(null, ShowRT, GetXYZ);
	        //}

	        if (StandardDevationToggle.isOn || 
                    (m_iSaveFrame > 0 && 
                     (0 == (m_iFrame % m_iSaveFrame)
                     || (m_iSaveFrame - 1) == (m_iFrame % m_iSaveFrame))
                    )
               )
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
            BtResolution.interactable = true;
            TogglePeriodic.interactable = true;
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
            BtResolution.interactable = false;
            TogglePeriodic.interactable = false;

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

            //CmpShader.SetInt("jxstep", 0);
            m_iJxStep = 0;
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

                    if (m_bMagSet && m_bJSet && (m_bCondSet || TogglePeriodic.isOn))
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

                    if (m_bMagSet && m_bJSet && (m_bCondSet || TogglePeriodic.isOn))
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

                if (m_bMagSet && m_bJSet && (m_bCondSet || TogglePeriodic.isOn))
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
                m_sCondName = sFileName.Substring(iLastSlash);
                BoundaryName.text = m_sCondName;
                m_bCondSet = true;

                if (m_bMagSet && m_bJSet && (m_bCondSet || TogglePeriodic.isOn))
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
grid={13}x{13}
step={1}
exchange strength J value={2}
initla magnetic={3}
boundary condition={4}
electrical current={5}
K={6}
D={7} + J * {8}
B={9}
Gilbert alpha={10}
time step={11}
last standard deviation/delta time={12}",
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
            m_fTimeStep,
            m_lstStdDevRes.Count > 0 ? m_lstStdDevRes[m_lstStdDevRes.Count - 1] : "Not Recorded",
            m_iResolution
            ), 
            m_iResolution,
            InternalRTR,
            InternalRTG,
            InternalRTB,
            MatShow,
            MatGetXYZ
            );
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

    public void OnTogglePeriodic()
    {
        if (TogglePeriodic.isOn)
        {
            CondButton.interactable = false;
            BoundaryName.text = "periodic";

            if (m_bMagSet && m_bJSet && (m_bCondSet || TogglePeriodic.isOn))
            {
                Bt.interactable = true;
            }
        }
        else
        {
            CondButton.interactable = true;
            BoundaryName.text = m_sCondName;

            if (!m_bCondSet)
            {
                Bt.interactable = false;
            }
        }
    }

    #region Load Magnetic

    private static Vector3[] _mags = new Vector3[512 * 512];
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

        for (int x = 0; x < m_iResolution; ++x)
        {
            for (int y = 0; y < m_iResolution; ++y)
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

                _mags[y * m_iResolution + x] = new Vector3(
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
    private static Color[] _txcR = new Color[512 * 512];
    private static Color[] _txcG = new Color[512 * 512];
    private static Color[] _txcB = new Color[512 * 512];

#if UNITY_EDITOR
    public RawImage tester;
#endif

    private bool LoadMagneticPic(string sPicFileName)
    {
        if (null == m_pTestT2 || m_iResolution != m_pTestT2.width)
        {
            m_pTestT2 = new Texture2D(m_iResolution, m_iResolution, TextureFormat.RGB24, false, true);
        }
        if (!m_pTestT2.LoadImage(File.ReadAllBytes(sPicFileName), false))
        {
            ShowErrorMessage("Not support this file format.");
            m_pTestT2 = new Texture2D(m_iResolution, m_iResolution, TextureFormat.RGB24, false, true);
            return false;
        }

        if (m_iResolution != m_pTestT2.width || m_iResolution != m_pTestT2.height)
        {
            ShowErrorMessage(string.Format("Only support {0} x {0} file.", m_iResolution));
            m_pTestT2 = new Texture2D(m_iResolution, m_iResolution, TextureFormat.RGB24, false, true);
            return false;
        }

#if UNITY_EDITOR
        tester.texture = m_pTestT2;
#endif

        for (int x = 0; x < m_iResolution; ++x)
        {
            for (int y = 0; y < m_iResolution; ++y)
            {
                Color c = m_pTestT2.GetPixel(x, y);
                //if ((0 == x && 0 == y)
                //  || (10 == x && 10 == y))
                //{
                //    Debug.Log(string.Format("x={0},y={1},c={2}", x, y, c));
                //}
                _mags[y* m_iResolution + x] = new Vector3(2.0f*c.r - 1.0f, 2.0f*c.g - 1.0f, 2.0f*c.b - 1.0f);
            }
        }

        SetCurrentState(_mags);

        return true;
    }

    private void SetCurrentState(Vector3[] magnetic)
    {
        if (null == _txR || m_iResolution != _txR.width)
        {
            _txR = new Texture2D(m_iResolution, m_iResolution, TextureFormat.RFloat, false, true);
        }
        if (null == _txG || m_iResolution != _txG.width)
        {
            _txG = new Texture2D(m_iResolution, m_iResolution, TextureFormat.RFloat, false, true);
        }
        if (null == _txB || m_iResolution != _txB.width)
        {
            _txB = new Texture2D(m_iResolution, m_iResolution, TextureFormat.RFloat, false, true);
        }

        for (int i = 0; i < m_iResolution * m_iResolution; ++i)
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
            InternalRTR = new RenderTexture(m_iResolution, m_iResolution, 0, RenderTextureFormat.RFloat);
            InternalRTR.enableRandomWrite = true;
            InternalRTR.Create();
            OutPutSingle.material.SetTexture("_Nx", InternalRTR);
        }
        if (null == InternalRTG)
        {
            InternalRTG = new RenderTexture(m_iResolution, m_iResolution, 0, RenderTextureFormat.RFloat);
            InternalRTG.enableRandomWrite = true;
            InternalRTG.Create();
            OutPutSingle.material.SetTexture("_Ny", InternalRTG);
        }
        if (null == InternalRTB)
        {
            InternalRTB = new RenderTexture(m_iResolution, m_iResolution, 0, RenderTextureFormat.RFloat);
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

        CmpShader.SetTexture(m_iK1HandleP, "magneticMomentumX", InternalRTR);
        CmpShader.SetTexture(m_iK1HandleP, "magneticMomentumY", InternalRTG);
        CmpShader.SetTexture(m_iK1HandleP, "magneticMomentumZ", InternalRTB);
        CmpShader.SetTexture(m_iK2HandleP, "magneticMomentumX", InternalRTR);
        CmpShader.SetTexture(m_iK2HandleP, "magneticMomentumY", InternalRTG);
        CmpShader.SetTexture(m_iK2HandleP, "magneticMomentumZ", InternalRTB);
        CmpShader.SetTexture(m_iK3HandleP, "magneticMomentumX", InternalRTR);
        CmpShader.SetTexture(m_iK3HandleP, "magneticMomentumY", InternalRTG);
        CmpShader.SetTexture(m_iK3HandleP, "magneticMomentumZ", InternalRTB);

        CmpShader.SetTexture(m_iKernelHandleP, "magneticMomentumX", InternalRTR);
        CmpShader.SetTexture(m_iKernelHandleP, "magneticMomentumY", InternalRTG);
        CmpShader.SetTexture(m_iKernelHandleP, "magneticMomentumZ", InternalRTB);

        //Graphics.Blit(null, ShowRT, GetXYZ);
    }

    #endregion

    #region Load J Value

    private static float[] _jvalues = new float[512 * 512];
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

        for (int x = 0; x < m_iResolution; ++x)
        {
            for (int y = 0; y < m_iResolution; ++y)
            {
                object[] r = func.Call2(x, y);
                if (1 != r.Length || !(r[0] is double))
                {
                    ShowErrorMessage("Call function GetJValueByLatticeIndex failed, function may have error.");
                    mgr.Destroy();
                    return false;
                }
                _jvalues[y * m_iResolution + x] = (float)(double)r[0];
            }
        }
        mgr.Destroy();
        SetJTexture(_jvalues);
        return true;
    }

    private void SetJTexture(float[] js)
    {
        if (null == JTexture || m_iResolution != JTexture.width)
        {
            JTexture = new Texture2D(m_iResolution, m_iResolution, TextureFormat.RFloat, false, true);
        }

        for (int i = 0; i < m_iResolution * m_iResolution; ++i)
        {
            _color512[i] = new Color(js[i], 0.0f, 0.0f);
        }
        JTexture.SetPixels(_color512);
        JTexture.Apply(false);

        CmpShader.SetTexture(m_iK1Handle, "exchangeStrength", JTexture);
        CmpShader.SetTexture(m_iK2Handle, "exchangeStrength", JTexture);
        CmpShader.SetTexture(m_iK3Handle, "exchangeStrength", JTexture);
        CmpShader.SetTexture(m_iKernelHandle, "exchangeStrength", JTexture);

        CmpShader.SetTexture(m_iK1HandleP, "exchangeStrength", JTexture);
        CmpShader.SetTexture(m_iK2HandleP, "exchangeStrength", JTexture);
        CmpShader.SetTexture(m_iK3HandleP, "exchangeStrength", JTexture);
        CmpShader.SetTexture(m_iKernelHandleP, "exchangeStrength", JTexture);
    }

    #endregion

    #region Load Boundary Condition

    private Texture2D _theEdgeTexture = null;
    private bool SetEdge(string sPNGFileName)
    {
        if (null == _theEdgeTexture || m_iResolution != _theEdgeTexture.width)
        {
            _theEdgeTexture = new Texture2D(m_iResolution, m_iResolution, TextureFormat.Alpha8, false);
        }
        if (!_theEdgeTexture.LoadImage(File.ReadAllBytes(sPNGFileName), true))
        {
            ShowErrorMessage("Not support this file format.");
            return false;
        }

        if (m_iResolution != _theEdgeTexture.width || m_iResolution != _theEdgeTexture.height)
        {
            ShowErrorMessage(string.Format("Only support {0} x {0} file.", m_iResolution));
            return false;
        }
        CmpShader.SetTexture(m_iKernelHandle, "boundaryCondition", _theEdgeTexture);
        return true;
    }

    #endregion

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
                JxTexture = new Texture2D(1, 1, TextureFormat.RFloat, false, true);
            }
            JxTexture.SetPixels(nonejxcolor);
            JxTexture.Apply(false);

            CmpShader.SetTexture(m_iK1Handle, "jxPeroidFunction", JxTexture);
            CmpShader.SetTexture(m_iK2Handle, "jxPeroidFunction", JxTexture);
            CmpShader.SetTexture(m_iK3Handle, "jxPeroidFunction", JxTexture);

            CmpShader.SetTexture(m_iKernelHandle, "jxPeroidFunction", JxTexture);

            CmpShader.SetTexture(m_iK1HandleP, "jxPeroidFunction", JxTexture);
            CmpShader.SetTexture(m_iK2HandleP, "jxPeroidFunction", JxTexture);
            CmpShader.SetTexture(m_iK3HandleP, "jxPeroidFunction", JxTexture);

            CmpShader.SetTexture(m_iKernelHandleP, "jxPeroidFunction", JxTexture);

            return;
        }

        if (null == JxTexture || JxTexture.width != jxStep)
        {
            JxTexture = new Texture2D(jxStep, 1, TextureFormat.RFloat, false, true);
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

        CmpShader.SetTexture(m_iK1HandleP, "jxPeroidFunction", JxTexture);
        CmpShader.SetTexture(m_iK2HandleP, "jxPeroidFunction", JxTexture);
        CmpShader.SetTexture(m_iK3HandleP, "jxPeroidFunction", JxTexture);

        CmpShader.SetTexture(m_iKernelHandleP, "jxPeroidFunction", JxTexture);
    }

    public void ShowErrorMessage(string sMsg)
    {
        m_sMessage = sMsg;
        m_bMsgShow = true;
    }

    #region Change Resolution

    private void ChangeResolution(int iNewResolution)
    {
        m_iResolution = iNewResolution;
        _color512 = new Color[iNewResolution * iNewResolution];
        _txcR = new Color[iNewResolution * iNewResolution];
        _txcG = new Color[iNewResolution * iNewResolution];
        _txcB = new Color[iNewResolution * iNewResolution];
        _mags = new Vector3[iNewResolution * iNewResolution];
        _jvalues = new float[iNewResolution * iNewResolution];

        BuildInitialTexture(iNewResolution);

        m_bJSet = false;
        m_bMagSet = false;
        m_bCondSet = false;
    }

    public void OnButtonResolution()
    {
        int iNewRes = 1;
        if (!int.TryParse(InputResolution.text, out iNewRes))
        {
            ShowErrorMessage("New resolution must be 512,1024,2048 and different from now.");
        }

        if (m_iResolution == iNewRes)
        {
            ShowErrorMessage("New resolution must be 512,1024,2048 and different from now.");
        }

        if (512 != iNewRes && 1024 != iNewRes && 2048 != iNewRes)
        {
            ShowErrorMessage("New resolution must be 512,1024,2048 and different from now.");
        }

        ChangeResolution(iNewRes);
        TxtResolution.text = string.Format("Change size(now={0}x{0})", iNewRes);
    }

    #endregion

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
