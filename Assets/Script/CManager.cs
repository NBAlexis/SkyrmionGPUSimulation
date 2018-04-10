using System;
using System.Collections.Generic;
using System.IO;
using SFB;
using UniLua;
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
    private int m_iKernelHandle;

    #endregion

    #region Calculators

    private RenderTexture InternalRTR;
    private RenderTexture InternalRTG;
    private RenderTexture InternalRTB;
    private Texture2D JTexture;
    private Texture2D JxTexture;

    #endregion

    private Texture2D m_pTestT2;
    private int m_iFrame = 0;
    private int m_iStopFrame = -1;
    private int m_iSaveFrame = -1;
    private bool m_bRunning = false;
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

        m_iKernelHandle = CmpShader.FindKernel("CSMain");
        CmpShader.SetInts("size", new int[] { 512, 512 });

        Bt.GetComponentInChildren<Text>().text = "Start";

        //ShowRT = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB64, RenderTextureReadWrite.sRGB);
        //OutPutSingle.texture = ShowRT;
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if (m_bRunning)
	    {
            CmpShader.Dispatch(m_iKernelHandle, 512 / 8, 512 / 8, 1);

            ++m_iFrame;
            FrameCount.text = m_iFrame.ToString();

	        //m_fShowSepTicker += Time.deltaTime;
	        //if (m_fShowSepTicker > m_fShowSep)
	        //{
	        //    m_fShowSepTicker = 0.0f;
            //    Graphics.Blit(null, ShowRT, GetXYZ);
	        //}

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
            CmpShader.SetFloat("timestep", float.Parse(timestep.text));
            CmpShader.SetFloat("alpha", float.Parse(alpha.text));

            m_bRunning = true;
            m_iStopFrame = int.Parse(stopstep.text);
            m_iSaveFrame = int.Parse(savestep.text);

            CmpShader.SetInt("jxstep", 0);
            if (!m_bJxPeroidSet)
            {
                CmpShader.SetInt("jxperoid", 0);
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
        if (sPath.Length > 0)
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
            timestep.text
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
    private static readonly float[] _mag_ret = new float[3];
    private bool LoadManetic(string sLuaFileName)
    {
        string sLuaCode = File.ReadAllText(sLuaFileName);
        ILuaState luaState = LuaAPI.NewState();
        luaState.L_OpenLibs();
        ThreadStatus status = luaState.L_DoString(sLuaCode);

        if (ThreadStatus.LUA_OK != status)
        {
            ShowErrorMessage("Lua file excute error.");
            return false;
        }
        if (!luaState.IsTable(-1))
        {
            ShowErrorMessage("Lua file does not return function table.");
            return false;
        }

        int tableIndex = luaState.GetTop();
        luaState.PushNil();
        List<string> funcNames = new List<string>();
        while (luaState.Next(tableIndex))
        {
            if (luaState.IsString(-2) && luaState.IsFunction(-1))
            {
                string sFuncName = luaState.ToString(-2);
                funcNames.Add(sFuncName);
            }
            luaState.Pop(1);
        }

        Dictionary<string, int> dicFunctions = new Dictionary<string, int>();
        foreach (string funcName in funcNames)
        {
            luaState.GetField(-1, funcName);
            if (!luaState.IsFunction(-1))
            {
                ShowErrorMessage("Lua file return a table, but the table does not return function or the name is wrong.");
                return false;
            }
            int iFunctionPointer = luaState.L_Ref(LuaDef.LUA_REGISTRYINDEX);
            dicFunctions.Add(funcName, iFunctionPointer);
        }

        luaState.Pop(1); //pop return table;

        if (!dicFunctions.ContainsKey("GetMagneticByLatticeIndex"))
        {
            ShowErrorMessage("GetMagneticByLatticeIndex function not found.");
            return false;
        }

        for (int x = 0; x < 512; ++x)
        {
            for (int y = 0; y < 512; ++y)
            {
                luaState.RawGetI(LuaDef.LUA_REGISTRYINDEX, dicFunctions["GetMagneticByLatticeIndex"]);
                int oldTop = luaState.GetTop();
                luaState.PushCSharpFunction(Traceback);
                luaState.Insert(oldTop);

                //Set input
                luaState.PushInteger(x); 
                luaState.PushInteger(y); 
                status = luaState.PCall(2, 3, oldTop);

                if (status != ThreadStatus.LUA_OK)
                {
                    ShowErrorMessage("Call function GetMagneticByLatticeIndex failed, function may have error.");
                    return false;
                }

                //get result out
                int newTop = luaState.GetTop();
                if (newTop == oldTop)
                {
                    ShowErrorMessage("function should return something.");
                    return false;
                }

                for (int i = oldTop + 1, j = 0; i <= newTop && j < 3; ++i, ++j)
                {
                    _mag_ret[j] = (float)luaState.ToNumber(i);
                }

                //if return is more then 3, pop them for next lua call
                if (oldTop != luaState.GetTop())
                {
                    luaState.Pop(luaState.GetTop() - oldTop);
                }

                _mags[y * 512 + x] = new Vector3(_mag_ret[0], _mag_ret[1], _mag_ret[2]);

                luaState.Remove(oldTop);
            }
        }

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

        CmpShader.SetTexture(m_iKernelHandle, "magneticMomentumX", InternalRTR);
        CmpShader.SetTexture(m_iKernelHandle, "magneticMomentumY", InternalRTG);
        CmpShader.SetTexture(m_iKernelHandle, "magneticMomentumZ", InternalRTB);

        //Graphics.Blit(null, ShowRT, GetXYZ);
    }

    private static readonly float[] _jvalues = new float[512 * 512];
    private bool LoadJValue(string sLuaFileName)
    {
        string sLuaCode = File.ReadAllText(sLuaFileName);
        ILuaState luaState = LuaAPI.NewState();
        luaState.L_OpenLibs();
        ThreadStatus status = luaState.L_DoString(sLuaCode);

        if (ThreadStatus.LUA_OK != status)
        {
            ShowErrorMessage("Lua file excute error.");
            return false;
        }
        if (!luaState.IsTable(-1))
        {
            ShowErrorMessage("Lua file does not return function table.");
            return false;
        }

        int tableIndex = luaState.GetTop();
        luaState.PushNil();
        List<string> funcNames = new List<string>();
        while (luaState.Next(tableIndex))
        {
            if (luaState.IsString(-2) && luaState.IsFunction(-1))
            {
                string sFuncName = luaState.ToString(-2);
                funcNames.Add(sFuncName);
            }
            luaState.Pop(1);
        }

        Dictionary<string, int> dicFunctions = new Dictionary<string, int>();
        foreach (string funcName in funcNames)
        {
            luaState.GetField(-1, funcName);
            if (!luaState.IsFunction(-1))
            {
                ShowErrorMessage("Lua file return a table, but the table does not return function or the name is wrong.");
                return false;
            }
            int iFunctionPointer = luaState.L_Ref(LuaDef.LUA_REGISTRYINDEX);
            dicFunctions.Add(funcName, iFunctionPointer);
        }

        luaState.Pop(1); //pop return table;

        if (!dicFunctions.ContainsKey("GetJValueByLatticeIndex"))
        {
            ShowErrorMessage("GetJValueByLatticeIndex function not found.");
            return false;
        }

        for (int x = 0; x < 512; ++x)
        {
            for (int y = 0; y < 512; ++y)
            {
                luaState.RawGetI(LuaDef.LUA_REGISTRYINDEX, dicFunctions["GetJValueByLatticeIndex"]);
                int oldTop = luaState.GetTop();
                luaState.PushCSharpFunction(Traceback);
                luaState.Insert(oldTop);

                //Set input
                luaState.PushInteger(x);
                luaState.PushInteger(y);
                status = luaState.PCall(2, 1, oldTop);

                if (status != ThreadStatus.LUA_OK)
                {
                    ShowErrorMessage("Call function GetMagneticByLatticeIndex failed, function may have error.");
                }

                //get result out
                int newTop = luaState.GetTop();
                if (newTop == oldTop)
                {
                    ShowErrorMessage("function should return something.");
                    return false;
                }

                _jvalues[y * 512 + x] = (float)luaState.ToNumber(oldTop + 1);

                //if return is more then 1, pop them for next lua call
                if (oldTop != luaState.GetTop())
                {
                    luaState.Pop(luaState.GetTop() - oldTop);
                }

                luaState.Remove(oldTop);
            }
        }

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
        string sLuaCode = File.ReadAllText(sLuaFileName);
        ILuaState luaState = LuaAPI.NewState();
        luaState.L_OpenLibs();
        ThreadStatus status = luaState.L_DoString(sLuaCode);

        if (ThreadStatus.LUA_OK != status)
        {
            ShowErrorMessage("Lua file excute error.");
            return false;
        }
        if (!luaState.IsTable(-1))
        {
            ShowErrorMessage("Lua file does not return function table.");
            return false;
        }

        int tableIndex = luaState.GetTop();
        luaState.PushNil();
        List<string> funcNames = new List<string>();
        while (luaState.Next(tableIndex))
        {
            if (luaState.IsString(-2) && luaState.IsFunction(-1))
            {
                string sFuncName = luaState.ToString(-2);
                funcNames.Add(sFuncName);
            }
            luaState.Pop(1);
        }

        Dictionary<string, int> dicFunctions = new Dictionary<string, int>();
        foreach (string funcName in funcNames)
        {
            luaState.GetField(-1, funcName);
            if (!luaState.IsFunction(-1))
            {
                ShowErrorMessage("Lua file return a table, but the table does not return function or the name is wrong.");
                return false;
            }
            int iFunctionPointer = luaState.L_Ref(LuaDef.LUA_REGISTRYINDEX);
            dicFunctions.Add(funcName, iFunctionPointer);
        }

        luaState.Pop(1); //pop return table;

        if (!dicFunctions.ContainsKey("GetJxValueInPeroid"))
        {
            ShowErrorMessage("GetJxValueInPeroid function not found.");
            return false;
        }

        if (!dicFunctions.ContainsKey("GetJxPeroidLength"))
        {
            ShowErrorMessage("GetJxPeroidLength function not found.");
            return false;
        }

        #region Get length

        luaState.RawGetI(LuaDef.LUA_REGISTRYINDEX, dicFunctions["GetJxPeroidLength"]);
        int oldTop1 = luaState.GetTop();
        luaState.PushCSharpFunction(Traceback);
        luaState.Insert(oldTop1);

        //Set input
        status = luaState.PCall(0, 1, oldTop1);

        if (status != ThreadStatus.LUA_OK)
        {
            ShowErrorMessage("Call function GetMagneticByLatticeIndex failed, function may have error.");
        }

        //get result out
        int newTop1 = luaState.GetTop();
        if (newTop1 == oldTop1)
        {
            ShowErrorMessage("function should return something.");
            return false;
        }

        int iStepLength = (int)luaState.ToNumber(oldTop1 + 1);

        //if return is more then 1, pop them for next lua call
        if (oldTop1 != luaState.GetTop())
        {
            luaState.Pop(luaState.GetTop() - oldTop1);
        }

        luaState.Remove(oldTop1);

        #endregion

        float[] jxvalue = null;
        if (iStepLength > 0)
        {
            jxvalue = new float[iStepLength];
            for (int y = 0; y < iStepLength; ++y)
            {
                luaState.RawGetI(LuaDef.LUA_REGISTRYINDEX, dicFunctions["GetJxValueInPeroid"]);
                int oldTop = luaState.GetTop();
                luaState.PushCSharpFunction(Traceback);
                luaState.Insert(oldTop);

                //Set input
                luaState.PushNumber(y / (float)iStepLength);
                status = luaState.PCall(1, 1, oldTop);

                if (status != ThreadStatus.LUA_OK)
                {
                    ShowErrorMessage("Call function GetMagneticByLatticeIndex failed, function may have error.");
                }

                //get result out
                int newTop = luaState.GetTop();
                if (newTop == oldTop)
                {
                    ShowErrorMessage("function should return something.");
                    return false;
                }

                jxvalue[y] = luaState.ToInteger(oldTop + 1);

                //if return is more then 1, pop them for next lua call
                if (oldTop != luaState.GetTop())
                {
                    luaState.Pop(luaState.GetTop() - oldTop);
                }

                luaState.Remove(oldTop);
            }
        }

        SetJXTexture(iStepLength, jxvalue);
        return true;
    }

    private void SetJXTexture(int jxStep, float[] jxValue)
    {
        if (jxStep < 1 || null == jxValue || 0 == jxValue.Length)
        {
            CmpShader.SetInt("jxperoid", 0);
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
        CmpShader.SetTexture(m_iKernelHandle, "jxPeroidFunction", JxTexture);
    }

    private static int Traceback(ILuaState lua)
    {
        var msg = lua.ToString(1);
        if (msg != null)
        {
            lua.L_Traceback(lua, msg, 1);
        }
        // is there an error object?
        else if (!lua.IsNoneOrNil(1))
        {
            // try its `tostring' metamethod
            if (!lua.L_CallMeta(1, "__tostring"))
            {
                lua.PushString("(no error message)");
            }
        }
        return 1;
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
