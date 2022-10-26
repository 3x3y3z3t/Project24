/*  NasUtils.cs
 *  Version: 1.0 (2022.10.26)
 *
 *  Contributor
 *      Arime-chan
 */
using System;
using System.Collections.Generic;
using System.IO;

namespace Project24.App.Utils
{
    public static class NasUtils
    {
        public enum FileType
        {
            Unset = 0,
            Directory,
            File
        }

        public class FileModel
        {
            public FileType FileType { get; set; } = FileType.Unset;
            public string Name { get; set; }
            public string Fullname { get; set; }
            public string RelativePath { get; set; }
            public DateTime LastModified { get; set; }
            public long Size { get; set; }
        }

        /* Returns all files and folders in the directory specified by `_path`.
         * Note that `_path` is relative path to DriveUtils.NasRootPath. 
         */
        public static List<FileModel> GetFilesInDirectory(string _path)
        {
            string absPath = Path.GetFullPath(DriveUtils.NasRootPath + "/" + _path);

            List<FileModel> list = new List<FileModel>();

            string[] directories = Directory.GetDirectories(absPath);
            foreach (string dir in directories)
            {
                DirectoryInfo di = new DirectoryInfo(dir);

                list.Add(new FileModel()
                {
                    FileType = FileType.Directory,
                    Name = di.Name,
                    Fullname = di.FullName,
                    RelativePath = _path,
                    LastModified = di.LastWriteTime
                });
            }

            string[] files = Directory.GetFiles(absPath);
            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);

                list.Add(new FileModel()
                {
                    FileType = FileType.File,
                    Name = fi.Name,
                    Fullname = fi.FullName,
                    RelativePath = _path,
                    LastModified = fi.LastWriteTime,
                    Size = fi.Length
                });
            }

            return list;
        }






    }

}
