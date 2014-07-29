using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace WG.UnityPods
{
    public static class  PackageUninstaller
    {
        public static void UninstallPackage(string packageName)
        {
            List<string> filesToDelete = UnityPodsAssetLabeler.FilesThatBelongToLibrary(packageName);
            foreach (var s in filesToDelete)
            {
                AssetDatabase.DeleteAsset(s);
            }
            AssetDatabase.Refresh();
            //cleanup empty directories
        
            for (int i = filesToDelete.Count -1; i >= 0; i--)
            {
                string dirToCheck = Path.GetDirectoryName(filesToDelete [i]);
                DirectoryInfo di = new DirectoryInfo(dirToCheck);
            
                FileInfo[] fi;
                try
                {
                    fi = di.GetFiles();
                } catch (DirectoryNotFoundException)
                {
                    continue;
                }

                bool isEmpty = true;
                foreach (var file in fi)
                {
                    if (!file.Name.EndsWith(".DS_Store"))
                    {
                        isEmpty = false;
                        break;
                    }
                }
            
                if (isEmpty)
                {
                    try
                    {
                        di.Delete();
                        File.Delete(di.FullName + ".meta");
                    } catch (DirectoryNotFoundException)
                    {
                        //its ok
                    } catch (FileNotFoundException)
                    {
                        //its also ok
                    }
                }
            }

            AssetDatabase.Refresh();
        }
    }
}
