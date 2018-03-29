using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SFB;
using UniLua;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[AddComponentMenu("Skyrmion/Scripts/Manager")]
public class CManager : MonoBehaviour
{
    #region Show

    public RawImage OutPutSingle;
    public RawImage Indicator;

    public Text FrameCount;

    public Button Bt;

    public InputField K;
    public InputField D;
    public InputField B;
    public InputField EJ;
    public InputField alpha;
    public InputField timestep;

    #endregion

    #region Compute Shader

    public ComputeShader CmpShader;

    private ComputeBuffer m_MagneticBuff;
    private Vector3[] m_vMagneticData;
    private int m_iKernelHandle;

    #endregion

    #region Calculators

    private RenderTexture InternalRT;
    private Texture2D JTexture;

    #endregion

    private Texture2D m_pTestT2;
    private int m_iFrame = 0;
    private bool m_bRunning = false;
    private bool m_bJSet = false;
    private bool m_bMagneticSet = false;
    
    private static readonly Color[] _color512 = new Color[512 * 512];

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
    }
	
	// Update is called once per frame
	void Update ()
    {
	    if (m_bRunning)
	    {
            CmpShader.Dispatch(m_iKernelHandle, 512 / 8, 512 / 8, 1);

            ++m_iFrame;
            FrameCount.text = m_iFrame.ToString();
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
            B.interactable = true;
            EJ.interactable = true;
            alpha.interactable = true;
            timestep.interactable = true;
        }
        else
        {
            Bt.GetComponentInChildren<Text>().text = "Stop";
            K.interactable = false;
            D.interactable = false;
            B.interactable = false;
            EJ.interactable = false;
            alpha.interactable = false;
            timestep.interactable = false;

            CmpShader.SetFloat("K", float.Parse(K.text));
            CmpShader.SetFloat("D", float.Parse(D.text));
            CmpShader.SetFloat("B", float.Parse(B.text));
            CmpShader.SetFloat("jx", float.Parse(EJ.text));
            CmpShader.SetFloat("timestep", float.Parse(timestep.text));
            CmpShader.SetFloat("alpha", float.Parse(alpha.text));

            m_bRunning = true;
        }
    }

    public void LoadMagneticBt()
    {
        string[] sPath = StandaloneFileBrowser.OpenFilePanel("Choose the script", Application.dataPath, "lua", false);
        if (sPath.Length > 0)
        {
            LoadManetic(sPath[0]);
        }
    }

    public void LoadJValueBt()
    {
        string[] sPath = StandaloneFileBrowser.OpenFilePanel("Choose the script", Application.dataPath, "lua", false);
        if (sPath.Length > 0)
        {
            LoadJValue(sPath[0]);
        }
    }

    private static readonly Vector3[] _mags = new Vector3[512 * 512];
    private static readonly float[] _mag_ret = new float[3];
    private void LoadManetic(string sLuaFileName)
    {
        string sLuaCode = File.ReadAllText(sLuaFileName);
        ILuaState luaState = LuaAPI.NewState();
        luaState.L_OpenLibs();
        ThreadStatus status = luaState.L_DoString(sLuaCode);

        if (ThreadStatus.LUA_OK != status)
        {
            Debug.LogError("Lua file excute error.");
            return;
        }
        if (!luaState.IsTable(-1))
        {
            Debug.LogError("Lua file does not return function table.");
            return;
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
                Debug.LogError("Lua file return a table, but the table does not return function or the name is wrong.");
                return;
            }
            int iFunctionPointer = luaState.L_Ref(LuaDef.LUA_REGISTRYINDEX);
            dicFunctions.Add(funcName, iFunctionPointer);
        }

        luaState.Pop(1); //pop return table;

        if (!dicFunctions.ContainsKey("GetMagneticByLatticeIndex"))
        {
            Debug.LogError("GetMagneticByLatticeIndex function not found.");
            return;
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
                    Debug.LogError("Call function GetMagneticByLatticeIndex failed, function may have error.");
                }

                //get result out
                int newTop = luaState.GetTop();
                if (newTop == oldTop)
                {
                    Debug.LogError("function should return something.");
                    return;
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
    }

    private void SetCurrentState(Vector3[] magnetic)
    {
        if (null == m_pTestT2)
        {
            m_pTestT2 = new Texture2D(512, 512, TextureFormat.RGBA32, false, false);
        }

        //m_vMagneticData = new Vector3[512 * 512];
        for (int i = 0; i < 512 * 512; ++i)
        {
            //m_vMagneticData[i] = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)).normalized;
            //Vector3 initalV = m_vMagneticData[i] * 0.5f + 0.5f * Vector3.one;

            magnetic[i].Normalize();
            magnetic[i] = magnetic[i] * 0.5f + 0.5f * Vector3.one;
            _color512[i] = new Color(magnetic[i].x, magnetic[i].y, magnetic[i].z);
        }

        m_pTestT2.SetPixels(_color512);
        m_pTestT2.Apply(false);

        if (null == InternalRT)
        {
            InternalRT = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB64, RenderTextureReadWrite.sRGB);
            InternalRT.enableRandomWrite = true;
            InternalRT.Create();

            OutPutSingle.texture = InternalRT;
        }

        Graphics.Blit(m_pTestT2, InternalRT);
        CmpShader.SetTexture(m_iKernelHandle, "magneticMomentum", InternalRT);
    }

    private static readonly float[] _jvalues = new float[512 * 512];
    private void LoadJValue(string sLuaFileName)
    {
        string sLuaCode = File.ReadAllText(sLuaFileName);
        ILuaState luaState = LuaAPI.NewState();
        luaState.L_OpenLibs();
        ThreadStatus status = luaState.L_DoString(sLuaCode);

        if (ThreadStatus.LUA_OK != status)
        {
            Debug.LogError("Lua file excute error.");
            return;
        }
        if (!luaState.IsTable(-1))
        {
            Debug.LogError("Lua file does not return function table.");
            return;
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
                Debug.LogError("Lua file return a table, but the table does not return function or the name is wrong.");
                return;
            }
            int iFunctionPointer = luaState.L_Ref(LuaDef.LUA_REGISTRYINDEX);
            dicFunctions.Add(funcName, iFunctionPointer);
        }

        luaState.Pop(1); //pop return table;

        if (!dicFunctions.ContainsKey("GetJValueByLatticeIndex"))
        {
            Debug.LogError("GetJValueByLatticeIndex function not found.");
            return;
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
                    Debug.LogError("Call function GetMagneticByLatticeIndex failed, function may have error.");
                }

                //get result out
                int newTop = luaState.GetTop();
                if (newTop == oldTop)
                {
                    Debug.LogError("function should return something.");
                    return;
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
    }

    private void SetJTexture(float[] js)
    {
        if (null == JTexture)
        {
            JTexture = new Texture2D(512, 512, TextureFormat.RFloat, false, true);
        }

        for (int i = 0; i < 512 * 512; ++i)
        {
            _color512[i] = new Color(js[i], 0.0f, 0.0f);
        }
        JTexture.SetPixels(_color512);
        JTexture.Apply(false);
        CmpShader.SetTexture(m_iKernelHandle, "exchangeStrength", JTexture);
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
}
