using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;
using Random = UnityEngine.Random;

public class CreateDefaultMask
{
    [MenuItem("Skyrmion/CreateDefaultMask")]
    static void Test()
    {

        string[] files1 = { "Gaussian30.png", "Gaussian50.png", "Gaussian80.png" };
        string[] files2 = { "Point10.png", "Point20.png", "Point30.png" };
        string[] files3 = { "Wall10.png", "Wall20.png" };
        string[] files4 = { "Wall30.png", "Wall50.png", "Wall80.png" };
        string[] files5 = { "Saddle30.png", "Saddle50.png", "Saddle80.png" };
        float[][] fRs =
        {
            new float[] {30.0f, 50.0f, 80.0f},
            new float[] {10.0f, 20.0f, 30.0f},
            new float[] {10.0f, 20.0f},
            new float[] {30.0f, 50.0f, 80.0f},
            new float[] {30.0f, 50.0f, 80.0f},
        };

        for (int i = 0; i < 3; ++i)
        {
            Gaussians(files1[i], fRs[0][i], 127, 127);
        }
        for (int i = 0; i < 3; ++i)
        {
            Points(files2[i], fRs[1][i], 127, 127);
        }
        for (int i = 0; i < 2; ++i)
        {
            HardWalls(files3[i], fRs[2][i], 127);
        }
        for (int i = 0; i < 3; ++i)
        {
            Walls(files4[i], fRs[3][i], 127);
        }
        for (int i = 0; i < 3; ++i)
        {
            Saddles(files5[i], fRs[4][i], 127);
        }

        Debug.Log("-- Finished ---");
    }

    private static float[,] GetGaussian(float radius, int iX, int iY)
    {
        float[,] ret = new float[256, 256];

        for (int i = 0; i < 256; ++i)
        {
            for (int j = 0; j < 256; ++j)
            {
                float fSq = (i - iX) * (i - iX) + (j - iY) * (j - iY);
                ret[i, j] = Mathf.Exp(-fSq / (radius * radius));
            }
        }
        return ret;
    }

    private static float[,] GetHardPoint(float radius, int iX, int iY)
    {
        float[,] ret = new float[256, 256];

        for (int i = 0; i < 256; ++i)
        {
            for (int j = 0; j < 256; ++j)
            {
                float fSq = (i - iX) * (i - iX) + (j - iY) * (j - iY);
                if (fSq < radius * radius)
                {
                    ret[i, j] = 1.0f;
                }
                else
                {
                    ret[i, j] = 0.0f;
                }
            }
        }
        return ret;
    }

    private static float[,] GetWall(float radius, int iY)
    {
        float[,] ret = new float[256, 256];

        for (int i = 0; i < 256; ++i)
        {
            for (int j = 0; j < 256; ++j)
            {
                float fSq = (j - iY) * (j - iY);
                ret[i, j] = Mathf.Exp(-fSq / (radius * radius));
            }
        }
        return ret;
    }

    private static float[,] GetWallHard(float radius, int iY)
    {
        float[,] ret = new float[256, 256];

        for (int i = 0; i < 256; ++i)
        {
            for (int j = 0; j < 256; ++j)
            {
                float fSq = (j - iY) * (j - iY);
                if (fSq < radius * radius)
                {
                    ret[i, j] = 1.0f;
                }
                else
                {
                    ret[i, j] = 0.0f;
                }
            }
        }
        return ret;
    }

    private static float[,] GetSaddle(float radius, int iY)
    {
        float[,] ret = new float[256, 256];

        for (int i = 0; i < 256; ++i)
        {
            for (int j = 0; j < 256; ++j)
            {
                float fSq = (j - iY) * (i - iY);
                ret[i, j] = Mathf.Exp(-fSq / (radius * radius));
            }
        }
        return ret;
    }

    private static Texture2D WriteConfigurationImage(float[,] vs)
    {
        Texture2D ret = new Texture2D(256, 256, TextureFormat.RGBAFloat, false);

        for (int i = 0; i < 256; ++i)
        {
            for (int j = 0; j < 256; ++j)
            {
                float fV = Mathf.Clamp01(vs[i, j]);
                ret.SetPixel(i, j, new Color(fV, fV, fV));
            }
        }
        ret.Apply(false);
        return ret;
    }

    private static void Gaussians(string sFileName, float fRadius, int iX, int iY)
    {
        Debug.Log("Writting " + sFileName);
        float[,] vs1 = GetGaussian(fRadius, iX, iY);
        Texture2D tx1 = WriteConfigurationImage(vs1);
        byte[] data = tx1.EncodeToPNG();
        File.WriteAllBytes(Application.streamingAssetsPath + "/Mask/" + sFileName, data);
    }

    private static void Points(string sFileName, float fRadius, int iX, int iY)
    {
        Debug.Log("Writting " + sFileName);
        float[,] vs1 = GetHardPoint(fRadius, iX, iY);
        Texture2D tx1 = WriteConfigurationImage(vs1);
        byte[] data = tx1.EncodeToPNG();
        File.WriteAllBytes(Application.streamingAssetsPath + "/Mask/" + sFileName, data);
    }

    private static void Walls(string sFileName, float fRadius, int iY)
    {
        Debug.Log("Writting " + sFileName);
        float[,] vs1 = GetWall(fRadius, iY);
        Texture2D tx1 = WriteConfigurationImage(vs1);
        byte[] data = tx1.EncodeToPNG();
        File.WriteAllBytes(Application.streamingAssetsPath + "/Mask/" + sFileName, data);
    }

    private static void HardWalls(string sFileName, float fRadius, int iY)
    {
        Debug.Log("Writting " + sFileName);
        float[,] vs1 = GetWallHard(fRadius, iY);
        Texture2D tx1 = WriteConfigurationImage(vs1);
        byte[] data = tx1.EncodeToPNG();
        File.WriteAllBytes(Application.streamingAssetsPath + "/Mask/" + sFileName, data);
    }

    private static void Saddles(string sFileName, float fRadius, int iY)
    {
        Debug.Log("Writting " + sFileName);
        float[,] vs1 = GetSaddle(fRadius, iY);
        Texture2D tx1 = WriteConfigurationImage(vs1);
        byte[] data = tx1.EncodeToPNG();
        File.WriteAllBytes(Application.streamingAssetsPath + "/Mask/" + sFileName, data);
    }
}
