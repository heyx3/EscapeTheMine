using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEditor;


/// <summary>
/// Copies the "Copy To Build" folder (alongside "Assets/") to the standalone build,
///     except for the sub-folder "Copy To Build/Copy To _Data", which gets copied into
///     the _Data folder alongside the standalone build.
/// </summary>
public static class CopyToBuild
{
    [UnityEditor.Callbacks.PostProcessBuild]
    public static void OnBuiltStandalone(BuildTarget target, string exePath)
    {
        string buildPath = Path.GetDirectoryName(exePath),
               exeName = Path.GetFileNameWithoutExtension(exePath),
               dataPath = Path.Combine(buildPath, exeName + "_Data");

        const string buildCopyFolder = "Copy To Build",
                     buildCopyFolder_Data = "Copy To _Data";


        //Copy the contents of the "Copy To Build" folder.
        DirectoryInfo buildCopyDir = new DirectoryInfo(Path.Combine(Application.dataPath,
                                                                    "..\\" + buildCopyFolder));
        foreach (FileInfo file in buildCopyDir.GetFiles())
            file.CopyTo(buildPath, true);
        foreach (DirectoryInfo dir in buildCopyDir.GetDirectories())
            if (dir.Name != buildCopyFolder_Data)
                CopyDir(dir.FullName, buildPath);

        //Copy the contents of the "Copy To Build/Copy To _Data" folder.
        DirectoryInfo buildCopyDir_Data = new DirectoryInfo(Path.Combine(buildCopyDir.FullName,
                                                                         buildCopyFolder_Data));
        foreach (FileInfo file in buildCopyDir_Data.GetFiles())
            file.CopyTo(dataPath);
        foreach (DirectoryInfo dir in buildCopyDir_Data.GetDirectories())
            CopyDir(dir.FullName, dataPath);
    }

    private static void CopyDir(string dirToCopy, string destination)
    {
        DirectoryInfo source = new DirectoryInfo(dirToCopy),
                      dest = new DirectoryInfo(Path.Combine(destination, source.Name));

        if (!dest.Exists)
            dest.Create();

        foreach (FileInfo file in source.GetFiles())
            file.CopyTo(Path.Combine(dest.FullName, file.Name), true);

        foreach (DirectoryInfo subDir in source.GetDirectories())
            CopyDir(subDir.FullName, Path.Combine(dest.FullName, subDir.Name));
    }
}