using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UniLua;

#if UNITY
using UnityEngine;
#endif

//!!Important: Keep the code page of this file 936 or 65001
//Uni-Lua only support ASCII lua file right now!

public class CLuaArray
{
    public CLuaArray(object[] array)
    {
        m_pTheArray = array;
    }
    public object[] m_pTheArray;
}

public class LuaExcuteManager
{
    private ILuaState m_pLuaState = null;
    private Dictionary<string, int> m_pFuncNameToPointer = null;
    //private int m_iPlayerIndex = 0; //DISCARD
    //private PlayerInfo m_pPlayer = null; //Discard

	public LuaExcuteManager()
	{
		m_pLuaState = LuaAPI.NewState();
		m_pLuaState.L_OpenLibs();
	}

	public ILuaState Lua_State
	{
		get { return m_pLuaState; }
	}

    public bool Initial(byte[] pCardBytes)
    {
        //m_pPlayer = new PlayerInfo();

        //LuaPlayerInfo.RegisterMe(m_pLuaState);
        m_pFuncNameToPointer = new Dictionary<string, int>();

        ThreadStatus status = ThreadStatus.LUA_OK;

		//byte[] deCompressed = GameServer.ZipWrapper.DeCompress(pCardBytes, 0, pCardBytes.Length);
		//string sUTF8Code = Encoding.UTF8.GetString(deCompressed);
		string sUTF8Code = Encoding.UTF8.GetString(pCardBytes);
        //CommonCode.Log(sUTF8Code);
        status = m_pLuaState.L_DoString(sUTF8Code);

        
        if (status != ThreadStatus.LUA_OK)
        {
            string sString = m_pLuaState.ToString(-1);
            if (sString.Length > 255)
            {
                sString = sString.Substring(sString.Length - 200, 200);
            }
            throw new Exception(sString);
        }

        if (!m_pLuaState.IsTable(-1))
        {
            throw new Exception(
                "hammer abc's return value is not a table");
        }

        //Let's see what's in there
        int tableIndex = m_pLuaState.GetTop();
        m_pLuaState.PushNil();
        List<string> funcNames = new List<string>();
        while (m_pLuaState.Next(tableIndex))
        {
            if (m_pLuaState.IsString(-2) && m_pLuaState.IsFunction(-1))
            {
                string sFuncName = m_pLuaState.ToString(-2);
                //CommonCode.Log(sFuncName);
                funcNames.Add(sFuncName);
            }
            m_pLuaState.Pop(1);
        }

        foreach (string funcName in funcNames)
        {
            m_pFuncNameToPointer.Add(funcName, StoreMethod(funcName));
        }

        //m_iPlayerIndex = StorePlayerIndex(); Discard

        m_pLuaState.Pop(1); //pop return table;
		return true;
    }

