using UnityEngine;
using System.Collections;
using System.IO;

namespace WG.UnityPods
{
    public class PathBuilder
    {
        public static string podmetaFolder = Path.Combine("Assets", "unitypods_meta");
        
        static public string GetMetaFileName(string libname)
        {
            return libname + "_meta";
        }

        //the remote folder name, cointaining the meta information for the package (this folder is only created on the ftp, locally the meta files are in the podmetaFolder
        public static string GetFolderMetaName(string packagename, Version version)
        {
            return packagename + "_" + version;
        }

        public static string GetPackageName(string libName, Version version)
        {
            return GetFolderMetaName(libName, version) + ".unitypackage";
        }

        public static string GetPackagePath(string libName, Version version)
        {
            string packageName = PathBuilder.GetPackageName(libName, version);
            return Path.Combine(libName, packageName);
        }

        static public string GetMetaFilePath(string libname)
        {
            return Path.Combine(podmetaFolder, GetMetaFileName(libname));
        }

        static public string GetPackageUrl(string libName, Version version)
        {
            return FtpHelper.ftpUrl + libName + "/" + PathBuilder.GetPackageName(libName, version);
        }

        static public string GetLibDirectoryUrl(string libName)
        {
            return FtpHelper.ftpUrl + libName;
        }

        //Gets the directory where the meta file is on the remote server
        static public string GetMetaDirectoryUrl(string libName, Version version)
        {
            return FtpHelper.ftpUrl + libName + "/" + PathBuilder.GetFolderMetaName(libName, version);
        }

        static public string GetMetaFileUrl(string libName, Version version)
        {
            return GetMetaDirectoryUrl(libName, version) + "/" + PathBuilder.GetMetaFileName(libName);
        }
    }
}