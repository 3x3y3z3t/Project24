/*  Home/Management/UpdaterPageDataModel.cs
 *  Version: v1.0 (2023.08.08)
 *  
 *  Author
 *      Arime-chan
 */

using System.Collections.Generic;
using Project24.App;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

namespace Project24.Model.Home.Maintenance
{
    public class UpdaterPageDataModel
    {
        public UpdaterStatus Status { get; set; } = UpdaterStatus.None;
        public string Main { get; set; } = null;
        public string Prev { get; set; } = null;
        public string Next { get; set; } = null;

        public List<FileInfoModel> Files { get; set; } = null;

        public string Message { get; set; } = "";

        public UpdaterPageDataModel()
        { }
    }

    public class UpdaterUploadMetadataModel
    {
        public int FilesCount { get; set; }
        public long FilesSize { get; set; }
        public int BatchesCount { get; set; }
        public List<KVPLongLong> LastModDates { get; set; }

        public UpdaterUploadMetadataModel()
        { }
    }

    public class FormFileDummy
    {
        [JsonInclude]
        public string name;
        [JsonInclude]
        public long size;
    }

    public class FileInfoModel
    {

        [JsonIgnore]
        public string Name { get => File.name; set => File.name = value; }
        public string Path { get; set; }
        public string LastMod { get; set; }
        [JsonIgnore]
        public long Size { get => File.size; set => File.size = value; }
        public FormFileDummy File { get; set; }
        public long HashCode { get; set; }

        public FileInfoModel()
        { File = new(); }

        public FileInfoModel(P24FileInfo _fileInfo, string _maskPath = "")
        {
            File = new();

            Name = _fileInfo.Name;
            Path = _fileInfo.Path.Replace(_maskPath, "").Replace('\\', '/').Trim('/');
            LastMod = MiscUtils.FormatDateTimeString_EndsWithMinute(_fileInfo.LastModified);
            Size = _fileInfo.Size;

            if (Path == "")
                HashCode = MiscUtils.ComputeCyrb53HashCode(_fileInfo.Name);
            else
                HashCode = MiscUtils.ComputeCyrb53HashCode(Path + "/" + _fileInfo.Name);

        }
    }

    public class UploadFileListModel
    {
        IList<UploadFileModel> FileList { get; set; } = null;


        public UploadFileListModel()
        { }
    }

    public class UploadFileModel
    {
        IFormFile FormFile { get; set; }
        string HashCode { get; set; }

        public UploadFileModel()
        { }
    }

}
