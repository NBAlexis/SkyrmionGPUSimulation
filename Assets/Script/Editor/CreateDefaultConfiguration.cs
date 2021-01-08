using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;
using Random = UnityEngine.Random;

public class CreateDefaultConfiguration
{
    [MenuItem("Skyrmion/CreateDefaultConfiguration")]
    static void Test()
    {

        Noise();

        string[] files1 = {"In.png", "Out.png", "Up.png", "Down.png", "Left.png", "Right.png"};
        Vector3[] vs1 =
        {
            new Vector3(0.0f, 0.0f, 1.0f),
            new Vector3(0.0f, 0.0f, -1.0f),
            new Vector3(0.0f, -1.0f, 0.0f),
            new Vector3(0.0f, 1.0f, 0.0f),
            new Vector3(1.0f, 0.0f, 0.0f),
            new Vector3(-1.0f, 0.0f, 0.0f)
        };

        for (int i = 0; i < 6; ++i)
        {
            Vs(files1[i], vs1[i]);
        }

        float[] fRs = {5.0f, 10.0f, 20.0f, 30.0f, 50.0f};

        string[] files2 = { "R5u.png", "R10u.png", "R20u.png", "R30u.png", "R50u.png" };

        for (int i = 0; i < 5; ++i)
        {
            Skyrmions(files2[i], fRs[i], 0.0f, 1, 1, 127, 127);
        }

        string[] files22 = { "R5d.png", "R10d.png", "R20d.png", "R30d.png", "R50d.png" };

        for (int i = 0; i < 5; ++i)
        {
            Skyrmions(files22[i], fRs[i], 0.0f, 1, -1, 127, 127);
        }

        string[] files3 = { "LeftSkyrmion.png", "RightSkyrmion.png", "UpSkyrmion.png", "DownSkyrmion.png" };
        int[] iX = { 63, 191, 127, 127 };
        int[] iY = { 127, 127, 63, 191 };

        for (int i = 0; i < 4; ++i)
        {
            Skyrmions(files3[i], 30.0f, 0.0f, 1, 1, iX[i], iY[i]);
        }

        string[] files4 = {"Gamma1.png", "Gamma2.png", "Gamma3.png", "Gamma4.png", "Gamma5.png", "Gamma6.png", "Gamma7.png", "Gamma8.png" };
        float[] fGammas =
        {
            Mathf.PI * 2.0f * 1.0f / 8.0f,
            Mathf.PI * 2.0f * 2.0f / 8.0f,
            Mathf.PI * 2.0f * 3.0f / 8.0f,
            Mathf.PI * 2.0f * 4.0f / 8.0f,
            Mathf.PI * 2.0f * 5.0f / 8.0f,
            Mathf.PI * 2.0f * 6.0f / 8.0f,
            Mathf.PI * 2.0f * 7.0f / 8.0f,
            Mathf.PI * 2.0f * 8.0f / 8.0f,
        };

        for (int i = 0; i < 5; ++i)
        {
            Skyrmions(files4[i], 30.0f, fGammas[i], 1, 1, 127, 127);
        }

        string[] files5 =
        {
            "m2g1.png", "m3g1.png", "antim2g1.png", "antim3g1.png",
            "m2ga1.png", "m3ga1.png", "antim2ga1.png", "antim3ga1.png",
        };
        int[] ims = {2, 3, -2, -3, 2, 3, -2, -3};
        int[] igs = {1, 1, 1, 1, -1, -1, -1, -1};

        for (int i = 0; i < 8; ++i)
        {
            Skyrmions(files5[i], 30.0f, 0.0f, ims[i], igs[i], 127, 127);
        }

        Skyrmions("antim1g1.png", 30.0f, 0.0f, -1, 1, 127, 127);
        Skyrmions("antim1ga1.png", 30.0f, 0.0f, -1, -1, 127, 127);

        Debug.Log("-- Finished ---");
    }

    private static Vector3[,] GetConfigurationConst(Vector3 v)
    {
        Vector3[,] ret = new Vector3[256, 256];

        for (int i = 0; i < 256; ++i)
        {
            for (int j = 0; j < 256; ++j)
            {
                ret[i,j] = v;
            }
        }
        return ret;
    }

    private static Vector3[,] GetConfigurationNoise()
    {
        Vector3[,] ret = new Vector3[256, 256];

        for (int i = 0; i < 256; ++i)
        {
            for (int j = 0; j < 256; ++j)
            {
                float fTheta = Random.Range(0.0f, Mathf.PI);
                float fPhi = Random.Range(0.0f, Mathf.PI * 2.0f);

                ret[i, j].x = Mathf.Cos(fPhi) * Mathf.Sin(fTheta);
                ret[i, j].y = Mathf.Sin(fPhi) * Mathf.Sin(fTheta);
                ret[i, j].z = Mathf.Cos(fTheta);
            }
        }
        return ret;
    }

