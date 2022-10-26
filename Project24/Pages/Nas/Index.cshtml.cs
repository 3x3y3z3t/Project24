/*  Index.cshtml.cs
 *  Version: 1.5 (2022.10.24)
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
using Project24.App.Utils;
using Project24.App;

namespace Project24.Pages.Nas
{
    [Authorize(Roles = P24Roles.NasUser + "," + P24Roles.NasTester)]
    public partial class IndexModel : PageModel
    {
        public class DataModel
        {
            public string Path { get; set; }
            public List<string> PathLayers { get; set; }
            public List<NasUtils.FileModel> Files { get; set; }
            public bool IsUploadMode { get; set; }

            public DataModel()
            {
                Path = "";
                PathLayers = new List<string>();
                Files = new List<NasUtils.FileModel>();
                IsUploadMode = false;
            }
        }

        public DataModel Data { get; set; } = null;


        public IndexModel(ILogger<IndexModel> _logger)
        {


        }


        public async Task<IActionResult> OnGetAsync(string _path)
        {
            const string prefixUpload = "<upload>";

            DataModel data = new DataModel();

            if (_path == null)
            {
                _path = "";
            }
            else if (_path.Contains(prefixUpload))
            {
                int pos = _path.LastIndexOf(prefixUpload) + prefixUpload.Length;
                _path = _path[pos..];

                data.IsUploadMode = true;
            }

            _path = _path.Trim('/');

            string absPath = Path.GetFullPath(DriveUtils.NasRootPath + "/" + _path);
            if (!absPath.Contains("nasData"))
            {
                return Partial("_NasBrowser", data);
            }

            if (!data.IsUploadMode)
            {
                FileAttributes attrib = System.IO.File.GetAttributes(absPath);
                if (!attrib.HasFlag(FileAttributes.Directory))
                {
                    await ProcessDownloadRequest(absPath);

                    //return await OnGetAsync(_path);

                    //int startIndex = absPath.IndexOf("nasData") + 7;
                    //int length = absPath.LastIndexOf('/') - startIndex;
                    //if (length <= 0)
                    //    return await OnGetAsync("");

                    //absPath = absPath.Substring(startIndex, length);
                    //return await OnGetAsync(absPath);
                }
            }

            List<NasUtils.FileModel> files = NasUtils.GetFilesInDirectory(_path);
            if (files == null)
            {
                return Partial("_NasBrowser", data);
            }

            List<string> pathLayers = new List<string>(_path.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries));

            data.Path = _path;
            data.PathLayers = pathLayers;
            data.Files = files;

            return Partial("_NasBrowser", data);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            return BadRequest();
        }

        private List<NasUtils.FileModel> GetFilesInDirectory(string _absPath)
        {
            int pos = _absPath.IndexOf("nasData") + 7;
            string relativePath = _absPath.Substring(pos);
            if (relativePath.Length != 0)
                relativePath += "/";

            List<NasUtils.FileModel> list = new List<NasUtils.FileModel>();

            string[] directories = Directory.GetDirectories(_absPath);
            foreach (string dir in directories)
            {
                DirectoryInfo di = new DirectoryInfo(dir);

                list.Add(new NasUtils.FileModel()
                {
                    FileType = NasUtils.FileType.Directory,
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

                list.Add(new NasUtils.FileModel()
                {
                    FileType = NasUtils.FileType.File,
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