    public bool Initial()
    {
        //m_pPlayer = new PlayerInfo();

        //LuaPlayerInfo.RegisterMe(m_pLuaState);
        m_pFuncNameToPointer = new Dictionary<string, int>();

        ThreadStatus status = ThreadStatus.LUA_OK;

#if !UNITY
        string filepath = "Text/lua";
        TextAsset pAsset = Resources.Load(filepath, typeof (TextAsset)) as TextAsset;
        if (null != pAsset)
        {
            byte[] pCardBytes = pAsset.bytes;
            byte[] deCoded = CEncode.NativeDecodeToBytes(pCardBytes);
            byte[] deCompressed = GameServer.ZipWrapper.DeCompress(deCoded, 0, deCoded.Length);
            string sUTF8Code = Encoding.UTF8.GetString(deCompressed);
            Debug.Log(sUTF8Code);
            status = m_pLuaState.L_DoString(sUTF8Code);
        }
        else
        {
            Debug.LogError("file not found");
            return false;
        }
#else
#if UNITY
        string filepath = Application.dataPath + "/Resources/Lua/debug.lua";
#else
        string filepath = CommonCode.GetAppDataRoot() + "/CustomResources/Lua/debug.lua";
#endif
        if (File.Exists(filepath))
        {
            FileStream pFile = File.OpenRead(filepath);
            long iFileLength = pFile.Length;
            byte[] pCardBytes = new byte[iFileLength];
            pFile.Read(pCardBytes, 0, (int)iFileLength);
            //byte[] deCoded = CEncode.NativeDecodeToBytes(pCardBytes);
            //byte[] deCompressed = GameServer.ZipWrapper.DeCompress(deCoded, 0, deCoded.Length);

			//byte[] deCompressed = GameServer.ZipWrapper.DeCompress(pCardBytes, 0, pCardBytes.Length);
            //string sUTF8Code = Encoding.UTF8.GetString(deCompressed);
            string sUTF8Code = Encoding.UTF8.GetString(pCardBytes);
            //CommonCode.Log(sUTF8Code);
			Debug.Log("Lua files read over!");
            status = m_pLuaState.L_DoString(sUTF8Code);
            
            pFile.Close();
        }
        else
        {
            Debug.Log("file not found");
            return false;
        }
#endif
        
        if (status != ThreadStatus.LUA_OK)
        {
            throw new Exception(m_pLuaState.ToString(-1));
        }

        if (!m_pLuaState.IsTable(-1))
        {
            throw new Exception(
                "hammer abc's return value is not a table");
        }

        //Let's see what's in there
        int tableIndex = m_pLuaState.GetTop();
        m_pLuaState.PushNil();
        List<string> funcNames = new List<string>();
        while (m_pLuaState.Next(tableIndex))
        {
            if (m_pLuaState.IsString(-2) && m_pLuaState.IsFunction(-1))
            {
                string sFuncName = m_pLuaState.ToString(-2);
                //CommonCode.Log(sFuncName);
                funcNames.Add(sFuncName);
            }
            m_pLuaState.Pop(1);
        }

        foreach (string funcName in funcNames)
        {
            m_pFuncNameToPointer.Add(funcName, StoreMethod(funcName));
        }

        //m_iPlayerIndex = StorePlayerIndex(); Discard

        m_pLuaState.Pop(1); //pop return table;

        object[] inParams = new object[] { };
        object[] outParams = new object[] { (string)"" };
        CallLuaMethod("GetVersion", inParams, ref outParams);
        string strLuaVersion = outParams[0] as string;
        if (!string.IsNullOrEmpty(strLuaVersion))
            Debug.Log(strLuaVersion);
		return true;
    }

    //原来在DoString之前必须register的
    public void NewState()
    {
        m_pLuaState = LuaAPI.NewState();
        m_pLuaState.L_OpenLibs();
    }

    public void Reload(string sLuaCode)
    {
        //LuaPlayerInfo.RegisterMe(m_pLuaState);
        m_pFuncNameToPointer = new Dictionary<string, int>();

        ThreadStatus status = ThreadStatus.LUA_OK;

        status = m_pLuaState.L_DoString(sLuaCode);


        if (status != ThreadStatus.LUA_OK)
        {
            throw new Exception(m_pLuaState.ToString(-1));
        }

        if (!m_pLuaState.IsTable(-1))
        {
            throw new Exception(
                "hammer abc's return value is not a table");
        }

        //Let's see what's in there
        int tableIndex = m_pLuaState.GetTop();
        m_pLuaState.PushNil();
        List<string> funcNames = new List<string>();
        while (m_pLuaState.Next(tableIndex))
        {
            if (m_pLuaState.IsString(-2) && m_pLuaState.IsFunction(-1))
            {
                string sFuncName = m_pLuaState.ToString(-2);
                funcNames.Add(sFuncName);
            }
            m_pLuaState.Pop(1);
        }

        foreach (string funcName in funcNames)
        {
            m_pFuncNameToPointer.Add(funcName, StoreMethod(funcName));
        }

        m_pLuaState.Pop(1); //pop return table;
    }

