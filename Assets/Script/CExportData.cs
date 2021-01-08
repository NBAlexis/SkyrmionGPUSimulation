using System.IO;
using UnityEngine;

public static class CExportData
{
    public struct SGetData
    {
        public Texture2D m_txRaw;
        public Texture2D m_txShow;
        public Texture2D m_txPic;

        //Those strings will cost toooooooo long time to generate!
        //public string m_sNx;
        //public string m_sNy;
        //public string m_sNz;
    }

    public static void Save(string sDate, int iStep, string profile, int iRes,
        RenderTexture datax,
        RenderTexture datay,
        RenderTexture dataz,
        Material matShow,
        Material matRaw)
    {
        string sTag = sDate + "_" + iStep;
        string sPathImag1 = Application.dataPath + CManager._outfolder + "Output/" + sTag + "_raw.png";
        string sPathImag2 = Application.dataPath + CManager._outfolder + "Output/" + sTag + "_pic.png";
        string sPathImag3 = Application.dataPath + CManager._outfolder + "Output/" + sTag + "_show.png";

        string sProf = Application.dataPath + CManager._outfolder + "Output/" + sTag + "_prof.txt";

        SGetData builtRes = BuildMap(iRes, datax, datay, dataz, matShow, matRaw);

        byte[] byDataRaw = builtRes.m_txRaw.EncodeToPNG();
        File.WriteAllBytes(sPathImag1, byDataRaw);

        byte[] byDataPic = builtRes.m_txPic.EncodeToPNG();
        File.WriteAllBytes(sPathImag2, byDataPic);

        byte[] byDataShow = builtRes.m_txShow.EncodeToPNG();
        File.WriteAllBytes(sPathImag3, byDataShow);

        File.WriteAllText(sProf, profile);

    }

