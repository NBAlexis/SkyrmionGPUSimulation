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

        DirectoryInfo targetDir1 = new DirectoryInfo(sFolder + "/LuaScript");
        DirectoryInfo targetDir2 = new DirectoryInfo(sFolder + "/BoundConditions");
        DirectoryInfo targetDir3 = new DirectoryInfo(sFolder + "/Doc");
        DirectoryInfo targetDir4 = new DirectoryInfo(sFolder + "/Output");

        if (targetDir1.Exists)
        {
            targetDir1.Delete(true);
        }
        if (targetDir2.Exists)
        {
            targetDir2.Delete(true);
        }
        if (targetDir3.Exists)
        {
            targetDir3.Delete(true);
        }
        if (targetDir4.Exists)
        {
            targetDir4.Delete(true);
        }

        targetDir.CreateSubdirectory("LuaScript");
        foreach (FileInfo f in new DirectoryInfo(Application.dataPath + "/LuaScript").GetFiles("*.lua"))
        {
            if (!f.FullName.ToLower().Contains("testluacall.lua"))
            {
                File.Copy(f.FullName, sFolder + "/LuaScript/" + f.Name);
            }
        }
        targetDir.CreateSubdirectory("BoundConditions");
        foreach (FileInfo f in new DirectoryInfo(Application.dataPath + "/BoundConditions").GetFiles("*.png"))
        {
            File.Copy(f.FullName, sFolder + "/BoundConditions/" + f.Name);
        }
        targetDir.CreateSubdirectory("Doc");
        foreach (FileInfo f in new DirectoryInfo(Application.dataPath + "/Doc").GetFiles("*.pdf"))
        {
            if (!f.FullName.Contains("rk4test")
             && !f.FullName.Contains("eps-converted"))
            {
                File.Copy(f.FullName, sFolder + "/Doc/" + f.Name);
            }
        }
        targetDir.CreateSubdirectory("Output");
        File.Copy(Application.dataPath + "/Output/.gitignore", sFolder + "/Output/.gitignore");
    }
}