    private int StoreMethod(string name)
    {
        m_pLuaState.GetField(-1, name);
        if (!m_pLuaState.IsFunction(-1))
        {
            throw new Exception(string.Format(
                "method {0} not found!", name));
        }
        return m_pLuaState.L_Ref(LuaDef.LUA_REGISTRYINDEX);
    }

#if DISCARD
    private int StorePlayerIndex()
    {
        m_pLuaState.GetField(-1, "HammerPlayer");
        //CommonCode.Log(m_pLuaState.Type(-1).ToString());
        if (!m_pLuaState.IsTable(-1))
        {
            throw new Exception(string.Format(
                "player {0} not found!", "HammerPlayer"));
        }
        return m_pLuaState.L_Ref(LuaDef.LUA_REGISTRYINDEX);
    }
#endif

    public void CallMethod(string sMethod, params object[] parames)
    {
        if (m_pFuncNameToPointer.ContainsKey(sMethod))
        {
            CallMethod(m_pFuncNameToPointer[sMethod], parames);    
        }
        else
        {
            throw new Exception(string.Format(
                "function {0} not found!", sMethod));
        }
    }

    private void CallMethod(int funcRef, params object[] parames)
    {
        //PlayerInfoToLua(); Discard

        m_pLuaState.RawGetI(LuaDef.LUA_REGISTRYINDEX, funcRef);

        // insert `traceback' function
        var b = m_pLuaState.GetTop();
        m_pLuaState.PushCSharpFunction(Traceback);
        m_pLuaState.Insert(b);

        foreach (object parame in parames)
        {
            if (parame is bool)
            {
                m_pLuaState.PushBoolean((bool)parame);
            }
            else if (parame is float)
            {
                m_pLuaState.PushNumber((float)parame);
            }
            else if (parame is double)
            {
                m_pLuaState.PushNumber((double)parame);
            }
            else if (parame is int)
            {
                m_pLuaState.PushUnsigned((uint)(int)parame);
            }
            else if (parame is uint)
            {
                m_pLuaState.PushUnsigned((uint)parame);
            }
            else if (parame is short)
            {
                m_pLuaState.PushUnsigned((uint)(short)parame);
            }
            else if (parame is ushort)
            {
                m_pLuaState.PushUnsigned((ushort)parame);
            }
            else if (parame is byte)
            {
                m_pLuaState.PushUnsigned((byte)parame);
            }
            else if (parame is char)
            {
                m_pLuaState.PushUnsigned((char)parame);
            }
            else if (parame is long)
            {
				m_pLuaState.PushUInt64((ulong)parame);
            }
            else if (parame is ulong)
            {
                m_pLuaState.PushUInt64((ulong)parame);
            }
            else if (parame is string)
            {
                m_pLuaState.PushString((string)parame);
            }
            else
            {
                throw new Exception(string.Format(
                    "param type {0} not supported!", parame.GetType().ToString()));
            }
        }

        var status = m_pLuaState.PCall(parames.Length, 0, b);
        if (status != ThreadStatus.LUA_OK)
        {
            string sMessage = m_pLuaState.ToString(-1);
            Debug.LogError("====" + sMessage.Substring(
                Mathf.RoundToInt(Mathf.Max(sMessage.Length - 1023, 0)),
                Mathf.RoundToInt(Mathf.Min(sMessage.Length, 1023))));
        }

        // remove `traceback' function
        m_pLuaState.Remove(b);

        //PlayerInfoFromLua(); Discard

        return;
    }

    public bool CallMethodB(string sMethod, params object[] parames)
    {
        if (m_pFuncNameToPointer.ContainsKey(sMethod))
        {
            return CallMethodB(m_pFuncNameToPointer[sMethod], parames);
        }
        else
        {
            throw new Exception(string.Format(
                "function {0} not found!", sMethod));
        }
    }

