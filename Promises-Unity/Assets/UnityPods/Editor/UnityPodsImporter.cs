using UnityEngine;
using UnityEditor;
using System.Net;
using System.IO;
using System.Collections.Generic;

namespace WG.UnityPods
{
/*
 * Provides some utility to import a package.
 */
    public class UnityPodsImporter
    {
//        public static void ImportLastVersionForPackage(string packageName)
//        {
//            ImportPackage(packageName, UnityPodsUpdateCheck.LastVersionForLibrary(packageName));
//        }

        public static void ImportPackage(string libName, Version version)
        {
            string filename = PathBuilder.GetPackageName(libName, version);
            
            using (WebClient request = new WebClient())
            {
                request.Credentials = new NetworkCredential(FtpHelper.ftpUserName, FtpHelper.ftpUserPassword);
                byte[] fileData = request.DownloadData(PathBuilder.GetPackageUrl(libName, version));
                
                using (FileStream file = File.Create(filename))
                {
                    file.Write(fileData, 0, fileData.Length);
                    file.Close();
                }
                Debug.Log("Download Complete");
            }
            
            AssetDatabase.ImportPackage(filename, true);
        }
    }
}