using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WG.UnityPods
{
    public class UpdateInstallWindow : EditorWindow
    {
        static bool includePrerelease = false;
        bool needRefresh = false;
        HashSet<string> toBeDeletedPackage = new HashSet<string>();
        List<string> remoteUnityPodNames;
        int podsSelectedIndex;
        PodDescription podDescription;
        UnityPodSpec unityPodSpec;

        [MenuItem("Wooga/Pods/Package Manager")]
        static void Init()
        {
            UpdateInstallWindow window = (UpdateInstallWindow)EditorWindow.GetWindow(typeof(UpdateInstallWindow));
            window.title = "Package Manager";
            window.minSize = new Vector2(350, 400);
        }

        void OnEnable()
        {
            try
            {
                unityPodSpec = new UnityPodspecParser().Parse();
            } catch (System.IO.FileLoadException)
            {
                unityPodSpec = null;
            }

            this.Refresh();
            this.FetchRemotePodNames();
        }

        void FetchRemotePodNames()
        {
            remoteUnityPodNames = FtpHelper.FetchPodsList();
            remoteUnityPodNames.Insert(0, "Select");
            this.podsSelectedIndex = 0;
            this.podDescription = null;
        }

        void OnGUI()
        {
            GUILayout.BeginVertical();
            
            GUILayout.Label("Available unity Pods");
    
            includePrerelease = EditorGUILayout.Toggle("Include prereleases", includePrerelease);

            DrawSelectionBar();

            DrawPodDescription();

            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");

            foreach (PodPackage package in PackageManager.packages)
            {
                // dirty way to draw a seperator //
                GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
                // draw the content //
                DrawPackageLine(package, 0);
            }

            GUILayout.EndVertical();

            GUILayout.BeginVertical();

            if (GUILayout.Button("Refresh"))
            {
                this.Refresh();
            }
            
            if (toBeDeletedPackage.Count == 0)
                GUI.enabled = false;

            if (GUILayout.Button("Delete selected"))
            {
                CommitChanges();
            }

            GUI.enabled = true;

            GUILayout.EndVertical();
        }

        void DrawPodDescription()
        {       
            if (podDescription != null)
            {
                EditorGUILayout.HelpBox(podDescription.ToString().Replace(':', '\n'), MessageType.Info, true);
            }
        }

        void DrawPackageLine(PodPackage package, int indentLevel)
        {
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.contentOffset = Vector2.zero;
            boxStyle.padding = new RectOffset();

            GUILayout.BeginHorizontal();
            
            string prependString = GetPrependStringForIndentLevel(indentLevel);

            string versionString;

            bool isMarkedForDeletion = toBeDeletedPackage.Contains(package.packagename);
            GUIStyle labelStyle = (isMarkedForDeletion) ? new GUIStyle(EditorStyles.boldLabel) : new GUIStyle(EditorStyles.label);
            labelStyle.contentOffset = Vector2.zero;
            labelStyle.fixedWidth = 200f;
            labelStyle.margin = new RectOffset(5, 0, 5, 5);
            labelStyle.normal.textColor = isMarkedForDeletion ? Color.red : Color.black;

            if (package.version != Version.VersionZero())
                versionString = "v" + package.version.ToString();
            else 
                versionString = "";

            GUILayout.Label(prependString + package.packagename + " " + versionString, labelStyle);

            if (package.IsNotInstalled())
            {
                DrawInstallButton(package);
                DrawDeleteButton(package);
            } else if (package.NeedsUpdate(includePrerelease))
            {
                DrawUpdateButton(package);
                DrawDeleteButton(package);
            } else
            {
                DrawInstallButton(package);
                DrawDeleteButton(package);
            }
        
            GUILayout.EndHorizontal();

            if (package.dependencies != null)
            {
                foreach (PodPackage depPack in package.dependencies)
                {
                    DrawPackageLine(depPack, indentLevel + 1);
                }
            }
        }
            
        Texture2D GetIcon(string name)
        {
            MonoScript thisScript = MonoScript.FromScriptableObject(this);
            string thisPath = AssetDatabase.GetAssetPath(thisScript);
            string iconPath = Path.GetDirectoryName(thisPath) + "/icons/" + name;

            return (Texture2D)Resources.LoadAssetAtPath(iconPath, typeof(Texture2D));
        }

        void DrawSelectionBar()
        {
            GUI.enabled = (remoteUnityPodNames != null && remoteUnityPodNames.Count > 1);           

            GUILayout.BeginHorizontal();

            DrawAddButton();

            if (remoteUnityPodNames != null)
            {
                GUIStyle popupStyle = new GUIStyle(EditorStyles.popup);
                popupStyle.contentOffset = Vector2.zero;
                popupStyle.fixedHeight = 20f;
                popupStyle.padding = new RectOffset(5, 0, 0, 0);

                List<string> filteredList = GetRemotePodsExcludingLocalInstalled();

                int selectionIndex = EditorGUILayout.Popup(this.podsSelectedIndex, filteredList.ToArray(), popupStyle);
                
                if (selectionIndex != 0 && selectionIndex != this.podsSelectedIndex)
                {
                    this.podDescription = PodDescription.GetDescriptionForPod(filteredList [selectionIndex]);
                    
                    this.podsSelectedIndex = selectionIndex; 
                }
            }

            GUI.enabled = true;

            GUILayout.EndHorizontal();
        }

        List<string> GetRemotePodsExcludingLocalInstalled()
        {
            List<string> localInstalledPackageNames = PackageManager.packages.Select(package => package.packagename).ToList();
            return remoteUnityPodNames.Except(localInstalledPackageNames).ToList();
        }

        void DrawAddButton()
        {
            GUI.enabled = this.podsSelectedIndex != 0;

            GUIStyle buttonStyle = new GUIStyle(EditorStyles.miniButton);
            buttonStyle.contentOffset = Vector2.zero;
            buttonStyle.fixedHeight = 20f;
            buttonStyle.fixedWidth = 20f;
            buttonStyle.padding = new RectOffset();
            
            GUIContent buttonContent = new GUIContent();
            buttonContent.image = this.GetIcon("add_icon.png");
            buttonContent.tooltip = "Add selected library";

            if (GUILayout.Button(buttonContent, buttonStyle))
            {
                UnityPodFile.AddDependency(podDescription.name);

                this.podDescription = null;
                this.podsSelectedIndex = 0;
                Refresh();
            }
            
            GUI.enabled = true;
        }

        void DrawUpdateButton(PodPackage package)
        {
            GUI.enabled = package.NeedsUpdate(includePrerelease);

            GUIStyle buttonStyle = new GUIStyle(EditorStyles.miniButtonLeft);
            buttonStyle.contentOffset = Vector2.zero;
            buttonStyle.fixedHeight = 20f;
            buttonStyle.padding = new RectOffset();

            GUIContent buttonContent = new GUIContent();
            buttonContent.image = this.GetIcon("download_icon.png");
            buttonContent.text = "Update";
            buttonContent.tooltip = "Update to the new available version '" + package.latestVersionAvailable + "'";
            
            if (GUILayout.Button(buttonContent, buttonStyle))
            {
                UnityPodsImporter.ImportPackage(package.packagename, package.latestVersionAvailable);
                needRefresh = true;
            }
            
            GUI.enabled = true;
        }

        void DrawInstallButton(PodPackage package)
        {
            GUI.enabled = package.IsNotInstalled();
            
            GUIStyle buttonStyle = new GUIStyle(EditorStyles.miniButtonLeft);
            buttonStyle.contentOffset = Vector2.zero;
            buttonStyle.fixedHeight = 20f;
            buttonStyle.padding = new RectOffset();
            
            GUIContent buttonContent = new GUIContent();
            buttonContent.image = this.GetIcon("install_icon.png");
            buttonContent.text = "Install";
            buttonContent.tooltip = "Install";

            if (GUILayout.Button(buttonContent, buttonStyle))
            {
                UnityPodsImporter.ImportPackage(package.packagename, package.latestVersionAvailable);
                needRefresh = true;
            }
            
            GUI.enabled = true;
        }

        void DrawDeleteButton(PodPackage package)
        {
            if (unityPodSpec != null)
                GUI.enabled = package.packagename != unityPodSpec.libraryName;

            GUIStyle buttonStyle = new GUIStyle(EditorStyles.miniButtonRight);
            buttonStyle.contentOffset = Vector2.zero;
            buttonStyle.fixedHeight = 20f;
            buttonStyle.padding = new RectOffset();

            bool isMarkForDeleting = toBeDeletedPackage.Contains(package.packagename);

            GUIContent buttonContent = new GUIContent();
            buttonContent.image = isMarkForDeleting ? this.GetIcon("revert_icon.png") : this.GetIcon("trashbin_icon.png");
            buttonContent.text = isMarkForDeleting ? "Undo  " : "Delete";
            buttonContent.tooltip = "Delete";

            if (GUILayout.Button(buttonContent, buttonStyle))
            {
                DeletePackage(package);
            }

            GUI.enabled = true;
        }

        void DeletePackage(PodPackage package)
        {
            if (!toBeDeletedPackage.Contains(package.packagename))
            {
                MarkPackageToBeDeleted(package);
            } else
            {
                toBeDeletedPackage.Remove(package.packagename);
            }
        }

        void CommitChanges()
        {
            foreach (string podPackageName in toBeDeletedPackage)
            {
                PackageUninstaller.UninstallPackage(podPackageName);
                UnityPodFile.RemoveDependency(podPackageName);
            }

            toBeDeletedPackage.Clear();
            Refresh();
        }

        //Mark a package to be deleted and all his dependencies
        void MarkPackageToBeDeleted(PodPackage package)
        {
            toBeDeletedPackage.Add(package.packagename);

            foreach (PodPackage podPackage in package.dependencies)
            {
                MarkPackageToBeDeleted(podPackage);
            }
        }

        string GetPrependStringForIndentLevel(int indentLevel)
        {
            string prependString = "";
            for (int i = 0; i < indentLevel; i++)
            {
                if (i == 0)
                    prependString = " |";
                
                prependString += "-";
                
                if (i == indentLevel - 1)
                    prependString += "";
            }
            return prependString;
        }

        void OnFocus()
        {
            if (needRefresh)
            {
                this.Refresh();
                needRefresh = false;
            }
        }

        void Refresh()
        {
            PackageManager.Refresh();
        }
    }
}   