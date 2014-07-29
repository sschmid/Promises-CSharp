using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WG.UnityPods
{
    public enum PackageState
    {
        Unknown = 0,
        NotInstalled = 1,
        ToUpdate = 2,
        Installed = 3
    }

    public class PodPackage
    {
        public string packagename;
        //current version number
        public Version version;
        //version number of the latest version on the server
        public Version latestVersionAvailable;
        //lastest version not counting alphas, betas and rc
        public Version lastestStableVersionAvailable;

//        public PackageState state
//        {
//            get
//            {
//                if (version == Version.VersionZero())
//                {
//                    return PackageState.NotInstalled;
//                } else if (NeedsUpdate())
//                {
//                    return PackageState.ToUpdate;
//                } else
//                {
//                    return PackageState.Installed;
//                }
//            }
//        }

        //if this package has dependencies, those are the links
        public List<PodPackage> dependencies = new List<PodPackage>();
        //if the package is a dependency of another package, this link to the parent package
        public PodPackage parentNode = null;

        public PodPackage(string name, Version v)
        {
            packagename = name;
            version = v;
        }

        public PodPackage(string name, string v = "0.0.0")
        {
            packagename = name;
            version = new Version(v);
        }

        public bool NeedsUpdate(bool includingPrerelease)
        {
            if (includingPrerelease)
                return latestVersionAvailable > version;

            return lastestStableVersionAvailable > version;
        }

        public bool IsNotInstalled()
        {
            if (version == Version.VersionZero())
                return true;

            return false;
        }

        public override string ToString()
        {
            return string.Format("[PodPackage: packageName={1} version={2}]", packagename, version);
        }
    }
}