    private bool CallMethodB(int funcRef, params object[] parames)
    {
        //PlayerInfoToLua(); Discard

        m_pLuaState.RawGetI(LuaDef.LUA_REGISTRYINDEX, funcRef);

        // insert `traceback' function
        var b = m_pLuaState.GetTop();
        m_pLuaState.PushCSharpFunction(Traceback);
        m_pLuaState.Insert(b);

        foreach (object parame in parames)
        {
            if (parame is bool)
            {
                m_pLuaState.PushBoolean((bool)parame);
            }
            else if (parame is float)
            {
                m_pLuaState.PushNumber((float)parame);
            }
            else if (parame is double)
            {
                m_pLuaState.PushNumber((double)parame);
            }
            else if (parame is int)
            {
                m_pLuaState.PushUnsigned((uint)(int)parame);
            }
            else if (parame is uint)
            {
                m_pLuaState.PushUnsigned((uint)parame);
            }
            else if (parame is short)
            {
                m_pLuaState.PushUnsigned((uint)(short)parame);
            }
            else if (parame is ushort)
            {
                m_pLuaState.PushUnsigned((ushort)parame);
            }
            else if (parame is byte)
            {
                m_pLuaState.PushUnsigned((byte)parame);
            }
            else if (parame is char)
            {
                m_pLuaState.PushUnsigned((char)parame);
            }
            else if (parame is long)
            {
                m_pLuaState.PushUInt64((ulong)(long)parame);
            }
            else if (parame is ulong)
            {
                m_pLuaState.PushUInt64((ulong)parame);
            }
            else if (parame is string)
            {
                m_pLuaState.PushString((string)parame);
            }
            else
            {
                throw new Exception(string.Format(
                    "param type {0} not supported!", parame.GetType().ToString()));
            }
        }

        var status = m_pLuaState.PCall(parames.Length, 1, b);
        if (status != ThreadStatus.LUA_OK)
        {
            string sMessage = m_pLuaState.ToString(-1);
            Debug.LogError("====" + sMessage.Substring(
                Mathf.RoundToInt(Mathf.Max(sMessage.Length - 1023, 0)),
                Mathf.RoundToInt(Mathf.Min(sMessage.Length, 1023))));
        }

        //Read return
        bool bRet = false;
        if (LuaType.LUA_TBOOLEAN == m_pLuaState.Type(-1))
        {
            bRet = m_pLuaState.ToBoolean(-1);
        }
        else
        {
            throw new Exception(string.Format(
                "return is not bool but {0}", m_pLuaState.Type(-1)));            
        }

        m_pLuaState.Pop(1); //Pop return

        // remove `traceback' function
        m_pLuaState.Remove(b);

        //PlayerInfoFromLua(); Discard

        return bRet;
    }

	public void CallLuaMethod(string funcName, object[] inParams, ref object[] outParams)
    {
#if UNITY
        Debug.Log("<color=#000088>Call Lua</color>" + funcName);
#endif
		if (!m_pFuncNameToPointer.ContainsKey(funcName))
		{
			throw new Exception(string.Format("function {0} not found!", funcName));
		}
		else
		{
			m_pLuaState.RawGetI(LuaDef.LUA_REGISTRYINDEX, m_pFuncNameToPointer[funcName]);

			// insert `traceback' function
			var oldTop = m_pLuaState.GetTop();
			
			m_pLuaState.PushCSharpFunction(Traceback);
			m_pLuaState.Insert(oldTop);

			PushParams(inParams);

            int inputNum = 0;
            if (inParams != null)
            {
                inputNum = inParams.Length;
            }

            var status = m_pLuaState.PCall(inputNum, -1, oldTop);
			if (status != ThreadStatus.LUA_OK)
			{
                string sMessage = m_pLuaState.ToString(-1);
                Debug.LogError("====" + sMessage.Substring(
                    Mathf.RoundToInt(Mathf.Max(sMessage.Length - 1023, 0)),
                    Mathf.RoundToInt(Mathf.Min(sMessage.Length, 1023))));
                ResumeLuaStack(oldTop);
				m_pLuaState.Remove(oldTop);
				return;
			}

			if (outParams != null)
				LuaMethodReturn(oldTop, ref outParams);

			m_pLuaState.Remove(oldTop);
		}
	}

    private void ResumeLuaStack(int oldState)
    {
        if (oldState == m_pLuaState.GetTop())
            return;

        m_pLuaState.Pop(m_pLuaState.GetTop() - oldState);
    }

