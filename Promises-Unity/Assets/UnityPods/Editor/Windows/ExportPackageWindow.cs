using UnityEngine;
using UnityEditor;
using System.Collections;

namespace WG.UnityPods
{
    public class ExportPackageWindow : EditorWindow
    {
        private static UnityPodSpec podSpec;

        [MenuItem ("Wooga/Pods/Export Package")]
        static void Init()
        {
            ExportPackageWindow window = (ExportPackageWindow)EditorWindow.GetWindow(typeof(ExportPackageWindow));
            window.title = "Export Package";
            window.minSize = new Vector2(350, 400);
        }

        void OnEnable()
        {
            if (UnityPodspecParser.HasFileOnDisk())
            {
                podSpec = new UnityPodspecParser().Parse();
            } else
            {
                podSpec = new UnityPodSpec("", Version.VersionZero(), "Library Description here", "");
            }
        }

        void OnGUI()
        {
            GUILayout.Label("Your Library ");

            string newLibName = EditorGUILayout.TextField("Library Name", podSpec.libraryName);

            if (!newLibName.Contains(" "))
            {
                podSpec.libraryName = newLibName;
            }

            string versionString = EditorGUILayout.TextField("Version", podSpec.version.VersionStringWithoutPrerelease());

            if (Version.ValidateVersionString(versionString))
            {
                podSpec.version.SetVersionFromString(versionString);
            }

            podSpec.version.releaseType = (VersionReleaseType)EditorGUILayout.EnumPopup("Release Type", podSpec.version.releaseType);
            if (podSpec.version.IsPrerelease())
            {
                podSpec.version.preReleaseNumber = EditorGUILayout.IntField("prerelease #", podSpec.version.preReleaseNumber);
            }
            EditorGUILayout.LabelField("Package Version", podSpec.version.ToString());

            podSpec.description = EditorGUILayout.TextArea(podSpec.description, GUILayout.Height(50f));
            podSpec.author = EditorGUILayout.TextField("Authors", podSpec.author);


            if (podSpec.libraryName == "" ||
                podSpec.author == "" ||
                podSpec.version == Version.VersionZero())
            {
                GUI.enabled = false;
            }

            if (GUILayout.Button("Export"))
            {
                podSpec.SavePodSpecOnDisk();
                UnityPodsExport.ExportPackage();
            }

            GUI.enabled = true;
        }
    }
}
