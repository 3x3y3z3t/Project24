/*  NasUtils.cs
 *  Version: 1.4 (2022.12.30)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.IO;
using Project24.Models.Nas;

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

        public enum NasLocation
        {
            AppRoot,
            AppNextRoot,
            NasRoot,
        }

        public enum NasOperationStatus
        {
            OK = 1 << 1,
            Fail = 1 << 2,
            MaximumDuplicationReached = 1 << 3,
            UnknownError = 1 << 4,
            FileExisted = 1 << 5,
            DestinationNotExisted = 1 << 6,

            OperationMakeCopy = 1 << 10,
            OperationCopyTo = 1 << 11,
            OperationMoveTo = 1 << 12,
        }

        public class FileModel : IComparable
        {
            public FileType FileType { get; set; } = FileType.Unset;
            public string Name { get; set; }
            public string Fullname { get; set; }
            public string RelativePath { get; set; }
            public DateTime LastModified { get; set; }
            public long Size { get; set; }

            public int CompareTo(object _other)
            {
                FileModel other = _other as FileModel;

                if (FileType > other.FileType)
                    return 1;

                if (FileType < other.FileType)
                    return -1;

                return Name.CompareTo(other.Name);
            }
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

            list.Sort();

            return list;
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

        public static NasOperationStatus MakeCopy(string _filepath, string _filename, out string _newFilename)
        {
            _newFilename = null;
            NasOperationStatus status = NasOperationStatus.OperationMakeCopy;

            string path = Path.GetFullPath(DriveUtils.NasRootPath + "/" + _filepath);
            string srcDir = path + "/" + _filename;

            FileAttributes attrib = File.GetAttributes(srcDir);
            if (attrib.HasFlag(FileAttributes.Directory))
            {
                for (int i = 1; i < Constants.MaxFileDuplication; ++i)
                {
                    string dstDir = srcDir + " (" + i + ")";
                    if (!Directory.Exists(dstDir))
                    {
                        CopyTo(srcDir, dstDir, true);

                        status |= NasOperationStatus.OK;
                        _newFilename = _filename + " (" + i + ")";
                        return status;
                    }
                }

                status |= NasOperationStatus.MaximumDuplicationReached;
                return status;
            }
            else
            {
                /* The following code will mess up on file with "multiple extension", 
                 * for example `something.tar.gz` (extension `.tar.gz`).
                 * However I don't care, that is the user's responsibility.
                 * Making copies of a file is meaningless anyway.
                 */
                string name = _filename;
                string extension = "";
                var pos = _filename.LastIndexOf('.');
                if (pos > 0)
                {
                    name = _filename[0..pos];
                    extension = _filename[(pos + 1)..];
                }

                for (int i = 1; i < Constants.MaxFileDuplication; ++i)
                {
                    string dstDir = path + "/" + name + " (" + i + ")." + extension;
                    if (!File.Exists(dstDir))
                    {
                        File.Copy(srcDir, dstDir);

                        status |= NasOperationStatus.OK;
                        _newFilename = name + " (" + i + ")." + extension;
                        return status;
                    }
                }

                status |= NasOperationStatus.MaximumDuplicationReached;
                return status;
            }
        }

        public static NasOperationStatus CopyTo(string _srcDir, string _dstDir, bool _force = false)
        {
            NasOperationStatus status = NasOperationStatus.OperationCopyTo;

            if (!_force && Directory.Exists(_dstDir))
            {
                status |= NasOperationStatus.FileExisted;
                return status;
            }

            DirectoryInfo srcDi = new DirectoryInfo(_srcDir);
            DirectoryInfo[] srcDiSub = srcDi.GetDirectories();

            Directory.CreateDirectory(_dstDir);

            foreach(FileInfo fi in srcDi.GetFiles())
            {
                string dstFileName = _dstDir + "/" + fi.Name;
                fi.CopyTo(dstFileName);
            }

            foreach(DirectoryInfo di in srcDiSub)
            {
                string dstDirName = _dstDir + "/" + di.Name;
                CopyTo(di.FullName, dstDirName);
            }

            status |= NasOperationStatus.OK;
            return status;
        }

        public static NasOperationStatus MoveTo(string _srcDir, string _dstDir)
        {
            NasOperationStatus status = NasOperationStatus.OperationMoveTo;

            FileAttributes attrib = File.GetAttributes(_srcDir);
            if (attrib.HasFlag(FileAttributes.Directory))
            {
                if (Directory.Exists(_dstDir))
                {
                    status |= NasOperationStatus.FileExisted;
                    return status;
                }

                Directory.Move(_srcDir, _dstDir);

                status |= NasOperationStatus.OK;
                return status;
            }
            else
            {
                if (File.Exists(_dstDir))
                {
                    status |= NasOperationStatus.FileExisted;
                    return status;
                }

                File.Move(_srcDir, _dstDir);

                status |= NasOperationStatus.OK;
                return status;
            }
        }
    }

    public static class NasBrowserUtils
    {
        public class RequestResult
        {
            public bool IsFileRequested = false;
            public string RequestedFilePath = null;
            public NasBrowserViewModel Data = null;
        }

        public static RequestResult HandleBrowseRequest(string _path, bool _isUploadMode = false)
        {
            RequestResult result = new RequestResult()
            {
                Data = new NasBrowserViewModel()
                {
                    IsUploadMode = _isUploadMode
                }
            };

            string absPath = Path.GetFullPath(DriveUtils.NasRootPath + "/" + _path);
            if (!absPath.Contains("nasData"))
                return result;

            FileAttributes attrib = File.GetAttributes(absPath);
            if (!attrib.HasFlag(FileAttributes.Directory))
            {
                int pos = _path.LastIndexOf('/');
                if (pos < 0)
                    _path = "";
                else
                    _path = _path[0..pos];

                result.IsFileRequested = true;
                result.RequestedFilePath = absPath;
            }

            List<NasUtils.FileModel> files = NasUtils.GetDirectoryContent(_path);
            if (files == null)
                return result;

            List<string> pathLayers = new List<string>(_path.Split('/', StringSplitOptions.RemoveEmptyEntries));

            result.Data.Path = _path.Trim('/');
            result.Data.PathLayers = pathLayers;
            result.Data.Files = files;

            return result;
        }

    }

}