    private static SGetData BuildMap(int iRes, RenderTexture datax, RenderTexture datay, RenderTexture dataz, Material matShow, Material matRaw)
    {
        RenderTexture rt = new RenderTexture(iRes, iRes, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);

        matRaw.SetTexture("_Nx", datax);
        matRaw.SetTexture("_Ny", datay);
        matRaw.SetTexture("_Nz", dataz);
        Graphics.Blit(null, rt, matRaw);
        Texture2D dataReader = new Texture2D(iRes, iRes, TextureFormat.ARGB32, false, true);
        RenderTexture.active = rt;
        dataReader.ReadPixels(new Rect(0, 0, iRes, iRes), 0, 0);
        dataReader.Apply();
        RenderTexture.active = null;
        rt.Release();

        RenderTexture rtshow = new RenderTexture(iRes, iRes, 0, RenderTextureFormat.ARGB32);
        matShow.SetTexture("_Nx", datax);
        matShow.SetTexture("_Ny", datay);
        matShow.SetTexture("_Nz", dataz);
        Graphics.Blit(null, rtshow, matShow);
        Texture2D imgShow = new Texture2D(iRes, iRes, TextureFormat.ARGB32, false);
        RenderTexture.active = rtshow;
        imgShow.ReadPixels(new Rect(0, 0, iRes, iRes), 0, 0);
        imgShow.Apply();
        RenderTexture.active = null;
        rtshow.Release();

        //RenderTexture nx = new RenderTexture(512, 512, 0, RenderTextureFormat.RFloat);
        //RenderTexture ny = new RenderTexture(512, 512, 0, RenderTextureFormat.RFloat);
        //RenderTexture nz = new RenderTexture(512, 512, 0, RenderTextureFormat.RFloat);
        //mat.SetVector("_Filter", new Vector4(1.0f, 0.0f, 0.0f, 1.0f));
        //Graphics.Blit(data, nx, mat);
        //mat.SetVector("_Filter", new Vector4(0.0f, 1.0f, 0.0f, 1.0f));
        //Graphics.Blit(data, ny, mat);
        //mat.SetVector("_Filter", new Vector4(0.0f, 0.0f, 1.0f, 1.0f));
        //Graphics.Blit(data, nz, mat);

        //Texture2D tnx = new Texture2D(512, 512, TextureFormat.RFloat, false);
        //Texture2D tny = new Texture2D(512, 512, TextureFormat.RFloat, false);
        //Texture2D tnz = new Texture2D(512, 512, TextureFormat.RFloat, false);
        //RenderTexture.active = nx;
        //tnx.ReadPixels(new Rect(0, 0, 512, 512), 0, 0);
        //tnx.Apply();
        //RenderTexture.active = ny;
        //tny.ReadPixels(new Rect(0, 0, 512, 512), 0, 0);
        //tny.Apply();
        //RenderTexture.active = nz;
        //tnz.ReadPixels(new Rect(0, 0, 512, 512), 0, 0);
        //tnz.Apply();
        //RenderTexture.active = null;
        //nx.Release();
        //ny.Release();
        //nz.Release();

        int iLRes = iRes*2 - 1;
        Texture2D retData = new Texture2D(iLRes, iLRes, TextureFormat.RGB24, false);
        //string sNx = "", sNy = "", sNz = "";

        for (int i = 0; i < iLRes; ++i)
        {
            for (int j = 0; j < iLRes; ++j)
            {
                if (0 == (i & 1) && 0 == (j & 1))
                {
                    Color c = dataReader.GetPixel(i / 2, j / 2);
                    //float fNx = tnx.GetPixel(i / 2, j / 2).r;
                    //float fNy = tny.GetPixel(i / 2, j / 2).r;
                    //float fNz = tnz.GetPixel(i / 2, j / 2).r;

                    retData.SetPixel(i, j, GetParula(c.b));

                    //sNx += (fNx * 2.0f - 1.0f) + (1022 == i ? "\n" : "\t");
                    //sNy += (fNy * 2.0f - 1.0f) + (1022 == i ? "\n" : "\t");
                    //sNz += (fNz * 2.0f - 1.0f) + (1022 == i ? "\n" : "\t");
                }
                else if (0 == (i & 1) && 0 != (j & 1))
                {
                    Color c1 = dataReader.GetPixel(i / 2, j / 2);
                    Color c2 = dataReader.GetPixel(i / 2, j / 2 + 1);
                    retData.SetPixel(i, j, GetParula(0.5f * (c1.b + c2.b)));
                }
                else if (0 != (i & 1) && 0 == (j & 1))
                {
                    Color c1 = dataReader.GetPixel(i / 2, j / 2);
                    Color c2 = dataReader.GetPixel(i / 2 + 1, j / 2);
                    retData.SetPixel(i, j, GetParula(0.5f * (c1.b + c2.b)));
                }
                else if (0 != (i & 1) && 0 != (j & 1))
                {
                    Color c1 = dataReader.GetPixel(i / 2, j / 2);
                    Color c2 = dataReader.GetPixel(i / 2 + 1, j / 2);
                    Color c3 = dataReader.GetPixel(i / 2, j / 2 + 1);
                    Color c4 = dataReader.GetPixel(i / 2 + 1, j / 2 + 1);
                    retData.SetPixel(i, j, GetParula(0.25f * (c1.b + c2.b + c3.b + c4.b)));
                }
            }
        }

        float fNXYMax = 0.0f;
        int iSRes = iRes/8;
        Vector2[,] v2N = new Vector2[iSRes, iSRes];
        for (int i = 0; i < iSRes; ++i)
        {
            for (int j = 0; j < iSRes; ++j)
            {
                //get data from 8 x 8 grid
                Vector2 v2Nt = Vector2.zero;
                for (int x = 0; x < 8; ++x)
                {
                    for (int y = 0; y < 8; ++y)
                    {
                        Color c = dataReader.GetPixel(x + i*8, y + j*8);
                        v2Nt += new Vector2(c.r * 2.0f - 1.0f, c.g * 2.0f - 1.0f);
                    }
                }
                v2N[i, j] = v2Nt / (float)iSRes;
                if (Mathf.Abs(v2N[i, j].x) > fNXYMax)
                {
                    fNXYMax = Mathf.Abs(v2N[i, j].x);
                }
                if (Mathf.Abs(v2N[i, j].y) > fNXYMax)
                {
                    fNXYMax = Mathf.Abs(v2N[i, j].y);
                }
            }
        }

        for (int i = 0; i < iSRes; ++i)
        {
            for (int j = 0; j < iSRes; ++j)
            {
                DrawArrow(iLRes, 
                    8 + 16 * i, 8 + 16 * j, 
                    Mathf.RoundToInt(v2N[i, j].x * 8.0f / fNXYMax), 
                    Mathf.RoundToInt(v2N[i, j].y * 8.0f / fNXYMax), retData);
            }
        }

        retData.Apply();
        return new SGetData
        {
            m_txRaw = dataReader,
            m_txShow = imgShow,
            m_txPic = retData,
            //m_sNx = sNx,
            //m_sNy = sNy,
            //m_sNz = sNz,
        };
    }

