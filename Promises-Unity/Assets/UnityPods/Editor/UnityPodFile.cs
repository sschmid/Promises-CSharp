using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

namespace WG.UnityPods
{	
	public sealed class UnityPodFile
	{
		private const string commandString = "{0} {1}";
		private const string podfileFileName = "Assets/UnityPodfile";
		private static readonly object padlock = new object ();
		private static UnityPodFile singletonInstance = null;

		public static UnityPodFile file {
			get {
				lock (padlock) {
					if (singletonInstance == null) {
						singletonInstance = new UnityPodFile ();
					}
					return singletonInstance;
				}
			}
		}
					
		private static bool HasFileOnDisk ()
		{
			return File.Exists (UnityPodFile.podfileFileName);
		}

		public static List<string> GetDependencies ()
		{
			if (!HasFileOnDisk ())
				return new List<string> ();
				
			file.ReadDependencies ();

			return file.dependeciesPodNames.ToList ();
		}

		public static bool ContainsDependency (string podName)
		{
			if (!HasFileOnDisk ())
				return false;

			if (file.dependeciesPodNames == null)
				file.ReadDependencies ();

			return file.dependeciesPodNames.Contains (podName);
		}
				
		public static void AddDependency (string podName)
		{
			if (!HasFileOnDisk ()) {

				File.Create (UnityPodFile.podfileFileName).Close ();

				InternalAddDependency (podName);

				RefreshUnityProjectWindow ();

			} else {
			
				if (!ContainsDependency (podName)) {

					InternalAddDependency (podName);
				}
			}

			file.ReadDependencies ();
		}
				
		private static void InternalAddDependency (string podName)
		{
			List<string> lines = File.ReadAllLines (podfileFileName).ToList ();

			lines.Add (string.Format (commandString, COMMAND.DEPENDENCY, podName));

			File.WriteAllLines (podfileFileName, lines.ToArray ());
		}

		public static void RemoveDependency (string podName)
		{
			if (!HasFileOnDisk ())
				return;

			string tempFile = Path.GetTempFileName ();
			string lineToRemove = string.Format (commandString, COMMAND.DEPENDENCY, podName);
			string[] linesToKeep = File.ReadAllLines (podfileFileName).Where (line => line != lineToRemove).ToArray ();
							
			File.WriteAllLines (tempFile, linesToKeep);
			File.Delete (podfileFileName);
			File.Move (tempFile, podfileFileName);
			try {
				File.Delete (tempFile);
			} catch {
			} // best effort
			DeleteFile ();
		}

		public static void DeleteFile ()
		{
			file.ReadDependencies ();
			
			if (file.dependeciesPodNames.Count == 0) {
				File.Delete (UnityPodFile.podfileFileName);
				RefreshUnityProjectWindow ();
			}
		}

		private static void RefreshUnityProjectWindow ()
		{
			UnityEditor.AssetDatabase.Refresh ();
		}

		/*
			 	* Internal instance class
			 	* */
		internal HashSet<string> dependeciesPodNames;

		internal void ReadDependencies ()
		{	
			this.dependeciesPodNames = new HashSet<string> ();

			string[] lines = File.ReadAllLines (UnityPodFile.podfileFileName);

			new LineReader ().Read (lines, (string command, string arg) => {
				
				switch (command) {
				case COMMAND.DEPENDENCY:
					dependeciesPodNames.Add (arg);
					break;
				default:
					Debug.LogError ("Failed to handle command " + command);
					break;
				}
			});

		}
	}
}	