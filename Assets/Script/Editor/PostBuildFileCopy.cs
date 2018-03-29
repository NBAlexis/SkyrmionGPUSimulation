using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class PostBuildFileCopy
{

    [PostProcessBuildAttribute(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        pathToBuiltProject = pathToBuiltProject.Replace("\\", "/");
        string sFolder = pathToBuiltProject.Substring(0, pathToBuiltProject.LastIndexOf("/") + 1);
        DirectoryInfo targetDir = new DirectoryInfo(sFolder);
        targetDir.CreateSubdirectory("LuaScript");
        foreach (FileInfo f in new DirectoryInfo(Application.dataPath + "/LuaScript").GetFiles("*.lua"))
        {
            File.Copy(f.FullName, sFolder + "/LuaScript/" + f.Name);
        }
        targetDir.CreateSubdirectory("BoundConditions");
        foreach (FileInfo f in new DirectoryInfo(Application.dataPath + "/BoundConditions").GetFiles("*.png"))
        {
            File.Copy(f.FullName, sFolder + "/BoundConditions/" + f.Name);
        }
        targetDir.CreateSubdirectory("Output");
    }
}
