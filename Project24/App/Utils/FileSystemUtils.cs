/*  App/Utils/FileSystemUtils.cs
 *  Version: v1.0 (2023.06.20)
 *  
 *  Contributor
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.IO;

namespace Project24.App
{
    public enum FileType: short
    {
        Unset = 0,
        Directory,
        Files,
    }

    public class P24FileInfo
    {
        public FileType FileType { get; private set; }
        public string Name { get; private set; }
        public string Path { get; private set; }
        public string FullName { get { return Path + "/" + Name; } }
        public DateTime LastModified { get; private set; }
        public long Size { get; private set; }


        public P24FileInfo(FileInfo _fileInfo, string _maskPath = null)
        {
            if (_fileInfo == null)
                throw new ArgumentNullException(nameof(_fileInfo));

            if (Utils.IsFlagSet((int)_fileInfo.Attributes, (int)FileAttributes.Directory))
                FileType = FileType.Directory;
            else
                FileType = FileType.Files;

            Name = _fileInfo.Name;
            Path = _fileInfo.DirectoryName;
            LastModified = _fileInfo.LastWriteTime;
            Size = _fileInfo.Length;

            if (_maskPath != null)
                Path = Path.Replace(_maskPath, "");
        }
    }

    static class FileSystemUtils
    {
        public static string AppRoot { get; }
        public static string AppNextRoot { get; }
        public static string AppPrevRoot { get; }


        static FileSystemUtils()
        {
            AppRoot = Directory.GetCurrentDirectory();
            AppNextRoot = Path.GetFullPath(AppRoot + "/" + Constants.AppNextDir);
            AppPrevRoot = Path.GetFullPath(AppRoot + "/" + Constants.AppPrevDir);



        }


        public static List<P24FileInfo> GetFilesInPrev()
        {
            List<P24FileInfo> result = new();
            DirectoryInfo dirInfo = new(AppPrevRoot);
            if (!dirInfo.Exists)
                return result;

            var filesEnum = dirInfo.EnumerateFiles("*", SearchOption.AllDirectories);

            foreach (FileInfo file in filesEnum)
            {
                result.Add(new P24FileInfo(file));
            }

            return result;
        }
        
    }

}
