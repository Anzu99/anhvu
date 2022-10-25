// filename BuildPostProcessor.cs
// put it in a folder Assets/Editor/
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
#if UNITY_IOS || UNITY_IPHONE
using UnityEditor.iOS.Xcode;
#endif

#if UNITY_IOS || UNITY_IPHONE
public class BuildPostProcessor {

    [PostProcessBuild]
    public static void ChangeXcodePlist(BuildTarget buildTarget, string path) {
        if (buildTarget == BuildTarget.iOS) {

            string plistPath = path + "/Info.plist";
            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(plistPath);

            PlistElementDict rootDict = plist.root;

            // example of changing a value:
            // rootDict.SetString("CFBundleVersion", "6.6.6");

            // example of adding a boolean key...
            // < key > ITSAppUsesNonExemptEncryption </ key > < false />
            rootDict.SetString("GADApplicationIdentifier", "ca-app-pub-2777953690987264~6759076705");
            rootDict.SetString("NSUserTrackingUsageDescription", "This identifier will be used to deliver personalized ads to you.");
            #if ENABLE_ADS_IRON
            PlistElementArray arr = rootDict.CreateArray("SKAdNetworkItems");
            string[] dicnet = {
                "su67r6k2v3.skadnetwork"//iron
                , "4pfyvq9l8r.skadnetwork" //adcolony
                , "cstr6suwn9.skadnetwork"//admob
                , "ludvb6z3bs.skadnetwork"//applovin
                // , "f38h382jlk.skadnetwork"//chartboost
                , "v9wttpbfk9.skadnetwork"//fb
                , "n38lu8286q.skadnetwork"//fb
                // ,"9g2aggbj52.skadnetwork"//Flyber
                // ,"nu4557a4je.skadnetwork"//HyprMx
                // ,"wzmmz9fp6w.skadnetwork"//inMobi
                // ,"v4nxqhlyqp.skadnetwork"//Maio
                // , "V4NXQHLYQP.skadnetwork"//Maio
                , "22mmun2rn5.skadnetwork"//Pangle non cn
                , "238da6jt44.skadnetwork"//Pangle cn
                // ,"424m5254lk.skadnetwork"//Snap
                // ,"ecpz2srf59.skadnetwork"//Tapjoy
                , "4DZT52R2T5.skadnetwork"//Unity
                //, "GTA9LK7P23.skadnetwork"//Vungle
                };
            for (int i = 0; i < dicnet.Length; i++)
            {
                PlistElementDict dicarr = arr.AddDict();
                dicarr.SetString("SKAdNetworkIdentifier", dicnet[i]);
            }
            #endif

            File.WriteAllText(plistPath, plist.WriteToString());

        }
    }

    [PostProcessBuildAttribute(1)]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string buildPath)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            // We need to construct our own PBX project path that corrently refers to the Bridging header
            // var projPath = PBXProject.GetPBXProjectPath(buildPath);
            var projPath = buildPath + "/Unity-iPhone.xcodeproj/project.pbxproj";
            var proj = new PBXProject();
            proj.ReadFromFile(projPath);

            //var targetGuid = proj.GetUnityFrameworkTargetGuid();
            var targetGuid = proj.GetUnityMainTargetGuid();

            //// Configure build settings
            proj.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");
            //proj.SetBuildProperty(targetGuid, "SWIFT_OBJC_BRIDGING_HEADER", "Libraries/Plugins/iOS/MyUnityPlugin/Source/MyUnityPlugin-Bridging-Header.h");
            //proj.SetBuildProperty(targetGuid, "SWIFT_OBJC_INTERFACE_HEADER_NAME", "MyUnityPlugin-Swift.h");
            //proj.AddBuildProperty(targetGuid, "LD_RUNPATH_SEARCH_PATHS", "@executable_path/Frameworks");

            proj.WriteToFile(projPath);
        }
    }
}
#endif
