using UnityEngine;
using System.Net;
using System.IO;
using System;

namespace WG.UnityPods
{
    public class PodDescription
    {
        public string name { get; private set; }

        public Version latestVersion { get; private set; }

        public Version latestStableVersion { get; private set; }

        public string description { get; private set; }

        public string author { get; private set; }

        public PodDescription(string name, Version version, Version lastStable, string description, string author)
        {
            this.name = name; 
            this.latestVersion = version; 
            this.latestStableVersion = lastStable;
            this.description = description; 
            this.author = author;
        }

        public override string ToString()
        {
            return string.Format("{0}={1}:last {2}={3}:last stable {4}={5}:{6}={7}:{8}={9}", 
                                 COMMAND.NAME, name, COMMAND.VERSION, latestVersion, COMMAND.VERSION, latestStableVersion, COMMAND.DESCRIPTION, description, COMMAND.AUTHOR, author);
        }

        public static PodDescription GetDescriptionForPod(string name)
        {
            string description = "unavailable";
            string author = "unknown";

            Version lastStable;
            Version latestVersionAvailable = UnityPodsUpdateCheck.LastVersionForLibrary(name, out lastStable);

            string metaFileFtpPath = PathBuilder.GetMetaFileUrl(name, latestVersionAvailable);

            try
            {
                using (WebClient request = new WebClient())
                {
                    request.Credentials = new NetworkCredential(FtpHelper.ftpUserName, FtpHelper.ftpUserPassword);
                    Stream stream = request.OpenRead(metaFileFtpPath);
								
                    StreamReader reader = new StreamReader(stream);

                    new LineReader().Read(reader, (string command, string args) => {

                        if (command == COMMAND.DESCRIPTION)
                        {	
                            description = args;
                        } else if (command == COMMAND.AUTHOR)
                        {
                            author = args;
                        }
                    });
			
                    reader.Dispose();
                    stream.Dispose();
                }
            } catch (WebException)
            { 
//								throw new FileNotFoundException ("Something went wrong fetching the description for pod: " + name + "Error message: " + exception.Message);
            }
						
            PodDescription podDescription = new PodDescription(name, latestVersionAvailable, lastStable, description, author);

            return podDescription;
        }
    }
}

