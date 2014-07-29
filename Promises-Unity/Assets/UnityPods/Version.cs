using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WG.UnityPods
{
    public enum VersionReleaseType
    {
        Final = 0,
        ReleaseCandidate = 1,
        Beta = 2,
        Alpha = 3,
    }

    /*
     * Class to hold version number in the format of D.D.D.D e.g. 0.2.1
     * VersionNumber s are comparable so  0.2.1 > 0.2 > 0.1.10 and 0.2 == 0.2.0 == 0.2.0.0
     * 0.2 > 0.2-beta.2 > 0.2-alpha.2
     */
    public class Version
    {
        private List<int> verNumbers;
        private string stringVersion;
        public VersionReleaseType releaseType = VersionReleaseType.Final;
        public int preReleaseNumber;

        public Version(string versionString)
        {
            SetVersionFromString(versionString);
        }

        public Version(string versionString, VersionReleaseType releaseType, int preReleaseNumber) : this(versionString)
        {
            this.releaseType = releaseType;
            this.preReleaseNumber = preReleaseNumber;

            if (preReleaseNumber <= 0)
                throw new System.Exception("prerelease version <= 0");
        }

        //Check if a string is valid version in the form X.Y.Z
        public static bool ValidateVersionString(string versionString)
        {
            string[] split = versionString.Split('.');

            if (split.Length != 3)
                return false;

            foreach (string token in split)
            {
                try
                {
                    System.Convert.ToInt32(token);
                } catch (System.FormatException)
                {
                    return false;
                }
            }

            return true;
        }

        public void SetVersionFromString(string versionString)
        {
            string[] releaseTypeSplit = versionString.Split('-');
            versionString = releaseTypeSplit [0];
            //if the version contain -, its a prerelease
            if (releaseTypeSplit.Length > 1)
            {
                string[] prereleaseSplit = releaseTypeSplit [1].Split('.');
                
                if (prereleaseSplit.Length != 2)
                    Debug.LogError("prerelease format is invalid");
                
                releaseType = ReleaseTypeFromString(prereleaseSplit [0]);
                preReleaseNumber = System.Convert.ToInt32(prereleaseSplit [1]);
            }
            
            verNumbers = new List<int>();
            stringVersion = versionString;
            string[] splittedVersion = versionString.Split('.');
            
            foreach (string token in splittedVersion)
            {
                try
                {
                    verNumbers.Add(System.Convert.ToInt32(token));
                } catch (System.FormatException ex)
                {
                    Debug.LogError("format exception parsing version number " + ex);
                    throw;
                }                
            }
        }

        public bool IsPrerelease()
        {
            if (releaseType == VersionReleaseType.Final)
                return false;
            return true;
        }

        public static Version VersionZero()
        {
            return new Version("0.0.0");
        }

        public override string ToString()
        {
            if (releaseType == VersionReleaseType.Final)
                return stringVersion;
            else
            {
                return stringVersion + "-" + PrereleaseString(releaseType) + "." + preReleaseNumber;
            }
        }

        public string VersionStringWithoutPrerelease()
        {
            return stringVersion;
        }

        public static VersionReleaseType ReleaseTypeFromString(string s)
        {
            switch (s)
            {
                case "":
                    return VersionReleaseType.Final;
                case "alpha":
                    return VersionReleaseType.Alpha;
                case "beta":
                    return VersionReleaseType.Beta;
                case "rc":
                    return VersionReleaseType.ReleaseCandidate;
                default:
                    Debug.LogError("cannot find a string for this release type");
                    return VersionReleaseType.Final;
            }
        }

        public static string PrereleaseString(VersionReleaseType type)
        {
            switch (type)
            {
                case VersionReleaseType.Final:
                    return "";
                case VersionReleaseType.Alpha:
                    return "alpha";
                case VersionReleaseType.Beta:
                    return "beta";
                case VersionReleaseType.ReleaseCandidate:
                    return "rc";
                default:
                    Debug.LogError("cannot find a string for this release type");
                    return "";
            }
        }

        public override bool Equals(System.Object obj)
        {
            // If parameter cannot be cast to VersionNumber return false:
            Version p = obj as Version;
            if ((object)p == null)
            {
                return false;
            }
            
            return this == p;
        }

        public override int GetHashCode()
        {
            return stringVersion.GetHashCode();
        }

        public static bool operator ==(Version a, Version b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
                return true;
            
            if (((object)a == null) || ((object)b == null))
                return false;

            if (a.verNumbers.Count != b.verNumbers.Count)
                return false;

            for (int i = 0; i < a.verNumbers.Count; i++)
            {
                if (a.verNumbers [i] != b.verNumbers [i])
                    return false;
            }

            //check prerelease
            if (a.releaseType != b.releaseType)
                return false;

            //release number is only used in prerelease versioning
            if (a.releaseType != VersionReleaseType.Final)
            {
                if (a.preReleaseNumber != b.preReleaseNumber)
                    return false;
            }

            return true;
        }

        public static bool operator >(Version a, Version b)
        {
            if (((object)a == null) || ((object)b == null))
                return false;
            for (int i = 0; i < a.verNumbers.Count && i < b.verNumbers.Count; i++)
            {
                if (a.verNumbers [i] < b.verNumbers [i])
                    return false;
                else if (a.verNumbers [i] > b.verNumbers [i])
                    return true;
            }
            
            if (a.verNumbers.Count > b.verNumbers.Count)
                return true;

            //check the prerelease. lower release type are >
            if (a.releaseType < b.releaseType)
                return true;
            else if (a.releaseType == b.releaseType)
            {
                if (a.preReleaseNumber > b.preReleaseNumber && a.releaseType != VersionReleaseType.Final)
                    return true;
            }

            return false;
        }
            
        public static bool operator <(Version a, Version b)
        {
            if (((object)a == null) || ((object)b == null))
                return false;
            for (int i = 0; i < a.verNumbers.Count && i < b.verNumbers.Count; i++)
            {
                if (a.verNumbers [i] < b.verNumbers [i])
                    return true;
                else if (a.verNumbers [i] > b.verNumbers [i])
                    return false;
            }

            if (a.verNumbers.Count < b.verNumbers.Count)
                return true;

            //check the prerelease. lower release type are >
            if (a.releaseType > b.releaseType)
                return true;
            else if (a.releaseType == b.releaseType)
            {
                if (a.preReleaseNumber < b.preReleaseNumber && a.releaseType != VersionReleaseType.Final)
                    return true;
            }

            return false;
        }

        public static bool operator <=(Version a, Version b)
        {
            return a < b || a == b;
        }

        public static bool operator >=(Version a, Version b)
        {
            return a > b || a == b;
        }

        public static bool operator !=(Version a, Version b)
        {
            return !(a == b);
        }

    }
}