    private static readonly Color dark = new Color(0.25f, 0.25f, 0.25f);
    private static void DrawArrow(int iMax, int gridX, int gridY, int nx, int ny, Texture2D canv)
    {
        if (0 == nx && 0 == ny)
        {
            canv.SetPixel(gridX, gridY, Color.black);
            return;
        }

        if (Mathf.Abs(nx) >= Mathf.Abs(ny))
        {
            int iSign = nx > 0 ? 1 : -1;
            for (int i = 0; i < iSign*nx; ++i)
            {
                int iTargetX = gridX + iSign*i;
                int iTargetY = Mathf.RoundToInt(iSign*i*ny/(float) nx) + gridY;

                if (iTargetX > 0
                 && iTargetX < iMax
                 && iTargetY > 0
                 && iTargetY < iMax)
                {
                    canv.SetPixel(iTargetX, iTargetY, 0 == i ? Color.black : dark);
                }
            }
        }
        else
        {
            int iSign = ny > 0 ? 1 : -1;
            for (int i = 0; i < iSign * ny; ++i)
            {
                int iTargetY = gridY + iSign * i;
                int iTargetX = Mathf.RoundToInt(iSign * i * nx / (float)ny) + gridX;

                if (iTargetX > 0
                 && iTargetX < iMax
                 && iTargetY > 0
                 && iTargetY < iMax)
                {
                    canv.SetPixel(iTargetX, iTargetY, 0 == i ? Color.black : dark);
                }
            }
        }
    }

    #region Parula

    private static Color GetParula(float fValue)
    {
        fValue = Mathf.Clamp01(1.0f - fValue);
        int iNz = Mathf.Clamp(Mathf.RoundToInt(fValue * 63.0f * 100.0f), 0, 6299);
        int iFloor = iNz / 100; //0 to 63
        float interval = (iNz % 100) / 100.0f;
        Color start = parula[iFloor];
        Color end = parula[iFloor + 1];
        return Color.Lerp(start, end, interval);
    }

