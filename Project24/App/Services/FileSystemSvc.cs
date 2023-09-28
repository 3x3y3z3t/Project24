/*  App/Service/FileSystemSvc.cs
 *  Version: v1.2 (2023.09.27)
 *  
 *  Contributor
 *      Arime-chan
 */

using System.Collections.Generic;
using System.IO;

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

        public static bool DeleteFiles(string _fullPath, List<string> _excludedFiles = null)
        {
            DirectoryInfo srcDirInfo = new(_fullPath);
            bool isDirectory = MiscUtils.IsFlagSet(srcDirInfo.Attributes, FileAttributes.Directory);

            // ==================================================;
            // case: delete all (no exclude files);

            if (_excludedFiles == null || _excludedFiles.Count <= 0)
            {
                if (isDirectory)
                    Directory.Delete(_fullPath, true);
                else
                    File.Delete(_fullPath);

                return true;
            }

            // ==================================================;
            // case: file;

            if (!isDirectory)
            {
                FileInfo fi = new(_fullPath);
                if (!_excludedFiles.Contains(fi.Name))
                    fi.Delete();

                return true;
            }

            // ==================================================;
            // case: directory;

            bool isEmpty = true;

            DirectoryInfo[] dirInfos = srcDirInfo.GetDirectories();
            foreach (DirectoryInfo di in dirInfos)
            {
                DeleteFiles(di.FullName, _excludedFiles);
                di.Delete();
            }

            FileInfo[] fileInfos = srcDirInfo.GetFiles();
            foreach (FileInfo fi in fileInfos)
            {
                if (_excludedFiles.Contains(fi.Name))
                {
                    isEmpty = false;
                    continue;
                }

                fi.Delete();
            }

            return isEmpty;
        }

        public static void CopyFiles(string _srcPath, string _dstPath, List<string> _excludedFiles = null)
        {
            DirectoryInfo srcDirInfo = new(_srcPath);
            bool isDirectory = MiscUtils.IsFlagSet(srcDirInfo.Attributes, FileAttributes.Directory);
            bool hasExclusion = _excludedFiles != null && _excludedFiles.Count > 0;

            // ==================================================;
            // case: file;

            if (!isDirectory)
            {
                FileInfo fi = new(_srcPath);
                if (hasExclusion && _excludedFiles.Contains(fi.Name))
                    return;

                string dst = _dstPath + "/" + fi.Name;
                fi.CopyTo(dst, true);

                return;
            }

            // ==================================================;
            // case: directory;

            Directory.CreateDirectory(_dstPath);
            DirectoryInfo[] dirInfos = srcDirInfo.GetDirectories();
            foreach (DirectoryInfo di in dirInfos)
            {
                string dst = _dstPath + "/" + di.Name;
                CopyFiles(di.FullName, dst, _excludedFiles);
            }

            FileInfo[] fileInfos = srcDirInfo.GetFiles();
            foreach (FileInfo fi in fileInfos)
            {
                if (hasExclusion && _excludedFiles.Contains(fi.Name))
                    continue;

                string dst = _dstPath + "/" + fi.Name;
                fi.CopyTo(dst, true);
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
