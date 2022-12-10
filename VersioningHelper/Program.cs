/*  Program.cs
 *  Version: 1.1 (2022.10.15)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace VersioningHelper
{
    class Program
    {
        static void Main(string[] _args)
        {
            int build = (int)(DateTime.Now - new DateTime(2022, 8, 31, 2, 18, 37, 135)).TotalDays;
            int revision = (int)(DateTime.Now.TimeOfDay.TotalSeconds * 0.5);

            Console.WriteLine("\nBuild check: x.x." + build + "." + revision);

            if (_args.Length != 2)
            {
                return;
            }

            AssemblyName assemblyName = AssemblyName.GetAssemblyName(_args[0] + _args[1]);
            if (assemblyName.Version == null)
                return;

            Version version = assemblyName.Version;
            VersionInfo info = new VersionInfo()
            {
                Major = version.Major,
                Minor = version.Minor,
                Build = version.Build,
                Revision = version.Revision
            };

            Console.WriteLine("Assembly version: " + version.ToString());

            string fileFullname = _args[0] + "version.xml";

            FileStream stream = File.Create(fileFullname);
            
            XmlSerializer serializer = new XmlSerializer(typeof(VersionInfo));
            serializer.Serialize(stream, info);

            stream.Close();

            Console.WriteLine("Version info file written.");
        }
    }

    public struct VersionInfo
    {
        public int Major;
        public int Minor;
        public int Build;
        public int Revision;
    }

}