    private static readonly Color[] parula = new[]
    {
        new Color(0.2422f, 0.1504f, 0.6603f, 1.0f),
        new Color(0.2504f, 0.1650f, 0.7076f, 1.0f),
        new Color(0.2578f, 0.1818f, 0.7511f, 1.0f),
        new Color(0.2647f, 0.1978f, 0.7952f, 1.0f),
        new Color(0.2706f, 0.2147f, 0.8364f, 1.0f),
        new Color(0.2751f, 0.2342f, 0.8710f, 1.0f),
        new Color(0.2783f, 0.2559f, 0.8991f, 1.0f),
        new Color(0.2803f, 0.2782f, 0.9221f, 1.0f),
        new Color(0.2813f, 0.3006f, 0.9414f, 1.0f),
        new Color(0.2810f, 0.3228f, 0.9579f, 1.0f),
        new Color(0.2795f, 0.3447f, 0.9717f, 1.0f),
        new Color(0.2760f, 0.3667f, 0.9829f, 1.0f),
        new Color(0.2699f, 0.3892f, 0.9906f, 1.0f),
        new Color(0.2602f, 0.4123f, 0.9952f, 1.0f),
        new Color(0.2440f, 0.4358f, 0.9988f, 1.0f),
        new Color(0.2206f, 0.4603f, 0.9973f, 1.0f),
        new Color(0.1963f, 0.4847f, 0.9892f, 1.0f),
        new Color(0.1834f, 0.5074f, 0.9798f, 1.0f),
        new Color(0.1786f, 0.5289f, 0.9682f, 1.0f),
        new Color(0.1764f, 0.5499f, 0.9520f, 1.0f),
        new Color(0.1687f, 0.5703f, 0.9359f, 1.0f),
        new Color(0.1540f, 0.5902f, 0.9218f, 1.0f),
        new Color(0.1460f, 0.6091f, 0.9079f, 1.0f),
        new Color(0.1380f, 0.6276f, 0.8973f, 1.0f),
        new Color(0.1248f, 0.6459f, 0.8883f, 1.0f),
        new Color(0.1113f, 0.6635f, 0.8763f, 1.0f),
        new Color(0.0952f, 0.6798f, 0.8598f, 1.0f),
        new Color(0.0689f, 0.6948f, 0.8394f, 1.0f),
        new Color(0.0297f, 0.7082f, 0.8163f, 1.0f),
        new Color(0.0036f, 0.7203f, 0.7917f, 1.0f),
        new Color(0.0067f, 0.7312f, 0.7660f, 1.0f),
        new Color(0.0433f, 0.7411f, 0.7394f, 1.0f),
        new Color(0.0964f, 0.7500f, 0.7120f, 1.0f),
        new Color(0.1408f, 0.7584f, 0.6842f, 1.0f),
        new Color(0.1717f, 0.7670f, 0.6554f, 1.0f),
        new Color(0.1938f, 0.7758f, 0.6251f, 1.0f),
        new Color(0.2161f, 0.7843f, 0.5923f, 1.0f),
        new Color(0.2470f, 0.7918f, 0.5567f, 1.0f),
        new Color(0.2906f, 0.7973f, 0.5188f, 1.0f),
        new Color(0.3406f, 0.8008f, 0.4789f, 1.0f),
        new Color(0.3909f, 0.8029f, 0.4354f, 1.0f),
        new Color(0.4456f, 0.8024f, 0.3909f, 1.0f),
        new Color(0.5044f, 0.7993f, 0.3480f, 1.0f),
        new Color(0.5616f, 0.7942f, 0.3045f, 1.0f),
        new Color(0.6174f, 0.7876f, 0.2612f, 1.0f),
        new Color(0.6720f, 0.7793f, 0.2227f, 1.0f),
        new Color(0.7242f, 0.7698f, 0.1910f, 1.0f),
        new Color(0.7738f, 0.7598f, 0.1646f, 1.0f),
        new Color(0.8203f, 0.7498f, 0.1535f, 1.0f),
        new Color(0.8634f, 0.7406f, 0.1596f, 1.0f),
        new Color(0.9035f, 0.7330f, 0.1774f, 1.0f),
        new Color(0.9393f, 0.7288f, 0.2100f, 1.0f),
        new Color(0.9728f, 0.7298f, 0.2394f, 1.0f),
        new Color(0.9956f, 0.7434f, 0.2371f, 1.0f),
        new Color(0.9970f, 0.7659f, 0.2199f, 1.0f),
        new Color(0.9952f, 0.7893f, 0.2028f, 1.0f),
        new Color(0.9892f, 0.8136f, 0.1885f, 1.0f),
        new Color(0.9786f, 0.8386f, 0.1766f, 1.0f),
        new Color(0.9676f, 0.8639f, 0.1643f, 1.0f),
        new Color(0.9610f, 0.8890f, 0.1537f, 1.0f),
        new Color(0.9597f, 0.9135f, 0.1423f, 1.0f),
        new Color(0.9628f, 0.9373f, 0.1265f, 1.0f),
        new Color(0.9691f, 0.9606f, 0.1064f, 1.0f),
        new Color(0.9769f, 0.9839f, 0.0805f, 1.0f),
    };

    #endregion

    #region Jet

    private static Color GetJet(float fValue)
    {
        fValue = Mathf.Clamp01(1.0f - fValue);
        int iNz = Mathf.Clamp(Mathf.RoundToInt(fValue * 63.0f * 100.0f), 0, 6299);
        int iFloor = iNz / 100; //0 to 63
        float interval = (iNz % 100) / 100.0f;
        Color start = jetColors[iFloor];
        Color end = jetColors[iFloor + 1];
        return Color.Lerp(start, end, interval);
    }

