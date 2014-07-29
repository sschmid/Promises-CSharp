using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Net;

namespace WG.UnityPods
{
    /*
     * Builds a list of PodPackage following specs in the Podfile, Checking if a package is installed or not (if the packagename_meta file exist then a package is installed)
     * and build the dependency tree for a package. it also update each PodPackage latest version available by checking if a new version is on the server (package.RefreshState).
     * wooga-unitypods is always added as a default dependency, so it can update itself with the PackageManager custom Window provided in unity.
     */
    public class PackageManager
    {
        public static List<PodPackage> packages = new List<PodPackage>();
        private static LineReader lineReader = new LineReader();

        public static void Refresh()
        {
            packages.Clear();
                        
            foreach (string requiredPackName in UnityPodFile.GetDependencies())
            {
                BuildPackage(requiredPackName);
            }
            //add the unitypod library as well, so it can get updated as it is a default dependency on any project using unitypods
            BuildPackage("wooga-unitypods");
        }
        
        static void BuildPackage(string requiredPackageName)
        {
            PodPackage package = new PodPackage(requiredPackageName);
            BuildDependencyTree(package);
            packages.Add(package);
        }

        static void BuildDependencyTree(PodPackage package)
        {
            package.latestVersionAvailable = UnityPodsUpdateCheck.LastVersionForLibrary(package.packagename, out package.lastestStableVersionAvailable);
            package.version = GetInstalledVersionForPackage(package);

            //meta file path on the server
            string metaFileFtpPath = PathBuilder.GetMetaFileUrl(package.packagename, package.latestVersionAvailable);
            //meta file path on local
            string metaFilePath = PathBuilder.GetMetaFilePath(package.packagename);

            //fist we try to get the dependency tree by checking if a meta file is available on the server
            try
            {
                using (WebClient request = new WebClient())
                {
                    request.Credentials = new NetworkCredential(FtpHelper.ftpUserName, FtpHelper.ftpUserPassword);
                    Stream stream = request.OpenRead(metaFileFtpPath);

                    StreamReader reader = new StreamReader(stream);
                    ParseStreamAndFillDependencies(reader, package);

                    reader.Dispose();
                    stream.Dispose();
                }
            } catch (WebException)
            { //for retrocompatibility we check on the disk for dependencies if we fail to find them online
                if (File.Exists(metaFilePath))
                {
                    StreamReader reader = new StreamReader(metaFilePath);
                    ParseStreamAndFillDependencies(reader, package);
                    reader.Dispose();
                }
            }

            if (package.dependencies != null)
            {
                //recursivly check all his dependencies and update the versions
                foreach (PodPackage pack in package.dependencies)
                {
                    BuildDependencyTree(pack);
                }
            }
        }

        static Version GetInstalledVersionForPackage(PodPackage package)
        {
            string metaFilePath = PathBuilder.GetMetaFilePath(package.packagename);
            Version version = Version.VersionZero();

            //if a meta file is on disk, means that we have a version of the lib installed, we parse the meta and get the version
            if (File.Exists(metaFilePath))
            {
                string[] lines = File.ReadAllLines(metaFilePath);
                lineReader.Read(lines, (string command, string args ) => {
                    if (command == COMMAND.VERSION)
                    {
                        version = new Version(args);
                    }
                });
            }
            //if meta file is not on disk, means the package is not installed
            return version;
        }

        static void ParseStreamAndFillDependencies(StreamReader reader, PodPackage package)
        {
            if (reader != null)
            {

                lineReader.Read(reader, (string command, string args ) => {

                    if (command == COMMAND.DEPENDENCY)
                    {
                        bool circularDependency = false;
                        string depName = args;
                        //anti circular dependency check
                        PodPackage parent = package;
                        while (parent != null)
                        {
                            if (depName == parent.packagename)
                            {
                                circularDependency = true;
                                break;
                            }
                            parent = parent.parentNode;
                        }
                                        
                        if (!circularDependency)
                        {
                            PodPackage dep = new PodPackage(depName);
                            dep.parentNode = package;
                            package.dependencies.Add(dep);
                        }
                    }
                });
            }
        }
    }
}
