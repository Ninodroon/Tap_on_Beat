using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class CriSampleBuildPostProcess
{
    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        AddIosFrameworkProcess(target, pathToBuiltProject);
        AddIosPrivacyUsageProcess(target, pathToBuiltProject);
    }

    private static void AddIosFrameworkProcess(BuildTarget target, string pathToBuiltProject)
    {
#if UNITY_IOS
        var pbxProject = new UnityEditor.iOS.Xcode.PBXProject();
        string projectFilePath = pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj";
        pbxProject.ReadFromString(File.ReadAllText(projectFilePath));

        string targetGuidName;
#if UNITY_2019_3_OR_NEWER
        targetGuidName = pbxProject.GetUnityFrameworkTargetGuid();
#else
        targetGuidName = pbxProject.TargetGuidByName("Unity-iPhone");
#endif //UNITY_2019_3_OR_NEWER
        Debug.Assert(!string.IsNullOrEmpty(targetGuidName));
        {
            pbxProject.AddFrameworkToProject(targetGuidName, "MediaPlayer.framework", false);
            Debug.Log("[CRIWARE][iOS] Add dependency frameworks (MediaPlayer) for sample application.");
        }

        File.WriteAllText(projectFilePath, pbxProject.WriteToString());
#else
        if (target != BuildTarget.iOS) {
            return;
        }
        Debug.LogWarning("[CRIWARE][iOS] Please add dependency frameworks (MediaPlayer.framework) on Xcode.");
#endif //UNITY_IOS
    }

    private static void AddIosPrivacyUsageProcess(BuildTarget target, string pathToBuiltProject)
    {
#if UNITY_IOS
        string appleMusicDescription = "NSAppleMusicUsageDescription";
        var filePath = pathToBuiltProject + "/info.plist";
        var infoPlist = new UnityEditor.iOS.Xcode.PlistDocument();
        infoPlist.ReadFromFile(filePath);
        infoPlist.root.SetString(appleMusicDescription, "necessary for demostrating the playback with other audio.");
        infoPlist.WriteToFile(filePath);
#else
        if (target != BuildTarget.iOS) {
            return;
        }
        Debug.LogWarning("[CRIWARE][iOS] Please add privacy usage description () on Xcode.");
#endif //UNITY_IOS
    }
}
