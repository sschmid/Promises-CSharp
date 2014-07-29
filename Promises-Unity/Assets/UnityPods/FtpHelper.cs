using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System;

namespace WG.UnityPods
{
		public class FtpHelper
		{
				static private int TIMEOUT = 4000;
				static public string ftpUrl = "ftp://172.19.252.15/unitypackages/";
				static public string ftpUserName = "jenkins";
				static public string ftpUserPassword = "woogaarcade";

				public static void CreateFTPDirectory (string path)
				{
						try {
								WebRequest request = WebRequest.Create (path);
								request.Timeout = TIMEOUT;
								request.Method = WebRequestMethods.Ftp.MakeDirectory;
								request.Credentials = new NetworkCredential (ftpUserName, ftpUserPassword);
								using (var resp = (FtpWebResponse) request.GetResponse()) {
										Debug.Log ("create directory " + resp.StatusCode);
								}
						} catch (WebException) {
								//it's ok, the dir already exist
						}
				}
        
				public static void UploadFile (string urlPath, string filepath)
				{
						using (WebClient request = new WebClient()) {
								request.Credentials = new NetworkCredential (ftpUserName, ftpUserPassword);
								byte[] responseArray = request.UploadData (urlPath, File.ReadAllBytes (filepath));
								Debug.Log ("Upload Complete: " + filepath + System.Text.Encoding.ASCII.GetString (responseArray));
						}
				}

				public static List<string> FetchPodsList ()
				{
						List<string> filesFromDirectory = new List<string> ();

						// Get the object used to communicate with the server.
						FtpWebRequest request = (FtpWebRequest)WebRequest.Create (ftpUrl);
						request.Timeout = TIMEOUT;
						request.Method = WebRequestMethods.Ftp.ListDirectory;
						request.Credentials = new NetworkCredential (ftpUserName, ftpUserPassword);
						try {
								FtpWebResponse response = (FtpWebResponse)request.GetResponse ();

								Stream responseStream = response.GetResponseStream ();
								StreamReader reader = new StreamReader (responseStream);
								try {
										while (!reader.EndOfStream) {
												string filename = reader.ReadLine ();
												if (!filename.EndsWith (".DS_Store")) {
														filesFromDirectory.Add (filename);
												}
										}
								} catch (Exception exception) {
										Debug.Log ("Failed to fetch pods listing. " + exception.Message);
								} finally {
										response.Close ();
										reader.Dispose ();
										reader.Close ();
								}
				
						} catch (WebException exception) {
								Debug.Log ("Failed to fetch pods listing. " + exception.Message);
						}

						return filesFromDirectory;
				}
		}
}