    private static Vector3[,] GetConfigurationOneSkyrmion(float fRadius, float fGamma, int iM, int iG, int iX, int iY)
    {
        Vector3[,] ret = new Vector3[256, 256];

        float fA = 3.0f / fRadius;
        for (int i = 0; i < 256; ++i)
        {
            for (int j = 0; j < 256; ++j)
            {
                float fRho = Mathf.Sqrt((i - iX) * (i - iX) + (j - iY) * (j - iY));
                float fPhi = Mathf.Atan2((j - iY), (i - iX));

                float fTheta = 4.0f * Mathf.Atan(Mathf.Exp(-fA * fRho));

                ret[i, j].x = Mathf.Cos(Mathf.PI * 0.5f + fGamma + iM * fPhi) * Mathf.Sin(fTheta);
                ret[i, j].y = Mathf.Sin(Mathf.PI * 0.5f + fGamma + iM * fPhi) * Mathf.Sin(fTheta);
                ret[i, j].z = iG * Mathf.Cos(fTheta);
            }
        }
        return ret;
    }

    private static Texture2D WriteConfigurationImage(Vector3[,] norms)
    {
        Texture2D ret = new Texture2D(256, 256, TextureFormat.RGBAFloat, false);

        for (int i = 0; i < 256; ++i)
        {
            for (int j = 0; j < 256; ++j)
            {
                //ret.SetPixel(i, j, GetColorFromFloat((norms[i,j].x + 1.0) * 0.5));
                //ret.SetPixel(i, j + 256, GetColorFromFloat((norms[i, j].y + 1.0) * 0.5));
                //ret.SetPixel(i, j + 512, GetColorFromFloat((norms[i, j].z + 1.0) * 0.5));

                float fTheta = Mathf.Acos(norms[i, j].z);
                while (fTheta < 0.0f)
                {
                    fTheta += Mathf.PI;
                }
                while (fTheta > 2.0f * Mathf.PI)
                {
                    fTheta -= 2.0f * Mathf.PI;
                }

                if (fTheta > Mathf.PI)
                {
                    fTheta = 2.0f * Mathf.PI - fTheta;
                }

                fTheta = fTheta / Mathf.PI;

                float fPhi = Mathf.Atan2(norms[i, j].y, norms[i, j].x);
                while (fPhi < 0.0f)
                {
                    fPhi += Mathf.PI * 2.0f;
                }
                while (fPhi > Mathf.PI * 2.0f)
                {
                    fPhi -= Mathf.PI * 2.0f;
                }

                fPhi = fPhi / (Mathf.PI * 2.0f);

                ret.SetPixel(i, j, new Color(
                    Mathf.Clamp01(fTheta), Mathf.Clamp01(fPhi), 0.0F));
                //ret.SetPixel(i, j + 256, new Color(fPhi, fPhi, fPhi));
            }
        }
        ret.Apply(false);
        return ret;
    }

    private static Color GetColorFromFloat(double f01)
    {
        long num = (long)Math.Round(f01 * 4294967296);
        if (num < 0)
        {
            num = 0;
        }

        if (num >= 4294967296)
        {
            num = 4294967295;
        }

        long iR = num / 16777216;
        num = num % 16777216;
        long iG = num / 65536;
        num = num % 65536;
        long iB = num / 256;
        long iA = num % 256;

        return new Color(
            (float)(iR / 255.0), 
            (float)(iG / 255.0), 
            (float)(iB / 255.0), 
            (float)(iA / 255.0));
    }

    private static void Noise()
    {
        Debug.Log("Writting Noise.png");
        Vector3[,] vs1 = GetConfigurationNoise();
        Texture2D tx1 = WriteConfigurationImage(vs1);
        byte[] data = tx1.EncodeToPNG();
        File.WriteAllBytes(Application.streamingAssetsPath + "/Configuration/Noise.png", data);
    }

    private static void Vs(string sFileName, Vector3 v)
    {
        Debug.Log("Writting " + sFileName);
        Vector3[,] vs1 = GetConfigurationConst(v);
        Texture2D tx1 = WriteConfigurationImage(vs1);
        byte[] data = tx1.EncodeToPNG();
        File.WriteAllBytes(Application.streamingAssetsPath + "/Configuration/" + sFileName, data);
    }

    private static void Skyrmions(string sFileName, float fR, float fGamma, int iM, int iG, int iX, int iY)
    {
        Debug.Log("Writting " + sFileName);
        Vector3[,] vs1 = GetConfigurationOneSkyrmion(fR, fGamma, iM, iG, iX, iY);
        Texture2D tx1 = WriteConfigurationImage(vs1);
        byte[] data = tx1.EncodeToPNG();
        File.WriteAllBytes(Application.streamingAssetsPath + "/Configuration/" + sFileName, data);
    }
}
