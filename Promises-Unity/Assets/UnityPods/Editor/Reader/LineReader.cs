using System;
using System.IO;
using UnityEngine;

namespace WG.UnityPods
{   
    public sealed class COMMAND
    {
        public const string NAME = "name";
        public const string AUTHOR = "author";
        public const string VERSION = "version";
        public const string DESCRIPTION = "description";
        public const string DEPENDENCY = "dependency";
    }

    public class LineReader
    {
        public void Read(string[] lines, Action<string,string> commandHandler)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                this.ParseLine(lines [i], commandHandler);
            }
        }

        public void Read(StreamReader streamReader, Action<string,string> commandHandler)
        {
            while (!streamReader.EndOfStream)
            {
                this.ParseLine(streamReader.ReadLine(), commandHandler);
            }
        }

        private void ParseLine(string line, Action<string,string> commandHandler)
        {
            line = line.Trim();

            if (line.Length <= 0) // found an empty line
                return;
            
            int firstSpaceIndex = line.IndexOf(" ");
            if (firstSpaceIndex <= 0) // found an empty line, index -1
                return;
            
            string command = line.Substring(0, firstSpaceIndex).Trim();
            string value = line.Substring(firstSpaceIndex + 1).Trim();
            
            if (command.StartsWith("//")) // found a comment
                return;

            commandHandler(command, value);
        }
    }
}