	private void LuaMethodReturn(int oldTop, ref object[] outParams)
	{
		int newTop = m_pLuaState.GetTop();
		if (newTop == oldTop)
			return;

        if (outParams == null)
        {
            ResumeLuaStack(oldTop);
            return;
        }

        for (int i = oldTop + 1, j = 0; i <= newTop && j < outParams.Length; ++i, ++j)
        {
            GetObject(ref outParams[j], i);
        }

        ResumeLuaStack(oldTop);
	}

	private void GetObject(ref object outData, int stackIndex)
	{
		if (outData is bool)
		{
			outData = m_pLuaState.ToBoolean(stackIndex);
		}
		else if (outData is float)
		{
			outData = (float)(m_pLuaState.ToNumber(stackIndex));
		}
		else if (outData is double)
		{
			outData = m_pLuaState.ToNumber(stackIndex);
		}
		else if (outData is int)
		{
			outData = m_pLuaState.ToInteger(stackIndex);
		}
		else if (outData is uint)
		{
			outData = m_pLuaState.ToUnsigned(stackIndex);
		}
		else if (outData is short)
		{
			outData = (short)(m_pLuaState.ToInteger(stackIndex));
		}
		else if (outData is ushort)
		{
			outData = (ushort)(m_pLuaState.ToUnsigned(stackIndex));
		}
		else if (outData is byte)
		{
			outData = (byte)(m_pLuaState.ToInteger(stackIndex));
		}
		else if (outData is char)
		{
			outData = (char)(m_pLuaState.ToInteger(stackIndex));
		}
		else if (outData is long)
		{
			outData = (long)(m_pLuaState.ToUInt64(stackIndex));
		}
		else if (outData is ulong)
		{
			outData = m_pLuaState.ToUInt64(stackIndex);
		}
		else if (outData is string)
		{
			outData = m_pLuaState.ToString(stackIndex);
		}
        else if (outData is Array) //param object[] may not enter here
        {
            Array dataArray = outData as Array;
            if(dataArray != null)
            {
                LuaTable tbl = m_pLuaState.ToTable(stackIndex);
                for (int i = 0; i < tbl.Length && i < dataArray.Length; ++i )
                {
                    StkId val = tbl.GetInt(i+1);
                    if (val.V.TtIsNumber())
                    {
                        (outData as Array).SetValue((int)val.V.NValue, i);
                    }
                    else if (val.V.TtIsUInt64())
                    {
                        (outData as Array).SetValue((ulong)val.V.UInt64Value, i);
                    }
                    else if (val.V.TtIsString())
                    {
                        (outData as Array).SetValue(val.V.SValue(), i);
                    }
                    else if (val.V.TtIsBoolean())
                    {
                        (outData as Array).SetValue(val.V.BValue(), i);
                    }
                    else
                    {
                        (outData as Array).SetValue(null, i);
                    }
                }
            }
        }
		else
		{
			throw new Exception(string.Format("param type {0} not supported!", outData.GetType().ToString()));
		}
	}

    public void CallLuaMethodAnonymity(string funcName, object[] inParams, ref object[] outParams)
    {
        if (!m_pFuncNameToPointer.ContainsKey(funcName))
        {
            throw new Exception(string.Format("function {0} not found!", funcName));
        }
        else
        {
            m_pLuaState.RawGetI(LuaDef.LUA_REGISTRYINDEX, m_pFuncNameToPointer[funcName]);

            // insert `traceback' function
            var oldTop = m_pLuaState.GetTop();

            m_pLuaState.PushCSharpFunction(Traceback);
            m_pLuaState.Insert(oldTop);

            PushParams(inParams);

            int inputNum = 0;
            if (inParams != null)
            {
                inputNum = inParams.Length;
            }

            var status = m_pLuaState.PCall(inputNum, -1, oldTop);
            if (status != ThreadStatus.LUA_OK)
            {
                string sMessage = m_pLuaState.ToString(-1);
                Debug.LogError("====" + sMessage.Substring(
                    Mathf.RoundToInt(Mathf.Max(sMessage.Length - 1023, 0)),
                    Mathf.RoundToInt(Mathf.Min(sMessage.Length, 1023))));
                ResumeLuaStack(oldTop);
                m_pLuaState.Remove(oldTop);
                return;
            }

            if (outParams != null)
                LuaMethodReturnAnonymity(oldTop, ref outParams);

            m_pLuaState.Remove(oldTop);
        }
    }

