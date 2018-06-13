using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class TestShader
{

	
	[MenuItem("Skyrmion/TestCoordinate")]
	static void Test()
    {
        Debug.Log("Test:");

        Texture2D test2D = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        test2D.LoadImage(File.ReadAllBytes(Application.dataPath + "/Resources/test.png"), false);

        Debug.Log(test2D.GetPixel(0, 0)); //red
        Debug.Log(test2D.GetPixel(1, 0)); //blue
        Debug.Log(test2D.GetPixel(0, 1)); //red
        Debug.Log(test2D.GetPixel(1, 1)); //green

        Material testMat = Resources.Load<Material>("testshader");

        RenderTexture rtshow = new RenderTexture(2, 2, 0, RenderTextureFormat.ARGB32);
        testMat.SetTexture("_Nx", test2D);
        Graphics.Blit(null, rtshow, testMat);
        Texture2D imgShow = new Texture2D(2, 2, TextureFormat.ARGB32, false);
        RenderTexture.active = rtshow;
        imgShow.ReadPixels(new Rect(0, 0, 2, 2), 0, 0);
        imgShow.Apply();
        RenderTexture.active = null;
        rtshow.Release();

        byte[] byDataRaw = imgShow.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/Resources/testoutput.png", byDataRaw);

        ComputeShader calcShader = Resources.Load<ComputeShader>("LLG_TestCoordinate");
        RenderTexture rtCalc = new RenderTexture(2, 2, 0, RenderTextureFormat.ARGB32);
        rtCalc.enableRandomWrite = true;
        rtCalc.Create();
        Graphics.Blit(test2D, rtCalc);
        int iKernelHandle = calcShader.FindKernel("CSMain");
        calcShader.SetTexture(iKernelHandle, "testInOut", rtCalc);
        calcShader.Dispatch(iKernelHandle, 1, 1, 1);

        RenderTexture rtshow2 = new RenderTexture(2, 2, 0, RenderTextureFormat.ARGB32);
        testMat.SetTexture("_Nx", rtCalc);
        Graphics.Blit(null, rtshow2, testMat);
        Texture2D imgShow2 = new Texture2D(2, 2, TextureFormat.ARGB32, false);
        RenderTexture.active = rtshow2;
        imgShow2.ReadPixels(new Rect(0, 0, 2, 2), 0, 0);
        imgShow2.Apply();
        RenderTexture.active = null;
        rtshow2.Release();

        byte[] byDataRaw2 = imgShow2.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/Resources/testcalc.png", byDataRaw2);
    }
}