    private static readonly Color[] jetColors = new[]
    {
        new Color(0.0f, 0.0f, 0.5625f, 1.0f),
        new Color(0.0f, 0.0f, 0.6250f, 1.0f),
        new Color(0.0f, 0.0f, 0.6875f, 1.0f),
        new Color(0.0f, 0.0f, 0.7500f, 1.0f),
        new Color(0.0f, 0.0f, 0.8125f, 1.0f),
        new Color(0.0f, 0.0f, 0.8750f, 1.0f),
        new Color(0.0f, 0.0f, 0.9375f, 1.0f),
        new Color(0.0f, 0.0f, 1.0000f, 1.0f),
        new Color(0.0f, 0.0625f, 1.0000f, 1.0f),
        new Color(0.0f, 0.1250f, 1.0000f, 1.0f),
        new Color(0.0f, 0.1875f, 1.0000f, 1.0f),
        new Color(0.0f, 0.2500f, 1.0000f, 1.0f),
        new Color(0.0f, 0.3125f, 1.0000f, 1.0f),
        new Color(0.0f, 0.3750f, 1.0000f, 1.0f),
        new Color(0.0f, 0.4375f, 1.0000f, 1.0f),
        new Color(0.0f, 0.5000f, 1.0000f, 1.0f),
        new Color(0.0f, 0.5625f, 1.0000f, 1.0f),
        new Color(0.0f, 0.6250f, 1.0000f, 1.0f),
        new Color(0.0f, 0.6875f, 1.0000f, 1.0f),
        new Color(0.0f, 0.7500f, 1.0000f, 1.0f),
        new Color(0.0f, 0.8125f, 1.0000f, 1.0f),
        new Color(0.0f, 0.8750f, 1.0000f, 1.0f),
        new Color(0.0f, 0.9375f, 1.0000f, 1.0f),
        new Color(0.0f, 1.0000f, 1.0000f, 1.0f),
        new Color(0.0625f, 1.0000f, 0.9375f, 1.0f),
        new Color(0.1250f, 1.0000f, 0.8750f, 1.0f),
        new Color(0.1875f, 1.0000f, 0.8125f, 1.0f),
        new Color(0.2500f, 1.0000f, 0.7500f, 1.0f),
        new Color(0.3125f, 1.0000f, 0.6875f, 1.0f),
        new Color(0.3750f, 1.0000f, 0.6250f, 1.0f),
        new Color(0.4375f, 1.0000f, 0.5625f, 1.0f),
        new Color(0.5000f, 1.0000f, 0.5000f, 1.0f),
        new Color(0.5625f, 1.0000f, 0.4375f, 1.0f),
        new Color(0.6250f, 1.0000f, 0.3750f, 1.0f),
        new Color(0.6875f, 1.0000f, 0.3125f, 1.0f),
        new Color(0.7500f, 1.0000f, 0.2500f, 1.0f),
        new Color(0.8125f, 1.0000f, 0.1875f, 1.0f),
        new Color(0.8750f, 1.0000f, 0.1250f, 1.0f),
        new Color(0.9375f, 1.0000f, 0.0625f, 1.0f),
        new Color(1.0000f, 1.0000f, 0.0f, 1.0f),
        new Color(1.0000f, 0.9375f, 0.0f, 1.0f),
        new Color(1.0000f, 0.8750f, 0.0f, 1.0f),
        new Color(1.0000f, 0.8125f, 0.0f, 1.0f),
        new Color(1.0000f, 0.7500f, 0.0f, 1.0f),
        new Color(1.0000f, 0.6875f, 0.0f, 1.0f),
        new Color(1.0000f, 0.6250f, 0.0f, 1.0f),
        new Color(1.0000f, 0.5625f, 0.0f, 1.0f),
        new Color(1.0000f, 0.5000f, 0.0f, 1.0f),
        new Color(1.0000f, 0.4375f, 0.0f, 1.0f),
        new Color(1.0000f, 0.3750f, 0.0f, 1.0f),
        new Color(1.0000f, 0.3125f, 0.0f, 1.0f),
        new Color(1.0000f, 0.2500f, 0.0f, 1.0f),
        new Color(1.0000f, 0.1875f, 0.0f, 1.0f),
        new Color(1.0000f, 0.1250f, 0.0f, 1.0f),
        new Color(1.0000f, 0.0625f, 0.0f, 1.0f),
        new Color(1.0000f, 0.0f, 0.0f, 1.0f),
        new Color(0.9375f, 0.0f, 0.0f, 1.0f),
        new Color(0.8750f, 0.0f, 0.0f, 1.0f),
        new Color(0.8125f, 0.0f, 0.0f, 1.0f),
        new Color(0.7500f, 0.0f, 0.0f, 1.0f),
        new Color(0.6875f, 0.0f, 0.0f, 1.0f),
        new Color(0.6250f, 0.0f, 0.0f, 1.0f),
        new Color(0.5625f, 0.0f, 0.0f, 1.0f),
        new Color(0.5000f, 0.0f, 0.0f, 1.0f),
    };

    #endregion
}
