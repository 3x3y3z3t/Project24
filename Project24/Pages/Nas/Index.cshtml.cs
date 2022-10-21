/*  Index.cshtml.cs
 *  Version: 1.3 (2022.10.22)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.Extensions.FileProviders;

namespace Project24.Pages.Nas
{
    [Authorize(Roles = P24Roles.NasUser + "," + P24Roles.NasTester)]
    public partial class IndexModel : PageModel
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

            public FileModel()
            { }
        }

        public class DataModel
        {
            public string Path { get; set; }
            public IList<string> PathLayers { get; set; }
            public IList<FileModel> Files { get; set; }

            public DataModel()
            {
                PathLayers = new List<string>();
                Files = new List<FileModel>();
            }
        }


        public DataModel Data { get; set; } = null;

        public IndexModel(ILogger<IndexModel> _logger)
        {


        }

        public async Task<IActionResult> OnGetAsync(string _path)
        {
            if (_path == null)
            {
                _path = "";
            }
            else if (_path.StartsWith("<upload>"))
            {
                return Partial("_NasUploader", this);
            }

            string absPath = Utils.AppRoot + "/" + AppConfig.NasRoot + "/" + _path;
            absPath = Path.GetFullPath(absPath).Replace('\\', '/').TrimEnd('/');

            if (!absPath.Contains("nasData"))
            {
                Data = new DataModel();
                return Partial("_NasBrowser", this);
            }

            FileAttributes attrib = System.IO.File.GetAttributes(absPath);
            if (!attrib.HasFlag(FileAttributes.Directory))
            {
                await ProcessDownloadRequest(absPath);

                int startIndex = absPath.IndexOf("nasData") + 7;
                int length = absPath.LastIndexOf('/') - startIndex;
                if (length <= 0)
                    return await OnGetAsync("");

                absPath = absPath.Substring(startIndex, length);
                return await OnGetAsync(absPath);
            }

            IList<FileModel> files = GetFilesInDirectory(absPath);

            if (files == null)
            {
                Data = new DataModel();
                return Partial("_NasBrowser", this);
            }

            List<string> pathLayers = _path.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();

            Data = new DataModel()
            {
                Path = _path,
                PathLayers = pathLayers,
                Files = files
            };

            return Partial("_NasBrowser", this);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            return BadRequest();
        }

        private IList<FileModel> GetFilesInDirectory(string _absPath)
        {
            int pos = _absPath.IndexOf("nasData") + 7;
            string relativePath = _absPath.Substring(pos);
            if (relativePath.Length != 0)
                relativePath += "/";

            List<FileModel> list = new List<FileModel>();

            string[] directories = Directory.GetDirectories(_absPath);
            foreach (string dir in directories)
            {
                DirectoryInfo di = new DirectoryInfo(dir);

                list.Add(new FileModel()
                {
                    FileType = FileType.Directory,
                    Name = di.Name,
                    Fullname = di.FullName,
                    RelativePath = relativePath,
                    LastModified = di.LastWriteTime
                });
            }

            string[] files = Directory.GetFiles(_absPath);
            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);

                list.Add(new FileModel()
                {
                    FileType = FileType.File,
                    Name = fi.Name,
                    Fullname = fi.FullName,
                    RelativePath = relativePath,
                    LastModified = fi.LastWriteTime,
                    Size = fi.Length
                });
            }

            return list;
        }

        private async Task ProcessDownloadRequest(string _absPath)
        {
            FileInfo fi = new FileInfo(_absPath);

            string encoded = System.Web.HttpUtility.UrlPathEncode(fi.Name);

            Response.Clear();

            Response.Headers.Add("Content-Disposition", string.Format("attachment; filename={0}", encoded));
            Response.Headers.Add("Content-Length", fi.Length.ToString());
            Response.ContentType = System.Net.Mime.MediaTypeNames.Application.Octet;

            var physFileInfo = new PhysicalFileProvider(fi.DirectoryName).GetFileInfo(fi.Name);

            await Response.SendFileAsync(physFileInfo);
        }
    }

}