    private void LuaMethodReturnAnonymity(int oldTop, ref object[] outParams)
    {
        int newTop = m_pLuaState.GetTop();
        if (newTop == oldTop)
            return;

        if (outParams == null)
        {
            ResumeLuaStack(oldTop);
            return;
        }

        for (int i = oldTop + 1, j = 0; i <= newTop && j < outParams.Length; ++i, ++j)
        {
            GetObjectAnonymity(ref outParams[j], i);
        }

        ResumeLuaStack(oldTop);
    }

    private void GetObjectAnonymity(ref object outData, int stackIndex)
    {
        switch (m_pLuaState.Type(stackIndex))
        {
            case LuaType.LUA_TNUMBER:
                outData = (float)(m_pLuaState.ToNumber(stackIndex));
                break;
            case LuaType.LUA_TBOOLEAN:
                outData = m_pLuaState.ToBoolean(stackIndex);
                break;
            case LuaType.LUA_TSTRING:
                outData = m_pLuaState.ToString(stackIndex);
                break;
            case LuaType.LUA_TTABLE:
                LuaTable tbl = m_pLuaState.ToTable(stackIndex);
                outData = new object[tbl.Length];
                for (int i = 0; i < tbl.Length; ++i)
                {
                    StkId val = tbl.GetInt(i + 1);
                    if (val.V.TtIsNumber())
                    {
                        (outData as Array).SetValue((int)val.V.NValue, i);
                    }
                    else if (val.V.TtIsUInt64())
                    {
                        (outData as Array).SetValue((ulong)val.V.UInt64Value, i);
                    }
                    else if (val.V.TtIsString())
                    {
                        (outData as Array).SetValue(val.V.SValue(), i);
                    }
                    else if (val.V.TtIsBoolean())
                    {
                        (outData as Array).SetValue(val.V.BValue(), i);
                    }
                    else
                    {
                        (outData as Array).SetValue(null, i);
                    }
                }
                break;
            case LuaType.LUA_TUINT64:
                outData = m_pLuaState.ToUInt64(stackIndex);
                break;
            default:
                break;
        }
    }

	private void PushParams(params object[] inParams)
	{
        if (inParams == null || inParams.Length == 0)
            return;

		foreach (object parame in inParams)
		{
			if (parame is bool)
			{
				m_pLuaState.PushBoolean((bool)parame);
			}
			else if (parame is float)
			{
				m_pLuaState.PushNumber((float)parame);
			}
			else if (parame is double)
			{
				m_pLuaState.PushNumber((double)parame);
			}
			else if (parame is int)
			{
				m_pLuaState.PushInteger((int)parame);
			}
			else if (parame is uint)
			{
				m_pLuaState.PushUnsigned((uint)parame);
			}
			else if (parame is short)
			{
				m_pLuaState.PushUnsigned((uint)(short)parame);
			}
			else if (parame is ushort)
			{
				m_pLuaState.PushUnsigned((ushort)parame);
			}
			else if (parame is byte)
			{
				m_pLuaState.PushUnsigned((byte)parame);
			}
			else if (parame is char)
			{
				m_pLuaState.PushUnsigned((char)parame);
			}
			else if (parame is long)
			{
				m_pLuaState.PushUInt64((ulong)(long)parame);
			}
			else if (parame is ulong)
			{
				m_pLuaState.PushUInt64((ulong)parame);
			}
			else if (parame is string)
			{
				m_pLuaState.PushString((string)parame);
			}
            else if (parame is CLuaArray) //param object[] may not enter here
            {
                LuaTable tbl = new LuaTable((LuaState)m_pLuaState);
                int i = 0;
                CLuaArray theArray = parame as CLuaArray;
                foreach (object obj in theArray.m_pTheArray as Array)
                {
                    TValue tv = new TValue();
                    ChangeToTValue(obj, ref tv);
                    tbl.SetInt(i + 1, ref tv);
                    ++i;
                }

                m_pLuaState.PushTable(tbl);
            }
            else if (parame is Array) //param object[] may not enter here
			{
                LuaTable tbl = new LuaTable((LuaState)m_pLuaState);
                int i = 0;
                foreach (object obj in parame as Array)
                {
                    TValue tv = new TValue();
                    ChangeToTValue(obj, ref tv);
                    tbl.SetInt(i + 1, ref tv);    // mark: 遵从Lua语法，table从1开始
                    ++i;
                }

				m_pLuaState.PushTable(tbl);
			}
			else
			{
				throw new Exception(string.Format(
					"param type {0} not supported!", parame.GetType().ToString()));
			}
		}
	}

