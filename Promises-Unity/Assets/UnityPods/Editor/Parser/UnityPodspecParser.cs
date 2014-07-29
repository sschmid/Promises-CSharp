using UnityEngine;
using System.Collections;
using System.IO;

/*
 * UnityPodspec example:
 * 
 * name wooga-unity-tracking
 * version 0.1
 * //comment line
 * 
 * dependencies are specified in the UnityPodfile, and they will be included in the _meta file distributed to the library user.
 */
namespace WG.UnityPods
{
    public class UnityPodspecParser
    {
        public static string podspecFileName = "Assets/UnityPodspec";
        private string libraryName;
        private Version version;
        private string description = "no description";
        private string author = "unknown";

        public UnityPodSpec Parse()
        {
            if (!System.IO.File.Exists(podspecFileName))
            {
                throw new System.IO.FileLoadException("UnityPodspec does not exist " + podspecFileName);
            }
						
            string[] lines = System.IO.File.ReadAllLines(podspecFileName);

            new LineReader().Read(lines, this.ParsePodspecCommand);

            ValidateMandatoryCommands();

            return new UnityPodSpec(libraryName, version, description, author);
        }

        public static bool HasFileOnDisk()
        {
            return File.Exists(UnityPodspecParser.podspecFileName);
        }

        private void ValidateMandatoryCommands()
        {
            if (libraryName == null || version == null)
            {
                string missingProperties = "";
				
                missingProperties += (libraryName != null) ? "" : COMMAND.NAME; 
                missingProperties += (version != null) ? "" : " " + COMMAND.VERSION; 
				
                throw new System.InvalidOperationException("UnityPodspec is missing command(s) " + missingProperties);
            }
        }
				
        private void ParsePodspecCommand(string command, string arg)
        {
            switch (command)
            {
                case COMMAND.NAME:
                    libraryName = arg;
                    break;
                case COMMAND.VERSION:
                    version = new Version(arg);
                    break;
                case COMMAND.DESCRIPTION:
                    description = arg;
                    break;
                case COMMAND.AUTHOR:
                    author = arg;
                    break;
                default:
                    throw new System.NotSupportedException("Failed parsing command ");
            }
        }
    }

    public class UnityPodSpec
    {
        public string libraryName { set; get; }

        public Version version { set; get; }

        public string description { set; get; }

        public string author { set; get; }

        public UnityPodSpec(string libraryName, Version version, string description, string author)
        {
            this.libraryName = libraryName;
            this.version = version;
            this.description = description;
            this.author = author;
        }

        public override string ToString()
        {
            return string.Format("[UnityPodSpec: libraryName={0}, version={1}, description={2}, author={3}]", libraryName, version, description, author);
        }

        public void SavePodSpecOnDisk()
        {
            if (!UnityPodspecParser.HasFileOnDisk())
            {
                File.Create(UnityPodspecParser.podspecFileName).Close();
            }
            string[] lines = {
                "name " + libraryName, 
                "version " + version.ToString(),
                "description " + description.Replace(System.Environment.NewLine, " "),
                "author " + author
            };
            File.WriteAllLines(UnityPodspecParser.podspecFileName, lines);
        }
    }
}
