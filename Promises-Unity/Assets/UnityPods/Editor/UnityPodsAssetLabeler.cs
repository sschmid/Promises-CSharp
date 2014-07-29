using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace WG.UnityPods
{
    /*
     * When we export a lib, we want to label every asset with a lib name. In such way all its dependency that were fetched via unitypods will have their library names, so its possible to strip out other dependency before building the exported package for the current lib.
     * We only export files relative to the library and not its dependency. in the _meta file we write the required dependency packages as meta information, so the user will be able to install them though unitypods.
     * This class will label all files that are not already labeled as part of another library and build a fileToInclude list that is used by the exported.
     */
    static public class UnityPodsAssetLabeler
    {
        static string labelPrefix = "wgpod_";

        //files that need to be included in the export, only files that belong to this very lib and not dependency
        private static List<string> fileToInclude = new List<string>();

        //everything that has not a wgpod_* label on it, label it with the name of the current podspec and return the list of files that were labeled
		public static List<string> LabelizeWithName(string labelName)
        {
            fileToInclude.Clear();
            DirectoryInfo directory = new DirectoryInfo("Assets");
			LabelFilesInDirectory(directory, labelName);
            return fileToInclude;
        }

		static void LabelFilesInDirectory(DirectoryInfo directory, string labelName)
        {
            FileInfo[] fi = directory.GetFiles();
			string assetLabelToApply = labelPrefix + labelName;

            foreach (var file in fi)
            {
                //some files to ignore and we dont export
                if (file.Name.EndsWith(".meta") || 
                    file.Name.EndsWith(".DS_Store") ||
                    file.Name == "UnityPodspec" ||
                    file.Name == "UnityPodfile")
                    continue;

                //from c# to unity path
                string unityPath = file.FullName.Substring(file.FullName.LastIndexOf(@"Assets/"));
                Object objAsset = AssetDatabase.LoadAssetAtPath(unityPath, typeof(Object));

                if (objAsset == null)
                {
                    Debug.Log("file not imported in unity? will not be added to the exported package. file: " + unityPath);
                    continue;
                }

                string[] assetLabels = AssetDatabase.GetLabels(objAsset);

                bool podLabelFound = false;
                foreach (string label in assetLabels)
                {
					if (label.StartsWith(labelPrefix))
					{
						podLabelFound = true;
                        break;
                    }
                }    

                //add the wgpod_libname label
                if (!podLabelFound)
                {
                    System.Array.Resize(ref assetLabels, assetLabels.Length + 1);
                    assetLabels [assetLabels.Length - 1] = assetLabelToApply;
                    AssetDatabase.SetLabels(objAsset, assetLabels);
                }

                CheckFileToInclude(unityPath, assetLabels, labelName);
            }

            //recursive look into dir
            DirectoryInfo[] dirs = directory.GetDirectories();
            foreach (var dir in dirs)
            {
				LabelFilesInDirectory(dir, labelName);
            }

            AssetDatabase.Refresh();
        }

        static void CheckFileToInclude(string path, string[] assetLabels, string labelName)
        {
			string assetLabelToApply = labelPrefix + labelName;

            foreach (string label in assetLabels)
            {
                if (label == assetLabelToApply)
                {
                    fileToInclude.Add(path);
                    break;
                }
            }
        }

        //return a list of all file paths that belong to a certain library, they are found by looking at the unity asset label
        public static List<string> FilesThatBelongToLibrary(string libraryName)
        {
            string label = labelPrefix + libraryName;
            List<string> fileList = new List<string>();
            DirectoryInfo di = new DirectoryInfo("Assets");
            AddFilesWithLabel(label, di, fileList);
            return fileList;
        }

        static void AddFilesWithLabel(string label, DirectoryInfo di, List<string> fileList)
        {
            FileInfo[] fi = di.GetFiles();
            foreach (var file in fi)
            {
                //some files to ignore and we dont export
                if (file.Name.EndsWith(".meta") || file.Name.EndsWith(".DS_Store"))
                    continue;
                
                //from c# to unity path
                string unityPath = file.FullName.Substring(file.FullName.LastIndexOf(@"Assets/"));
                Object objAsset = AssetDatabase.LoadAssetAtPath(unityPath, typeof(Object));

                foreach (string l in AssetDatabase.GetLabels(objAsset))
                {
					if (l.ToLower() == label.ToLower())
					{
						fileList.Add(unityPath);
                        break;
                    }
                }    
            }

            //recursive look into dir
            DirectoryInfo[] dirs = di.GetDirectories();
            foreach (var dir in dirs)
            {
                AddFilesWithLabel(label, dir, fileList);
            }
        }
    }
}