    private void StkIdToValue(ref object param, ref StkId val)
    {
        if (param is int)
        {
            param = (int)val.V.OValue;
        }
    }

	private void ChangeToTValue(object parame, ref TValue tv)
	{
		if (parame is bool)
		{
			tv.SetBValue((bool)parame);
		}
		else if (parame is float)
		{
			tv.SetNValue((float)parame);
		}
		else if (parame is double)
		{
			tv.SetNValue((double)parame);
		}
		else if (parame is int)
		{
			tv.SetNValue((int)parame);
		}
		else if (parame is uint)
		{
			tv.SetNValue((uint)parame);
		}
		else if (parame is short)
		{
			tv.SetNValue((uint)(short)parame);
		}
		else if (parame is ushort)
		{
			tv.SetNValue((ushort)parame);
		}
		else if (parame is byte)
		{
			tv.SetNValue((byte)parame);
		}
		else if (parame is char)
		{
			tv.SetNValue((char)parame);
		}
		else if (parame is long)
		{
			tv.SetUInt64Value((ulong)(long)parame);
		}
		else if (parame is ulong)
		{
			tv.SetUInt64Value((ulong)parame);
		}
		else if (parame is string)
		{
			tv.SetSValue((string)parame);
		}
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

    public bool Reload()
    {
        RemoveAllFuncs();

        Initial();
        return true;
    }

    private void RemoveAllFuncs()
    {
        foreach (var funcRef in m_pFuncNameToPointer.Values)
        {
            m_pLuaState.L_Unref(LuaDef.LUA_REGISTRYINDEX, funcRef);
        }
    }
#if DISCARD
    private void PlayerInfoToLua()
    {
        //TODO Player Info Format not fixed! DO NOT USE string Index here!

        //m_pLuaState.GetGlobal("HammerPlayer");
        m_pLuaState.RawGetI(LuaDef.LUA_REGISTRYINDEX, m_iPlayerIndex);
        m_pLuaState.PushNumber(m_pPlayer.m_iExp); //-1 as value
        m_pLuaState.SetField(-2, "exp"); //pop value
        m_pLuaState.PushNumber(m_pPlayer.m_iPower); //-1 as value
        m_pLuaState.SetField(-2, "power"); //pop value

        
        m_pLuaState.Pop(1);
    }

    private void PlayerInfoFromLua()
    {
        m_pLuaState.RawGetI(LuaDef.LUA_REGISTRYINDEX, m_iPlayerIndex);
        int tableIndex = m_pLuaState.GetTop();
        m_pLuaState.PushNil();
        List<string> fieldNames = new List<string>();
        while (m_pLuaState.Next(tableIndex))
        {
            if (m_pLuaState.IsString(-2))
            {
                string sFieldName = m_pLuaState.ToString(-2);
                //CommonCode.Log(sFieldName);
                ReadFiled(sFieldName);
            }
            m_pLuaState.Pop(1);
        }

        m_pLuaState.Pop(1);
    }

    private void ReadFiled(string sFieldName)
    {
        bool bOut = false;
        if (sFieldName.Equals("exp"))
        {
            m_pPlayer.m_iExp = m_pLuaState.ToIntegerX(-1, out bOut);
        }
        else if (sFieldName.Equals("power"))
        {
            m_pPlayer.m_iPower = m_pLuaState.ToIntegerX(-1, out bOut);
        }
    }
#endif
}
