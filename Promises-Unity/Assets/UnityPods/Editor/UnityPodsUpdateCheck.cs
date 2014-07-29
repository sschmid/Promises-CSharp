using UnityEngine;
using System.Net;
using System.IO;
using System.Collections;

namespace WG.UnityPods
{
    /*
     * Utility to check if a package has an update
     */
    public static class UnityPodsUpdateCheck
    {
        // return true if the server has a bigger version number available
        // as an additional out parameter returns the latestStableVersion, means not inclusind alphas betas and rc
        public static Version LastVersionForLibrary(string libName, out Version latestStableVersion)
        {
            try
            {
                WebRequest request = WebRequest.Create(PathBuilder.GetLibDirectoryUrl(libName) + "/");
                request.Timeout = 4000;
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential("jenkins", "woogaarcade");
                using (var resp = (FtpWebResponse) request.GetResponse())
                {
                    Stream responseStream = resp.GetResponseStream();
                    StreamReader reader = new StreamReader(responseStream);

                    //find the most recent version on the server
                    Version latestServerVersion = null;
                    latestStableVersion = null;

                    while (!reader.EndOfStream)
                    {
                        string packagefile = reader.ReadLine();
                        
                        if (!packagefile.EndsWith(".unitypackage"))
                            continue;
                        
                        string version = packagefile.Substring(libName.Length + 1); //remove packagename_ , +1 is for the '_' character
                        version = version.Remove(version.Length - ".unitypackage".Length, ".unitypackage".Length);

                        Version serverVersion = new Version(version);

                        if (latestServerVersion == null || serverVersion > latestServerVersion)
                        {
                            latestServerVersion = serverVersion;

                            //if there are no other version, pick the only one as last stable
                            if (latestStableVersion == null)
                                latestStableVersion = latestServerVersion;

                            if (!serverVersion.IsPrerelease())
                                latestStableVersion = serverVersion;
                        }
                    }

                    reader.Close();

                    return latestServerVersion;
                }
            } catch (WebException ex)
            {
                Debug.LogWarning("CheckVersions() exception " + ex.ToString());
                latestStableVersion = null;
                return null;
            }
        }
    }
}
