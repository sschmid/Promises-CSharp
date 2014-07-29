using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace WG.UnityPods
{
/*
 * This class export the current project as a unitypackage, using meta information found in the UnityPodspec file to an ftp server
 */
    public class UnityPodsExport
    {
        public static void ExportPackage()
        {
            UnityPodspecParser podSpecParser = new UnityPodspecParser();
            UnityPodSpec unityPodSpec = podSpecParser.Parse();

            //update the version in the meta file before exporting the lib
            string metaFilePath = UnityPodsMeta.UpdateMetaFile(unityPodSpec/*libName, version, author, description*/);
            //label all asset that belongs to this library we want to export
            List<string> fileToInclude = UnityPodsAssetLabeler.LabelizeWithName(unityPodSpec.libraryName);

            System.IO.Directory.CreateDirectory(unityPodSpec.libraryName);
            string packagePath = PathBuilder.GetPackagePath(unityPodSpec.libraryName, unityPodSpec.version);

            //only export file labeled to be in this library (not dependencies)
            AssetDatabase.ExportPackage(fileToInclude.ToArray(), packagePath, ExportPackageOptions.Default);
                        
            //create the directory containing all the lib versions
            FtpHelper.CreateFTPDirectory(PathBuilder.GetLibDirectoryUrl(unityPodSpec.libraryName));
            //creating a directory for this specific version to contain the meta
            string metaFtpDirName = PathBuilder.GetMetaDirectoryUrl(unityPodSpec.libraryName, unityPodSpec.version);
            FtpHelper.CreateFTPDirectory(metaFtpDirName);

            //upload the unitypackage
            FtpHelper.UploadFile(PathBuilder.GetPackageUrl(unityPodSpec.libraryName, unityPodSpec.version), packagePath);

            //upload the metafile for this lib
            FtpHelper.UploadFile(PathBuilder.GetMetaFileUrl(unityPodSpec.libraryName, unityPodSpec.version), metaFilePath);
                    
        }
    }
}

