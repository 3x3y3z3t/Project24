/*  App/Service/FileSystemSvc.cs
 *  Version: v1.1 (2023.09.12)
 *  
 *  Contributor
 *      Arime-chan
 */


using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;

namespace Project24.App.Services
{
    public class FileSystemSvc
    {
        public string AppRoot { get; private set; }
        public string AppMainRoot { get; private set; }
        public string AppNextRoot { get; private set; }
        public string AppPrevRoot { get; private set; }


        public FileSystemSvc()
        {
            // HACK: temporary patch;
            AppRoot = Path.GetFullPath(MiscUtils.WhereAmI() + "/");

            AppMainRoot = Path.GetFullPath(AppRoot + "/.." + Constants.AppMainDir);
            AppNextRoot = Path.GetFullPath(AppRoot + "/.." + Constants.AppNextDir);
            AppPrevRoot = Path.GetFullPath(AppRoot + "/.." + Constants.AppPrevDir);



            ReconstructDirectories();
        }


        public void ReconstructDirectories()
        {
            Directory.CreateDirectory(AppMainRoot);
            Directory.CreateDirectory(AppNextRoot);
            Directory.CreateDirectory(AppPrevRoot);



        }

        public List<P24FileInfo> GetFiles(string _fullPath)
        {
            if (!IsPathValid(_fullPath))
                return null;

            List<P24FileInfo> result = new();
            DirectoryInfo dirInfo = new(_fullPath);
            if (!dirInfo.Exists)
                return result;

            var filesEnum = dirInfo.EnumerateFiles("*", SearchOption.AllDirectories);

            foreach (FileInfo file in filesEnum)
            {
                result.Add(new P24FileInfo(file));
            }

            return result;
        }

        public static void DeleteFiles(string _fullPath, List<string> _excludedFiles = null)
        {
            if (_excludedFiles == null || _excludedFiles.Count <= 0)
            {
                Directory.Delete(_fullPath, true);
                return;
            }

            DirectoryInfo srcDirInfo = new(_fullPath);

            DirectoryInfo[] dirInfos = srcDirInfo.GetDirectories();
            FileInfo[] fileInfos = srcDirInfo.GetFiles();

            foreach (FileInfo fi in fileInfos)
            {
                if (_excludedFiles.Contains(fi.Name))
                    continue;

                fi.Delete();
            }

            foreach (DirectoryInfo di in dirInfos)
            {
                DeleteFiles(di.FullName, _excludedFiles);
            }
        }

        public static void CopyFiles(string _srcPath, string _dstPath, List<string> _excludedFiles = null)
        {
            DirectoryInfo srcDirInfo = new(_srcPath);

            DirectoryInfo[] dirInfos = srcDirInfo.GetDirectories();
            FileInfo[] fileInfos = srcDirInfo.GetFiles();
            bool hasExclusion = _excludedFiles != null && _excludedFiles.Count > 0;

            Directory.CreateDirectory(_dstPath);

            foreach (FileInfo fi in fileInfos)
            {
                if (hasExclusion)
                {
                    if (_excludedFiles.Contains(fi.Name))
                        continue;
                }

                string dst = _dstPath + "/" + fi.Name;
                fi.CopyTo(dst, true);
            }

            foreach (DirectoryInfo di in dirInfos)
            {
                string dst = _dstPath + "/" + di.Name;
                CopyFiles(di.FullName, dst, _excludedFiles);
            }
        }

        //public List<P24FileInfo> GetFilesInMain() => GetFiles(AppRoot);
        public List<P24FileInfo> GetFilesInNext() => GetFiles(AppNextRoot);
        //public List<P24FileInfo> GetFilesInPrev() => GetFiles(AppPrevRoot);


        private bool IsPathValid(string _path)
        {

            return true;
        }


    }

}
