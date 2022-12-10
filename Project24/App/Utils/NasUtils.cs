/*  NasUtils.cs
 *  Version: 1.1 (2022.12.06)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.IO;

namespace Project24.App
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

        /// <summary>Returns a list of all folders and files in the directory specified by <c>_path</c>.</summary>
        /// <param name="_path">The relative path to DriveUtils.NasRootPath that specify the directory to get its content.</param>
        /// <returns type="System.Collection.List">A list of folders and files in the specified directory.</returns>
        public static List<FileModel> GetDirectoryContent(string _path)
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

        public enum NasLocation
        {
            AppRoot,
            AppNextRoot,
            NasRoot,
        }

        /// <summary>Returns a list of all files in the directory specified by <c>_path</c> and its subdirectories.</summary>
        /// <param name="_path">The relative path to DriveUtils.NasRootPath that specify the directory to get its content.</param>
        /// <returns>A list of all files in the specified directory and its subdirectories.</returns>
        public static List<FileModel> GetAllFilesInDirectory(string _path, NasLocation _location = NasLocation.NasRoot)
        {
            string absPath;

            switch (_location)
            {
                case NasLocation.NasRoot:
                    absPath = Path.GetFullPath(DriveUtils.NasRootPath + "/" + _path);
                    break;
                case NasLocation.AppRoot:
                    absPath = Path.GetFullPath(AppUtils.AppRoot + "/" + _path);
                    break;
                case NasLocation.AppNextRoot:
                    absPath = Path.GetFullPath(DriveUtils.AppNextRootPath + "/" + _path);
                    break;

                default:
                    return new List<FileModel>();
            }

            List<FileModel> list = new List<FileModel>();

            DirectoryInfo dirInfo = new DirectoryInfo(absPath);


            var files = dirInfo.EnumerateFiles("*", new EnumerationOptions() { RecurseSubdirectories = true });
            foreach (FileInfo fi in files)
            {
                string fullname = fi.FullName.Replace(absPath, "").Replace('\\', '/').Trim('/');

                string parent = "";
                int pos = fullname.LastIndexOf('/');
                if (pos > 0)
                {
                    parent = fullname[0..pos];
                }

                list.Add(new FileModel()
                {
                    FileType = FileType.File,
                    Name = fi.Name,
                    Fullname = fi.FullName,
                    RelativePath = parent,
                    LastModified = fi.LastWriteTime,
                    Size = fi.Length
                });
            }

            return list;
        }

    }

}
