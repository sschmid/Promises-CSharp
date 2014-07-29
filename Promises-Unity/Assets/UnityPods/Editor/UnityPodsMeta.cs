using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

/*
 * This class manipulates data in the unitypods_meta dir. this data is necessary for adding meta information on a build .unitypackage
 */ 
namespace WG.UnityPods
{
		static public class UnityPodsMeta
		{
				//Build the _meta file for the current library, writing information that will be needed by unitypods to update and resolve dependencies on installed libraries
				//return the path of the metafile
				static public string UpdateMetaFile (UnityPodSpec unityPodSpec)
				{
						Debug.Log ("Updating meta info for " + unityPodSpec.libraryName);

						if (!Directory.Exists (PathBuilder.podmetaFolder)) {
								Debug.LogError ("unitypods_meta folder not found. reinstall unitypods");
								return null;
						}
             
						string metaFilePath = PathBuilder.GetMetaFilePath (unityPodSpec.libraryName);


						string whitespace = " ";

						using (StreamWriter newMetaFile = new StreamWriter(metaFilePath, false)) { 
								newMetaFile.WriteLine (COMMAND.NAME + whitespace + unityPodSpec.libraryName);
								newMetaFile.WriteLine (COMMAND.VERSION + whitespace + unityPodSpec.version.ToString ());
								newMetaFile.WriteLine (COMMAND.DESCRIPTION + whitespace + unityPodSpec.description);
								newMetaFile.WriteLine (COMMAND.AUTHOR + whitespace + unityPodSpec.author);


								//we include in the metafile information on the dependencies of the current library, those dependency are listed in the UnityPodfile if present
								//for each dependency we write a line
								foreach (string depName in UnityPodFile.GetDependencies()) {
										newMetaFile.WriteLine (COMMAND.DEPENDENCY + whitespace + depName);
								}
						}

						AssetDatabase.Refresh ();

						return metaFilePath;
				}
		}
}