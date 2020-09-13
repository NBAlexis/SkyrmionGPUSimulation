using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;

public class CreateDefaultBoundary 
{
    [MenuItem("Skyrmion/CreateDefaultBoundary")]
    static void Test()
    {
        Open("Open.png");
        Periodic("Periodic.png");

        string[] files1 = { "Defect1.png", "Defect5.png", "Defect30.png" };
        string[] files2 = { "OpenDefect1.png", "OpenDefect5.png", "OpenDefect30.png" };

        float[][] fRs =
        {
            new float[] {0.01f, 0.05f, 0.3f},
        };

        for (int i = 0; i < 3; ++i)
        {
            Defects(files1[i], fRs[0][i]);
        }
        for (int i = 0; i < 3; ++i)
        {
            OpenDefects(files2[i], fRs[0][i]);
        }

        Debug.Log("-- Finished ---");
    }

    private static float[,] Open()
    {
        float[,] ret = new float[256, 256];

        for (int i = 0; i < 256; ++i)
        {
            for (int j = 0; j < 256; ++j)
            {
                ret[i, j] = 1.0f;
                if (0 == i || 0 == j || 255 == i || 255 == j)
                {
                    ret[i, j] = 0.0f;
                }
            }
        }
        return ret;
    }

    private static float[,] Defect(float odds)
    {
        float[,] ret = new float[256, 256];

        for (int i = 0; i < 256; ++i)
        {
            for (int j = 0; j < 256; ++j)
            {
                ret[i, j] = Random.Range(0.0f, 1.0f) > odds ? 1.0f : 0.0f;
            }
        }
        return ret;
    }

    private static float[,] OpenDefect(float odds)
    {
        float[,] ret = new float[256, 256];

        for (int i = 0; i < 256; ++i)
        {
            for (int j = 0; j < 256; ++j)
            {
                ret[i, j] = Random.Range(0.0f, 1.0f) > odds ? 1.0f : 0.0f;
                if (0 == i || 0 == j || 255 == i || 255 == j)
                {
                    ret[i, j] = 0.0f;
                }
            }
        }
        return ret;
    }

    private static float[,] Periodic()
    {
        float[,] ret = new float[256, 256];

        for (int i = 0; i < 256; ++i)
        {
            for (int j = 0; j < 256; ++j)
            {
                ret[i, j] = 1.0f;
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

    private static void Open(string sFileName)
    {
        Debug.Log("Writting " + sFileName);
        float[,] vs1 = Open();
        Texture2D tx1 = WriteConfigurationImage(vs1);
        byte[] data = tx1.EncodeToPNG();
        File.WriteAllBytes(Application.streamingAssetsPath + "/Boundary/" + sFileName, data);
    }

    private static void Periodic(string sFileName)
    {
        Debug.Log("Writting " + sFileName);
        float[,] vs1 = Periodic();
        Texture2D tx1 = WriteConfigurationImage(vs1);
        byte[] data = tx1.EncodeToPNG();
        File.WriteAllBytes(Application.streamingAssetsPath + "/Boundary/" + sFileName, data);
    }

    private static void Defects(string sFileName, float fOdds)
    {
        Debug.Log("Writting " + sFileName);
        float[,] vs1 = Defect(fOdds);
        Texture2D tx1 = WriteConfigurationImage(vs1);
        byte[] data = tx1.EncodeToPNG();
        File.WriteAllBytes(Application.streamingAssetsPath + "/Boundary/" + sFileName, data);
    }

    private static void OpenDefects(string sFileName, float fOdds)
    {
        Debug.Log("Writting " + sFileName);
        float[,] vs1 = OpenDefect(fOdds);
        Texture2D tx1 = WriteConfigurationImage(vs1);
        byte[] data = tx1.EncodeToPNG();
        File.WriteAllBytes(Application.streamingAssetsPath + "/Boundary/" + sFileName, data);
    }
}
